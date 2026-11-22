using Microsoft.UI.Xaml;
using UpInBlue.Leonardo.Gallery.Services;

namespace UpInBlue.Leonardo.Gallery;

/// <summary>
/// The Leonardo gallery application.
/// </summary>
public partial class App : Application
{
    private MainWindow? _window;

    /// <summary>
    /// Initializes the application.
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The user's settings: theme and list density.
    /// </summary>
    public static AppSettings Settings { get; } = AppSettings.Load(StartupOptions.Current.KeepSettings);

    /// <summary>
    /// The live state of the Windows settings Leonardo falls back on. Set by the main window before the first page opens.
    /// </summary>
    public static SystemState SystemSettings { get; internal set; } = null!;

    /// <inheritdoc />
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        StartupOptions.Current.ApplyTo(Settings);
        _window = new MainWindow();
        _window.Activate();
    }
}
