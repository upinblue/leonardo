using System.ComponentModel;
using System.Globalization;
using Microsoft.UI.Xaml.Controls;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// The live state of transparency, animations, contrast themes and text size, with instructions to change each.
/// </summary>
public sealed partial class FallbacksPage : Page, INotifyPropertyChanged
{
    /// <summary>
    /// Initializes the page.
    /// </summary>
    public FallbacksPage()
    {
        InitializeComponent();
        Loaded += (_, _) => App.SystemSettings.PropertyChanged += OnSystemChanged;
        Unloaded += (_, _) => App.SystemSettings.PropertyChanged -= OnSystemChanged;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// "Off", or "On: " and the name of the contrast theme.
    /// </summary>
    public string ContrastState => App.SystemSettings.HighContrast
        ? $"On: {App.SystemSettings.HighContrastScheme}"
        : "Off";

    /// <summary>
    /// The text size as a percentage, for example "100 %".
    /// </summary>
    public string TextSizeState => string.Create(CultureInfo.GetCultureInfo("en-US"), $"{App.SystemSettings.TextScaleFactor:P0}");

    private void OnSystemChanged(object? sender, PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContrastState)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextSizeState)));
    }
}
