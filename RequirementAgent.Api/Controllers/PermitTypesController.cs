using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RequirementAgent.Api.Data;
using RequirementAgent.Api.Dtos.PermitTypes;
using RequirementAgent.Api.Extensions;
using RequirementAgent.Api.Models;

namespace RequirementAgent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermitTypesController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PermitTypeDto>>> GetPermitTypes([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = dbContext.PermitTypes.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(pt => pt.IsActive);
        }

        var results = await query
            .OrderBy(pt => pt.Name)
            .Select(pt => pt.ToDto())
            .ToListAsync(cancellationToken);

        return Ok(results);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<PermitTypeDto>> GetPermitType(Guid id, CancellationToken cancellationToken = default)
    {
        var permitType = await dbContext.PermitTypes.FindAsync(new object?[] { id }, cancellationToken);
        return permitType is null ? NotFound() : Ok(permitType.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PermitTypeDto>> CreatePermitType([FromBody] CreatePermitTypeRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var permitType = new PermitType
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive
        };

        dbContext.PermitTypes.Add(permitType);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetPermitType), new { id = permitType.Id }, permitType.ToDto());
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PermitTypeDto>> UpdatePermitType(Guid id, [FromBody] UpdatePermitTypeRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var permitType = await dbContext.PermitTypes.FirstOrDefaultAsync(pt => pt.Id == id, cancellationToken);
        if (permitType is null)
        {
            return NotFound();
        }

        permitType.Name = request.Name;
        permitType.Description = request.Description;
        permitType.IsActive = request.IsActive;
        permitType.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(permitType.ToDto());
    }
}

