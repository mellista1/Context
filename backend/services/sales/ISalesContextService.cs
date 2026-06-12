namespace backend.Services.Sales;

public interface ISalesContextService
{
    /// <summary>
    /// Returns a sales summary for the given business on a specific date.
    /// Returns null if no sales data exists for that date (e.g., future dates or no orders recorded).
    /// </summary>
    Task<DailySalesSummary?> GetSalesSummaryForDateAsync(
        int businessId,
        DateOnly date,
        CancellationToken cancellationToken = default);
}

public record DailySalesSummary(
    decimal TotalSalesAmount,
    string TopProduct,
    int TopProductQuantity,
    decimal? IncreaseVsAveragePercent
);
