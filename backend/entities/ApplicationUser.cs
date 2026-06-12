using backend.Entities.Business;
using Microsoft.AspNetCore.Identity;

namespace backend.Entities;

public class ApplicationUser : IdentityUser
{
    public required string FullName { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    
    public ICollection<BusinessMembership> BusinessMemberships { get; set; } = new List<BusinessMembership>();
}