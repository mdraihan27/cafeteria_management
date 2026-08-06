using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Customer;

public class CustomerLoginForm : BaseForm
{
    private readonly TextBox _emailBox;
    private readonly TextBox _passwordBox;

    public CustomerLoginForm()
    {
        Text = "Customer Login";
        ClientSize = new Size(420, 310);

        var heading = UiHelpers.MakeHeading("Customer Login", 40, 25, 320);

        var emailLabel = UiHelpers.MakeLabel("Email", 40, 85, 100);
        _emailBox = UiHelpers.MakeTextBox(150, 83, 210);

        var passwordLabel = UiHelpers.MakeLabel("Password", 40, 125, 100);
        _passwordBox = UiHelpers.MakeTextBox(150, 123, 210, isPassword: true);

        var loginButton = UiHelpers.MakeButton("Login", 150, 175, 120);
        loginButton.Click += async (_, _) => await LoginAsync();

        var backButton = UiHelpers.MakeButton("Back", 280, 175, 80);
        backButton.Click += (_, _) => Close();

        var registerLink = UiHelpers.MakeLinkLabel("New here? Create an account", 90, 235, 250);
        registerLink.LinkClicked += (_, _) =>
        {
            Hide();
            using var form = new RegisterForm();
            form.ShowDialog();
            Close();
        };

        var forgotLink = UiHelpers.MakeLinkLabel("Forgot your password?", 90, 265, 250);
        forgotLink.LinkClicked += (_, _) =>
        {
            Hide();
            using var form = new ForgotPasswordForm();
            form.ShowDialog();
            Close();
        };

        AcceptButton = loginButton;

        Controls.Add(heading);
        Controls.Add(emailLabel);
        Controls.Add(_emailBox);
        Controls.Add(passwordLabel);
        Controls.Add(_passwordBox);
        Controls.Add(loginButton);
        Controls.Add(backButton);
        Controls.Add(registerLink);
        Controls.Add(forgotLink);
    }

    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(_emailBox.Text) || string.IsNullOrWhiteSpace(_passwordBox.Text))
        {
            AppMessageBox.Show("Please enter both email and password.", "Missing information");
            return;
        }

        try
        {
            var token = await AuthService.CustomerLoginAsync(_emailBox.Text.Trim(), _passwordBox.Text);
            SessionManager.StartSession(token.AccessToken, token.Role, token.UserId, token.FullName);
            DialogResult = DialogResult.OK;

            Hide();
            using var mainForm = new CustomerMainForm();
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
