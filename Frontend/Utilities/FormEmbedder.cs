namespace Frontend.Utilities;

/// <summary>
/// A small trick used by AdminMainForm and CustomerMainForm: instead
/// of building one giant window with every screen crammed into it,
/// each screen (Dashboard, Products, Orders, ...) is its own normal
/// Form class. This helper displays one of those forms *inside* a
/// Panel, as if it were just another control, so clicking a nav
/// button can swap the visible screen in and out.
/// </summary>
public static class FormEmbedder
{
    public static void ShowInPanel(Panel container, Form childForm)
    {
        container.Controls.Clear();

        childForm.TopLevel = false;
        childForm.FormBorderStyle = FormBorderStyle.None;
        childForm.Dock = DockStyle.Fill;

        container.Controls.Add(childForm);
        childForm.Show();
    }
}
