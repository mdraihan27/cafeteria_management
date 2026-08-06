namespace Frontend.Utilities;

/// <summary>
/// Holds information about whoever is currently logged in, for as
/// long as the app stays open. Nothing here is saved to disk - when
/// the app is closed, the session is gone and the user has to log in
/// again next time (this is normal for a desktop app like this one).
/// </summary>
public static class SessionManager
{
    public static string Token { get; private set; }
    public static string Role { get; private set; } // "admin" or "customer"
    public static int UserId { get; private set; }
    public static string FullName { get; private set; }

    public static bool IsLoggedIn => !string.IsNullOrEmpty(Token);
    public static bool IsAdmin => Role == "admin";
    public static bool IsCustomer => Role == "customer";

    public static void StartSession(string token, string role, int userId, string fullName)
    {
        Token = token;
        Role = role;
        UserId = userId;
        FullName = fullName;
    }

    /// <summary>Clears the session. Called on logout.</summary>
    public static void EndSession()
    {
        Token = null;
        Role = null;
        UserId = 0;
        FullName = null;
    }
}
