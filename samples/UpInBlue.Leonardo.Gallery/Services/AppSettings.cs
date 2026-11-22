using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using UpInBlue.Leonardo.WinUI;

namespace UpInBlue.Leonardo.Gallery.Services;

/// <summary>
/// The row height of lists. Standard is the default; the choice is stored per user.
/// </summary>
public enum ListDensity
{
    /// <summary>Compact rows.</summary>
    Compact,

    /// <summary>Standard rows, the default.</summary>
    Standard,

    /// <summary>Comfortable rows.</summary>
    Comfortable,
}

/// <summary>
/// The settings of the gallery, stored per user in the local application data folder.
/// </summary>
public sealed partial class AppSettings : ObservableObject
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "UpInBlue",
        "Leonardo Gallery",
        "settings.json");

    private bool _persist;

    private AppSettings()
    {
        Theme = ElementTheme.Default;
        Density = ListDensity.Standard;
    }

    /// <summary>
    /// The app theme. <see cref="ElementTheme.Default"/> follows the Windows setting.
    /// </summary>
    [ObservableProperty]
    public partial ElementTheme Theme { get; set; }

    /// <summary>
    /// The list density.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RowHeight))]
    public partial ListDensity Density { get; set; }

    /// <summary>
    /// The row height of lists for the current density, from the density tokens.
    /// </summary>
    public double RowHeight => Density switch
    {
        ListDensity.Compact => LeonardoTokens.Density.Compact,
        ListDensity.Comfortable => LeonardoTokens.Density.Comfortable,
        _ => LeonardoTokens.Density.Standard,
    };

    /// <summary>
    /// Loads the stored settings, or the defaults if there are none or the file cannot be read.
    /// </summary>
    /// <param name="persist">Whether changes are written back to disk.</param>
    public static AppSettings Load(bool persist)
    {
        var settings = new AppSettings();
        try
        {
            if (File.Exists(FilePath))
            {
                var stored = JsonSerializer.Deserialize<StoredSettings>(File.ReadAllText(FilePath));
                if (stored is not null)
                {
                    settings.Theme = Enum.IsDefined(stored.Theme) ? stored.Theme : ElementTheme.Default;
                    settings.Density = Enum.IsDefined(stored.Density) ? stored.Density : ListDensity.Standard;
                }
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            // Unreadable settings fall back to the defaults; the next change overwrites the file.
        }

        settings._persist = persist;
        return settings;
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (_persist && e.PropertyName is nameof(Theme) or nameof(Density))
        {
            Save();
        }
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(new StoredSettings(Theme, Density)));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // The setting still applies for this session.
        }
    }

    private sealed record StoredSettings(ElementTheme Theme, ListDensity Density);
}
