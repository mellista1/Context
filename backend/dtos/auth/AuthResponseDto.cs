namespace backend.dtos.auth;

public class AuthResponseDto
{
    public required string Token { get; set; }
    public required CurrentUserDto User { get; set; }
}