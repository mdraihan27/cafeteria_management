namespace Frontend.Services;

/// <summary>
/// Thrown whenever the backend responds with an error. The message
/// on this exception is already a friendly, human-readable sentence
/// pulled out of the API's response - forms can show it directly in
/// an AppMessageBox without any extra work.
/// </summary>
public class ApiException : Exception
{
    public ApiException(string message) : base(message)
    {
    }
}
