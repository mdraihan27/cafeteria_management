using Frontend.Models;
using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Admin;

public class AdminProfileForm : BaseForm
{
    private readonly TextBox _fullNameBox;
    private readonly TextBox _emailBox;
    private readonly TextBox _usernameBox;

    private readonly TextBox _currentPasswordBox;
    private readonly TextBox _newPasswordBox;
    private readonly TextBox _confirmPasswordBox;

    public AdminProfileForm()
    {
        FormBorderStyle = FormBorderStyle.None;

        var heading = UiHelpers.MakeHeading("My Profile", 20, 15, 300);

        var usernameLabel = UiHelpers.MakeLabel("Username", 20, 70, 130);
        _usernameBox = UiHelpers.MakeTextBox(160, 68, 260);
        _usernameBox.Enabled = false; // usernames don't change

        var fullNameLabel = UiHelpers.MakeLabel("Full Name", 20, 110, 130);
        _fullNameBox = UiHelpers.MakeTextBox(160, 108, 260);

        var emailLabel = UiHelpers.MakeLabel("Email", 20, 150, 130);
        _emailBox = UiHelpers.MakeTextBox(160, 148, 260);

        var saveProfileButton = UiHelpers.MakeButton("Save Profile", 160, 195, 160);
        saveProfileButton.Click += async (_, _) => await SaveProfileAsync();

        var passwordHeading = UiHelpers.MakeHeading("Change Password", 20, 260, 300);

        var currentPasswordLabel = UiHelpers.MakeLabel("Current Password", 20, 310, 150);
        _currentPasswordBox = UiHelpers.MakeTextBox(180, 308, 240, isPassword: true);

        var newPasswordLabel = UiHelpers.MakeLabel("New Password", 20, 350, 150);
        _newPasswordBox = UiHelpers.MakeTextBox(180, 348, 240, isPassword: true);

        var confirmPasswordLabel = UiHelpers.MakeLabel("Confirm New Password", 20, 390, 150);
        _confirmPasswordBox = UiHelpers.MakeTextBox(180, 388, 240, isPassword: true);

        var changePasswordButton = UiHelpers.MakeButton("Change Password", 180, 435, 190);
        changePasswordButton.Click += async (_, _) => await ChangePasswordAsync();

        Controls.Add(heading);
        Controls.Add(usernameLabel);
        Controls.Add(_usernameBox);
        Controls.Add(fullNameLabel);
        Controls.Add(_fullNameBox);
        Controls.Add(emailLabel);
        Controls.Add(_emailBox);
        Controls.Add(saveProfileButton);
        Controls.Add(passwordHeading);
        Controls.Add(currentPasswordLabel);
        Controls.Add(_currentPasswordBox);
        Controls.Add(newPasswordLabel);
        Controls.Add(_newPasswordBox);
        Controls.Add(confirmPasswordLabel);
        Controls.Add(_confirmPasswordBox);
        Controls.Add(changePasswordButton);

        Load += async (_, _) => await LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        try
        {
            var profile = await AuthService.GetAdminProfileAsync();
            _usernameBox.Text = profile.Username;
            _fullNameBox.Text = profile.FullName;
            _emailBox.Text = profile.Email;
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not load profile");
        }
    }

    private async Task SaveProfileAsync()
    {
        if (string.IsNullOrWhiteSpace(_fullNameBox.Text) || string.IsNullOrWhiteSpace(_emailBox.Text))
        {
            AppMessageBox.Show("Full name and email are required.", "Missing information");
            return;
        }

        try
        {
            await AuthService.UpdateAdminProfileAsync(new AdminProfileUpdateRequest
            {
                FullName = _fullNameBox.Text.Trim(),
                Email = _emailBox.Text.Trim(),
            });
            AppMessageBox.Show("Profile updated successfully.", "Success");
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not update profile");
        }
    }

    private async Task ChangePasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentPasswordBox.Text) || string.IsNullOrWhiteSpace(_newPasswordBox.Text))
        {
            AppMessageBox.Show("Please fill in every password field.", "Missing information");
            return;
        }

        if (_newPasswordBox.Text != _confirmPasswordBox.Text)
        {
            AppMessageBox.Show("New passwords do not match.", "Invalid password");
            return;
        }

        try
        {
            await AuthService.ChangeAdminPasswordAsync(_currentPasswordBox.Text, _newPasswordBox.Text);
            AppMessageBox.Show("Password changed successfully.", "Success");
            _currentPasswordBox.Clear();
            _newPasswordBox.Clear();
            _confirmPasswordBox.Clear();
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not change password");
        }
    }
}
