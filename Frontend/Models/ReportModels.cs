using System.Text.Json.Serialization;

namespace Frontend.Models;

public class DailySales
{
    [JsonPropertyName("day")] public int Day { get; set; }
    [JsonPropertyName("total_sales")] public decimal TotalSales { get; set; }
    [JsonPropertyName("order_count")] public int OrderCount { get; set; }
}

public class MonthlySalesReport
{
    [JsonPropertyName("year")] public int Year { get; set; }
    [JsonPropertyName("month")] public int Month { get; set; }
    [JsonPropertyName("total_sales")] public decimal TotalSales { get; set; }
    [JsonPropertyName("order_count")] public int OrderCount { get; set; }
    [JsonPropertyName("daily_breakdown")] public List<DailySales> DailyBreakdown { get; set; } = new();
}

public class MonthlyBreakdownItem
{
    [JsonPropertyName("month")] public int Month { get; set; }
    [JsonPropertyName("total_sales")] public decimal TotalSales { get; set; }
    [JsonPropertyName("order_count")] public int OrderCount { get; set; }
}

public class YearlySalesReport
{
    [JsonPropertyName("year")] public int Year { get; set; }
    [JsonPropertyName("total_sales")] public decimal TotalSales { get; set; }
    [JsonPropertyName("order_count")] public int OrderCount { get; set; }
    [JsonPropertyName("monthly_breakdown")] public List<MonthlyBreakdownItem> MonthlyBreakdown { get; set; } = new();
}

public class ProductSalesItem
{
    [JsonPropertyName("product_id")] public int? ProductId { get; set; }
    [JsonPropertyName("product_name")] public string ProductName { get; set; }
    [JsonPropertyName("quantity_sold")] public int QuantitySold { get; set; }
    [JsonPropertyName("total_revenue")] public decimal TotalRevenue { get; set; }
}

public class ProductSalesReport
{
    [JsonPropertyName("items")] public List<ProductSalesItem> Items { get; set; } = new();
}

public class InventoryItem
{
    [JsonPropertyName("product_id")] public int ProductId { get; set; }
    [JsonPropertyName("product_name")] public string ProductName { get; set; }
    [JsonPropertyName("category_name")] public string CategoryName { get; set; }
    [JsonPropertyName("price")] public decimal Price { get; set; }
    [JsonPropertyName("stock_quantity")] public int StockQuantity { get; set; }
    [JsonPropertyName("is_low_stock")] public bool IsLowStock { get; set; }
    [JsonPropertyName("is_active")] public bool IsActive { get; set; }
}

public class InventoryReport
{
    [JsonPropertyName("items")] public List<InventoryItem> Items { get; set; } = new();
    [JsonPropertyName("low_stock_threshold")] public int LowStockThreshold { get; set; }
}
