using Frontend.Models;
using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Customer;

public class RegisterForm : BaseForm
{
    private readonly TextBox _fullNameBox;
    private readonly TextBox _emailBox;
    private readonly TextBox _phoneBox;
    private readonly TextBox _addressBox;
    private readonly TextBox _passwordBox;
    private readonly TextBox _confirmPasswordBox;

    public RegisterForm()
    {
        Text = "Create an Account";
        ClientSize = new Size(460, 425);

        var heading = UiHelpers.MakeHeading("Create an Account", 40, 20, 340);

        var fullNameLabel = UiHelpers.MakeLabel("Full Name", 40, 75, 120);
        _fullNameBox = UiHelpers.MakeTextBox(170, 73, 230);

        var emailLabel = UiHelpers.MakeLabel("Email", 40, 115, 120);
        _emailBox = UiHelpers.MakeTextBox(170, 113, 230);

        var phoneLabel = UiHelpers.MakeLabel("Phone", 40, 155, 120);
        _phoneBox = UiHelpers.MakeTextBox(170, 153, 230);

        var addressLabel = UiHelpers.MakeLabel("Address", 40, 195, 120);
        _addressBox = UiHelpers.MakeTextBox(170, 193, 230);

        var passwordLabel = UiHelpers.MakeLabel("Password", 40, 235, 120);
        _passwordBox = UiHelpers.MakeTextBox(170, 233, 230, isPassword: true);

        var confirmLabel = UiHelpers.MakeLabel("Confirm Password", 40, 275, 120);
        _confirmPasswordBox = UiHelpers.MakeTextBox(170, 273, 230, isPassword: true);

        var hintLabel = UiHelpers.MakeLabel("Password must be at least 6 characters.", 40, 315, 360);

        var registerButton = UiHelpers.MakeButton("Create Account", 90, 365, 160);
        registerButton.Click += async (_, _) => await RegisterAsync();

        var backButton = UiHelpers.MakeButton("Back to Login", 260, 365, 140);
        backButton.Click += (_, _) => Close();

        Controls.Add(heading);
        Controls.Add(fullNameLabel);
        Controls.Add(_fullNameBox);
        Controls.Add(emailLabel);
        Controls.Add(_emailBox);
        Controls.Add(phoneLabel);
        Controls.Add(_phoneBox);
        Controls.Add(addressLabel);
        Controls.Add(_addressBox);
        Controls.Add(passwordLabel);
        Controls.Add(_passwordBox);
        Controls.Add(confirmLabel);
        Controls.Add(_confirmPasswordBox);
        Controls.Add(hintLabel);
        Controls.Add(registerButton);
        Controls.Add(backButton);
    }

    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(_fullNameBox.Text) || string.IsNullOrWhiteSpace(_emailBox.Text) || string.IsNullOrWhiteSpace(_passwordBox.Text))
        {
            AppMessageBox.Show("Full name, email, and password are required.", "Missing information");
            return;
        }

        if (_passwordBox.Text.Length < 6)
        {
            AppMessageBox.Show("Password must be at least 6 characters.", "Invalid password");
            return;
        }

        if (_passwordBox.Text != _confirmPasswordBox.Text)
        {
            AppMessageBox.Show("Passwords do not match.", "Invalid password");
            return;
        }

        var request = new CustomerRegisterRequest
        {
            FullName = _fullNameBox.Text.Trim(),
            Email = _emailBox.Text.Trim(),
            Password = _passwordBox.Text,
            Phone = string.IsNullOrWhiteSpace(_phoneBox.Text) ? null : _phoneBox.Text.Trim(),
            Address = string.IsNullOrWhiteSpace(_addressBox.Text) ? null : _addressBox.Text.Trim(),
        };

        try
        {
            var token = await AuthService.CustomerRegisterAsync(request);
            SessionManager.StartSession(token.AccessToken, token.Role, token.UserId, token.FullName);
            AppMessageBox.Show("Account created successfully. Welcome!", "Success");

            Hide();
            using var mainForm = new CustomerMainForm();
            mainForm.ShowDialog();
            Close();
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not create account");
        }
        catch (Exception)
        {
            AppMessageBox.Show("Could not reach the server. Make sure the backend is running.", "Connection error");
        }
    }
}
