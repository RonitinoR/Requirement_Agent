namespace RequirementAgent.Api.Dtos.Uploads;

public record UploadDto(
    Guid Id,
    Guid SubmissionId,
    string FileName,
    string? Mime,
    long Size,
    string? Url,
    DateTime CreatedAt
);
