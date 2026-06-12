using backend.dtos.businesses;

namespace backend.Services;

public interface IBusinessService
{
    Task<BusinessResponseDto> CreateBusinessAsync(CreateBusinessRequestDto request, string userId);
    Task<BusinessResponseDto?> GetByUserIdAsync(string userId);
}