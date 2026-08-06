using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Customer;

/// <summary>
/// The main window a Customer sees after logging in. Same left-nav /
/// content-panel layout as AdminMainForm, just with the screens a
/// customer is allowed to use.
/// </summary>
public class CustomerMainForm : BaseForm
{
    private readonly Panel _contentPanel;

    public CustomerMainForm()
    {
        Text = "Cafeteria Management System - Customer";
        Width = 1150;
        Height = 700;
        WindowState = FormWindowState.Maximized;
        MaximizeBox = true;
        FormBorderStyle = FormBorderStyle.Sizable;

        var topBar = new Panel { Dock = DockStyle.Top, Height = 60, BorderStyle = BorderStyle.FixedSingle };
        var titleLabel = UiHelpers.MakeHeading("Cafeteria Management System", 15, 15, 450);
        var welcomeLabel = UiHelpers.MakeLabel($"Welcome, {SessionManager.FullName}", 650, 18, 250);
        welcomeLabel.TextAlign = ContentAlignment.MiddleRight;
        var logoutButton = UiHelpers.MakeButton("Logout", 950, 12, 110, 36);
        logoutButton.Click += async (_, _) => await LogoutAsync();
        topBar.Controls.Add(titleLabel);
        topBar.Controls.Add(welcomeLabel);
        topBar.Controls.Add(logoutButton);

        var navPanel = new Panel { Dock = DockStyle.Left, Width = 200, BorderStyle = BorderStyle.FixedSingle };
        var dashboardButton = UiHelpers.MakeButton("Dashboard", 15, 20, 170, 42);
        var menuButton = UiHelpers.MakeButton("Browse Menu", 15, 70, 170, 42);
        var cartButton = UiHelpers.MakeButton("My Cart", 15, 120, 170, 42);
        var ordersButton = UiHelpers.MakeButton("My Orders", 15, 170, 170, 42);
        var profileButton = UiHelpers.MakeButton("My Profile", 15, 220, 170, 42);

        navPanel.Controls.Add(dashboardButton);
        navPanel.Controls.Add(menuButton);
        navPanel.Controls.Add(cartButton);
        navPanel.Controls.Add(ordersButton);
        navPanel.Controls.Add(profileButton);

        _contentPanel = new Panel { Dock = DockStyle.Fill };

        dashboardButton.Click += (_, _) => ShowDashboard();
        menuButton.Click += (_, _) => FormEmbedder.ShowInPanel(_contentPanel, new MenuForm());
        cartButton.Click += (_, _) => FormEmbedder.ShowInPanel(_contentPanel, new CartForm());
        ordersButton.Click += (_, _) => FormEmbedder.ShowInPanel(_contentPanel, new MyOrdersForm());
        profileButton.Click += (_, _) => FormEmbedder.ShowInPanel(_contentPanel, new CustomerProfileForm());

        Controls.Add(_contentPanel);
        Controls.Add(navPanel);
        Controls.Add(topBar);

        Load += (_, _) => ShowDashboard();
    }

    private void ShowDashboard() => FormEmbedder.ShowInPanel(_contentPanel, new CustomerDashboardForm());

    private async Task LogoutAsync()
    {
        if (!AppMessageBox.Confirm("Are you sure you want to log out?", "Logout"))
        {
            return;
        }

        try
        {
            await AuthService.CustomerLogoutAsync();
        }
        catch
        {
            // Ignore network errors on logout - we still clear the local session below.
        }

        SessionManager.EndSession();
        Close();
    }
}
