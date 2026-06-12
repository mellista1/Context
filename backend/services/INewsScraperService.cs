using backend.dtos.news;

namespace backend.Services;

public interface INewsScraperService
{
    Task<NewsRawResponseDto> FetchRawArticleAsync(string url);
    Task<IEnumerable<NewsEnrichedDto>> FetchEnrichedArticlesAsync(IEnumerable<NewsRawRequestDto> requests);
}
