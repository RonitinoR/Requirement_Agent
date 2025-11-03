using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RequirementAgent.Api.Data;
using RequirementAgent.Api.Dtos.Submissions;
using RequirementAgent.Api.Extensions;
using RequirementAgent.Api.Models;

namespace RequirementAgent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubmissionsController(AppDbContext dbContext, ILogger<SubmissionsController> logger) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<SubmissionDto>>> GetSubmissions([FromQuery] Guid? permitTypeId, [FromQuery] string? clientEmail, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Submissions.Include(s => s.PermitType).AsQueryable();

        if (permitTypeId.HasValue)
        {
            query = query.Where(s => s.PermitTypeId == permitTypeId);
        }

        if (!string.IsNullOrWhiteSpace(clientEmail))
        {
            query = query.Where(s => s.ClientEmail == clientEmail);
        }

        var results = await query
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => s.ToDto())
            .ToListAsync(cancellationToken);

        return Ok(results);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SubmissionDto>> GetSubmission(Guid id, CancellationToken cancellationToken = default)
    {
        var submission = await dbContext.Submissions
            .Include(s => s.PermitType)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (submission is null)
        {
            return NotFound();
        }

        return Ok(submission.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Client,Admin")]
    public async Task<ActionResult<SubmissionDto>> CreateSubmission([FromBody] CreateSubmissionRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var permitType = await dbContext.PermitTypes.FirstOrDefaultAsync(pt => pt.Id == request.PermitTypeId && pt.IsActive, cancellationToken);
        if (permitType is null)
        {
            return BadRequest("Permit type is not active or does not exist.");
        }

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            PermitTypeId = request.PermitTypeId,
            ClientEmail = request.ClientEmail,
            ProjectName = request.ProjectName,
            AnswersJson = request.AnswersJson,
            Transcript = request.Transcript
        };

        dbContext.Submissions.Add(submission);
        await dbContext.SaveChangesAsync(cancellationToken);

        submission.PermitType = permitType;
        logger.LogInformation("Created submission {SubmissionId} for {PermitType}", submission.Id, permitType.Name);
        return CreatedAtAction(nameof(GetSubmission), new { id = submission.Id }, submission.ToDto());
    }

    [HttpPut("{id:guid}/answers")]
    [Authorize(Roles = "Client,Admin")]
    public async Task<ActionResult<SubmissionDto>> UpdateSubmissionAnswers(Guid id, [FromBody] UpdateSubmissionAnswersRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var submission = await dbContext.Submissions.Include(s => s.PermitType).FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (submission is null)
        {
            return NotFound();
        }

        submission.AnswersJson = request.AnswersJson;
        submission.Transcript = request.Transcript;
        submission.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated answers for submission {SubmissionId}", submission.Id);
        return Ok(submission.ToDto());
    }
}

