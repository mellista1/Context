using backend.Data;
using backend.dtos.businesses;
using backend.Entities.Business;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class BusinessService : IBusinessService
{
    private const int OwnerRoleId = 1;

    private readonly AppDbContext _context;

    public BusinessService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BusinessResponseDto> CreateBusinessAsync(CreateBusinessRequestDto request, string userId)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);

        if (!userExists)
        {
            throw new InvalidOperationException("User does not exist.");
        }

        var ownerRoleExists = await _context.BusinessRoles.AnyAsync(r => r.Id == OwnerRoleId);

        if (!ownerRoleExists)
        {
            throw new InvalidOperationException("Owner role does not exist.");
        }

        var business = new Business
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            Address = request.Address.Trim()
        };

        var membership = new BusinessMembership
        {
            UserId = userId,
            Business = business,
            RoleId = OwnerRoleId
        };

        _context.Businesses.Add(business);
        _context.BusinessMemberships.Add(membership);

        await _context.SaveChangesAsync();

        return new BusinessResponseDto
        {
            Id = business.Id,
            Name = business.Name,
            Description = business.Description,
            Address = business.Address,
            IsActive = business.IsActive,
            CreatedAt = business.CreatedAt
        };
    }

    public async Task<BusinessResponseDto?> GetByUserIdAsync(string userId)
    {
        var membership = await _context.BusinessMemberships
            .Include(m => m.Business)
            .FirstOrDefaultAsync(m => m.UserId == userId);

        if (membership is null) return null;

        var b = membership.Business;
        return new BusinessResponseDto
        {
            Id = b.Id,
            Name = b.Name,
            Description = b.Description,
            Address = b.Address,
            IsActive = b.IsActive,
            CreatedAt = b.CreatedAt
        };
    }
}