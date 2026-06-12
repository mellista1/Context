using System.Security.Claims;
using backend.dtos.businesses;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BusinessesController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public BusinessesController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<BusinessResponseDto>> GetMyBusiness()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null) return Unauthorized();

        var business = await _businessService.GetByUserIdAsync(userId);

        if (business is null) return NotFound();

        return Ok(business);
    }

    [HttpPost]
    public async Task<ActionResult<BusinessResponseDto>> CreateBusiness(CreateBusinessRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return Unauthorized();
        }

        var business = await _businessService.CreateBusinessAsync(request, userId);
        return StatusCode(StatusCodes.Status201Created, business);
    }
}