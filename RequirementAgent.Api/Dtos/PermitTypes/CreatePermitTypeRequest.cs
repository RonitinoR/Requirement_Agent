using System.ComponentModel.DataAnnotations;

namespace RequirementAgent.Api.Dtos.PermitTypes;

public class CreatePermitTypeRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
