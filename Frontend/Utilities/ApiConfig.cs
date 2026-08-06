namespace Frontend.Utilities;

/// <summary>
/// The one place that knows where the backend API lives. If you run
/// the backend on a different machine or a different port, this is
/// the only line you need to change.
/// </summary>
public static class ApiConfig
{
    public const string BaseUrl = "http://127.0.0.1:8000";
}
