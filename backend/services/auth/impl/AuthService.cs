using System.Security.Claims;
using backend.dtos.auth;
using backend.Entities;
using Microsoft.AspNetCore.Identity;
using backend.Services.Auth;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.services.auth.impl;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _dbContext;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        AppDbContext dbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException("Email already registered.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }

        var token = _tokenService.CreateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            User = new CurrentUserDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName
            }
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            false
        );

        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var token = _tokenService.CreateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            User = new CurrentUserDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName
            }
        };
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(ClaimsPrincipal userClaims)
    {
        var userId = userClaims.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            throw new UnauthorizedAccessException("Invalid token.");
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        return new CurrentUserDto
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName
        };
    }

    public async Task DeleteAccountAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var memberships = await _dbContext.BusinessMemberships
                .Include(membership => membership.Role)
                .Where(membership => membership.UserId == userId)
                .ToListAsync();

            var ownedBusinessIds = memberships
                .Where(membership => membership.Role.Name == "Owner")
                .Select(membership => membership.BusinessId)
                .Distinct()
                .ToList();

            var businessesToDelete = await _dbContext.Businesses
                .Where(business => ownedBusinessIds.Contains(business.Id))
                .ToListAsync();

            _dbContext.BusinessMemberships.RemoveRange(memberships);

            if (businessesToDelete.Count > 0)
            {
                _dbContext.Businesses.RemoveRange(businessesToDelete);
            }

            var deleteUserResult = await _userManager.DeleteAsync(user);

            if (!deleteUserResult.Succeeded)
            {
                var errors = string.Join(", ", deleteUserResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not delete user: {errors}");
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}