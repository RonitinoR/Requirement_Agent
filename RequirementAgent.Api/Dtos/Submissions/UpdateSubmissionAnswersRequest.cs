using System.ComponentModel.DataAnnotations;

namespace RequirementAgent.Api.Dtos.Submissions;

public class UpdateSubmissionAnswersRequest
{
    [Required]
    public string AnswersJson { get; set; } = "{}";

    public string? Transcript { get; set; }
}
