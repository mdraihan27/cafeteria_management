using Frontend.Forms;

namespace Frontend;

/// <summary>
/// The entry point of the application. This is the very first code
/// that runs when the app starts.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        // The app always starts on the role-selection / login screen.
        Application.Run(new LoginForm());
    }
}
