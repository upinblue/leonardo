using Microsoft.UI.Xaml.Controls;
using UpInBlue.Leonardo.Gallery.ViewModels;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// The StatusDot control: its three kinds, its use in a list and its rules.
/// </summary>
public sealed partial class StatusDotPage : Page
{
    /// <summary>
    /// Initializes the page.
    /// </summary>
    public StatusDotPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The samples.
    /// </summary>
    public StatusDotViewModel ViewModel { get; } = new();
}
