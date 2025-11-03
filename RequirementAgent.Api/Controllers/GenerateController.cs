using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RequirementAgent.Api.Services.DocumentGeneration;

namespace RequirementAgent.Api.Controllers;

[ApiController]
[Route("api/submissions/{submissionId:guid}/[controller]")]
[Authorize(Roles = "Admin")]
public class GenerateController(IDocumentGenerationService documentGenerationService, ILogger<GenerateController> logger) : ControllerBase
{
    [HttpGet("usecase")]
    public async Task<IActionResult> GetUseCase(Guid submissionId, CancellationToken cancellationToken)
    {
        var result = await documentGenerationService.GenerateUseCaseAsync(submissionId, cancellationToken);
        logger.LogInformation("Generated Use Case for submission {SubmissionId}", submissionId);
        return File(Encoding.UTF8.GetBytes(result.Content), result.ContentType, result.FileName);
    }

    [HttpGet("userstories")]
    public async Task<IActionResult> GetUserStories(Guid submissionId, CancellationToken cancellationToken)
    {
        var result = await documentGenerationService.GenerateUserStoriesAsync(submissionId, cancellationToken);
        logger.LogInformation("Generated User Stories for submission {SubmissionId}", submissionId);
        return File(Encoding.UTF8.GetBytes(result.Content), result.ContentType, result.FileName);
    }

    [HttpGet("datadictionary")]
    public async Task<IActionResult> GetDataDictionary(Guid submissionId, CancellationToken cancellationToken)
    {
        var result = await documentGenerationService.GenerateDataDictionaryAsync(submissionId, cancellationToken);
        logger.LogInformation("Generated Data Dictionary for submission {SubmissionId}", submissionId);
        return File(Encoding.UTF8.GetBytes(result.Content), result.ContentType, result.FileName);
    }

    [HttpGet("aipack")]
    public async Task<IActionResult> GetAiPack(Guid submissionId, CancellationToken cancellationToken)
    {
        var documents = await documentGenerationService.GenerateAiPackAsync(submissionId, cancellationToken);

        await using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var document in documents)
            {
                var entry = archive.CreateEntry(document.FileName, CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                var buffer = Encoding.UTF8.GetBytes(document.Content);
                await entryStream.WriteAsync(buffer, cancellationToken);
            }
        }

        memoryStream.Position = 0;
        logger.LogInformation("Generated AI pack for submission {SubmissionId}", submissionId);
        return File(memoryStream.ToArray(), "application/zip", "AI_Pack.zip");
    }
}

