using Frontend.Models;
using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Admin;

public class AdminProductsForm : BaseForm
{
    private readonly DataGridView _grid;
    private List<Product> _products = new();
    private List<Category> _categories = new();

    public AdminProductsForm()
    {
        FormBorderStyle = FormBorderStyle.None;

        var heading = UiHelpers.MakeHeading("Products", 20, 15, 300);

        var addButton = UiHelpers.MakeButton("Add Product", 780, 15, 150);
        addButton.Click += async (_, _) => await AddProductAsync();

        var editButton = UiHelpers.MakeButton("Edit Selected", 780, 60, 150);
        editButton.Click += async (_, _) => await EditSelectedAsync();

        var deleteButton = UiHelpers.MakeButton("Delete Selected", 780, 105, 150);
        deleteButton.Click += async (_, _) => await DeleteSelectedAsync();

        var reactivateButton = UiHelpers.MakeButton("Reactivate Selected", 780, 150, 150);
        reactivateButton.Click += async (_, _) => await ReactivateSelectedAsync();

        _grid = UiHelpers.MakeGrid(20, 60, 740, 550);
        _grid.Columns.Add("product_name", "Product Name");
        _grid.Columns.Add("category_name", "Category");
        _grid.Columns.Add("price", "Price");
        _grid.Columns.Add("stock_quantity", "Stock");
        _grid.Columns.Add("is_active", "Active");

        Controls.Add(heading);
        Controls.Add(addButton);
        Controls.Add(editButton);
        Controls.Add(deleteButton);
        Controls.Add(reactivateButton);
        Controls.Add(_grid);

        Load += async (_, _) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _categories = await CategoryService.GetCategoriesAsync();
            _products = await ProductService.GetAllProductsForAdminAsync();

            _grid.Rows.Clear();
            foreach (var product in _products)
            {
                _grid.Rows.Add(product.ProductName, product.CategoryName, $"{product.Price:0.00} Taka", product.StockQuantity, product.IsActive ? "Yes" : "No");
            }
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not load products");
        }
        catch (Exception)
        {
            AppMessageBox.Show("Could not reach the server.", "Connection error");
        }
    }

    private Product GetSelectedProduct()
    {
        if (_grid.SelectedRows.Count == 0) return null;
        var index = _grid.SelectedRows[0].Index;
        return index >= 0 && index < _products.Count ? _products[index] : null;
    }

    private async Task AddProductAsync()
    {
        if (_categories.Count == 0)
        {
            AppMessageBox.Show("Please create at least one category before adding products.", "No categories");
            return;
        }

        using var form = new ProductEditForm(_categories);
        form.ShowDialog();
        if (form.WasSaved) await LoadDataAsync();
    }

    private async Task EditSelectedAsync()
    {
        var product = GetSelectedProduct();
        if (product == null)
        {
            AppMessageBox.Show("Please select a product first.", "No selection");
            return;
        }

        using var form = new ProductEditForm(_categories, product);
        form.ShowDialog();
        if (form.WasSaved) await LoadDataAsync();
    }

    private async Task DeleteSelectedAsync()
    {
        var product = GetSelectedProduct();
        if (product == null)
        {
            AppMessageBox.Show("Please select a product first.", "No selection");
            return;
        }

        if (!AppMessageBox.Confirm($"Delete product '{product.ProductName}'? It will be hidden from the menu, but past orders will still show it.", "Confirm Delete"))
        {
            return;
        }

        try
        {
            await ProductService.DeleteProductAsync(product.ProductId);
            await LoadDataAsync();
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not delete product");
        }
        catch (Exception)
        {
            AppMessageBox.Show("Could not reach the server.", "Connection error");
        }
    }

    private async Task ReactivateSelectedAsync()
    {
        var product = GetSelectedProduct();
        if (product == null)
        {
            AppMessageBox.Show("Please select a product first.", "No selection");
            return;
        }

        try
        {
            await ProductService.ReactivateProductAsync(product.ProductId);
            await LoadDataAsync();
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not reactivate product");
        }
        catch (Exception)
        {
            AppMessageBox.Show("Could not reach the server.", "Connection error");
        }
    }
}
