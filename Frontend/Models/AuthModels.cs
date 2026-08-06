using System.Text.Json.Serialization;

namespace Frontend.Models;

public class AdminLoginRequest
{
    [JsonPropertyName("username")] public string Username { get; set; }
    [JsonPropertyName("password")] public string Password { get; set; }
}

public class CustomerLoginRequest
{
    [JsonPropertyName("email")] public string Email { get; set; }
    [JsonPropertyName("password")] public string Password { get; set; }
}

public class CustomerRegisterRequest
{
    [JsonPropertyName("full_name")] public string FullName { get; set; }
    [JsonPropertyName("email")] public string Email { get; set; }
    [JsonPropertyName("password")] public string Password { get; set; }
    [JsonPropertyName("phone")] public string Phone { get; set; }
    [JsonPropertyName("address")] public string Address { get; set; }
}

public class ChangePasswordRequest
{
    [JsonPropertyName("current_password")] public string CurrentPassword { get; set; }
    [JsonPropertyName("new_password")] public string NewPassword { get; set; }
}

public class ForgotPasswordRequest
{
    [JsonPropertyName("email")] public string Email { get; set; }
    [JsonPropertyName("phone")] public string Phone { get; set; }
    [JsonPropertyName("new_password")] public string NewPassword { get; set; }
}

public class TokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; }
    [JsonPropertyName("token_type")] public string TokenType { get; set; }
    [JsonPropertyName("role")] public string Role { get; set; }
    [JsonPropertyName("user_id")] public int UserId { get; set; }
    [JsonPropertyName("full_name")] public string FullName { get; set; }
}
