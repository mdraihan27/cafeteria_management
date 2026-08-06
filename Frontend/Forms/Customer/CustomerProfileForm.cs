using Frontend.Models;
using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Customer;

public class CustomerProfileForm : BaseForm
{
    private readonly TextBox _emailBox;
    private readonly TextBox _fullNameBox;
    private readonly TextBox _phoneBox;
    private readonly TextBox _addressBox;

    private readonly TextBox _currentPasswordBox;
    private readonly TextBox _newPasswordBox;
    private readonly TextBox _confirmPasswordBox;

    public CustomerProfileForm()
    {
        FormBorderStyle = FormBorderStyle.None;

        var heading = UiHelpers.MakeHeading("My Profile", 20, 15, 300);

        var emailLabel = UiHelpers.MakeLabel("Email", 20, 70, 130);
        _emailBox = UiHelpers.MakeTextBox(180, 68, 260);
        _emailBox.Enabled = false; // email is the login identifier and can't be changed here

        var fullNameLabel = UiHelpers.MakeLabel("Full Name", 20, 110, 130);
        _fullNameBox = UiHelpers.MakeTextBox(180, 108, 260);

        var phoneLabel = UiHelpers.MakeLabel("Phone", 20, 150, 130);
        _phoneBox = UiHelpers.MakeTextBox(180, 148, 260);

        var addressLabel = UiHelpers.MakeLabel("Address", 20, 190, 130);
        _addressBox = UiHelpers.MakeTextBox(180, 188, 260);

        var saveProfileButton = UiHelpers.MakeButton("Save Profile", 180, 235, 160);
        saveProfileButton.Click += async (_, _) => await SaveProfileAsync();

        var passwordHeading = UiHelpers.MakeHeading("Change Password", 20, 300, 300);

        var currentPasswordLabel = UiHelpers.MakeLabel("Current Password", 20, 350, 150);
        _currentPasswordBox = UiHelpers.MakeTextBox(180, 348, 240, isPassword: true);

        var newPasswordLabel = UiHelpers.MakeLabel("New Password", 20, 390, 150);
        _newPasswordBox = UiHelpers.MakeTextBox(180, 388, 240, isPassword: true);

        var confirmPasswordLabel = UiHelpers.MakeLabel("Confirm New Password", 20, 430, 150);
        _confirmPasswordBox = UiHelpers.MakeTextBox(180, 428, 240, isPassword: true);

        var changePasswordButton = UiHelpers.MakeButton("Change Password", 180, 475, 190);
        changePasswordButton.Click += async (_, _) => await ChangePasswordAsync();

        Controls.Add(heading);
        Controls.Add(emailLabel);
        Controls.Add(_emailBox);
        Controls.Add(fullNameLabel);
        Controls.Add(_fullNameBox);
        Controls.Add(phoneLabel);
        Controls.Add(_phoneBox);
        Controls.Add(addressLabel);
        Controls.Add(_addressBox);
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
            var profile = await AuthService.GetCustomerProfileAsync();
            _emailBox.Text = profile.Email;
            _fullNameBox.Text = profile.FullName;
            _phoneBox.Text = profile.Phone;
            _addressBox.Text = profile.Address;
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not load profile");
        }
    }

    private async Task SaveProfileAsync()
    {
        if (string.IsNullOrWhiteSpace(_fullNameBox.Text))
        {
            AppMessageBox.Show("Full name is required.", "Missing information");
            return;
        }

        try
        {
            await AuthService.UpdateCustomerProfileAsync(new CustomerProfileUpdateRequest
            {
                FullName = _fullNameBox.Text.Trim(),
                Phone = string.IsNullOrWhiteSpace(_phoneBox.Text) ? null : _phoneBox.Text.Trim(),
                Address = string.IsNullOrWhiteSpace(_addressBox.Text) ? null : _addressBox.Text.Trim(),
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
            await AuthService.ChangeCustomerPasswordAsync(_currentPasswordBox.Text, _newPasswordBox.Text);
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
