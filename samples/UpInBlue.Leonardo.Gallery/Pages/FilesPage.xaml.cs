using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using UpInBlue.Leonardo.Gallery.Models;
using UpInBlue.Leonardo.Gallery.ViewModels;
using UpInBlue.Leonardo.WinUI;
using UpInBlue.Leonardo.WinUI.Motion;
using Windows.System;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// The Files scenario: folder tree, file list with metadata columns and details pane. Opening a folder dives into it:
/// its name flies up and becomes the place name. Back reverses the motion.
/// </summary>
public sealed partial class FilesPage : Page
{
    /// <summary>The element of the row template that flies when diving in.</summary>
    private const string FlyingElement = "EntryName";

    /// <summary>The folder a back navigation comes from, so that the parent page knows where its name flies back to.</summary>
    private static FolderNode? s_returningFrom;

    /// <summary>The direction of a jump through the tree or the path, which staggers the page in instead of diving.</summary>
    private static MotionDirection? s_jumpDirection;

    private NavigationMode _arrival;

    /// <summary>
    /// Initializes the page.
    /// </summary>
    public FilesPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += (_, _) =>
        {
            ViewModel.PropertyChanged -= OnViewModelChanged;
            ViewModel.FilterRequested -= OnFilterRequested;
        };
    }

    /// <summary>
    /// The state of the page.
    /// </summary>
    public FilesViewModel ViewModel { get; private set; } = new();

    /// <inheritdoc />
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new FilesViewModel(e.Parameter as string);
        _arrival = e.NavigationMode;
    }

    /// <inheritdoc />
    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        if (e.NavigationMode == NavigationMode.Back && e.SourcePageType == typeof(FilesPage))
        {
            s_returningFrom = ViewModel.Folder;
            DiveIn.PrepareBack(PlaceHeader.TitleElement ?? PlaceHeader);
        }
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        FolderTree.SelectedItem = ViewModel.Folder;
        ViewModel.PropertyChanged += OnViewModelChanged;
        ViewModel.FilterRequested += OnFilterRequested;

        var returningFrom = s_returningFrom;
        var jump = s_jumpDirection;
        s_returningFrom = null;
        s_jumpDirection = null;

        if (_arrival == NavigationMode.Back && returningFrom?.Parent == ViewModel.Folder
            && ViewModel.Entries.FirstOrDefault(entry => entry.Folder == returningFrom) is { } entry)
        {
            // Back: the place name flies back into the row it came from; the rest enters from the left.
            ViewModel.SelectedEntry = entry;
            await DiveIn.StartBackAsync(FileList, entry, FlyingElement, [Breadcrumb, PlaceHeader]);
        }
        else if (jump is { } direction)
        {
            Stagger.Enter([Breadcrumb, PlaceHeader, FileList], direction);
        }
        else if (_arrival == NavigationMode.New)
        {
            // Forward: completes a prepared dive; without one (the first visit), nothing moves.
            DiveIn.Start(PlaceHeader.TitleElement ?? PlaceHeader, [Breadcrumb, FileList]);
        }
        else
        {
            DiveIn.Cancel();
        }
    }

    /// <summary>
    /// Opens a subfolder: its name flies up and becomes the place name of the next page.
    /// </summary>
    private void Open(FileItem? entry)
    {
        if (entry?.Folder is not { } folder)
        {
            return;
        }

        FileList.ScrollIntoView(entry);
        FileList.UpdateLayout();
        DiveIn.Prepare(FileList, entry, FlyingElement);
        Frame.Navigate(typeof(FilesPage), folder.Key, new SuppressNavigationTransitionInfo());
    }

    /// <summary>
    /// Jumps to a folder through the tree or the path. Going up enters from the left, anything else from the right.
    /// </summary>
    private void JumpTo(FolderNode folder)
    {
        if (folder == ViewModel.Folder)
        {
            return;
        }

        s_jumpDirection = ViewModel.Folder.Trail.Contains(folder) ? MotionDirection.Back : MotionDirection.Forward;
        DiveIn.Cancel();
        Frame.Navigate(typeof(FilesPage), folder.Key, new SuppressNavigationTransitionInfo());
    }

    private void OnFolderInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is FolderNode folder)
        {
            JumpTo(folder);
        }
    }

    private void OnBreadcrumbClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Item is FolderNode folder)
        {
            JumpTo(folder);
        }
    }

    private void OnFileListDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if ((e.OriginalSource as FrameworkElement)?.DataContext is FileItem entry)
        {
            Open(entry);
        }
    }

    private void OnFileListPreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && ViewModel.SelectedEntry is { IsFolder: true } entry)
        {
            e.Handled = true;
            Open(entry);
        }
    }

    private void OnOpenClicked(object sender, RoutedEventArgs e)
        => Open((sender as FrameworkElement)?.DataContext as FileItem ?? ViewModel.SelectedEntry);

    /// <summary>
    /// Shows the optional columns that fit. The rows have the list padding, the item padding on both sides, the status column and the icon.
    /// </summary>
    private void OnFileListSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var resources = Application.Current.Resources;
        var rowChrome = LeonardoTokens.Space.XS + (2 * LeonardoTokens.Space.XS) + (double)resources["LeonardoStatusDotSize"] + (double)resources["GalleryIconSize"] + LeonardoTokens.Space.XS;
        TableColumns.Files.Fit(e.NewSize.Width - rowChrome, LeonardoTokens.Space.M);
    }

    private void OnFileSelectionChanged(object sender, SelectionChangedEventArgs e)
        => ViewModel.SelectedEntry = FileList.SelectedItem as FileItem;

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FilesViewModel.SelectedEntry) && !ReferenceEquals(FileList.SelectedItem, ViewModel.SelectedEntry))
        {
            FileList.SelectedItem = ViewModel.SelectedEntry;
        }
    }

    /// <summary>
    /// Deletes a file: the row leaves, then the rows below close the gap. Undo brings it back.
    /// </summary>
    private async void OnDeleteClicked(object sender, RoutedEventArgs e)
    {
        var entry = sender is MenuFlyoutItem item ? item.DataContext as FileItem : ViewModel.SelectedEntry;
        if (!ViewModel.CanDelete(entry) || entry is null)
        {
            return;
        }

        if (FileList.ContainerFromItem(entry) is not UIElement container)
        {
            ViewModel.DeleteFile(entry);
            return;
        }

        await Leave.RunAsync(container, () =>
        {
            var reflow = Reflow.Capture(FileList);
            ViewModel.DeleteFile(entry);
            reflow.Play();
        });
        FileList.Focus(FocusState.Programmatic);
    }

    /// <summary>
    /// A phrase of the summary line was turned on or off: the rows that stay glide to their place.
    /// </summary>
    private void OnFilterRequested(object? sender, EventArgs e)
    {
        var reflow = Reflow.Capture(FileList);
        ViewModel.ApplyFilter(explain: true);
        reflow.Play();
    }

    private void OnUndoClicked(object sender, RoutedEventArgs e)
    {
        var reflow = Reflow.Capture(FileList);
        ViewModel.UndoCommand.Execute(null);
        reflow.Play();
    }

    private void OnShareClicked(object sender, RoutedEventArgs e)
        => ViewModel.SampleOnlyCommand.Execute("Sharing");
}
