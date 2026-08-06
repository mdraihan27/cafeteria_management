using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Admin;

/// <summary>Read-only list of every registered customer.</summary>
public class AdminCustomersForm : BaseForm
{
    private readonly DataGridView _grid;

    public AdminCustomersForm()
    {
        FormBorderStyle = FormBorderStyle.None;

        var heading = UiHelpers.MakeHeading("Customers", 20, 15, 300);

        _grid = UiHelpers.MakeGrid(20, 60, 900, 550);
        _grid.Columns.Add("full_name", "Full Name");
        _grid.Columns.Add("email", "Email");
        _grid.Columns.Add("phone", "Phone");
        _grid.Columns.Add("address", "Address");
        _grid.Columns.Add("created_at", "Joined On");

        Controls.Add(heading);
        Controls.Add(_grid);

        Load += async (_, _) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var customers = await CustomerService.GetCustomersAsync();
            _grid.Rows.Clear();
            foreach (var customer in customers)
            {
                _grid.Rows.Add(customer.FullName, customer.Email, customer.Phone, customer.Address, customer.CreatedAt.ToString("yyyy-MM-dd"));
            }
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not load customers");
        }
        catch (Exception)
        {
            AppMessageBox.Show("Could not reach the server.", "Connection error");
        }
    }
}
