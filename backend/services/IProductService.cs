using backend.dtos.products;

namespace backend.Services;

public interface IProductService
{
    Task<IEnumerable<ProductResponseDto>> GetAllAsync();

    Task<ProductResponseDto?> GetByIdAsync(int id);

    Task<ProductResponseDto> CreateAsync(CreateProductRequestDto request);

    Task<ProductResponseDto?> UpdateAsync(int id, UpdateProductRequestDto request);

    Task<bool> DeleteAsync(int id);
}
