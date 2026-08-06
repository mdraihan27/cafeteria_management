namespace Frontend.Utilities;

/// <summary>
/// Applies the app's look consistently everywhere: white backgrounds,
/// black text, black/white buttons, and square corners (no rounding
/// anywhere - so no rounded panels, no rounded buttons).
///
/// Usage: call <c>Theme.Apply(this)</c> as the very last line of a
/// form's constructor, after every control has been added to it.
/// </summary>
public static class Theme
{
    public static readonly Font TitleFont = new("Segoe UI", 16F, FontStyle.Bold);
    public static readonly Font HeadingFont = new("Segoe UI", 12F, FontStyle.Bold);
    public static readonly Font NormalFont = new("Segoe UI", 10F);

    public static void Apply(Form form)
    {
        form.BackColor = AppColors.Background;
        form.ForeColor = AppColors.Text;
        form.Font = NormalFont;
        // Square corners on the window itself, and no maximize-driven
        // resizing surprises for these fixed-purpose screens.
        form.FormBorderStyle = FormBorderStyle.FixedSingle;

        ApplyToChildren(form.Controls);
    }

    private static void ApplyToChildren(Control.ControlCollection controls)
    {
        foreach (Control control in controls)
        {
            switch (control)
            {
                case Button button:
                    StyleButton(button);
                    break;
                case DataGridView grid:
                    StyleGrid(grid);
                    break;
                case Label label:
                    label.BackColor = AppColors.Background;
                    label.ForeColor = AppColors.Text;
                    break;
                case Panel or GroupBox or TabPage:
                    control.BackColor = AppColors.Background;
                    control.ForeColor = AppColors.Text;
                    break;
                default:
                    control.BackColor = AppColors.Background;
                    control.ForeColor = AppColors.Text;
                    break;
            }

            if (control.HasChildren)
            {
                ApplyToChildren(control.Controls);
            }
        }
    }

    /// <summary>Every button in the app: black background, white bold text, square corners.</summary>
    public static void StyleButton(Button button)
    {
        button.BackColor = AppColors.ButtonBackground;
        button.ForeColor = AppColors.ButtonText;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = AppColors.Text;
        button.FlatAppearance.BorderSize = 2;
        // The button background is already black, so "darkening" it on
        // hover wouldn't show up - lighten it a little instead, just for
        // that momentary hover feedback.
        button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(AppColors.ButtonBackground, 0.3f);
        button.Font = new Font(button.Font ?? NormalFont, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;
    }

    /// <summary>Grids get a plain black-and-white look with square cell borders.</summary>
    public static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = AppColors.Background;
        grid.ForeColor = AppColors.Text;
        grid.GridColor = AppColors.Text;
        grid.BorderStyle = BorderStyle.FixedSingle;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        grid.EnableHeadersVisualStyles = false;
        grid.RowHeadersVisible = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToResizeRows = false;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        grid.DefaultCellStyle.BackColor = AppColors.Background;
        grid.DefaultCellStyle.ForeColor = AppColors.Text;
        grid.DefaultCellStyle.SelectionBackColor = AppColors.ButtonBackground;
        grid.DefaultCellStyle.SelectionForeColor = AppColors.ButtonText;

        grid.ColumnHeadersDefaultCellStyle.BackColor = AppColors.Text;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font(NormalFont, FontStyle.Bold);
        grid.ColumnHeadersHeight = 32;

        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
    }
}
