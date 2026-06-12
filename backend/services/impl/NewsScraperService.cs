using System.Text.Json;
using System.Text.RegularExpressions;
using backend.dtos.news;
using HtmlAgilityPack;

namespace backend.Services;

public class NewsScraperService : INewsScraperService
{
    private readonly HttpClient _httpClient;
    private readonly INewsRelevanceFilterService _newsRelevanceFilterService;

    private static readonly string[] NoisyTags =
        ["script", "style", "noscript", "svg", "nav", "footer", "header", "aside", "form", "button"];

    private static readonly HashSet<string> ArticleLikeJsonLdTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "NewsArticle",
        "Article",
        "ReportageNewsArticle",
        "BlogPosting"
    };

    private readonly INewsEnricherService _newsEnricherService;

    public NewsScraperService(
        HttpClient httpClient,
        INewsRelevanceFilterService newsRelevanceFilterService,
        INewsEnricherService newsEnricherService)
    {
        _httpClient = httpClient;
        _newsRelevanceFilterService = newsRelevanceFilterService;
        _newsEnricherService = newsEnricherService;
    }

    public async Task<IEnumerable<NewsEnrichedDto>> FetchEnrichedArticlesAsync(IEnumerable<NewsRawRequestDto> requests)
    {
        var results = new List<NewsEnrichedDto>();

        foreach (var request in requests)
        {
            try
            {
                var raw = await FetchRawArticleAsync(request.Url);
                var isRelevant = await _newsRelevanceFilterService.IsRelevantAsync(raw);

                if (!isRelevant)
                    continue;

                results.Add(await _newsEnricherService.EnrichAsync(raw));
            }
            catch (Exception)
            {
                // skip failed articles and continue with the rest
            }
        }

        return results;
    }

    public async Task<NewsRawResponseDto> FetchRawArticleAsync(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new ArgumentException("URL must be absolute.");

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            throw new ArgumentException("URL must use HTTP or HTTPS.");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(url);
        }
        catch (TaskCanceledException)
        {
            throw new HttpRequestException("Request timed out fetching the article.");
        }
        catch (HttpRequestException)
        {
            throw new HttpRequestException("Could not reach the article URL.");
        }

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Article URL returned status {(int)response.StatusCode}.");

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
        if (!contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("URL did not return an HTML document.");

        var html = await response.Content.ReadAsStringAsync();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var (jsonLdTitle, jsonLdDesc, jsonLdDate) = ExtractFromJsonLd(doc);
        var (ogTitle, ogDesc) = ExtractOpenGraph(doc);
        var (twTitle, twDesc) = ExtractTwitterMeta(doc);
        var standardTitle = ExtractTitleTag(doc);
        var standardDesc = ExtractMetaDescription(doc);

        RemoveNoisyNodes(doc);
        var rawText = ExtractRawText(doc);

        var res = new NewsRawResponseDto
        {
            SourceUrl = url,
            Title = jsonLdTitle ?? ogTitle ?? twTitle ?? standardTitle,
            Description = jsonLdDesc ?? ogDesc ?? twDesc ?? standardDesc,
            PublishedAt = jsonLdDate ?? ExtractOpenGraphDate(doc),
            RawText = rawText,
            ExtractedAt = DateTime.UtcNow
        };

        return res;
    }

    private static (string? title, string? description, string? publishedAt) ExtractFromJsonLd(HtmlDocument doc)
    {
        var scripts = doc.DocumentNode.SelectNodes("//script[@type='application/ld+json']");
        if (scripts == null) return (null, null, null);

        foreach (var script in scripts)
        {
            try
            {
                var json = HtmlEntity.DeEntitize(script.InnerText).Trim();

                if (string.IsNullOrWhiteSpace(json))
                    continue;

                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                var result = ExtractFromJsonLdElement(root);

                if (HasUsefulJsonLdArticleData(result))
                    return result;
            }
            catch (JsonException)
            {
                // Ignore invalid JSON-LD blocks and continue with the next one.
            }
        }

        return (null, null, null);
    }

    private static (string? title, string? description, string? publishedAt) ExtractFromJsonLdElement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var result = ExtractFromJsonLdElement(item);

                if (HasUsefulJsonLdArticleData(result))
                    return result;
            }

            return (null, null, null);
        }

        if (element.ValueKind != JsonValueKind.Object)
            return (null, null, null);

        if (element.TryGetProperty("@graph", out var graph) && graph.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in graph.EnumerateArray())
            {
                var result = ExtractFromJsonLdElement(item);

                if (HasUsefulJsonLdArticleData(result))
                    return result;
            }
        }

        return ParseJsonLdObject(element);
    }

    private static bool HasUsefulJsonLdArticleData(
        (string? title, string? description, string? publishedAt) result
    )
    {
        return !string.IsNullOrWhiteSpace(result.title)
            || !string.IsNullOrWhiteSpace(result.description)
            || !string.IsNullOrWhiteSpace(result.publishedAt);
    }

    private static (string? title, string? description, string? publishedAt) ParseJsonLdObject(JsonElement element)
    {
        if (!IsArticleLikeJsonLdObject(element))
            return (null, null, null);

        string? title = null;
        string? description = null;
        string? publishedAt = null;

        if (element.TryGetProperty("headline", out var headline))
            title = GetJsonStringValue(headline);
        else if (element.TryGetProperty("name", out var name))
            title = GetJsonStringValue(name);

        if (element.TryGetProperty("description", out var desc))
            description = GetJsonStringValue(desc);

        if (element.TryGetProperty("datePublished", out var datePublished))
            publishedAt = GetJsonStringValue(datePublished);

        return (
            CleanString(title),
            CleanString(description),
            CleanString(publishedAt)
        );
    }

    private static bool IsArticleLikeJsonLdObject(JsonElement element)
    {
        if (!element.TryGetProperty("@type", out var type))
            return false;

        if (type.ValueKind == JsonValueKind.String)
        {
            var typeValue = type.GetString();
            return typeValue is not null && ArticleLikeJsonLdTypes.Contains(typeValue);
        }

        if (type.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in type.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String)
                    continue;

                var typeValue = item.GetString();

                if (typeValue is not null && ArticleLikeJsonLdTypes.Contains(typeValue))
                    return true;
            }
        }

        return false;
    }

    private static string? GetJsonStringValue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
            return element.GetString();

        if (element.ValueKind == JsonValueKind.Array)
        {
            var firstString = element
                .EnumerateArray()
                .FirstOrDefault(item => item.ValueKind == JsonValueKind.String);

            return firstString.ValueKind == JsonValueKind.String
                ? firstString.GetString()
                : null;
        }

        return null;
    }

    private static (string? title, string? description) ExtractOpenGraph(HtmlDocument doc)
    {
        var title = GetMetaContent(doc, "property", "og:title");
        var description = GetMetaContent(doc, "property", "og:description");
        return (title, description);
    }

    private static string? ExtractOpenGraphDate(HtmlDocument doc) =>
        GetMetaContent(doc, "property", "article:published_time")
        ?? GetMetaContent(doc, "name", "date")
        ?? GetMetaContent(doc, "name", "pubdate")
        ?? GetMetaContent(doc, "name", "publish-date");

    private static (string? title, string? description) ExtractTwitterMeta(HtmlDocument doc)
    {
        var title = GetMetaContent(doc, "name", "twitter:title");
        var description = GetMetaContent(doc, "name", "twitter:description");
        return (title, description);
    }

    private static string? ExtractTitleTag(HtmlDocument doc)
    {
        var title = doc.DocumentNode.SelectSingleNode("//title")?.InnerText;

        return CleanString(title);
    }

    private static string? ExtractMetaDescription(HtmlDocument doc) =>
        GetMetaContent(doc, "name", "description");

    private static string? GetMetaContent(HtmlDocument doc, string attribute, string value)
    {
        var node = doc.DocumentNode.SelectSingleNode($"//meta[@{attribute}='{value}']");
        var content = node?.GetAttributeValue("content", null);

        return CleanString(content);
    }

    private static void RemoveNoisyNodes(HtmlDocument doc)
    {
        var xpath = string.Join("|", NoisyTags.Select(t => $"//{t}"));
        var nodes = doc.DocumentNode.SelectNodes(xpath);

        if (nodes == null) return;

        foreach (var node in nodes.ToList())
            node.Remove();
    }

    private static string ExtractRawText(HtmlDocument doc)
    {
        var raw = doc.DocumentNode.InnerText;
        var decoded = HtmlEntity.DeEntitize(raw);

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var lines = decoded
            .Split('\n')
            .Select(l => Regex.Replace(l.Trim(), @"\s+", " "))
            .Where(l => l.Length > 25)
            .Where(seen.Add);

        return string.Join("\n", lines);
    }

    private static string? CleanString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var decoded = HtmlEntity.DeEntitize(value);
        var normalized = Regex.Replace(decoded.Trim(), @"\s+", " ");

        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}