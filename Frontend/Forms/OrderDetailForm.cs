using Frontend.Models;
using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms;

public enum OrderDetailMode
{
    AdminView,
    CustomerView,
}

/// <summary>
/// Shows everything about one order: its items, total, and payment
/// (if any). Depending on who is looking at it, it also offers the
/// right action buttons:
///   - Admin, pending order:    "Mark Completed" and "Cancel Order"
///   - Customer, pending order: "Cancel Order"
///   - Anything else:           read-only
/// </summary>
public class OrderDetailForm : BaseForm
{
    private readonly Order _order;
    private readonly OrderDetailMode _mode;

    public bool ActionTaken { get; private set; }

    public OrderDetailForm(Order order, OrderDetailMode mode)
    {
        _order = order;
        _mode = mode;

        Text = $"Order #{order.OrderId}";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;

        var heading = UiHelpers.MakeHeading($"Order #{order.OrderId}", 25, 20, 300);

        var customerLabel = UiHelpers.MakeLabel($"Customer: {order.CustomerName}", 25, 65, 480);
        var dateLabel = UiHelpers.MakeLabel($"Order Date: {order.OrderDate:yyyy-MM-dd HH:mm}", 25, 90, 480);
        var statusLabel = UiHelpers.MakeLabel($"Status: {order.Status.ToUpperInvariant()}", 25, 115, 480);

        var itemsHeading = UiHelpers.MakeLabel("Items", 25, 150, 200);
        itemsHeading.Font = Theme.HeadingFont;

        var grid = UiHelpers.MakeGrid(25, 180, 490, 200);
        grid.Columns.Add("product_name", "Product");
        grid.Columns.Add("unit_price", "Unit Price");
        grid.Columns.Add("quantity", "Qty");
        grid.Columns.Add("subtotal", "Subtotal");
        foreach (var item in order.Items)
        {
            grid.Rows.Add(item.ProductName, $"{item.UnitPrice:0.00} Taka", item.Quantity, $"{item.Subtotal:0.00} Taka");
        }

        var totalLabel = UiHelpers.MakeLabel($"Total: {order.TotalAmount:0.00} Taka", 25, 395, 300);
        totalLabel.Font = Theme.HeadingFont;

        var paymentText = order.Payment == null
            ? "Payment: not recorded yet"
            : $"Payment: {order.Payment.Amount:0.00} Taka via {order.Payment.PaymentMethod} on {order.Payment.PaymentDate:yyyy-MM-dd}";
        var paymentLabel = UiHelpers.MakeLabel(paymentText, 25, 425, 480);

        Controls.Add(heading);
        Controls.Add(customerLabel);
        Controls.Add(dateLabel);
        Controls.Add(statusLabel);
        Controls.Add(itemsHeading);
        Controls.Add(grid);
        Controls.Add(totalLabel);
        Controls.Add(paymentLabel);

        // Everything below here is stacked one row at a time, tracking
        // how far down we've drawn so nothing ever overlaps or gets cut
        // off - no matter how many action buttons a given order needs.
        var nextRowTop = 460;
        nextRowTop = AddActionButtons(order, nextRowTop);

        var closeButton = UiHelpers.MakeButton("Close", 375, nextRowTop, 140);
        closeButton.Click += (_, _) => Close();
        Controls.Add(closeButton);
        nextRowTop += closeButton.Height;

        // ClientSize (unlike Width/Height) is the usable content area,
        // not counting the title bar - so this guarantees every control
        // above actually fits on screen, regardless of Windows theme.
        ClientSize = new Size(540, nextRowTop + 20);
    }

    /// <summary>Adds whatever action buttons apply, starting at <paramref name="top"/>. Returns the Y position just below the last row added.</summary>
    private int AddActionButtons(Order order, int top)
    {
        if (order.Status != "pending")
        {
            return top;
        }

        if (_mode == OrderDetailMode.AdminView)
        {
            var paymentMethodLabel = UiHelpers.MakeLabel("Payment Method", 25, top, 130);
            var paymentMethodCombo = UiHelpers.MakeComboBox(160, top - 2, 150);
            paymentMethodCombo.Items.AddRange(new object[] { "cash", "card", "mobile_banking" });
            paymentMethodCombo.SelectedIndex = 0;
            Controls.Add(paymentMethodLabel);
            Controls.Add(paymentMethodCombo);
            top += 40;

            var completeButton = UiHelpers.MakeButton("Mark Completed", 25, top, 160);
            completeButton.Click += async (_, _) =>
            {
                try
                {
                    await OrderService.CompleteOrderAsync(order.OrderId, paymentMethodCombo.SelectedItem?.ToString() ?? "cash");
                    ActionTaken = true;
                    Close();
                }
                catch (ApiException ex)
                {
                    AppMessageBox.Show(ex.Message, "Could not complete order");
                }
            };

            var cancelButton = UiHelpers.MakeButton("Cancel Order", 195, top, 150);
            cancelButton.Click += async (_, _) =>
            {
                if (!AppMessageBox.Confirm("Cancel this order and restore stock?", "Confirm Cancel")) return;
                try
                {
                    await OrderService.CancelOrderAsync(order.OrderId);
                    ActionTaken = true;
                    Close();
                }
                catch (ApiException ex)
                {
                    AppMessageBox.Show(ex.Message, "Could not cancel order");
                }
            };

            Controls.Add(completeButton);
            Controls.Add(cancelButton);
            top += completeButton.Height + 15;
        }
        else
        {
            var cancelButton = UiHelpers.MakeButton("Cancel Order", 25, top, 160);
            cancelButton.Click += async (_, _) =>
            {
                if (!AppMessageBox.Confirm("Cancel this order?", "Confirm Cancel")) return;
                try
                {
                    await OrderService.CancelMyOrderAsync(order.OrderId);
                    ActionTaken = true;
                    Close();
                }
                catch (ApiException ex)
                {
                    AppMessageBox.Show(ex.Message, "Could not cancel order");
                }
            };
            Controls.Add(cancelButton);
            top += cancelButton.Height + 15;
        }

        return top;
    }
}
