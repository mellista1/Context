namespace backend.Entities.Business;

public class Business
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public required string Address { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BusinessMembership> Memberships { get; set; } = new List<BusinessMembership>();
}