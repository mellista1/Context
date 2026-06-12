using backend.Data;
using backend.dtos.calendar;
using backend.Entities.Calendar;
using backend.Services.Sales;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Calendar;

public class CalendarEventService : ICalendarEventService
{
    private readonly AppDbContext _context;
    private readonly ISalesContextService _salesContextService;

    public CalendarEventService(AppDbContext context, ISalesContextService salesContextService)
    {
        _context = context;
        _salesContextService = salesContextService;
    }

    public async Task<CalendarEventResponseDto> CreateIfNotExistsAsync(
        int businessId,
        CreateCalendarEventDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.CalendarEvents.FirstOrDefaultAsync(
            ce => ce.BusinessId == businessId
               && ce.Title == dto.Title
               && ce.EventDate == dto.EventDate,
            cancellationToken);

        if (existing is not null)
            return MapToDto(existing, null);

        var calendarEvent = new CalendarEvent
        {
            BusinessId = businessId,
            EventType = dto.EventType,
            Title = dto.Title,
            Description = dto.Description,
            Location = dto.Location,
            EventDate = dto.EventDate,
        };

        _context.CalendarEvents.Add(calendarEvent);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(calendarEvent, null);
    }

    public async Task<List<CalendarEventResponseDto>> GetEventsForMonthAsync(
        int businessId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1);

        var events = await _context.CalendarEvents
            .Where(ce => ce.BusinessId == businessId && ce.EventDate >= start && ce.EventDate < end)
            .OrderBy(ce => ce.EventDate)
            .ToListAsync(cancellationToken);

        var result = new List<CalendarEventResponseDto>(events.Count);

        foreach (var ev in events)
        {
            DailySalesSummary? sales = null;

            if (ev.EventDate < DateOnly.FromDateTime(DateTime.UtcNow))
                sales = await _salesContextService.GetSalesSummaryForDateAsync(
                    businessId, ev.EventDate, cancellationToken);

            result.Add(MapToDto(ev, sales));
        }

        return result;
    }

    public async Task<List<CalendarEventWithContext>> GetHistoricalContextAsync(
        int businessId,
        string eventType,
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        var past = await _context.CalendarEvents
            .Where(ce => ce.BusinessId == businessId
                      && ce.EventType == eventType
                      && ce.EventDate < DateOnly.FromDateTime(DateTime.UtcNow))
            .OrderByDescending(ce => ce.EventDate)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var result = new List<CalendarEventWithContext>();

        foreach (var ev in past)
        {
            var sales = await _salesContextService.GetSalesSummaryForDateAsync(
                businessId, ev.EventDate, cancellationToken);

            if (sales is not null)
                result.Add(new CalendarEventWithContext(ev.EventType, ev.Title, ev.EventDate, ev.Location, sales));
        }

        return result;
    }

    private static CalendarEventResponseDto MapToDto(CalendarEvent ev, DailySalesSummary? sales)
    {
        SalesOutcomeDto? outcome = null;

        if (sales is not null)
        {
            outcome = new SalesOutcomeDto
            {
                TopProduct = sales.TopProduct,
                TopProductQuantity = sales.TopProductQuantity,
                SalesIncreasePercent = sales.IncreaseVsAveragePercent,
            };
        }

        return new CalendarEventResponseDto
        {
            Id = ev.Id,
            EventType = ev.EventType,
            Title = ev.Title,
            Description = ev.Description,
            Location = ev.Location,
            EventDate = ev.EventDate,
            Outcome = outcome,
        };
    }
}
