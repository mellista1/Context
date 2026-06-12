using backend.dtos.news;
using backend.dtos.notifications;

namespace backend.Services;

public interface INewsEnricherService
{
    Task<NewsItemDto> EnrichAsync(NewsRawResponseDto raw);
}
