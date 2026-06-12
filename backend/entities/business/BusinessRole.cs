namespace backend.Entities.Business;

public class BusinessRole
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public ICollection<BusinessMembership> Memberships { get; set; } = new List<BusinessMembership>();
}