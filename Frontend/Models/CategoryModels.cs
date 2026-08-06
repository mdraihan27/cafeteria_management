using System.Text.Json.Serialization;

namespace Frontend.Models;

public class Category
{
    [JsonPropertyName("category_id")] public int CategoryId { get; set; }
    [JsonPropertyName("category_name")] public string CategoryName { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("product_count")] public int ProductCount { get; set; }

    /// <summary>Makes categories look nice when dropped straight into a ComboBox.</summary>
    public override string ToString() => CategoryName;
}

public class CategoryCreateRequest
{
    [JsonPropertyName("category_name")] public string CategoryName { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
}

public class CategoryUpdateRequest
{
    [JsonPropertyName("category_name")] public string CategoryName { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
}
