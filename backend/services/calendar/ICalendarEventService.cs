using backend.dtos.calendar;
using backend.Services.Sales;

namespace backend.Services.Calendar;

public record CalendarEventWithContext(
    string EventType,
    string Title,
    DateOnly EventDate,
    string? Location,
    DailySalesSummary Sales
);

public interface ICalendarEventService
{
    /// <summary>
    /// Creates the event if no event with the same business+title+date already exists.
    /// Returns the existing or newly created event.
    /// </summary>
    Task<CalendarEventResponseDto> CreateIfNotExistsAsync(
        int businessId,
        CreateCalendarEventDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all calendar events for the given month, enriched with sales outcome
    /// for events that already occurred.
    /// </summary>
    Task<List<CalendarEventResponseDto>> GetEventsForMonthAsync(
        int businessId,
        int year,
        int month,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the last <paramref name="limit"/> past events of the same type
    /// that have sales data. Used to build the AI suggestion prompt.
    /// </summary>
    Task<List<CalendarEventWithContext>> GetHistoricalContextAsync(
        int businessId,
        string eventType,
        int limit = 5,
        CancellationToken cancellationToken = default);
}
