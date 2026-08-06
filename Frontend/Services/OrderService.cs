using Frontend.Models;

namespace Frontend.Services;

public static class OrderService
{
    // --- Customer ------------------------------------------------------
    public static Task<Order> CheckoutAsync() =>
        ApiClient.PostAsync<Order>("/orders/checkout", new CheckoutRequest());

    public static Task<List<Order>> GetMyOrdersAsync() =>
        ApiClient.GetAsync<List<Order>>("/orders/my");

    public static Task<Order> GetMyOrderAsync(int orderId) =>
        ApiClient.GetAsync<Order>($"/orders/my/{orderId}");

    public static Task<Order> CancelMyOrderAsync(int orderId) =>
        ApiClient.PostAsync<Order>($"/orders/my/{orderId}/cancel");

    // --- Admin ---------------------------------------------------------
    public static Task<List<Order>> GetAllOrdersAsync(string statusFilter = null)
    {
        var queryString = string.IsNullOrWhiteSpace(statusFilter) ? "" : $"?status_filter={statusFilter}";
        return ApiClient.GetAsync<List<Order>>($"/orders{queryString}");
    }

    public static Task<Order> GetOrderAsync(int orderId) =>
        ApiClient.GetAsync<Order>($"/orders/{orderId}");

    public static Task<Order> CompleteOrderAsync(int orderId, string paymentMethod) =>
        ApiClient.PutAsync<Order>($"/orders/{orderId}/status", new OrderStatusUpdateRequest { Status = "completed", PaymentMethod = paymentMethod });

    public static Task<Order> CancelOrderAsync(int orderId) =>
        ApiClient.PutAsync<Order>($"/orders/{orderId}/status", new OrderStatusUpdateRequest { Status = "cancelled" });
}
