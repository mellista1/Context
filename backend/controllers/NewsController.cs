using backend.dtos.news;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/news")]
public class NewsController : ControllerBase
{
    private readonly INewsScraperService _newsScraperService;

    public NewsController(INewsScraperService newsScraperService)
    {
        _newsScraperService = newsScraperService;
    }

    [HttpPost("raw")]
    public async Task<ActionResult<NewsRawResponseDto>> GetRawArticle(NewsRawRequestDto request)
    {
        try
        {
            var result = await _newsScraperService.FetchRawArticleAsync(request.Url);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("enrich")]
    public async Task<ActionResult<IEnumerable<NewsEnrichedDto>>> EnrichNews(List<NewsRawRequestDto> requests)
    {
        var results = await _newsScraperService.FetchEnrichedArticlesAsync(requests);
        return Ok(results);
    }
}
