using System.Text;
using System.Text.Json;
using backend.dtos.calendar;
using backend.dtos.notifications;
using backend.Services.Calendar;

namespace backend.Services.Notifications;

public class NotificationService : INotificationService
{
    private static readonly string[] DayNames =
        ["Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"];

    private static readonly string[] MonthNamesShort =
        ["ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic"];

    private static readonly string[] MonthNamesLong =
        ["enero", "febrero", "marzo", "abril", "mayo", "junio",
         "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"];

    private static readonly List<NewsItemDto> MockNewsItems =
    [
        new NewsItemDto
        {
            Title = "Festival Gastronómico en Plaza Italia este fin de semana",
            Description = "Se esperan más de 20.000 visitantes en la feria de gastronomía local en Palermo. Habrá puestos de comida, música en vivo y actividades para toda la familia.",
            Type = "calendar",
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            Location = "Palermo",
            Link = "https://example.com/festival-palermo"
        },
        new NewsItemDto
        {
            Title = "Corte de tránsito en Av. Corrientes por obras de pavimentación",
            Description = "Se prevé un corte total en Av. Corrientes entre Florida y Maipú a partir del lunes. Se estima que afectará el tránsito por 72 horas.",
            Type = "calendar",
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            Location = "Microcentro",
            Link = "https://example.com/corte-corrientes"
        },
        new NewsItemDto
        {
            Title = "Tendencia: empanadas artesanales viralizan en redes sociales porteñas",
            Description = "Las empanadas de masa artesanal con rellenos creativos están siendo compartidas masivamente en Instagram y TikTok en Buenos Aires, generando colas en locales del barrio de Palermo y San Telmo.",
            Type = "suggestion",
            Date = null,
            Location = null,
            Link = "https://example.com/tendencia-empanadas"
        }
    ];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;
    private readonly ICalendarEventService _calendarEventService;

    public NotificationService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<NotificationService> logger,
        ICalendarEventService calendarEventService)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _calendarEventService = calendarEventService;
    }

    public Task<List<ProcessedNotificationDto>> GetNotificationsAsync(
        int businessId,
        CancellationToken cancellationToken = default)
        => ProcessNewsItemsAsync(MockNewsItems, businessId, cancellationToken);

    public async Task<List<ProcessedNotificationDto>> ProcessNewsItemsAsync(
        List<NewsItemDto> items,
        int businessId,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ProcessedNotificationDto>();

        foreach (var item in items)
        {
            var notification = BuildNotification(item);
            var calendarEvent = BuildCalendarEvent(item);
            var aiSuggestion = await GetAiSuggestionAsync(item, notification, businessId, cancellationToken);

            if (calendarEvent.ShouldCreate && item.Date.HasValue)
            {
                await _calendarEventService.CreateIfNotExistsAsync(businessId, new CreateCalendarEventDto
                {
                    EventType = calendarEvent.EventType,
                    Title = item.Title,
                    Description = item.Description,
                    Location = item.Location,
                    EventDate = item.Date.Value,
                }, cancellationToken);
            }

            results.Add(new ProcessedNotificationDto
            {
                Notification = notification,
                CalendarEvent = calendarEvent,
                AiSuggestion = aiSuggestion
            });
        }

        return results;
    }

    private static NotificationDetailDto BuildNotification(NewsItemDto item)
    {
        var summary = item.Description.Length > 160
            ? item.Description[..157] + "..."
            : item.Description;

        return new NotificationDetailDto
        {
            Title = item.Title,
            Summary = summary,
            Date = item.Date.HasValue ? FormatDateSpanish(item.Date.Value) : null,
            Location = item.Location
        };
    }

    private static CalendarEventDto BuildCalendarEvent(NewsItemDto item)
    {
        var isCalendar = item.Type == "calendar";

        return new CalendarEventDto
        {
            ShouldCreate = isCalendar,
            EventType = DetectEventType(item),
            Title = item.Title,
            Date = isCalendar ? item.Date?.ToString("yyyy-MM-dd") : null
        };
    }

    private static string DetectEventType(NewsItemDto item)
    {
        var text = $"{item.Title} {item.Description}".ToLower();

        if (ContainsAny(text, "festival", "feria", "marcha", "concierto", "evento", "fiesta", "carnaval", "encuentro"))
            return "mass_event";

        if (ContainsAny(text, "corte", "tránsito", "paro", "huelga", "subte", "colectivo", "tren", "obra", "paviment"))
            return "service_disruption";

        if (ContainsAny(text, "lluvia", "tormenta", "granizo", "calor", "frío", "temperatura", "clima", "viento"))
            return "weather";

        if (ContainsAny(text, "tendencia", "viral", "trend", "popular", "redes sociales", "instagram", "tiktok"))
            return "trend";

        return "other";
    }

    private static bool ContainsAny(string text, params string[] keywords)
        => keywords.Any(text.Contains);

    private static string FormatDateSpanish(DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (date == today)
            return "Hoy";
        if (date == today.AddDays(1))
            return "Mañana";
        if (date <= today.AddDays(7))
            return DayNames[(int)date.DayOfWeek];

        return $"{DayNames[(int)date.DayOfWeek]} {date.Day} de {MonthNamesLong[date.Month - 1]}";
    }

    private async Task<string> GetAiSuggestionAsync(
        NewsItemDto item,
        NotificationDetailDto notification,
        int businessId,
        CancellationToken cancellationToken)
    {
        var apiKey = _configuration["Anthropic:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogDebug("ANTHROPIC_API_KEY not configured — skipping AI suggestion for '{Title}'", item.Title);
            return "";
        }

        try
        {
            var prompt = await BuildAiPromptAsync(item, notification, businessId, cancellationToken);
            return await CallAnthropicApiAsync(apiKey, prompt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI suggestion failed for '{Title}' — returning empty string", item.Title);
            return "";
        }
    }

    private async Task<string> BuildAiPromptAsync(
        NewsItemDto item,
        NotificationDetailDto notification,
        int businessId,
        CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Sos un asistente para dueños de negocios gastronómicos en Argentina.");
        sb.AppendLine("Generá una sugerencia corta y accionable en español (es-AR) basada en el siguiente evento o situación.");
        sb.AppendLine("Respondé SOLO con la sugerencia, sin encabezados, comillas ni explicaciones. Máximo 2 oraciones.");
        sb.AppendLine();
        sb.AppendLine($"Evento: {notification.Title}");
        sb.AppendLine($"Descripción: {notification.Summary}");

        if (notification.Date is not null)
            sb.AppendLine($"Cuándo: {notification.Date}");

        if (notification.Location is not null)
            sb.AppendLine($"Dónde: {notification.Location}");

        sb.AppendLine();

        var eventType = BuildCalendarEvent(item).EventType;
        var history = await _calendarEventService.GetHistoricalContextAsync(
            businessId, eventType, 5, cancellationToken);

        if (history.Count > 0)
        {
            sb.AppendLine("Historial de eventos similares registrados en tu negocio:");
            foreach (var ctx in history)
            {
                var dateStr = $"{ctx.EventDate.Day} {MonthNamesShort[ctx.EventDate.Month - 1]} {ctx.EventDate.Year}";
                var increase = ctx.Sales.IncreaseVsAveragePercent.HasValue
                    ? $", ventas +{ctx.Sales.IncreaseVsAveragePercent}% vs promedio"
                    : "";
                sb.AppendLine($"- {ctx.Title} ({dateStr}): producto estrella {ctx.Sales.TopProduct} ({ctx.Sales.TopProductQuantity} unidades){increase}");
            }
        }
        else
        {
            sb.AppendLine("No hay historial previo de eventos similares registrados para este negocio.");
        }

        return sb.ToString();
    }

    private async Task<string> CallAnthropicApiAsync(
        string apiKey,
        string prompt,
        CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            model = "claude-sonnet-4-20250514",
            max_tokens = 256,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/messages");
        request.Headers.Add("x-api-key", apiKey);
        request.Content = content;

        var httpClient = _httpClientFactory.CreateClient("Anthropic");
        var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseJson);

        return doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? "";
    }
}
