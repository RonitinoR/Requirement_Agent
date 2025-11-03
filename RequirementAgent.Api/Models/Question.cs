using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RequirementAgent.Api.Models.Enums;

namespace RequirementAgent.Api.Models;

public class Question
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid PermitTypeId { get; set; }

    [ForeignKey(nameof(PermitTypeId))]
    public PermitType PermitType { get; set; } = default!;

    [Required]
    public int Order { get; set; }

    [Required]
    [MaxLength(200)]
    public string Key { get; set; } = default!;

    [Required]
    [MaxLength(1000)]
    public string Prompt { get; set; } = default!;

    [Required]
    public QuestionType Type { get; set; }

    public string? OptionsJson { get; set; }

    public bool Required { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
