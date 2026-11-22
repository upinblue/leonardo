using System.ComponentModel;
using System.Windows;

using UpInBlue.Leonardo.Gallery.Wpf.ViewModels;
using UpInBlue.Leonardo.Wpf;

namespace UpInBlue.Leonardo.Gallery.Wpf;

/// <summary>
/// The only window of the sample.
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    public MainWindow(MainViewModel viewModel)
    {
        this.viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
        ApplyRowHeight();
        viewModel.PropertyChanged += OnViewModelChanged;
    }

    /// <summary>
    /// Stagger: the four groups of the window enter in reading order. Only on request: the window appears without an
    /// entrance when the app starts, because Leonardo staggers on navigation, never at launch.
    /// </summary>
    private void PlayEntrance() => LeonardoMotion.Stagger([HeaderGroup, ColorGroup, TypographyGroup, DensityGroup]);

    private void OnReplayEntrance(object sender, RoutedEventArgs e) => PlayEntrance();

    private void OnHyperlinkClick(object sender, RoutedEventArgs e)
    {
        // The sample has no contract to open; the link only shows the link color.
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.RowHeightKey))
        {
            ApplyRowHeight();
        }
    }

    /// <summary>
    /// Points the row height of the list at the density resource of the current setting.
    /// </summary>
    private void ApplyRowHeight() => ContractsList.Resources["GalleryRowHeight"] = FindResource(viewModel.RowHeightKey);
}
