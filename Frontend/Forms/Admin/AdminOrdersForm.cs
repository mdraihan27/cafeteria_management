using Frontend.Models;
using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Admin;

/// <summary>Admins can view and manage every order in the system.</summary>
public class AdminOrdersForm : BaseForm
{
    private readonly DataGridView _grid;
    private readonly ComboBox _statusFilterCombo;
    private List<Order> _orders = new();

    public AdminOrdersForm()
    {
        FormBorderStyle = FormBorderStyle.None;

        var heading = UiHelpers.MakeHeading("Orders", 20, 15, 200);

        var filterLabel = UiHelpers.MakeLabel("Filter by status:", 780, 20, 150);
        _statusFilterCombo = UiHelpers.MakeComboBox(780, 45, 150);
        _statusFilterCombo.Items.AddRange(new object[] { "All", "pending", "completed", "cancelled" });
        _statusFilterCombo.SelectedIndex = 0;
        _statusFilterCombo.SelectedIndexChanged += async (_, _) => await LoadDataAsync();

        var viewButton = UiHelpers.MakeButton("View / Manage", 780, 90, 150);
        viewButton.Click += async (_, _) => await ViewSelectedAsync();

        var refreshButton = UiHelpers.MakeButton("Refresh", 780, 135, 150);
        refreshButton.Click += async (_, _) => await LoadDataAsync();

        _grid = UiHelpers.MakeGrid(20, 60, 740, 550);
        _grid.Columns.Add("order_id", "Order #");
        _grid.Columns.Add("customer_name", "Customer");
        _grid.Columns.Add("order_date", "Date");
        _grid.Columns.Add("status", "Status");
        _grid.Columns.Add("total_amount", "Total");

        Controls.Add(heading);
        Controls.Add(filterLabel);
        Controls.Add(_statusFilterCombo);
        Controls.Add(viewButton);
        Controls.Add(refreshButton);
        Controls.Add(_grid);

        Load += async (_, _) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var filter = _statusFilterCombo.SelectedItem?.ToString();
            _orders = await OrderService.GetAllOrdersAsync(filter == "All" ? null : filter);

            _grid.Rows.Clear();
            foreach (var order in _orders)
            {
                _grid.Rows.Add(order.OrderId, order.CustomerName, order.OrderDate.ToString("yyyy-MM-dd HH:mm"), order.Status, $"{order.TotalAmount:0.00} Taka");
            }
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not load orders");
        }
        catch (Exception)
        {
            AppMessageBox.Show("Could not reach the server.", "Connection error");
        }
    }

    private async Task ViewSelectedAsync()
    {
        if (_grid.SelectedRows.Count == 0)
        {
            AppMessageBox.Show("Please select an order first.", "No selection");
            return;
        }

        var order = _orders[_grid.SelectedRows[0].Index];
        using var form = new OrderDetailForm(order, OrderDetailMode.AdminView);
        form.ShowDialog();
        if (form.ActionTaken) await LoadDataAsync();
    }
}
