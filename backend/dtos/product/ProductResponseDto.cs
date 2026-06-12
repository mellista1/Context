namespace backend.dtos.products;

public class ProductResponseDto
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public decimal Price { get; set; }
}
