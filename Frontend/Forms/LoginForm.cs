using Frontend.Forms.Admin;
using Frontend.Forms.Customer;
using Frontend.Utilities;

namespace Frontend.Forms;

/// <summary>
/// The very first screen the app shows. It just asks "who are you"
/// and sends the user on to the matching login screen.
/// </summary>
public class LoginForm : BaseForm
{
    public LoginForm()
    {
        Text = "Cafeteria Management System";
        ClientSize = new Size(480, 290);

        var titleLabel = UiHelpers.MakeHeading("Cafeteria Management System", 40, 50, 400, 40);
        titleLabel.Font = Theme.TitleFont;
        titleLabel.TextAlign = ContentAlignment.MiddleCenter;

        var subtitleLabel = UiHelpers.MakeLabel("Please choose how you would like to continue:", 40, 100, 400, 24);
        subtitleLabel.TextAlign = ContentAlignment.MiddleCenter;

        var adminButton = UiHelpers.MakeButton("Admin Login", 90, 160, 280, 50);
        adminButton.Click += (_, _) =>
        {
            Hide();
            using var form = new AdminLoginForm();
            form.ShowDialog();
            Show();
        };

        var customerButton = UiHelpers.MakeButton("Customer Login", 90, 225, 280, 50);
        customerButton.Click += (_, _) =>
        {
            Hide();
            using var form = new CustomerLoginForm();
            form.ShowDialog();
            Show();
        };

        Controls.Add(titleLabel);
        Controls.Add(subtitleLabel);
        Controls.Add(adminButton);
        Controls.Add(customerButton);
    }
}
