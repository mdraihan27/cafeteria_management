using Frontend.Utilities;

namespace Frontend.Forms;

/// <summary>
/// Every window in this app inherits from BaseForm instead of the
/// plain WinForms Form class. That means every window automatically
/// gets the same look (white background, black text, square corners)
/// without each screen having to remember to apply it.
/// </summary>
public class BaseForm : Form
{
    protected BaseForm()
    {
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        Font = Theme.NormalFont;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Theme.Apply(this);
    }
}
