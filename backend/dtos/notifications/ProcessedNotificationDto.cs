namespace backend.dtos.notifications;

public class ProcessedNotificationDto
{
    public NotificationDetailDto Notification { get; set; } = null!;
    public CalendarEventDto CalendarEvent { get; set; } = null!;
    public string AiSuggestion { get; set; } = "";
}

public class NotificationDetailDto
{
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";

    /// <summary>Human-readable date in Spanish, e.g. "Viernes a las 9"</summary>
    public string? Date { get; set; }

    public string? Location { get; set; }
}

public class CalendarEventDto
{
    public bool ShouldCreate { get; set; }

    /// <summary>"mass_event" | "service_disruption" | "weather" | "trend" | "other"</summary>
    public string EventType { get; set; } = "other";

    public string Title { get; set; } = "";

    /// <summary>ISO8601 date string, null for non-calendar items</summary>
    public string? Date { get; set; }
}
