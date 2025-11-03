namespace RequirementAgent.Api.Dtos.Submissions;

public record SubmissionDto(
    Guid Id,
    Guid PermitTypeId,
    string PermitTypeName,
    string ClientEmail,
    string ProjectName,
    string AnswersJson,
    string? Transcript,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
