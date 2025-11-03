using RequirementAgent.Api.Dtos.PermitTypes;
using RequirementAgent.Api.Models;

namespace RequirementAgent.Api.Extensions;

public static class PermitTypeExtensions
{
    public static PermitTypeDto ToDto(this PermitType permitType) => new(
        permitType.Id,
        permitType.Name,
        permitType.Description,
        permitType.IsActive,
        permitType.CreatedAt,
        permitType.UpdatedAt);
}
