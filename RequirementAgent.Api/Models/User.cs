using System.ComponentModel.DataAnnotations;
using RequirementAgent.Api.Models.Enums;

namespace RequirementAgent.Api.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = default!;

    [Required]
    [MaxLength(512)]
    public string PasswordHash { get; set; } = default!;

    [Required]
    public UserRole Role { get; set; }
}
