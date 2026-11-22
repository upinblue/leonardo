using System.IO;
using System.Text.Json;

using CommunityToolkit.Mvvm.ComponentModel;

namespace UpInBlue.Leonardo.Gallery.Wpf.Services;

/// <summary>
/// The app theme the user chose.
/// </summary>
public enum AppTheme
{
    /// <summary>Follows the Windows app mode.</summary>
    System,

    /// <summary>Always light.</summary>
    Light,

    /// <summary>Always dark.</summary>
    Dark,
}

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
/// The settings of the sample, stored per user in the local application data folder.
/// </summary>
public sealed partial class GallerySettings : ObservableObject
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "UpInBlue",
        "Leonardo WPF Gallery",
        "settings.json");

    private bool persist;

    private GallerySettings()
    {
        Theme = AppTheme.System;
        Density = ListDensity.Standard;
    }

    /// <summary>
    /// The app theme.
    /// </summary>
    [ObservableProperty]
    public partial AppTheme Theme { get; set; }

    /// <summary>
    /// The list density.
    /// </summary>
    [ObservableProperty]
    public partial ListDensity Density { get; set; }

    /// <summary>
    /// Loads the stored settings, or the defaults if there are none or the file cannot be read.
    /// </summary>
    /// <param name="persist">Whether changes are written back to disk.</param>
    /// <returns>The settings.</returns>
    public static GallerySettings Load(bool persist)
    {
        var settings = new GallerySettings();
        try
        {
            if (File.Exists(FilePath) && JsonSerializer.Deserialize<StoredSettings>(File.ReadAllText(FilePath)) is { } stored)
            {
                settings.Theme = Enum.IsDefined(stored.Theme) ? stored.Theme : AppTheme.System;
                settings.Density = Enum.IsDefined(stored.Density) ? stored.Density : ListDensity.Standard;
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            // Unreadable settings fall back to the defaults; the next change overwrites the file.
        }

        settings.persist = persist;
        return settings;
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (persist && e.PropertyName is nameof(Theme) or nameof(Density))
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

    private sealed record StoredSettings(AppTheme Theme, ListDensity Density);
}
