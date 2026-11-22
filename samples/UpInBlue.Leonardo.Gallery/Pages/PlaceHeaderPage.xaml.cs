using Microsoft.UI.Xaml.Controls;
using UpInBlue.Leonardo.Gallery.ViewModels;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// The PlaceHeader control. The page's own header is the live example.
/// </summary>
public sealed partial class PlaceHeaderPage : Page
{
    /// <summary>
    /// Initializes the page.
    /// </summary>
    public PlaceHeaderPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The content to scroll through.
    /// </summary>
    public PlaceHeaderViewModel ViewModel { get; } = new();

    /// <summary>
    /// The live state of the header, for example "The header is compact."
    /// </summary>
    public string HeaderState(bool isCompact) => isCompact
        ? "The header is a compact line now. Scroll back to the top to make it large again."
        : "The header is large.";
}
