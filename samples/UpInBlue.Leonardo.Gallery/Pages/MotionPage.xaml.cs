using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using UpInBlue.Leonardo.Gallery.Helpers;
using UpInBlue.Leonardo.Gallery.ViewModels;
using UpInBlue.Leonardo.WinUI.Motion;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// The Motion page: settings to inspect motion, the curves and values, and every motion to try.
/// </summary>
public sealed partial class MotionPage : Page
{
    /// <summary>
    /// Initializes the page.
    /// </summary>
    public MotionPage()
    {
        InitializeComponent();
        Unloaded += (_, _) => ViewModel.Dispose();
    }

    /// <summary>
    /// The state of the page.
    /// </summary>
    public MotionViewModel ViewModel { get; } = new();

    private void OnDemoFolderClicked(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not string folder)
        {
            return;
        }

        DiveIn.Prepare(DiveList, folder, "FolderName");
        ViewModel.OpenFolder = folder;
        DiveHeading.UpdateLayout();
        DiveIn.Start(DiveHeading, [DiveContent, DiveBackButton]);
        DiveBackButton.Focus(FocusState.Programmatic);
    }

    private async void OnDemoBackClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel.OpenFolder is not { } folder)
        {
            return;
        }

        DiveIn.PrepareBack(DiveHeading);
        ViewModel.OpenFolder = null;
        DiveList.UpdateLayout();
        await DiveIn.StartBackAsync(DiveList, folder, "FolderName", []);
        if (DiveList.ContainerFromItem(folder) is Control container)
        {
            container.Focus(FocusState.Programmatic);
        }
    }

    private void OnAddCommentClicked(object sender, RoutedEventArgs e)
    {
        Transform.Run(CommentButton, CommentPanel, () => ViewModel.IsCommenting = true);
        CommentBox.Focus(FocusState.Programmatic);
    }

    private void OnCommentDoneClicked(object sender, RoutedEventArgs e)
    {
        Transform.Run(CommentPanel, CommentButton, () => ViewModel.IsCommenting = false);
        CommentButton.Focus(FocusState.Programmatic);
    }

    private void OnPlayStaggerClicked(object sender, RoutedEventArgs e)
        => Stagger.Enter([StaggerHeading, StaggerSummary, StaggerDetails, StaggerAction]);

    private void OnSortByNameClicked(object sender, RoutedEventArgs e) => Sort(bySize: false);

    private void OnSortBySizeClicked(object sender, RoutedEventArgs e) => Sort(bySize: true);

    private void Sort(bool bySize)
    {
        var selected = ReflowList.SelectedItem;
        var reflow = Reflow.Capture(ReflowList);
        ViewModel.SortFiles(bySize);
        reflow.Play();
        ReflowList.SelectedItem = selected;
    }

    private void OnAddEventClicked(object sender, RoutedEventArgs e)
    {
        var reflow = Reflow.Capture(EventList);
        var added = ViewModel.AddEvent();
        reflow.Play();
        if (EventList.ContainerFromItem(added) is UIElement container)
        {
            Arrive.Run(container, VisualTree.FindDescendant<Ellipse>(container, "ArriveMarker"), $"New event: {added.Text}");
        }
    }

    private async void OnRemoveEventClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel.DemoEvents.Count == 0 || EventList.ContainerFromIndex(0) is not UIElement container)
        {
            return;
        }

        var removed = ViewModel.DemoEvents[0];
        await Leave.RunAsync(container, () =>
        {
            var reflow = Reflow.Capture(EventList);
            ViewModel.DemoEvents.Remove(removed);
            reflow.Play();
        });
    }
}
