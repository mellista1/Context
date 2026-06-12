using backend.Data;
using backend.dtos.orders;
using backend.dtos.products;
using backend.Entities.Order;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto request)
    {
        var products = await _context.Products
            .Where(p => request.ProductIds.Contains(p.Id))
            .ToListAsync();

        var order = new Order
        {
            TableNumber = request.TableNumber,
            CreatedAt = DateTime.UtcNow,
            Products = products
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return MapToDto(order);
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Products)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            return null;
        }

        return MapToDto(order);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.Products)
            .ToListAsync();

        return orders.Select(MapToDto);
    }

    private static OrderResponseDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        TableNumber = order.TableNumber,
        CreatedAt = order.CreatedAt,
        Products = order.Products.Select(p => new ProductResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price
        }).ToList(),
        TotalPrice = order.TotalPrice
    };
}
