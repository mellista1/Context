namespace backend.dtos.auth;

public class CurrentUserDto
{
    public required string UserId { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public required string FullName { get; set; } = string.Empty;
}