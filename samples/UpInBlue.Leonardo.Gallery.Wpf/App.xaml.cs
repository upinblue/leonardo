using System.ComponentModel;
using System.Windows;

using UpInBlue.Leonardo.Gallery.Wpf.Services;
using UpInBlue.Leonardo.Gallery.Wpf.ViewModels;
using UpInBlue.Leonardo.Wpf;

namespace UpInBlue.Leonardo.Gallery.Wpf;

/// <summary>
/// The WPF sample of Leonardo: one window with colors, typography and a list with a density switch.
/// </summary>
public partial class App : Application
{
    private GallerySettings? settings;

    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var options = StartupOptions.Current;
        settings = GallerySettings.Load(persist: options.KeepSettings);
        options.ApplyTo(settings);
        LeonardoMotion.AnimationsOverride = options.Animations;

        LeonardoTheme.Apply(this, MainViewModel.ThemeModeOf(settings.Theme));
        settings.PropertyChanged += OnSettingsChanged;

        var window = new MainWindow(new MainViewModel(settings));
        MainWindow = window;
        window.Show();
    }

    private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GallerySettings.Theme) && settings is not null)
        {
            LeonardoTheme.Apply(this, MainViewModel.ThemeModeOf(settings.Theme));
        }
    }
}
