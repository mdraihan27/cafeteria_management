using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Customer;

/// <summary>
/// No email server is set up for this project, so instead of emailing
/// a reset link, the customer proves who they are with the email and
/// phone number they registered with.
/// </summary>
public class ForgotPasswordForm : BaseForm
{
    private readonly TextBox _emailBox;
    private readonly TextBox _phoneBox;
    private readonly TextBox _newPasswordBox;
    private readonly TextBox _confirmPasswordBox;

    public ForgotPasswordForm()
    {
        Text = "Forgot Password";
        ClientSize = new Size(440, 380);

        var heading = UiHelpers.MakeHeading("Reset Your Password", 40, 20, 340);
        var explanation = UiHelpers.MakeLabel(
            "Enter the email and phone number on your account, then choose a new password.",
            40, 65, 340, 45);

        var emailLabel = UiHelpers.MakeLabel("Email", 40, 120, 130);
        _emailBox = UiHelpers.MakeTextBox(180, 118, 200);

        var phoneLabel = UiHelpers.MakeLabel("Phone Number", 40, 160, 130);
        _phoneBox = UiHelpers.MakeTextBox(180, 158, 200);

        var newPasswordLabel = UiHelpers.MakeLabel("New Password", 40, 200, 130);
        _newPasswordBox = UiHelpers.MakeTextBox(180, 198, 200, isPassword: true);

        var confirmLabel = UiHelpers.MakeLabel("Confirm Password", 40, 240, 130);
        _confirmPasswordBox = UiHelpers.MakeTextBox(180, 238, 200, isPassword: true);

        var submitButton = UiHelpers.MakeButton("Reset Password", 90, 300, 160);
        submitButton.Click += async (_, _) => await SubmitAsync();

        var backButton = UiHelpers.MakeButton("Back to Login", 260, 300, 140);
        backButton.Click += (_, _) => Close();

        Controls.Add(heading);
        Controls.Add(explanation);
        Controls.Add(emailLabel);
        Controls.Add(_emailBox);
        Controls.Add(phoneLabel);
        Controls.Add(_phoneBox);
        Controls.Add(newPasswordLabel);
        Controls.Add(_newPasswordBox);
        Controls.Add(confirmLabel);
        Controls.Add(_confirmPasswordBox);
        Controls.Add(submitButton);
        Controls.Add(backButton);
    }

    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(_emailBox.Text) || string.IsNullOrWhiteSpace(_phoneBox.Text) || string.IsNullOrWhiteSpace(_newPasswordBox.Text))
        {
            AppMessageBox.Show("Please fill in every field.", "Missing information");
            return;
        }

        if (_newPasswordBox.Text.Length < 6)
        {
            AppMessageBox.Show("Password must be at least 6 characters.", "Invalid password");
            return;
        }

        if (_newPasswordBox.Text != _confirmPasswordBox.Text)
        {
            AppMessageBox.Show("Passwords do not match.", "Invalid password");
            return;
        }

        try
        {
            await AuthService.ForgotPasswordAsync(_emailBox.Text.Trim(), _phoneBox.Text.Trim(), _newPasswordBox.Text);
            AppMessageBox.Show("Your password has been reset. You can now log in.", "Success");
            Close();
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not reset password");
        }
        catch (Exception)
        {
            AppMessageBox.Show("Could not reach the server. Make sure the backend is running.", "Connection error");
        }
    }
}
