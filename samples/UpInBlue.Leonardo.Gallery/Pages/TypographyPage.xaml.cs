using Microsoft.UI.Xaml.Controls;
using UpInBlue.Leonardo.Gallery.ViewModels;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// The type roles with sample text, wrapping of long German words and tabular figures.
/// </summary>
public sealed partial class TypographyPage : Page
{
    /// <summary>
    /// Initializes the page.
    /// </summary>
    public TypographyPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The role descriptions and sample numbers.
    /// </summary>
    public TypographyViewModel ViewModel { get; } = new();
}
