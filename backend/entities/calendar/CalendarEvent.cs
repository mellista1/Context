namespace backend.Entities.Calendar;

public class CalendarEvent
{
    public int Id { get; set; }

    public int BusinessId { get; set; }

    public required string EventType { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public string? Location { get; set; }

    public DateOnly EventDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Business.Business Business { get; set; } = null!;
}
