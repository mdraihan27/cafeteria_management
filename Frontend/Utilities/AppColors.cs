namespace Frontend.Utilities;

/// <summary>
/// The entire app uses exactly two colors, on purpose:
///   - White for every background.
///   - Black for every piece of text - and buttons flip that around
///     (black behind white text), so they still stand out as the
///     thing you can click.
/// Nothing in this app should ever use a rounded corner either - see
/// <see cref="Theme"/>, which applies these rules everywhere.
/// </summary>
public static class AppColors
{
    public static readonly Color Background = Color.White;
    public static readonly Color Text = Color.Black;
    public static readonly Color ButtonBackground = Color.Black;
    public static readonly Color ButtonText = Color.White;
}
