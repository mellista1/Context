namespace backend.dtos.notifications;

public class NewsItemDto
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";

    /// <summary>"calendar" | "suggestion"</summary>
    public string Type { get; set; } = "suggestion";

    public DateOnly? Date { get; set; }
    public string? Location { get; set; }
    public string? Link { get; set; }
}
