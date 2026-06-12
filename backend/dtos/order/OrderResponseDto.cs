using backend.dtos.products;

namespace backend.dtos.orders;

public class OrderResponseDto
{
    public int Id { get; set; }

    public int TableNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<ProductResponseDto> Products { get; set; } = [];

    public decimal TotalPrice { get; set; }
}
