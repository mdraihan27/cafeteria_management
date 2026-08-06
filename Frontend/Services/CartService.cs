using Frontend.Models;

namespace Frontend.Services;

public static class CartService
{
    public static Task<Cart> GetCartAsync() =>
        ApiClient.GetAsync<Cart>("/cart");

    public static Task<Cart> AddItemAsync(int productId, int quantity) =>
        ApiClient.PostAsync<Cart>("/cart/items", new CartItemAddRequest { ProductId = productId, Quantity = quantity });

    public static Task<Cart> UpdateItemAsync(int cartItemId, int quantity) =>
        ApiClient.PutAsync<Cart>($"/cart/items/{cartItemId}", new CartItemUpdateRequest { Quantity = quantity });

    public static Task<Cart> RemoveItemAsync(int cartItemId) =>
        ApiClient.DeleteAsync<Cart>($"/cart/items/{cartItemId}");

    public static Task<Cart> ClearCartAsync() =>
        ApiClient.DeleteAsync<Cart>("/cart");
}
