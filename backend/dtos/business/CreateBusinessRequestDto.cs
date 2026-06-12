namespace backend.dtos.businesses;

public class CreateBusinessRequestDto
{
    public required string Name { get; set; }

    public required string Description { get; set; }

    public required string Address { get; set; }
}