using Frontend.Models;
using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Customer;

/// <summary>The customer's full order history, with the ability to cancel pending orders.</summary>
public class MyOrdersForm : BaseForm
{
    private readonly DataGridView _grid;
    private List<Order> _orders = new();

    public MyOrdersForm()
    {
        FormBorderStyle = FormBorderStyle.None;

        var heading = UiHelpers.MakeHeading("My Orders", 20, 15, 300);

        var viewButton = UiHelpers.MakeButton("View Details", 780, 60, 150);
        viewButton.Click += async (_, _) => await ViewSelectedAsync();

        var refreshButton = UiHelpers.MakeButton("Refresh", 780, 105, 150);
        refreshButton.Click += async (_, _) => await LoadOrdersAsync();

        _grid = UiHelpers.MakeGrid(20, 60, 740, 550);
        _grid.Columns.Add("order_id", "Order #");
        _grid.Columns.Add("order_date", "Date");
        _grid.Columns.Add("status", "Status");
        _grid.Columns.Add("total_amount", "Total");

        Controls.Add(heading);
        Controls.Add(viewButton);
        Controls.Add(refreshButton);
        Controls.Add(_grid);

        Load += async (_, _) => await LoadOrdersAsync();
    }

    private async Task LoadOrdersAsync()
    {
        try
        {
            _orders = await OrderService.GetMyOrdersAsync();
            _grid.Rows.Clear();
            foreach (var order in _orders)
            {
                _grid.Rows.Add(order.OrderId, order.OrderDate.ToString("yyyy-MM-dd HH:mm"), order.Status, $"{order.TotalAmount:0.00} Taka");
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
        using var form = new OrderDetailForm(order, OrderDetailMode.CustomerView);
        form.ShowDialog();
        if (form.ActionTaken) await LoadOrdersAsync();
    }
}
