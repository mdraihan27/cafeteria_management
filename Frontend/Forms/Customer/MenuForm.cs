using Frontend.Models;
using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Customer;

/// <summary>Lets a customer browse the menu and add items to their cart.</summary>
public class MenuForm : BaseForm
{
    private readonly ComboBox _categoryCombo;
    private readonly TextBox _searchBox;
    private readonly DataGridView _grid;
    private readonly NumericUpDown _quantityBox;
    private List<Product> _products = new();
    private List<Category> _categories = new();

    public MenuForm()
    {
        FormBorderStyle = FormBorderStyle.None;

        var heading = UiHelpers.MakeHeading("Browse Menu", 20, 15, 300);

        var categoryLabel = UiHelpers.MakeLabel("Category", 20, 60, 80);
        _categoryCombo = UiHelpers.MakeComboBox(100, 58, 200);
        _categoryCombo.SelectedIndexChanged += async (_, _) => await LoadProductsAsync();

        var searchLabel = UiHelpers.MakeLabel("Search", 320, 60, 60);
        _searchBox = UiHelpers.MakeTextBox(390, 58, 200);
        var searchButton = UiHelpers.MakeButton("Search", 600, 56, 100, 30);
        searchButton.Click += async (_, _) => await LoadProductsAsync();

        _grid = UiHelpers.MakeGrid(20, 100, 740, 500);
        _grid.Columns.Add("product_name", "Product");
        _grid.Columns.Add("category_name", "Category");
        _grid.Columns.Add("price", "Price");
        _grid.Columns.Add("stock_quantity", "In Stock");

        var quantityLabel = UiHelpers.MakeLabel("Quantity", 780, 100, 150);
        _quantityBox = UiHelpers.MakeNumericUpDown(780, 125, 150, min: 1, max: 999);
        _quantityBox.Value = 1;

        var addButton = UiHelpers.MakeButton("Add to Cart", 780, 170, 150);
        addButton.Click += async (_, _) => await AddToCartAsync();

        Controls.Add(heading);
        Controls.Add(categoryLabel);
        Controls.Add(_categoryCombo);
        Controls.Add(searchLabel);
        Controls.Add(_searchBox);
        Controls.Add(searchButton);
        Controls.Add(_grid);
        Controls.Add(quantityLabel);
        Controls.Add(_quantityBox);
        Controls.Add(addButton);

        Load += async (_, _) => await LoadCategoriesAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            _categories = await CategoryService.GetCategoriesAsync();
            _categoryCombo.Items.Clear();
            _categoryCombo.Items.Add("All Categories");
            foreach (var category in _categories) _categoryCombo.Items.Add(category);
            _categoryCombo.SelectedIndex = 0;
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not load categories");
        }
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            int? categoryId = _categoryCombo.SelectedItem is Category selected ? selected.CategoryId : null;
            var search = string.IsNullOrWhiteSpace(_searchBox.Text) ? null : _searchBox.Text.Trim();

            _products = await ProductService.GetProductsAsync(categoryId, search);

            _grid.Rows.Clear();
            foreach (var product in _products)
            {
                _grid.Rows.Add(product.ProductName, product.CategoryName, $"{product.Price:0.00} Taka", product.StockQuantity);
            }
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not load products");
        }
    }

    private async Task AddToCartAsync()
    {
        if (_grid.SelectedRows.Count == 0)
        {
            AppMessageBox.Show("Please select a product first.", "No selection");
            return;
        }

        var product = _products[_grid.SelectedRows[0].Index];

        try
        {
            await CartService.AddItemAsync(product.ProductId, (int)_quantityBox.Value);
            AppMessageBox.Show($"Added {(int)_quantityBox.Value} x {product.ProductName} to your cart.", "Added to cart");
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not add to cart");
        }
    }
}
