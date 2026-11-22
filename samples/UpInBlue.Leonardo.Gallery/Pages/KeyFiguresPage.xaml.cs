using Microsoft.UI.Xaml.Controls;
using UpInBlue.Leonardo.Gallery.ViewModels;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// The Key figures scenario: figures expressed typographically and one small chart.
/// </summary>
public sealed partial class KeyFiguresPage : Page
{
    /// <summary>
    /// Initializes the page.
    /// </summary>
    public KeyFiguresPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The figures and the chart.
    /// </summary>
    public KeyFiguresViewModel ViewModel { get; } = new();
}
