namespace backend.dtos.news;

public class NewsRawResponseDto
{
    public required string SourceUrl { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? PublishedAt { get; set; }
    public string? RawText { get; set; }
    public DateTime ExtractedAt { get; set; }
}
