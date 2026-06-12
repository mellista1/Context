using System.ComponentModel.DataAnnotations.Schema;
using backend.Entities.Products;

namespace backend.Entities.Order;

public class Order
{
    public int Id { get; set; }

    public int TableNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<Product> Products { get; set; } = [];

    [NotMapped]
    public decimal TotalPrice => Products.Sum(p => p.Price);
}
