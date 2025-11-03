using System.ComponentModel.DataAnnotations;

namespace RequirementAgent.Api.Dtos.Submissions;

public class CreateSubmissionRequest
{
    [Required]
    public Guid PermitTypeId { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string ClientEmail { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string ProjectName { get; set; } = string.Empty;

    public string AnswersJson { get; set; } = "{}";

    public string? Transcript { get; set; }
}
