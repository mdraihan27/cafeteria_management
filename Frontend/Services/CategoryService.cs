using Frontend.Models;

namespace Frontend.Services;

public static class CategoryService
{
    public static Task<List<Category>> GetCategoriesAsync() =>
        ApiClient.GetAsync<List<Category>>("/categories");

    public static Task<Category> CreateCategoryAsync(string name, string description) =>
        ApiClient.PostAsync<Category>("/categories", new CategoryCreateRequest { CategoryName = name, Description = description });

    public static Task<Category> UpdateCategoryAsync(int categoryId, string name, string description) =>
        ApiClient.PutAsync<Category>($"/categories/{categoryId}", new CategoryUpdateRequest { CategoryName = name, Description = description });

    public static Task<MessageResponse> DeleteCategoryAsync(int categoryId) =>
        ApiClient.DeleteAsync<MessageResponse>($"/categories/{categoryId}");
}
