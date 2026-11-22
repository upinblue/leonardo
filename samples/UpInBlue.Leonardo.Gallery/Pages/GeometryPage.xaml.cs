using Microsoft.UI.Xaml.Controls;
using UpInBlue.Leonardo.Gallery.ViewModels;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// Spacing, the alignment line, corner radii with the concentric rule, and list densities.
/// </summary>
public sealed partial class GeometryPage : Page
{
    /// <summary>
    /// Initializes the page.
    /// </summary>
    public GeometryPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The values to show.
    /// </summary>
    public GeometryViewModel ViewModel { get; } = new();
}
