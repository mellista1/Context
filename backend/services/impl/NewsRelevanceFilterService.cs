using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using backend.dtos.news;

namespace backend.Services;

public class NewsRelevanceFilterService : INewsRelevanceFilterService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NewsRelevanceFilterService> _logger;
    private readonly string _apiKey;

    private const string TitleSystemPrompt = """
        You are a relevance filter for FoodOps, a restaurant operations platform.
        You will receive a news article title. Determine whether it could be relevant to a restaurant business.

        Respond with ONLY "YES" or "NO". No explanation, no punctuation, nothing else.

        An article is relevant if its title suggests an event, disruption, or situation that could affect:
        restaurant sales, customer demand, delivery operations, traffic, street accessibility, foot traffic,
        stock planning, or staffing needs.

        Relevant topics: food festivals, concerts, sports matches, cultural events, fairs, public holidays,
        special dates, street closures, transport strikes, subway/bus/train disruptions, protests,
        demonstrations, weather alerts, tourism events, massive events, neighborhood celebrations,
        marathons, road closures, power outages, water outages, safety alerts affecting an area,
        events that bring many people to a zone.

        Irrelevant topics: generic national politics, international news, celebrity gossip, opinion articles,
        generic economy news, sports results with no event/location impact, crime with no operational impact,
        technology news, health news with no local business effect.
        """;

    private const string DescriptionSystemPrompt = """
        You are a relevance filter for FoodOps, a restaurant operations platform.
        You will receive a news article description. Determine whether it confirms that the article is relevant to a restaurant business.

        Respond with ONLY "YES" or "NO". No explanation, no punctuation, nothing else.

        Confirm relevance if the description provides evidence of an event, disruption, or situation that could affect:
        restaurant sales, customer demand, delivery operations, traffic, street accessibility, foot traffic,
        stock planning, or staffing needs.

        Reject if the description reveals the article is actually about generic politics, international news,
        celebrity gossip, opinion, economy, crime with no local operational impact, or technology/health topics
        with no direct restaurant business effect.
        """;

    public NewsRelevanceFilterService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<NewsRelevanceFilterService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _apiKey = configuration["Anthropic:ApiKey"]
            ?? throw new InvalidOperationException("Anthropic:ApiKey is not configured");
    }

    public async Task<bool> IsRelevantAsync(NewsRawResponseDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return false;

        return await CallClaudeAsync(TitleSystemPrompt, dto.Title);
    }

    private async Task<bool> CallClaudeAsync(string systemPrompt, string content, bool isRetry = false)
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
                max_tokens = 10,
                system = systemPrompt,
                messages = new[]
                {
                    new { role = "user", content }
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
                    return await CallClaudeAsync(systemPrompt, content, isRetry: true);
                }

                _logger.LogError(
                    "Anthropic API failed. Status: {StatusCode}. Body: {Body}",
                    response.StatusCode,
                    responseText
                );

                throw new HttpRequestException(
                    $"Anthropic API failed with status code {response.StatusCode}"
                );
            }

            using var json = JsonDocument.Parse(responseText);

            var text = json.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            return text.Trim().StartsWith("YES", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Anthropic call failed");
            throw;
        }
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
}