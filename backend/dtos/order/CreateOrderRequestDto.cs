namespace backend.dtos.orders;

public class CreateOrderRequestDto
{
    public int TableNumber { get; set; }

    public required List<int> ProductIds { get; set; }
}
