using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RequirementAgent.Api.Models;

public class Submission
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid PermitTypeId { get; set; }

    [ForeignKey(nameof(PermitTypeId))]
    public PermitType PermitType { get; set; } = default!;

    [Required]
    [MaxLength(256)]
    public string ClientEmail { get; set; } = default!;

    [Required]
    [MaxLength(256)]
    public string ProjectName { get; set; } = default!;

    public string AnswersJson { get; set; } = "{}";

    public string? Transcript { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Upload> Uploads { get; set; } = new List<Upload>();
}
