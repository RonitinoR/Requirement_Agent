using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RequirementAgent.Api.Data;
using RequirementAgent.Api.Models;

namespace RequirementAgent.Api.Services.DocumentGeneration;

public class DocumentGenerationService(AppDbContext dbContext, ILogger<DocumentGenerationService> logger) : IDocumentGenerationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<DocumentResult> GenerateUseCaseAsync(Guid submissionId, CancellationToken cancellationToken = default)
    {
        var submission = await LoadSubmissionAsync(submissionId, cancellationToken);
        var answers = ParseAnswers(submission);

        var builder = new StringBuilder();
        builder.AppendLine("# Use Case: " + submission.ProjectName);
        builder.AppendLine();
        builder.AppendLine($"**Primary Actor:** Client ({submission.ClientEmail})");
        builder.AppendLine("**Permit Type:** " + submission.PermitType.Name);
        builder.AppendLine();
        builder.AppendLine("## Main Success Scenario");
        foreach (var answer in answers)
        {
            builder.AppendLine($"- {FormatKey(answer.Key)}: {answer.Value}");
        }

        builder.AppendLine();
        builder.AppendLine("## Extensions");
        builder.AppendLine("- Capture outstanding questions or dependencies in the open items document.");

        return new DocumentResult("UseCase.md", "text/markdown", builder.ToString());
    }

    public async Task<DocumentResult> GenerateUserStoriesAsync(Guid submissionId, CancellationToken cancellationToken = default)
    {
        var submission = await LoadSubmissionAsync(submissionId, cancellationToken);
        var answers = ParseAnswers(submission);

        var builder = new StringBuilder();
        builder.AppendLine("# User Stories");
        builder.AppendLine();

        foreach (var answer in answers)
        {
            builder.AppendLine($"## {FormatKey(answer.Key)}");
            builder.AppendLine("As an Admin, I want to capture \"" + FormatKey(answer.Key) + "\" so that the implementation team has the necessary detail.");
            builder.AppendLine();
            builder.AppendLine("### Acceptance Criteria");
            builder.AppendLine("- [ ] Response captured: " + answer.Value);
            builder.AppendLine();
        }

        return new DocumentResult("UserStories.md", "text/markdown", builder.ToString());
    }

    public async Task<DocumentResult> GenerateDataDictionaryAsync(Guid submissionId, CancellationToken cancellationToken = default)
    {
        var submission = await LoadSubmissionAsync(submissionId, cancellationToken);
        var answers = ParseAnswers(submission);

        var builder = new StringBuilder();
        builder.AppendLine("Key,Value");
        foreach (var answer in answers)
        {
            builder.AppendLine($"\"{EscapeCsv(answer.Key)}\",\"{EscapeCsv(answer.Value)}\"");
        }

        return new DocumentResult("DataDictionary.csv", "text/csv", builder.ToString());
    }

    public async Task<IReadOnlyList<DocumentResult>> GenerateAiPackAsync(Guid submissionId, CancellationToken cancellationToken = default)
    {
        var submission = await LoadSubmissionAsync(submissionId, cancellationToken);
        var answers = ParseAnswers(submission);

        var summary = new StringBuilder();
        summary.AppendLine("# Summary");
        summary.AppendLine($"Permit: {submission.PermitType.Name}");
        summary.AppendLine($"Project: {submission.ProjectName}");
        summary.AppendLine($"Client Email: {submission.ClientEmail}");
        summary.AppendLine("Total Answers: " + answers.Count);

        var detailed = new StringBuilder();
        detailed.AppendLine("# Detailed Requirements");
        foreach (var answer in answers)
        {
            detailed.AppendLine($"## {FormatKey(answer.Key)}");
            detailed.AppendLine(answer.Value);
            detailed.AppendLine();
        }

        var configJson = JsonSerializer.Serialize(new
        {
            submissionId,
            permitType = submission.PermitType.Name,
            answers,
            generatedAtUtc = DateTime.UtcNow
        }, new JsonSerializerOptions { WriteIndented = true });

        var openItems = new StringBuilder();
        openItems.AppendLine("# Open Items");
        openItems.AppendLine("- [ ] Review answers and capture outstanding questions.");
        openItems.AppendLine("- [ ] Confirm file uploads cover all necessary documents.");

        return new List<DocumentResult>
        {
            new("Summary.md", "text/markdown", summary.ToString()),
            new("Detailed_Requirements.md", "text/markdown", detailed.ToString()),
            new("Config.json", "application/json", configJson),
            new("Open_Items.md", "text/markdown", openItems.ToString())
        };
    }

    private async Task<Submission> LoadSubmissionAsync(Guid submissionId, CancellationToken cancellationToken)
    {
        var submission = await dbContext.Submissions
            .Include(s => s.PermitType)
            .Include(s => s.Uploads)
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);

        if (submission is null)
        {
            throw new KeyNotFoundException($"Submission {submissionId} was not found.");
        }

        return submission;
    }

    private Dictionary<string, string> ParseAnswers(Submission submission)
    {
        if (string.IsNullOrWhiteSpace(submission.AnswersJson))
        {
            return new Dictionary<string, string>();
        }

        try
        {
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(submission.AnswersJson, JsonOptions)
                             ?? new Dictionary<string, JsonElement>();

            return dictionary.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ValueKind switch
                {
                    JsonValueKind.Array => string.Join(", ", pair.Value.EnumerateArray().Select(x => x.ToString())),
                    JsonValueKind.Object => pair.Value.ToString(),
                    JsonValueKind.String => pair.Value.GetString() ?? string.Empty,
                    JsonValueKind.Null => string.Empty,
                    _ => pair.Value.ToString()
                },
                StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse answers for submission {SubmissionId}.", submission.Id);
            return new Dictionary<string, string>();
        }
    }

    private static string EscapeCsv(string value) => value.Replace("\"", "\"\"");

    private static string FormatKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return key;
        }

        return string.Join(' ', key
            .Replace("_", " ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
    }
}
