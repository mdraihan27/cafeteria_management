using System.Text.Json.Serialization;

namespace Frontend.Models;

public class AdminDashboard
{
    [JsonPropertyName("total_products")] public int TotalProducts { get; set; }
    [JsonPropertyName("total_customers")] public int TotalCustomers { get; set; }
    [JsonPropertyName("total_orders")] public int TotalOrders { get; set; }
    [JsonPropertyName("monthly_sales")] public decimal MonthlySales { get; set; }
    [JsonPropertyName("low_stock_products")] public List<Product> LowStockProducts { get; set; } = new();
}

public class CustomerDashboard
{
    [JsonPropertyName("recent_orders")] public List<Order> RecentOrders { get; set; } = new();
    [JsonPropertyName("total_orders")] public int TotalOrders { get; set; }
}
