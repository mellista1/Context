using System.Security.Claims;
using backend.Data;
using backend.dtos.news;
using backend.dtos.notifications;
using backend.Services;
using backend.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private static readonly List<NewsRawRequestDto> HardcodedArticleLinks =
    [
        new() { Url = "https://www.infobae.com/deportes/2026/06/06/la-seleccion-argentina-confirmo-que-leonardo-balerdi-no-jugara-el-mundial-por-lesion/"},
        new() { Url = "https://www.lanacion.com.ar/que-sale/arte-en-recoleta-te-en-un-palacio-y-cenas-con-ramen-vino-y-omakase-la-agenda-gastro-de-la-semana-nid03062026/"},
        new() { Url = "https://buenosaires.gob.ar/gcaba_historico/noticias/cortes-y-desvios-por-evento-musical-en-river-plate?utm_source=chatgpt.com"},
        new() { Url = "https://www.timeout.com/es/buenos-aires/nuevas-aperturas-restaurantes-sushi-parrilla-hamburguesas-bares-gastronomia?utm_source=chatgpt.com"},
        new() { Url = "https://turismo.buenosaires.gob.ar/es/article/buenos-aires-fan-fest"},
    ];

    private readonly INotificationService _notificationService;
    private readonly INewsScraperService _newsScraperService;
    private readonly AppDbContext _context;

    public NotificationsController(
        INotificationService notificationService,
        INewsScraperService newsScraperService,
        AppDbContext context)
    {
        _notificationService = notificationService;
        _newsScraperService = newsScraperService;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProcessedNotificationDto>>> GetNotifications(
        CancellationToken cancellationToken)
    {
        var businessId = await ResolveBusinessIdAsync(cancellationToken);
        if (businessId is null) return Unauthorized("El usuario no tiene un negocio asociado.");

        var notifications = await _notificationService.GetNotificationsAsync(
            businessId.Value, cancellationToken);

        return Ok(notifications);
    }

    [HttpPost("process")]
    public async Task<ActionResult<List<ProcessedNotificationDto>>> ProcessNewsItems(
        [FromBody] List<NewsItemDto> items,
        CancellationToken cancellationToken)
    {
        if (items is null || items.Count == 0)
            return BadRequest("Se requiere al menos un ítem de noticias.");

        var businessId = await ResolveBusinessIdAsync(cancellationToken);
        if (businessId is null) return Unauthorized("El usuario no tiene un negocio asociado.");

        var notifications = await _notificationService.ProcessNewsItemsAsync(
            items, businessId.Value, cancellationToken);

        return Ok(notifications);
    }

    [HttpPost("update-notifications")]
    public async Task<ActionResult<List<ProcessedNotificationDto>>> UpdateNotifications(
        CancellationToken cancellationToken)
    {
        var businessId = await ResolveBusinessIdAsync(cancellationToken);
        if (businessId is null) return Unauthorized("El usuario no tiene un negocio asociado.");

        var enrichedArticles = await _newsScraperService.FetchEnrichedArticlesAsync(
            HardcodedArticleLinks);

        var items = enrichedArticles.ToList();
        if (items.Count == 0)
            return Ok(new List<ProcessedNotificationDto>());

        var notifications = await _notificationService.ProcessNewsItemsAsync(
            items, businessId.Value, cancellationToken);

        return Ok(notifications);
    }

    private async Task<int?> ResolveBusinessIdAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return null;

        var membership = await _context.BusinessMemberships
            .FirstOrDefaultAsync(m => m.UserId == userId && m.IsActive, cancellationToken);

        return membership?.BusinessId;
    }
}
