namespace backend.dtos.businesses;

public class BusinessResponseDto
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public required string Address { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}