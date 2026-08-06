using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Admin;

/// <summary>
/// The main window an Admin sees after logging in. It has a
/// navigation panel on the left and a content area on the right that
/// swaps between the Dashboard, Products, Categories, Customers,
/// Orders, Reports, and Profile screens.
/// </summary>
public class AdminMainForm : BaseForm
{
    private readonly Panel _contentPanel;

    public AdminMainForm()
    {
        Text = "Cafeteria Management System - Admin";
        Width = 1150;
        Height = 700;
        WindowState = FormWindowState.Maximized;
        MaximizeBox = true;
        FormBorderStyle = FormBorderStyle.Sizable;

        // --- Top bar ---------------------------------------------------
        var topBar = new Panel { Dock = DockStyle.Top, Height = 60, BorderStyle = BorderStyle.FixedSingle };
        var titleLabel = UiHelpers.MakeHeading("Cafeteria Management System - Admin Panel", 15, 15, 550);
        var welcomeLabel = UiHelpers.MakeLabel($"Welcome, {SessionManager.FullName}", 650, 18, 250);
        welcomeLabel.TextAlign = ContentAlignment.MiddleRight;
        var logoutButton = UiHelpers.MakeButton("Logout", 950, 12, 110, 36);
        logoutButton.Click += async (_, _) => await LogoutAsync();
        topBar.Controls.Add(titleLabel);
        topBar.Controls.Add(welcomeLabel);
        topBar.Controls.Add(logoutButton);

        // --- Left navigation panel --------------------------------------
        var navPanel = new Panel { Dock = DockStyle.Left, Width = 200, BorderStyle = BorderStyle.FixedSingle };

        var dashboardButton = UiHelpers.MakeButton("Dashboard", 15, 20, 170, 42);
        var productsButton = UiHelpers.MakeButton("Products", 15, 70, 170, 42);
        var categoriesButton = UiHelpers.MakeButton("Categories", 15, 120, 170, 42);
        var customersButton = UiHelpers.MakeButton("Customers", 15, 170, 170, 42);
        var ordersButton = UiHelpers.MakeButton("Orders", 15, 220, 170, 42);
        var reportsButton = UiHelpers.MakeButton("Reports", 15, 270, 170, 42);
        var profileButton = UiHelpers.MakeButton("My Profile", 15, 320, 170, 42);

        navPanel.Controls.Add(dashboardButton);
        navPanel.Controls.Add(productsButton);
        navPanel.Controls.Add(categoriesButton);
        navPanel.Controls.Add(customersButton);
        navPanel.Controls.Add(ordersButton);
        navPanel.Controls.Add(reportsButton);
        navPanel.Controls.Add(profileButton);

        // --- Content panel ------------------------------------------------
        _contentPanel = new Panel { Dock = DockStyle.Fill };

        dashboardButton.Click += (_, _) => ShowDashboard();
        productsButton.Click += (_, _) => FormEmbedder.ShowInPanel(_contentPanel, new AdminProductsForm());
        categoriesButton.Click += (_, _) => FormEmbedder.ShowInPanel(_contentPanel, new AdminCategoriesForm());
        customersButton.Click += (_, _) => FormEmbedder.ShowInPanel(_contentPanel, new AdminCustomersForm());
        ordersButton.Click += (_, _) => FormEmbedder.ShowInPanel(_contentPanel, new AdminOrdersForm());
        reportsButton.Click += (_, _) => FormEmbedder.ShowInPanel(_contentPanel, new AdminReportsForm());
        profileButton.Click += (_, _) => FormEmbedder.ShowInPanel(_contentPanel, new AdminProfileForm());

        Controls.Add(_contentPanel);
        Controls.Add(navPanel);
        Controls.Add(topBar);

        Load += (_, _) => ShowDashboard();
    }

    private void ShowDashboard() => FormEmbedder.ShowInPanel(_contentPanel, new AdminDashboardForm());

    private async Task LogoutAsync()
    {
        if (!AppMessageBox.Confirm("Are you sure you want to log out?", "Logout"))
        {
            return;
        }

        try
        {
            await AuthService.AdminLogoutAsync();
        }
        catch
        {
            // Even if the logout call fails (e.g. server unreachable), we still
            // clear the local session so the user isn't stuck.
        }

        SessionManager.EndSession();
        Close();
    }
}
