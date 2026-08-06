using System.Text.Json.Serialization;

namespace Frontend.Models;

/// <summary>Many endpoints (logout, delete, change-password, ...) just reply with a short message.</summary>
public class MessageResponse
{
    [JsonPropertyName("message")] public string Message { get; set; }
}
