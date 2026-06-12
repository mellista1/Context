using backend.dtos.news;

namespace backend.Services;

public interface INewsRelevanceFilterService
{
    Task<bool> IsRelevantAsync(NewsRawResponseDto dto);
}
