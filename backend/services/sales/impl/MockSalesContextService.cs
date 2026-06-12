namespace backend.Services.Sales;

// TODO: Replace with RealSalesContextService when the orders branch merges.
// The real implementation will query:
//
//   var total = await _context.Orders
//       .Where(o => /* filter by business */ && o.CreatedAt.Date == date.Date)
//       .SumAsync(o => o.TotalAmount);
//
//   var topProduct = await _context.OrderProducts        // navigation table: OrderId, ProductName, Quantity
//       .Where(op => op.Order.CreatedAt.Date == date.Date /* + business filter */)
//       .GroupBy(op => op.ProductName)
//       .OrderByDescending(g => g.Sum(op => op.Quantity))
//       .Select(g => new { g.Key, Total = g.Sum(op => op.Quantity) })
//       .FirstOrDefaultAsync();

public class MockSalesContextService : ISalesContextService
{
    private static readonly (string Product, int Quantity, decimal Total, decimal Increase)[] Outcomes =
    [
        ("Scon de queso",      47, 195_000m, 35m),
        ("Medialunas",         38, 162_000m, 22m),
        ("Empanadas de carne", 52, 178_000m, 41m),
        ("Scon de queso",      31, 140_000m, 18m),
        ("Croissant",          29, 135_000m, 15m),
    ];

    public Task<DailySalesSummary?> GetSalesSummaryForDateAsync(
        int businessId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        if (date >= DateOnly.FromDateTime(DateTime.UtcNow))
            return Task.FromResult<DailySalesSummary?>(null);

        var idx = Math.Abs(date.DayOfYear + businessId) % Outcomes.Length;
        var (product, qty, total, increase) = Outcomes[idx];

        return Task.FromResult<DailySalesSummary?>(
            new DailySalesSummary(total, product, qty, increase));
    }
}
