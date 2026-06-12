using System.Security.Claims;
using backend.Data;
using backend.dtos.calendar;
using backend.Services.Calendar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/calendar-events")]
[Authorize]
public class CalendarEventsController : ControllerBase
{
    private readonly ICalendarEventService _calendarEventService;
    private readonly AppDbContext _context;

    public CalendarEventsController(ICalendarEventService calendarEventService, AppDbContext context)
    {
        _calendarEventService = calendarEventService;
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<CalendarEventResponseDto>> CreateCalendarEvent(
        [FromBody] CreateCalendarEventDto dto,
        CancellationToken cancellationToken)
    {
        var businessId = await ResolveBusinessIdAsync(cancellationToken);
        if (businessId is null) return Unauthorized("El usuario no tiene un negocio asociado.");

        var result = await _calendarEventService.CreateIfNotExistsAsync(
            businessId.Value, dto, cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<CalendarEventResponseDto>>> GetCalendarEvents(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken)
    {
        var businessId = await ResolveBusinessIdAsync(cancellationToken);
        if (businessId is null) return Unauthorized("El usuario no tiene un negocio asociado.");

        var now = DateTime.UtcNow;
        var y = year ?? now.Year;
        var m = month ?? now.Month;

        var result = await _calendarEventService.GetEventsForMonthAsync(
            businessId.Value, y, m, cancellationToken);

        return Ok(result);
    }

    private async Task<int?> ResolveBusinessIdAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return null;

        var membership = await _context.BusinessMemberships
            .FirstOrDefaultAsync(m => m.UserId == userId && m.IsActive, cancellationToken);

        return membership?.BusinessId;
    }
}
