using System.Text.Json.Serialization;

namespace Frontend.Models;

public class CustomerProfile
{
    [JsonPropertyName("customer_id")] public int CustomerId { get; set; }
    [JsonPropertyName("full_name")] public string FullName { get; set; }
    [JsonPropertyName("email")] public string Email { get; set; }
    [JsonPropertyName("phone")] public string Phone { get; set; }
    [JsonPropertyName("address")] public string Address { get; set; }
    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
}

public class CustomerProfileUpdateRequest
{
    [JsonPropertyName("full_name")] public string FullName { get; set; }
    [JsonPropertyName("phone")] public string Phone { get; set; }
    [JsonPropertyName("address")] public string Address { get; set; }
}

/// <summary>What an Admin sees in the customer list - same shape as CustomerProfile.</summary>
public class CustomerListItem
{
    [JsonPropertyName("customer_id")] public int CustomerId { get; set; }
    [JsonPropertyName("full_name")] public string FullName { get; set; }
    [JsonPropertyName("email")] public string Email { get; set; }
    [JsonPropertyName("phone")] public string Phone { get; set; }
    [JsonPropertyName("address")] public string Address { get; set; }
    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
}
