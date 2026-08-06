using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Admin;

/// <summary>The admin's "at a glance" screen: totals plus a low-stock warning list.</summary>
public class AdminDashboardForm : BaseForm
{
    private readonly Label _totalProductsValue;
    private readonly Label _totalCustomersValue;
    private readonly Label _totalOrdersValue;
    private readonly Label _monthlySalesValue;
    private readonly DataGridView _lowStockGrid;

    public AdminDashboardForm()
    {
        FormBorderStyle = FormBorderStyle.None;

        var heading = UiHelpers.MakeHeading("Dashboard", 20, 15, 300);

        var (productsBox, productsValue) = MakeStatCard("Total Products", 20, 60);
        var (customersBox, customersValue) = MakeStatCard("Total Customers", 260, 60);
        var (ordersBox, ordersValue) = MakeStatCard("Total Orders", 500, 60);
        var (salesBox, salesValue) = MakeStatCard("Monthly Sales", 740, 60);
        _totalProductsValue = productsValue;
        _totalCustomersValue = customersValue;
        _totalOrdersValue = ordersValue;
        _monthlySalesValue = salesValue;

        var lowStockHeading = UiHelpers.MakeHeading("Low Stock Products", 20, 200, 300);
        _lowStockGrid = UiHelpers.MakeGrid(20, 240, 950, 350);
        _lowStockGrid.Columns.Add("product_name", "Product");
        _lowStockGrid.Columns.Add("category_name", "Category");
        _lowStockGrid.Columns.Add("stock_quantity", "Stock Left");
        _lowStockGrid.Columns.Add("price", "Price");

        Controls.Add(heading);
        Controls.Add(productsBox);
        Controls.Add(customersBox);
        Controls.Add(ordersBox);
        Controls.Add(salesBox);
        Controls.Add(lowStockHeading);
        Controls.Add(_lowStockGrid);

        Load += async (_, _) => await LoadDataAsync();
    }

    private static (Panel, Label) MakeStatCard(string title, int x, int y)
    {
        var box = new Panel { Location = new Point(x, y), Size = new Size(220, 110), BorderStyle = BorderStyle.FixedSingle };
        var titleLabel = UiHelpers.MakeLabel(title, 15, 12, 190, 24);
        var valueLabel = new Label
        {
            Text = "-",
            Location = new Point(15, 45),
            Size = new Size(190, 50),
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
        };
        box.Controls.Add(titleLabel);
        box.Controls.Add(valueLabel);
        return (box, valueLabel);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var dashboard = await DashboardService.GetAdminDashboardAsync();
            _totalProductsValue.Text = dashboard.TotalProducts.ToString();
            _totalCustomersValue.Text = dashboard.TotalCustomers.ToString();
            _totalOrdersValue.Text = dashboard.TotalOrders.ToString();
            _monthlySalesValue.Text = $"{dashboard.MonthlySales:0.00} Taka";

            _lowStockGrid.Rows.Clear();
            foreach (var product in dashboard.LowStockProducts)
            {
                _lowStockGrid.Rows.Add(product.ProductName, product.CategoryName, product.StockQuantity, $"{product.Price:0.00} Taka");
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
