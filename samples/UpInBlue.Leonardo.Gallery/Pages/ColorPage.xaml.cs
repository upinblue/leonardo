using Microsoft.UI.Xaml.Controls;
using UpInBlue.Leonardo.Gallery.ViewModels;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// The blue scale and the theme colors in light and dark with their contrast.
/// </summary>
public sealed partial class ColorPage : Page
{
    /// <summary>
    /// Initializes the page.
    /// </summary>
    public ColorPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The colors to show.
    /// </summary>
    public ColorViewModel ViewModel { get; } = new();
}
