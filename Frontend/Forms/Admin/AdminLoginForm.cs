using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Admin;

public class AdminLoginForm : BaseForm
{
    private readonly TextBox _usernameBox;
    private readonly TextBox _passwordBox;

    public AdminLoginForm()
    {
        Text = "Admin Login";
        ClientSize = new Size(420, 240);

        var heading = UiHelpers.MakeHeading("Admin Login", 40, 30, 320);

        var usernameLabel = UiHelpers.MakeLabel("Username", 40, 90, 100);
        _usernameBox = UiHelpers.MakeTextBox(150, 88, 210);

        var passwordLabel = UiHelpers.MakeLabel("Password", 40, 130, 100);
        _passwordBox = UiHelpers.MakeTextBox(150, 128, 210, isPassword: true);

        var loginButton = UiHelpers.MakeButton("Login", 150, 180, 120);
        loginButton.Click += async (_, _) => await LoginAsync();

        var backButton = UiHelpers.MakeButton("Back", 280, 180, 80);
        backButton.Click += (_, _) => Close();

        AcceptButton = loginButton;

        Controls.Add(heading);
        Controls.Add(usernameLabel);
        Controls.Add(_usernameBox);
        Controls.Add(passwordLabel);
        Controls.Add(_passwordBox);
        Controls.Add(loginButton);
        Controls.Add(backButton);
    }

    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(_usernameBox.Text) || string.IsNullOrWhiteSpace(_passwordBox.Text))
        {
            AppMessageBox.Show("Please enter both username and password.", "Missing information");
            return;
        }

        try
        {
            var token = await AuthService.AdminLoginAsync(_usernameBox.Text.Trim(), _passwordBox.Text);
            SessionManager.StartSession(token.AccessToken, token.Role, token.UserId, token.FullName);
            DialogResult = DialogResult.OK;

            Hide();
            using var mainForm = new AdminMainForm();
            mainForm.ShowDialog();
            Close();
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Login failed");
        }
        catch (Exception)
        {
            AppMessageBox.Show("Could not reach the server. Make sure the backend is running.", "Connection error");
        }
    }
}
