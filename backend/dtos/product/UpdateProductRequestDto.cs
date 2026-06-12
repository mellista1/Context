namespace backend.dtos.products;

public class UpdateProductRequestDto
{
    public required string Name { get; set; }

    public required string Description { get; set; }

    public decimal Price { get; set; }
}
