using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using backend.dtos.news;
using backend.dtos.notifications;

namespace backend.Services;

public class NewsEnricherService : INewsEnricherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NewsEnricherService> _logger;
    private readonly string _apiKey;

    private const int MaxRawTextLength = 3000;

    private const string SystemPrompt = """
        You are a structured data extractor for FoodOps, a restaurant operations platform.
        You will receive a news article. Extract the following fields and return them as a strict JSON object.
        No markdown, no code fences, no explanation — only the JSON object.

        Fields:
        - "description": A concise 1-2 sentence summary of the article in the same language as the article.
        - "type": Either "calendar" if the article mentions a concrete date or scheduled event, or "suggestion" if relevant but has no specific actionable date.
        - "date": The event or publication date in ISO 8601 UTC format (e.g. "2026-06-05T09:00:00Z"), or null if not available.
        - "location": The most specific area, neighborhood, or city mentioned, or null if not mentioned.

        Return only this JSON: {"description": "...", "type": "calendar" or "suggestion", "date": "..." or null, "location": "..." or null}
        """;

    public NewsEnricherService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<NewsEnricherService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["Anthropic:ApiKey"]
            ?? throw new InvalidOperationException("Anthropic:ApiKey is not configured");
    }

    public async Task<NewsEnrichedDto> EnrichAsync(NewsRawResponseDto raw)
    {
        var content = BuildArticleContent(raw);

        var extraction = await CallClaudeAsync(content);

        return new NewsEnrichedDto
        {
            Title = raw.Title ?? raw.SourceUrl,
            Description = extraction?.Description ?? string.Empty,
            Type = extraction?.Type ?? "suggestion",
            Date = extraction?.Date is string dateStr && DateTime.TryParse(dateStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsedDate)
                ? DateOnly.FromDateTime(parsedDate)
                : null,
            Location = extraction?.Location,
            Link = raw.SourceUrl
        };
    }

    private static string BuildArticleContent(NewsRawResponseDto raw)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(raw.Title))
            sb.AppendLine($"Title: {raw.Title}");

        if (!string.IsNullOrWhiteSpace(raw.Description))
            sb.AppendLine($"Description: {raw.Description}");

        if (!string.IsNullOrWhiteSpace(raw.RawText))
        {
            var truncated = raw.RawText.Length > MaxRawTextLength
                ? raw.RawText[..MaxRawTextLength]
                : raw.RawText;
            sb.AppendLine($"Content: {truncated}");
        }

        return sb.ToString();
    }

    private async Task<EnrichmentResult?> CallClaudeAsync(string articleContent, bool isRetry = false)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/messages");
            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var body = new
            {
                model = "claude-haiku-4-5",
                max_tokens = 300,
                system = SystemPrompt,
                messages = new[]
                {
                    new { role = "user", content = articleContent }
                }
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            using var response = await _httpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if ((int)response.StatusCode == 429 && !isRetry)
                {
                    var delay = ParseResetDelay(responseText);
                    _logger.LogWarning("Rate limited, retrying in {Seconds}s...", (int)delay.TotalSeconds);
                    await Task.Delay(delay);
                    return await CallClaudeAsync(articleContent, isRetry: true);
                }

                _logger.LogError(
                    "Anthropic API failed. Status: {StatusCode}. Body: {Body}",
                    response.StatusCode,
                    responseText
                );
                return null;
            }

            using var json = JsonDocument.Parse(responseText);
            var text = json.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            return ParseEnrichmentResult(StripMarkdownFences(text.Trim()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Claude enrichment call failed");
            return null;
        }
    }

    private EnrichmentResult? ParseEnrichmentResult(string jsonText)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonText);
            var root = doc.RootElement;

            return new EnrichmentResult(
                Description: root.TryGetProperty("description", out var d) ? d.GetString() : null,
                Type: root.TryGetProperty("type", out var t) ? t.GetString() : null,
                Date: root.TryGetProperty("date", out var dt) && dt.ValueKind != JsonValueKind.Null
                    ? dt.GetString()
                    : null,
                Location: root.TryGetProperty("location", out var loc) && loc.ValueKind != JsonValueKind.Null
                    ? loc.GetString()
                    : null
            );
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Claude enrichment response: {Response}", jsonText);
            return null;
        }
    }

    private static string StripMarkdownFences(string text)
    {
        if (text.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewline = text.IndexOf('\n');
            if (firstNewline >= 0)
                text = text[(firstNewline + 1)..];
        }

        if (text.EndsWith("```", StringComparison.Ordinal))
            text = text[..^3];

        return text.Trim();
    }

    private static TimeSpan ParseResetDelay(string responseText)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseText);
            var message = doc.RootElement.GetProperty("error").GetProperty("message").GetString() ?? "";

            const string marker = "Limit resets at: ";
            var idx = message.IndexOf(marker, StringComparison.Ordinal);
            if (idx >= 0)
            {
                var resetStr = message[(idx + marker.Length)..].Replace(" UTC", "Z").Trim();
                if (DateTimeOffset.TryParse(resetStr, out var resetTime))
                {
                    var delay = resetTime.UtcDateTime - DateTime.UtcNow;
                    if (delay > TimeSpan.Zero)
                        return delay.Add(TimeSpan.FromSeconds(1));
                }
            }
        }
        catch { }

        return TimeSpan.FromSeconds(60);
    }

    private sealed record EnrichmentResult(
        string? Description,
        string? Type,
        string? Date,
        string? Location
    );
}
