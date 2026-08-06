using Frontend.Models;
using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Admin;

/// <summary>Modal popup used for both "Add Product" and "Edit Product".</summary>
public class ProductEditForm : BaseForm
{
    private readonly Product _existingProduct;
    private readonly TextBox _nameBox;
    private readonly TextBox _descriptionBox;
    private readonly NumericUpDown _priceBox;
    private readonly NumericUpDown _stockBox;
    private readonly ComboBox _categoryCombo;
    private readonly CheckBox _isActiveCheck;

    public bool WasSaved { get; private set; }

    public ProductEditForm(List<Category> categories, Product existingProduct = null)
    {
        _existingProduct = existingProduct;

        Text = existingProduct == null ? "Add Product" : "Edit Product";
        // ClientSize (not Width/Height) so the OS title bar can't eat into
        // the space our controls below are actually laid out in.
        ClientSize = new Size(460, 430);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;

        var nameLabel = UiHelpers.MakeLabel("Product Name", 30, 25, 150);
        _nameBox = UiHelpers.MakeTextBox(180, 23, 230);

        var descriptionLabel = UiHelpers.MakeLabel("Description", 30, 65, 150);
        _descriptionBox = UiHelpers.MakeTextBox(180, 63, 230, 60);
        _descriptionBox.Multiline = true;

        var categoryLabel = UiHelpers.MakeLabel("Category", 30, 140, 150);
        _categoryCombo = UiHelpers.MakeComboBox(180, 138, 230);
        _categoryCombo.DataSource = categories;

        var priceLabel = UiHelpers.MakeLabel("Price (Taka)", 30, 180, 150);
        _priceBox = UiHelpers.MakeNumericUpDown(180, 178, 150);
        _priceBox.DecimalPlaces = 2;
        _priceBox.Maximum = 100000;

        var stockLabel = UiHelpers.MakeLabel("Stock Quantity", 30, 220, 150);
        _stockBox = UiHelpers.MakeNumericUpDown(180, 218, 150);
        _stockBox.Maximum = 100000;

        _isActiveCheck = new CheckBox
        {
            Text = "Active (visible on the menu)",
            Location = new Point(180, 260),
            Size = new Size(250, 24),
            Checked = true,
        };

        var saveButton = UiHelpers.MakeButton("Save", 150, 360, 120);
        saveButton.Click += async (_, _) => await SaveAsync();

        var cancelButton = UiHelpers.MakeButton("Cancel", 280, 360, 110);
        cancelButton.Click += (_, _) => Close();

        Controls.Add(nameLabel);
        Controls.Add(_nameBox);
        Controls.Add(descriptionLabel);
        Controls.Add(_descriptionBox);
        Controls.Add(categoryLabel);
        Controls.Add(_categoryCombo);
        Controls.Add(priceLabel);
        Controls.Add(_priceBox);
        Controls.Add(stockLabel);
        Controls.Add(_stockBox);
        Controls.Add(_isActiveCheck);
        Controls.Add(saveButton);
        Controls.Add(cancelButton);

        if (existingProduct != null)
        {
            _nameBox.Text = existingProduct.ProductName;
            _descriptionBox.Text = existingProduct.Description;
            _priceBox.Value = existingProduct.Price;
            _stockBox.Value = existingProduct.StockQuantity;
            _isActiveCheck.Checked = existingProduct.IsActive;
            var match = categories.FirstOrDefault(c => c.CategoryId == existingProduct.CategoryId);
            if (match != null) _categoryCombo.SelectedItem = match;
        }
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_nameBox.Text))
        {
            AppMessageBox.Show("Product name is required.", "Missing information");
            return;
        }

        if (_categoryCombo.SelectedItem is not Category selectedCategory)
        {
            AppMessageBox.Show("Please choose a category.", "Missing information");
            return;
        }

        try
        {
            if (_existingProduct == null)
            {
                var request = new ProductCreateRequest
                {
                    ProductName = _nameBox.Text.Trim(),
                    Description = _descriptionBox.Text.Trim(),
                    Price = _priceBox.Value,
                    StockQuantity = (int)_stockBox.Value,
                    CategoryId = selectedCategory.CategoryId,
                };
                await ProductService.CreateProductAsync(request);
            }
            else
            {
                var request = new ProductUpdateRequest
                {
                    ProductName = _nameBox.Text.Trim(),
                    Description = _descriptionBox.Text.Trim(),
                    Price = _priceBox.Value,
                    StockQuantity = (int)_stockBox.Value,
                    CategoryId = selectedCategory.CategoryId,
                    IsActive = _isActiveCheck.Checked,
                };
                await ProductService.UpdateProductAsync(_existingProduct.ProductId, request);
            }
            WasSaved = true;
            Close();
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not save product");
        }
        catch (Exception)
        {
            AppMessageBox.Show("Could not reach the server.", "Connection error");
        }
    }
}
