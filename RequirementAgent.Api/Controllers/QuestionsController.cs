using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RequirementAgent.Api.Data;
using RequirementAgent.Api.Dtos.Questions;
using RequirementAgent.Api.Extensions;
using RequirementAgent.Api.Models;

namespace RequirementAgent.Api.Controllers;

[ApiController]
[Route("api")]
public class QuestionsController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("permittypes/{permitTypeId:guid}/questions")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<QuestionDto>>> GetQuestionsForPermit(Guid permitTypeId, CancellationToken cancellationToken = default)
    {
        var questions = await dbContext.Questions
            .Where(q => q.PermitTypeId == permitTypeId)
            .OrderBy(q => q.Order)
            .Select(q => q.ToDto())
            .ToListAsync(cancellationToken);

        return Ok(questions);
    }

    [HttpGet("questions/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<QuestionDto>> GetQuestion(Guid id, CancellationToken cancellationToken = default)
    {
        var question = await dbContext.Questions.FindAsync(new object?[] { id }, cancellationToken);
        return question is null ? NotFound() : Ok(question.ToDto());
    }

    [HttpPost("permittypes/{permitTypeId:guid}/questions")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<QuestionDto>> CreateQuestion(Guid permitTypeId, [FromBody] CreateQuestionRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (permitTypeId != request.PermitTypeId)
        {
            return BadRequest("Permit type mismatch between route and body.");
        }

        var permitExists = await dbContext.PermitTypes.AnyAsync(pt => pt.Id == permitTypeId, cancellationToken);
        if (!permitExists)
        {
            return NotFound($"Permit type {permitTypeId} not found.");
        }

        var question = new Question
        {
            Id = Guid.NewGuid(),
            PermitTypeId = permitTypeId,
            Order = request.Order,
            Key = request.Key,
            Prompt = request.Prompt,
            Type = request.Type,
            OptionsJson = request.OptionsJson,
            Required = request.Required
        };

        dbContext.Questions.Add(question);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, question.ToDto());
    }

    [HttpPut("questions/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<QuestionDto>> UpdateQuestion(Guid id, [FromBody] UpdateQuestionRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var question = await dbContext.Questions.FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
        if (question is null)
        {
            return NotFound();
        }

        if (question.PermitTypeId != request.PermitTypeId)
        {
            var permitExists = await dbContext.PermitTypes.AnyAsync(pt => pt.Id == request.PermitTypeId, cancellationToken);
            if (!permitExists)
            {
                return BadRequest("Target permit type does not exist.");
            }

            question.PermitTypeId = request.PermitTypeId;
        }

        question.Order = request.Order;
        question.Key = request.Key;
        question.Prompt = request.Prompt;
        question.Type = request.Type;
        question.OptionsJson = request.OptionsJson;
        question.Required = request.Required;
        question.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(question.ToDto());
    }

    [HttpDelete("questions/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteQuestion(Guid id, CancellationToken cancellationToken = default)
    {
        var question = await dbContext.Questions.FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
        if (question is null)
        {
            return NotFound();
        }

        dbContext.Questions.Remove(question);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
