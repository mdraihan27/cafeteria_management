using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Frontend.Utilities;

namespace Frontend.Services;

/// <summary>
/// The single place in the whole app that actually talks HTTP to the
/// backend. Every other Service class (AuthService, ProductService,
/// etc.) calls one of the methods below instead of using HttpClient
/// directly - that keeps things like error handling and the login
/// token consistent everywhere.
/// </summary>
public static class ApiClient
{
    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri(ApiConfig.BaseUrl),
        Timeout = TimeSpan.FromSeconds(15),
    };

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new DecimalJsonConverter() },
    };

    private static void AttachAuthHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = SessionManager.IsLoggedIn
            ? new AuthenticationHeaderValue("Bearer", SessionManager.Token)
            : null;
    }

    public static async Task<T> GetAsync<T>(string path)
    {
        AttachAuthHeader();
        using var response = await _httpClient.GetAsync(path);
        return await ReadResponseAsync<T>(response);
    }

    public static async Task<T> PostAsync<T>(string path, object body = null)
    {
        AttachAuthHeader();
        using var response = await _httpClient.PostAsJsonAsync(path, body ?? new { }, _jsonOptions);
        return await ReadResponseAsync<T>(response);
    }

    public static async Task<T> PutAsync<T>(string path, object body)
    {
        AttachAuthHeader();
        using var response = await _httpClient.PutAsJsonAsync(path, body, _jsonOptions);
        return await ReadResponseAsync<T>(response);
    }

    public static async Task<T> DeleteAsync<T>(string path)
    {
        AttachAuthHeader();
        using var response = await _httpClient.DeleteAsync(path);
        return await ReadResponseAsync<T>(response);
    }

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        var rawBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException(ExtractErrorMessage(rawBody, response.StatusCode));
        }

        if (string.IsNullOrWhiteSpace(rawBody))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(rawBody, _jsonOptions);
        }
        catch (JsonException)
        {
            throw new ApiException("The server sent back a response the app didn't understand.");
        }
    }

    /// <summary>
    /// FastAPI error responses look like either:
    ///   {"detail": "Some message"}                     (a normal error)
    ///   {"detail": [{"msg": "...", "loc": [...]}, ...]} (a validation error)
    /// This pulls a single readable sentence out of either shape.
    /// </summary>
    private static string ExtractErrorMessage(string rawBody, System.Net.HttpStatusCode statusCode)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
        {
            return $"The server returned an error ({(int)statusCode}).";
        }

        try
        {
            using var document = JsonDocument.Parse(rawBody);
            if (!document.RootElement.TryGetProperty("detail", out var detail))
            {
                return $"The server returned an error ({(int)statusCode}).";
            }

            if (detail.ValueKind == JsonValueKind.String)
            {
                return detail.GetString();
            }

            if (detail.ValueKind == JsonValueKind.Array)
            {
                var messages = new List<string>();
                foreach (var item in detail.EnumerateArray())
                {
                    if (item.TryGetProperty("msg", out var msg))
                    {
                        messages.Add(msg.GetString());
                    }
                }
                return messages.Count > 0
                    ? string.Join("\n", messages)
                    : $"The server returned an error ({(int)statusCode}).";
            }

            return $"The server returned an error ({(int)statusCode}).";
        }
        catch (JsonException)
        {
            return $"The server returned an error ({(int)statusCode}).";
        }
    }
}
