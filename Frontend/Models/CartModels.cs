using System.Text.Json.Serialization;

namespace Frontend.Models;

public class CartItem
{
    [JsonPropertyName("cart_item_id")] public int CartItemId { get; set; }
    [JsonPropertyName("product_id")] public int ProductId { get; set; }
    [JsonPropertyName("product_name")] public string ProductName { get; set; }
    [JsonPropertyName("price")] public decimal Price { get; set; }
    [JsonPropertyName("stock_quantity")] public int StockQuantity { get; set; }
    [JsonPropertyName("quantity")] public int Quantity { get; set; }
    [JsonPropertyName("subtotal")] public decimal Subtotal { get; set; }
}

public class Cart
{
    [JsonPropertyName("cart_id")] public int CartId { get; set; }
    [JsonPropertyName("items")] public List<CartItem> Items { get; set; } = new();
    [JsonPropertyName("total_amount")] public decimal TotalAmount { get; set; }
}

public class CartItemAddRequest
{
    [JsonPropertyName("product_id")] public int ProductId { get; set; }
    [JsonPropertyName("quantity")] public int Quantity { get; set; }
}

public class CartItemUpdateRequest
{
    [JsonPropertyName("quantity")] public int Quantity { get; set; }
}
