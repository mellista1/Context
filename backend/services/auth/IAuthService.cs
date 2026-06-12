using System.Security.Claims;
using backend.dtos.auth;

namespace backend.services.auth;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<CurrentUserDto> GetCurrentUserAsync(ClaimsPrincipal userClaims);
    Task DeleteAccountAsync(string userId);
}