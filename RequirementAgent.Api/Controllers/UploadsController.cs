using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RequirementAgent.Api.Data;
using RequirementAgent.Api.Dtos.Uploads;
using RequirementAgent.Api.Extensions;
using RequirementAgent.Api.Models;

namespace RequirementAgent.Api.Controllers;

[ApiController]
[Route("api/submissions/{submissionId:guid}/[controller]")]
public class UploadsController(AppDbContext dbContext, IWebHostEnvironment environment, ILogger<UploadsController> logger) : ControllerBase
{
    private const string UploadFolder = "uploads";

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UploadDto>>> GetUploads(Guid submissionId, CancellationToken cancellationToken = default)
    {
        var uploads = await dbContext.Uploads
            .Where(u => u.SubmissionId == submissionId)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => u.ToDto())
            .ToListAsync(cancellationToken);

        return Ok(uploads);
    }

    [HttpPost]
    [Authorize(Roles = "Client,Admin")]
    [RequestSizeLimit(1024L * 1024L * 50L)]
    public async Task<ActionResult<UploadDto>> UploadFile(Guid submissionId, IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("File is required.");
        }

        var submission = await dbContext.Submissions.FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);
        if (submission is null)
        {
            return NotFound("Submission not found.");
        }

        var uploadsRoot = Path.Combine(environment.ContentRootPath, "wwwroot", UploadFolder, submissionId.ToString());
        Directory.CreateDirectory(uploadsRoot);

        var fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadsRoot, fileName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var relativePath = Path.Combine("/", UploadFolder, submissionId.ToString(), fileName).Replace("\\", "/");

        var upload = new Upload
        {
            Id = Guid.NewGuid(),
            SubmissionId = submissionId,
            FileName = file.FileName,
            Mime = file.ContentType,
            Size = file.Length,
            Url = relativePath,
            StoragePath = filePath
        };

        dbContext.Uploads.Add(upload);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Uploaded file {FileName} for submission {SubmissionId}", file.FileName, submissionId);

        return Created(relativePath, upload.ToDto());
    }
}

