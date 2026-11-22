using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UpInBlue.Leonardo.Gallery.Models;
using UpInBlue.Leonardo.Gallery.ViewModels;
using UpInBlue.Leonardo.WinUI;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// The Files scenario: folder tree, file list with metadata columns and details pane.
/// </summary>
public sealed partial class FilesPage : Page
{
    /// <summary>
    /// Initializes the page.
    /// </summary>
    public FilesPage()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            FolderTree.SelectedItem = ViewModel.SelectedFolder;
            ViewModel.PropertyChanged += OnViewModelChanged;
        };
        Unloaded += (_, _) => ViewModel.PropertyChanged -= OnViewModelChanged;
    }

    /// <summary>
    /// The state of the page.
    /// </summary>
    public FilesViewModel ViewModel { get; } = new();

    private void OnFolderInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is FolderNode folder)
        {
            ViewModel.SelectedFolder = folder;
        }
    }

    private void OnBreadcrumbClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Item is FolderNode folder)
        {
            ViewModel.SelectedFolder = folder;
        }
    }

    /// <summary>
    /// Shows the optional columns that fit. The rows have the list padding, the item padding on both sides and the icon.
    /// </summary>
    private void OnFileListSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var rowChrome = LeonardoTokens.Space.XS + (2 * LeonardoTokens.Space.M) + (double)Application.Current.Resources["GalleryIconSize"] + LeonardoTokens.Space.XS;
        TableColumns.Files.Fit(e.NewSize.Width - rowChrome, LeonardoTokens.Space.M);
    }

    private void OnFileSelectionChanged(object sender, SelectionChangedEventArgs e)
        => ViewModel.SelectedFile = FileList.SelectedItem as FileItem;

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(FilesViewModel.SelectedFolder):
                FolderTree.SelectedItem = ViewModel.SelectedFolder;
                break;
            case nameof(FilesViewModel.SelectedFile) when !ReferenceEquals(FileList.SelectedItem, ViewModel.SelectedFile):
                FileList.SelectedItem = ViewModel.SelectedFile;
                break;
        }
    }

    private void OnDeleteClicked(object sender, RoutedEventArgs e)
        => ViewModel.DeleteFile((sender as FrameworkElement)?.DataContext as FileItem);

    private void OnShareClicked(object sender, RoutedEventArgs e)
        => ViewModel.SampleOnlyCommand.Execute("Sharing");
}
