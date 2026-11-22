using Microsoft.UI.Xaml.Controls;
using UpInBlue.Leonardo.Gallery.ViewModels;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// The Activity scenario: a timeline of events.
/// </summary>
public sealed partial class ActivityPage : Page
{
    /// <summary>
    /// Initializes the page.
    /// </summary>
    public ActivityPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The timeline.
    /// </summary>
    public ActivityViewModel ViewModel { get; } = new();
}
