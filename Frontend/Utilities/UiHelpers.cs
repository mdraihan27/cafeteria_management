namespace Frontend.Utilities;

/// <summary>
/// Small factory methods that build commonly-used controls with
/// sensible defaults, so every form doesn't have to repeat the same
/// five lines to make a label or a text box. Forms still position
/// controls themselves (via Location/Size) - this just removes the
/// repetitive boilerplate.
/// </summary>
public static class UiHelpers
{
    public static Label MakeLabel(string text, int x, int y, int width = 160, int height = 24)
    {
        return new Label
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            Font = Theme.NormalFont,
            TextAlign = ContentAlignment.MiddleLeft,
        };
    }

    public static Label MakeHeading(string text, int x, int y, int width = 300, int height = 32)
    {
        return new Label
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            Font = Theme.HeadingFont,
            TextAlign = ContentAlignment.MiddleLeft,
        };
    }

    public static TextBox MakeTextBox(int x, int y, int width = 220, int height = 26, bool isPassword = false)
    {
        return new TextBox
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            Font = Theme.NormalFont,
            UseSystemPasswordChar = isPassword,
        };
    }

    public static Button MakeButton(string text, int x, int y, int width = 130, int height = 38)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
        };
        Theme.StyleButton(button);
        return button;
    }

    public static ComboBox MakeComboBox(int x, int y, int width = 220, int height = 26)
    {
        return new ComboBox
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            Font = Theme.NormalFont,
            DropDownStyle = ComboBoxStyle.DropDownList,
        };
    }

    public static NumericUpDown MakeNumericUpDown(int x, int y, int width = 100, int height = 26, decimal min = 0, decimal max = 100000)
    {
        return new NumericUpDown
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            Font = Theme.NormalFont,
            Minimum = min,
            Maximum = max,
        };
    }

    public static DataGridView MakeGrid(int x, int y, int width, int height)
    {
        var grid = new DataGridView
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            AutoGenerateColumns = false,
        };
        Theme.StyleGrid(grid);
        return grid;
    }

    public static LinkLabel MakeLinkLabel(string text, int x, int y, int width = 220, int height = 24)
    {
        return new LinkLabel
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            LinkColor = AppColors.Text,
            ActiveLinkColor = AppColors.Text,
            VisitedLinkColor = AppColors.Text,
            Font = Theme.NormalFont,
        };
    }
}
