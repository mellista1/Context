using backend.dtos.orders;

namespace backend.Services;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto request);

    Task<OrderResponseDto?> GetOrderByIdAsync(int id);

    Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync();
}
