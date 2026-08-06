using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Admin;

/// <summary>
/// The four reports an Admin can view. A row of buttons along the
/// top switches which report is shown in the panel below - the same
/// "swap the content area" idea used by AdminMainForm, just one level
/// deeper.
/// </summary>
public class AdminReportsForm : BaseForm
{
    private readonly Panel _reportPanel;

    public AdminReportsForm()
    {
        FormBorderStyle = FormBorderStyle.None;

        var heading = UiHelpers.MakeHeading("Reports", 20, 15, 300);

        var monthlyButton = UiHelpers.MakeButton("Monthly Sales", 20, 60, 160);
        var yearlyButton = UiHelpers.MakeButton("Yearly Sales", 190, 60, 160);
        var productButton = UiHelpers.MakeButton("Product Sales", 360, 60, 160);
        var inventoryButton = UiHelpers.MakeButton("Inventory", 530, 60, 160);

        monthlyButton.Click += (_, _) => ShowMonthlySales();
        yearlyButton.Click += (_, _) => ShowYearlySales();
        productButton.Click += (_, _) => ShowProductSales();
        inventoryButton.Click += (_, _) => ShowInventory();

        _reportPanel = new Panel { Location = new Point(20, 110), Size = new Size(950, 520), BorderStyle = BorderStyle.FixedSingle };

        Controls.Add(heading);
        Controls.Add(monthlyButton);
        Controls.Add(yearlyButton);
        Controls.Add(productButton);
        Controls.Add(inventoryButton);
        Controls.Add(_reportPanel);

        Load += (_, _) => ShowMonthlySales();
    }

    // --- Monthly Sales Report ------------------------------------------

    private void ShowMonthlySales()
    {
        _reportPanel.Controls.Clear();

        var yearLabel = UiHelpers.MakeLabel("Year", 20, 15, 60);
        var yearBox = UiHelpers.MakeNumericUpDown(85, 13, 90, min: 2000, max: 2100);
        yearBox.Value = DateTime.Today.Year;

        var monthLabel = UiHelpers.MakeLabel("Month", 200, 15, 60);
        var monthCombo = UiHelpers.MakeComboBox(260, 13, 130);
        for (var m = 1; m <= 12; m++) monthCombo.Items.Add(System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(m));
        monthCombo.SelectedIndex = DateTime.Today.Month - 1;

        var loadButton = UiHelpers.MakeButton("Load", 410, 10, 100, 30);

        var summaryLabel = UiHelpers.MakeLabel("", 20, 55, 500);
        summaryLabel.Font = Theme.HeadingFont;

        var grid = UiHelpers.MakeGrid(20, 90, 900, 400);
        grid.Columns.Add("day", "Day");
        grid.Columns.Add("total_sales", "Total Sales");
        grid.Columns.Add("order_count", "Orders");

        loadButton.Click += async (_, _) =>
        {
            try
            {
                var report = await ReportService.GetMonthlySalesAsync((int)yearBox.Value, monthCombo.SelectedIndex + 1);
                summaryLabel.Text = $"Total Sales: {report.TotalSales:0.00} Taka   |   Orders: {report.OrderCount}";
                grid.Rows.Clear();
                foreach (var day in report.DailyBreakdown)
                {
                    grid.Rows.Add(day.Day, $"{day.TotalSales:0.00} Taka", day.OrderCount);
                }
            }
            catch (ApiException ex)
            {
                AppMessageBox.Show(ex.Message, "Could not load report");
            }
        };

        _reportPanel.Controls.Add(yearLabel);
        _reportPanel.Controls.Add(yearBox);
        _reportPanel.Controls.Add(monthLabel);
        _reportPanel.Controls.Add(monthCombo);
        _reportPanel.Controls.Add(loadButton);
        _reportPanel.Controls.Add(summaryLabel);
        _reportPanel.Controls.Add(grid);

        loadButton.PerformClick();
    }

    // --- Yearly Sales Report --------------------------------------------

    private void ShowYearlySales()
    {
        _reportPanel.Controls.Clear();

        var yearLabel = UiHelpers.MakeLabel("Year", 20, 15, 60);
        var yearBox = UiHelpers.MakeNumericUpDown(85, 13, 90, min: 2000, max: 2100);
        yearBox.Value = DateTime.Today.Year;

        var loadButton = UiHelpers.MakeButton("Load", 200, 10, 100, 30);

        var summaryLabel = UiHelpers.MakeLabel("", 20, 55, 500);
        summaryLabel.Font = Theme.HeadingFont;

        var grid = UiHelpers.MakeGrid(20, 90, 900, 400);
        grid.Columns.Add("month", "Month");
        grid.Columns.Add("total_sales", "Total Sales");
        grid.Columns.Add("order_count", "Orders");

        loadButton.Click += async (_, _) =>
        {
            try
            {
                var report = await ReportService.GetYearlySalesAsync((int)yearBox.Value);
                summaryLabel.Text = $"Total Sales: {report.TotalSales:0.00} Taka   |   Orders: {report.OrderCount}";
                grid.Rows.Clear();
                foreach (var month in report.MonthlyBreakdown)
                {
                    var monthName = System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month.Month);
                    grid.Rows.Add(monthName, $"{month.TotalSales:0.00} Taka", month.OrderCount);
                }
            }
            catch (ApiException ex)
            {
                AppMessageBox.Show(ex.Message, "Could not load report");
            }
        };

        _reportPanel.Controls.Add(yearLabel);
        _reportPanel.Controls.Add(yearBox);
        _reportPanel.Controls.Add(loadButton);
        _reportPanel.Controls.Add(summaryLabel);
        _reportPanel.Controls.Add(grid);

        loadButton.PerformClick();
    }

    // --- Product Sales Report --------------------------------------------

    private void ShowProductSales()
    {
        _reportPanel.Controls.Clear();

        var grid = UiHelpers.MakeGrid(20, 20, 900, 470);
        grid.Columns.Add("product_name", "Product");
        grid.Columns.Add("quantity_sold", "Quantity Sold");
        grid.Columns.Add("total_revenue", "Total Revenue");

        _reportPanel.Controls.Add(grid);

        LoadProductSalesAsync(grid);
    }

    private async void LoadProductSalesAsync(DataGridView grid)
    {
        try
        {
            var report = await ReportService.GetProductSalesAsync();
            grid.Rows.Clear();
            foreach (var item in report.Items)
            {
                grid.Rows.Add(item.ProductName, item.QuantitySold, $"{item.TotalRevenue:0.00} Taka");
            }
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not load report");
        }
    }

    // --- Inventory Report --------------------------------------------

    private void ShowInventory()
    {
        _reportPanel.Controls.Clear();

        var grid = UiHelpers.MakeGrid(20, 20, 900, 470);
        grid.Columns.Add("product_name", "Product");
        grid.Columns.Add("category_name", "Category");
        grid.Columns.Add("price", "Price");
        grid.Columns.Add("stock_quantity", "Stock");
        grid.Columns.Add("is_low_stock", "Low Stock?");
        grid.Columns.Add("is_active", "Active?");

        _reportPanel.Controls.Add(grid);

        LoadInventoryAsync(grid);
    }

    private async void LoadInventoryAsync(DataGridView grid)
    {
        try
        {
            var report = await ReportService.GetInventoryAsync();
            grid.Rows.Clear();
            foreach (var item in report.Items)
            {
                grid.Rows.Add(item.ProductName, item.CategoryName, $"{item.Price:0.00} Taka", item.StockQuantity,
                    item.IsLowStock ? "Yes" : "No", item.IsActive ? "Yes" : "No");
            }
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not load report");
        }
    }
}
