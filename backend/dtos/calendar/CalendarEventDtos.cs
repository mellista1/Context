namespace backend.dtos.calendar;

public class CreateCalendarEventDto
{
    public required string EventType { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? Location { get; set; }
    public DateOnly EventDate { get; set; }
}

public class CalendarEventResponseDto
{
    public int Id { get; set; }
    public string EventType { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Location { get; set; }
    public DateOnly EventDate { get; set; }
    /// <summary>null when the event is upcoming or no orders data exists for that date yet</summary>
    public SalesOutcomeDto? Outcome { get; set; }
}

public class SalesOutcomeDto
{
    public string TopProduct { get; set; } = "";
    public int TopProductQuantity { get; set; }
    public decimal? SalesIncreasePercent { get; set; }
}
