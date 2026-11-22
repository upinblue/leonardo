using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UpInBlue.Leonardo.Gallery.Helpers;
using UpInBlue.Leonardo.Gallery.ViewModels;
using UpInBlue.Leonardo.WinUI.Motion;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// The Activity scenario: a timeline of events with a summary of what happened since the last visit. New events arrive
/// at the top of today with a short accent bar, the events below make room, and screen readers announce the new event.
/// </summary>
public sealed partial class ActivityPage : Page
{
    /// <summary>
    /// Initializes the page.
    /// </summary>
    public ActivityPage()
    {
        InitializeComponent();
        Loaded += (_, _) => ViewModel.FilterRequested += OnFilterRequested;
        Unloaded += (_, _) => ViewModel.FilterRequested -= OnFilterRequested;
    }

    /// <summary>
    /// The timeline.
    /// </summary>
    public ActivityViewModel ViewModel { get; } = new();

    private ItemsControl[] Lists()
        => [DayList, .. Enumerable.Range(0, ViewModel.Days.Count)
            .Select(index => VisualTree.FindDescendant<ItemsControl>(DayList.ContainerFromIndex(index), "EventList"))
            .OfType<ItemsControl>()];

    private void OnFilterRequested(object? sender, EventArgs e)
    {
        var reflow = Reflow.Capture(Lists());
        ViewModel.ApplyFilter();
        reflow.Play();
    }

    private void OnSimulateEventClicked(object sender, RoutedEventArgs e)
    {
        var today = VisualTree.FindDescendant<ItemsControl>(DayList.ContainerFromIndex(0), "EventList");
        var reflow = Reflow.Capture(Lists());
        var added = ViewModel.AddEvent();
        reflow.Play();

        if (added is not null && today?.ContainerFromItem(added) is UIElement container)
        {
            Arrive.Run(container, VisualTree.FindDescendant<Border>(container, "ArriveMarker"), $"New event: {added.Text}");
        }
    }
}
