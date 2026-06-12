namespace backend.Entities.Business;

public class BusinessMembership
{
    public int Id { get; set; }

    public required string UserId { get; set; }

    public int BusinessId { get; set; }

    public int RoleId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }

    public Business? Business { get; set; }

    public BusinessRole? Role { get; set; }
}