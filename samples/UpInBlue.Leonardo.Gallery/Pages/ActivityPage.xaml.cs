using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using UpInBlue.Leonardo.Gallery.Helpers;
using UpInBlue.Leonardo.Gallery.ViewModels;
using UpInBlue.Leonardo.WinUI.Motion;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// The Activity scenario: a timeline of events. New events arrive at the top of today with a short accent marker,
/// the events below make room, and screen readers announce the new event.
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

    private void OnSimulateEventClicked(object sender, RoutedEventArgs e)
    {
        var today = VisualTree.FindDescendant<ItemsControl>(DayList.ContainerFromIndex(0), "EventList");
        var reflow = today is null ? Reflow.Capture(DayList) : Reflow.Capture(DayList, today);
        var added = ViewModel.AddEvent();
        reflow.Play();

        if (today?.ContainerFromItem(added) is UIElement container)
        {
            Arrive.Run(container, VisualTree.FindDescendant<Ellipse>(container, "ArriveMarker"), $"New event: {added.Text}");
        }
    }
}
