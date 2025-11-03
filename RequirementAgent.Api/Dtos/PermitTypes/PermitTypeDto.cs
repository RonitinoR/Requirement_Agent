namespace RequirementAgent.Api.Dtos.PermitTypes;

public record PermitTypeDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

