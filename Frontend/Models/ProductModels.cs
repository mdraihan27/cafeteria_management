using System.Text.Json.Serialization;

namespace Frontend.Models;

public class Product
{
    [JsonPropertyName("product_id")] public int ProductId { get; set; }
    [JsonPropertyName("product_name")] public string ProductName { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
    [JsonPropertyName("price")] public decimal Price { get; set; }
    [JsonPropertyName("stock_quantity")] public int StockQuantity { get; set; }
    [JsonPropertyName("category_id")] public int CategoryId { get; set; }
    [JsonPropertyName("category_name")] public string CategoryName { get; set; }
    [JsonPropertyName("is_active")] public bool IsActive { get; set; }
    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updated_at")] public DateTime UpdatedAt { get; set; }
}

public class ProductCreateRequest
{
    [JsonPropertyName("product_name")] public string ProductName { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
    [JsonPropertyName("price")] public decimal Price { get; set; }
    [JsonPropertyName("stock_quantity")] public int StockQuantity { get; set; }
    [JsonPropertyName("category_id")] public int CategoryId { get; set; }
}

public class ProductUpdateRequest
{
    [JsonPropertyName("product_name")] public string ProductName { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
    [JsonPropertyName("price")] public decimal Price { get; set; }
    [JsonPropertyName("stock_quantity")] public int StockQuantity { get; set; }
    [JsonPropertyName("category_id")] public int CategoryId { get; set; }
    [JsonPropertyName("is_active")] public bool IsActive { get; set; }
}
