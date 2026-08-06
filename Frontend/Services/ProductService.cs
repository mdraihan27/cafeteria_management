using Frontend.Models;

namespace Frontend.Services;

public static class ProductService
{
    /// <summary>Active products only - used by the customer menu screen.</summary>
    public static Task<List<Product>> GetProductsAsync(int? categoryId = null, string search = null)
    {
        var query = new List<string>();
        if (categoryId.HasValue) query.Add($"category_id={categoryId.Value}");
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
        var queryString = query.Count > 0 ? "?" + string.Join("&", query) : "";
        return ApiClient.GetAsync<List<Product>>($"/products{queryString}");
    }

    /// <summary>Active AND inactive products - used by the admin product management screen.</summary>
    public static Task<List<Product>> GetAllProductsForAdminAsync() =>
        ApiClient.GetAsync<List<Product>>("/products/admin/all");

    public static Task<Product> GetProductAsync(int productId) =>
        ApiClient.GetAsync<Product>($"/products/{productId}");

    public static Task<Product> CreateProductAsync(ProductCreateRequest request) =>
        ApiClient.PostAsync<Product>("/products", request);

    public static Task<Product> UpdateProductAsync(int productId, ProductUpdateRequest request) =>
        ApiClient.PutAsync<Product>($"/products/{productId}", request);

    public static Task<MessageResponse> DeleteProductAsync(int productId) =>
        ApiClient.DeleteAsync<MessageResponse>($"/products/{productId}");

    public static Task<MessageResponse> ReactivateProductAsync(int productId) =>
        ApiClient.PutAsync<MessageResponse>($"/products/{productId}/reactivate", null);
}
