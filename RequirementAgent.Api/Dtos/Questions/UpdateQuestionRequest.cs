using System.ComponentModel.DataAnnotations;
using RequirementAgent.Api.Models.Enums;

namespace RequirementAgent.Api.Dtos.Questions;

public class UpdateQuestionRequest
{
    [Required]
    public Guid PermitTypeId { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Order { get; set; }

    [Required]
    [MaxLength(200)]
    public string Key { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Prompt { get; set; } = string.Empty;

    [Required]
    public QuestionType Type { get; set; }

    public string? OptionsJson { get; set; }

    public bool Required { get; set; }
}
