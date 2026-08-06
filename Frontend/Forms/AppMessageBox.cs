using Frontend.Utilities;

namespace Frontend.Forms;

/// <summary>
/// A small popup window used instead of the built-in MessageBox.Show.
/// We use our own so every popup in the app follows the same three
/// colors and square-corner look, instead of the operating system's
/// default (rounded, blue) message box style.
/// </summary>
public class AppMessageBox : BaseForm
{
    private bool _result;

    private AppMessageBox(string title, string message, bool showCancel)
    {
        Text = title;
        ClientSize = new Size(420, 210);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;

        var titleLabel = new Label
        {
            Text = title,
            Font = Theme.HeadingFont,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(20, 15),
            Size = new Size(380, 30),
        };

        var messageLabel = new Label
        {
            Text = message,
            Font = Theme.NormalFont,
            AutoSize = false,
            TextAlign = ContentAlignment.TopLeft,
            Location = new Point(20, 55),
            Size = new Size(380, 90),
        };

        var okButton = new Button
        {
            Text = "OK",
            Size = new Size(100, 36),
            Location = new Point(showCancel ? 190 : 300, 150),
            DialogResult = DialogResult.OK,
        };
        okButton.Click += (_, _) => { _result = true; Close(); };

        Controls.Add(titleLabel);
        Controls.Add(messageLabel);
        Controls.Add(okButton);

        if (showCancel)
        {
            var cancelButton = new Button
            {
                Text = "Cancel",
                Size = new Size(100, 36),
                Location = new Point(300, 150),
                DialogResult = DialogResult.Cancel,
            };
            cancelButton.Click += (_, _) => { _result = false; Close(); };
            Controls.Add(cancelButton);
            AcceptButton = okButton;
            CancelButton = cancelButton;
        }
        else
        {
            AcceptButton = okButton;
            CancelButton = okButton;
        }
    }

    /// <summary>Shows a simple information/error popup with just an OK button.</summary>
    public static void Show(string message, string title = "Message")
    {
        using var box = new AppMessageBox(title, message, showCancel: false);
        box.ShowDialog();
    }

    /// <summary>Shows a Yes/No style confirmation popup. Returns true if the user clicked OK.</summary>
    public static bool Confirm(string message, string title = "Please Confirm")
    {
        using var box = new AppMessageBox(title, message, showCancel: true);
        box.ShowDialog();
        return box._result;
    }
}
