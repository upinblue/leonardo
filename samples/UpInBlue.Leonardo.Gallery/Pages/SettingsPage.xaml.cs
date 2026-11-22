using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UpInBlue.Leonardo.Gallery.Services;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// The settings of the gallery: theme and list density.
/// </summary>
public sealed partial class SettingsPage : Page
{
    private static readonly ElementTheme[] Themes = [ElementTheme.Light, ElementTheme.Dark, ElementTheme.Default];

    /// <summary>
    /// Initializes the page.
    /// </summary>
    public SettingsPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The theme as the position in the combo box: Light, Dark, Use system setting.
    /// </summary>
    public int ThemeIndex
    {
        get => Array.IndexOf(Themes, App.Settings.Theme);
        set
        {
            if (value >= 0 && value < Themes.Length)
            {
                App.Settings.Theme = Themes[value];
            }
        }
    }

    /// <summary>
    /// The density as the position in the radio buttons: Compact, Standard, Comfortable.
    /// </summary>
    public int DensityIndex
    {
        get => (int)App.Settings.Density;
        set
        {
            if (Enum.IsDefined((ListDensity)value))
            {
                App.Settings.Density = (ListDensity)value;
            }
        }
    }

    /// <summary>
    /// The version of the gallery.
    /// </summary>
    public string Version => $"Version {Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)}";
}
