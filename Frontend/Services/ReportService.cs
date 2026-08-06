using Frontend.Models;

namespace Frontend.Services;

/// <summary>Admin-only sales and inventory reports.</summary>
public static class ReportService
{
    public static Task<MonthlySalesReport> GetMonthlySalesAsync(int year, int month) =>
        ApiClient.GetAsync<MonthlySalesReport>($"/reports/monthly-sales?year={year}&month={month}");

    public static Task<YearlySalesReport> GetYearlySalesAsync(int year) =>
        ApiClient.GetAsync<YearlySalesReport>($"/reports/yearly-sales?year={year}");

    public static Task<ProductSalesReport> GetProductSalesAsync() =>
        ApiClient.GetAsync<ProductSalesReport>("/reports/product-sales");

    public static Task<InventoryReport> GetInventoryAsync() =>
        ApiClient.GetAsync<InventoryReport>("/reports/inventory");
}
