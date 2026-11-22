using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;

namespace UpInBlue.Leonardo.Gallery.ViewModels;

/// <summary>
/// Which optional columns of the file table fit into the width of the list. The column header and the rows bind to
/// the same instance, so they always show the same columns. Columns that do not fit are hidden instead of clipped.
/// </summary>
public sealed partial class TableColumns : ObservableObject
{
    /// <summary>The columns of the file table. Bound from item templates, which cannot reach the page.</summary>
    public static TableColumns Files { get; } = new();

    /// <summary>The width of the date column; zero when hidden.</summary>
    [ObservableProperty]
    public partial GridLength DateWidth { get; private set; } = Resource("GalleryDateColumnWidth");

    /// <summary>The width of the person column; zero when hidden.</summary>
    [ObservableProperty]
    public partial GridLength PersonWidth { get; private set; } = Resource("GalleryPersonColumnWidth");

    /// <summary>Whether the date column is shown.</summary>
    [ObservableProperty]
    public partial Visibility DateVisibility { get; private set; }

    /// <summary>Whether the person column is shown.</summary>
    [ObservableProperty]
    public partial Visibility PersonVisibility { get; private set; }

    /// <summary>
    /// Shows the optional columns that fit next to the name column.
    /// </summary>
    /// <param name="available">The width for the columns, without the padding of the rows.</param>
    /// <param name="spacing">The spacing between columns.</param>
    public void Fit(double available, double spacing)
    {
        var date = Resource("GalleryDateColumnWidth").Value + spacing;
        var person = Resource("GalleryPersonColumnWidth").Value + spacing;
        var fixedWidth = Resource("GalleryNameColumnMinWidth").Value + Resource("GalleryNumberColumnWidth").Value + spacing;

        var showDate = available >= fixedWidth + date;
        var showPerson = available >= fixedWidth + date + person;

        DateWidth = showDate ? Resource("GalleryDateColumnWidth") : new GridLength(0);
        PersonWidth = showPerson ? Resource("GalleryPersonColumnWidth") : new GridLength(0);
        DateVisibility = showDate ? Visibility.Visible : Visibility.Collapsed;
        PersonVisibility = showPerson ? Visibility.Visible : Visibility.Collapsed;
    }

    private static GridLength Resource(string key) => (GridLength)Application.Current.Resources[key];
}
