using backend.Entities;

namespace backend.Services.Auth;

public interface ITokenService
{
    string CreateToken(ApplicationUser user);
}