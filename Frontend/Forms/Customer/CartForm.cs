using Frontend.Models;
using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Customer;

public class CartForm : BaseForm
{
    private readonly DataGridView _grid;
    private readonly Label _totalLabel;
    private readonly NumericUpDown _quantityBox;
    private Cart _cart = new();

    public CartForm()
    {
        FormBorderStyle = FormBorderStyle.None;

        var heading = UiHelpers.MakeHeading("My Cart", 20, 15, 300);

        _grid = UiHelpers.MakeGrid(20, 60, 740, 480);
        _grid.Columns.Add("product_name", "Product");
        _grid.Columns.Add("price", "Unit Price");
        _grid.Columns.Add("quantity", "Quantity");
        _grid.Columns.Add("subtotal", "Subtotal");

        _totalLabel = UiHelpers.MakeLabel("Total: 0.00 Taka", 20, 555, 300);
        _totalLabel.Font = Theme.HeadingFont;

        var quantityLabel = UiHelpers.MakeLabel("New Quantity", 780, 60, 150);
        _quantityBox = UiHelpers.MakeNumericUpDown(780, 85, 150, min: 1, max: 999);
        _quantityBox.Value = 1;

        var updateButton = UiHelpers.MakeButton("Update Quantity", 780, 130, 150);
        updateButton.Click += async (_, _) => await UpdateQuantityAsync();

        var removeButton = UiHelpers.MakeButton("Remove Item", 780, 175, 150);
        removeButton.Click += async (_, _) => await RemoveItemAsync();

        var clearButton = UiHelpers.MakeButton("Clear Cart", 780, 220, 150);
        clearButton.Click += async (_, _) => await ClearCartAsync();

        var checkoutButton = UiHelpers.MakeButton("Checkout", 780, 280, 150, 50);
        checkoutButton.Click += async (_, _) => await CheckoutAsync();

        Controls.Add(heading);
        Controls.Add(_grid);
        Controls.Add(_totalLabel);
        Controls.Add(quantityLabel);
        Controls.Add(_quantityBox);
        Controls.Add(updateButton);
        Controls.Add(removeButton);
        Controls.Add(clearButton);
        Controls.Add(checkoutButton);

        Load += async (_, _) => await LoadCartAsync();
    }

    private async Task LoadCartAsync()
    {
        try
        {
            _cart = await CartService.GetCartAsync();
            RefreshGrid();
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not load cart");
        }
    }

    private void RefreshGrid()
    {
        _grid.Rows.Clear();
        foreach (var item in _cart.Items)
        {
            _grid.Rows.Add(item.ProductName, $"{item.Price:0.00} Taka", item.Quantity, $"{item.Subtotal:0.00} Taka");
        }
        _totalLabel.Text = $"Total: {_cart.TotalAmount:0.00} Taka";
    }

    private CartItem GetSelectedItem()
    {
        if (_grid.SelectedRows.Count == 0) return null;
        var index = _grid.SelectedRows[0].Index;
        return index >= 0 && index < _cart.Items.Count ? _cart.Items[index] : null;
    }

    private async Task UpdateQuantityAsync()
    {
        var item = GetSelectedItem();
        if (item == null)
        {
            AppMessageBox.Show("Please select a cart item first.", "No selection");
            return;
        }

        try
        {
            _cart = await CartService.UpdateItemAsync(item.CartItemId, (int)_quantityBox.Value);
            RefreshGrid();
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not update quantity");
        }
    }

    private async Task RemoveItemAsync()
    {
        var item = GetSelectedItem();
        if (item == null)
        {
            AppMessageBox.Show("Please select a cart item first.", "No selection");
            return;
        }

        try
        {
            _cart = await CartService.RemoveItemAsync(item.CartItemId);
            RefreshGrid();
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not remove item");
        }
    }

    private async Task ClearCartAsync()
    {
        if (_cart.Items.Count == 0) return;
        if (!AppMessageBox.Confirm("Remove all items from your cart?", "Confirm")) return;

        try
        {
            _cart = await CartService.ClearCartAsync();
            RefreshGrid();
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not clear cart");
        }
    }

    private async Task CheckoutAsync()
    {
        if (_cart.Items.Count == 0)
        {
            AppMessageBox.Show("Your cart is empty. Add some products from the menu first.", "Nothing to check out");
            return;
        }

        if (!AppMessageBox.Confirm($"Place this order for {_cart.TotalAmount:0.00} Taka?", "Confirm Order"))
        {
            return;
        }

        try
        {
            var order = await OrderService.CheckoutAsync();
            AppMessageBox.Show($"Order #{order.OrderId} placed successfully! You can track it under My Orders.", "Order Placed");
            await LoadCartAsync();
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not place order");
        }
    }
}
