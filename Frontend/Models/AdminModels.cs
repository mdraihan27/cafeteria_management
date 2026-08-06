using System.Text.Json.Serialization;

namespace Frontend.Models;

public class AdminProfile
{
    [JsonPropertyName("admin_id")] public int AdminId { get; set; }
    [JsonPropertyName("full_name")] public string FullName { get; set; }
    [JsonPropertyName("username")] public string Username { get; set; }
    [JsonPropertyName("email")] public string Email { get; set; }
    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
}

public class AdminProfileUpdateRequest
{
    [JsonPropertyName("full_name")] public string FullName { get; set; }
    [JsonPropertyName("email")] public string Email { get; set; }
}
