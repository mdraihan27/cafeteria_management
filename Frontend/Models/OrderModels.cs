using System.Text.Json.Serialization;

namespace Frontend.Models;

public class OrderItem
{
    [JsonPropertyName("order_item_id")] public int OrderItemId { get; set; }
    [JsonPropertyName("product_id")] public int? ProductId { get; set; }
    [JsonPropertyName("product_name")] public string ProductName { get; set; }
    [JsonPropertyName("unit_price")] public decimal UnitPrice { get; set; }
    [JsonPropertyName("quantity")] public int Quantity { get; set; }
    [JsonPropertyName("subtotal")] public decimal Subtotal { get; set; }
}

public class Payment
{
    [JsonPropertyName("payment_id")] public int PaymentId { get; set; }
    [JsonPropertyName("amount")] public decimal Amount { get; set; }
    [JsonPropertyName("payment_method")] public string PaymentMethod { get; set; }
    [JsonPropertyName("payment_status")] public string PaymentStatus { get; set; }
    [JsonPropertyName("payment_date")] public DateTime PaymentDate { get; set; }
}

public class Order
{
    [JsonPropertyName("order_id")] public int OrderId { get; set; }
    [JsonPropertyName("customer_id")] public int CustomerId { get; set; }
    [JsonPropertyName("customer_name")] public string CustomerName { get; set; }
    [JsonPropertyName("order_date")] public DateTime OrderDate { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; }
    [JsonPropertyName("total_amount")] public decimal TotalAmount { get; set; }
    [JsonPropertyName("items")] public List<OrderItem> Items { get; set; } = new();
    [JsonPropertyName("payment")] public Payment Payment { get; set; }
}

public class CheckoutRequest
{
    [JsonPropertyName("note")] public string Note { get; set; }
}

public class OrderStatusUpdateRequest
{
    [JsonPropertyName("status")] public string Status { get; set; }
    [JsonPropertyName("payment_method")] public string PaymentMethod { get; set; }
}
