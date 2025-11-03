using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RequirementAgent.Api.Models;

public class Upload
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid SubmissionId { get; set; }

    [ForeignKey(nameof(SubmissionId))]
    public Submission Submission { get; set; } = default!;

    [Required]
    [MaxLength(512)]
    public string FileName { get; set; } = default!;

    [MaxLength(128)]
    public string? Mime { get; set; }

    public long Size { get; set; }

    [MaxLength(1024)]
    public string? Url { get; set; }

    [MaxLength(1024)]
    public string? StoragePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
