using RequirementAgent.Api.Models;

namespace RequirementAgent.Api.Services.Auth;

public interface ITokenService
{
    string CreateToken(User user);
}
