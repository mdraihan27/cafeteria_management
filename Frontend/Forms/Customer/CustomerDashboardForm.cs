using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Customer;

/// <summary>The customer's "at a glance" screen: recent orders and how many they've placed in total.</summary>
public class CustomerDashboardForm : BaseForm
{
    private readonly Label _totalOrdersValue;
    private readonly DataGridView _recentOrdersGrid;

    public CustomerDashboardForm()
    {
        FormBorderStyle = FormBorderStyle.None;

        var heading = UiHelpers.MakeHeading("Dashboard", 20, 15, 300);

        var statBox = new Panel { Location = new Point(20, 60), Size = new Size(220, 110), BorderStyle = BorderStyle.FixedSingle };
        var statTitle = UiHelpers.MakeLabel("Total Orders Placed", 15, 12, 190, 24);
        _totalOrdersValue = new Label
        {
            Text = "-",
            Location = new Point(15, 45),
            Size = new Size(190, 50),
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
        };
        statBox.Controls.Add(statTitle);
        statBox.Controls.Add(_totalOrdersValue);

        var recentHeading = UiHelpers.MakeHeading("Recent Orders", 20, 200, 300);

        _recentOrdersGrid = UiHelpers.MakeGrid(20, 240, 900, 380);
        _recentOrdersGrid.Columns.Add("order_id", "Order #");
        _recentOrdersGrid.Columns.Add("order_date", "Date");
        _recentOrdersGrid.Columns.Add("status", "Status");
        _recentOrdersGrid.Columns.Add("total_amount", "Total");

        Controls.Add(heading);
        Controls.Add(statBox);
        Controls.Add(recentHeading);
        Controls.Add(_recentOrdersGrid);

        Load += async (_, _) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var dashboard = await DashboardService.GetCustomerDashboardAsync();
            _totalOrdersValue.Text = dashboard.TotalOrders.ToString();

            _recentOrdersGrid.Rows.Clear();
            foreach (var order in dashboard.RecentOrders)
            {
                _recentOrdersGrid.Rows.Add(order.OrderId, order.OrderDate.ToString("yyyy-MM-dd HH:mm"), order.Status, $"{order.TotalAmount:0.00} Taka");
            }
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not load dashboard");
        }
        catch (Exception)
        {
            AppMessageBox.Show("Could not reach the server.", "Connection error");
        }
    }
}
