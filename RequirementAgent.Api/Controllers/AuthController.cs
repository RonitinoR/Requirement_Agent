using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RequirementAgent.Api.Data;
using RequirementAgent.Api.Dtos.Auth;
using RequirementAgent.Api.Services.Auth;

namespace RequirementAgent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext dbContext, ITokenService tokenService, ILogger<AuthController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null)
        {
            logger.LogInformation("Login attempt failed for {Email}", request.Email);
            return Unauthorized();
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            logger.LogInformation("Invalid password for {Email}", request.Email);
            return Unauthorized();
        }

        var token = tokenService.CreateToken(user);
        return Ok(new LoginResponse(token, user.Role.ToString(), user.Email));
    }
}
