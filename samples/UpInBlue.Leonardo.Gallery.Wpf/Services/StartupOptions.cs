namespace UpInBlue.Leonardo.Gallery.Wpf.Services;

/// <summary>
/// Optional command-line options, for example to check both motion paths without changing Windows settings:
/// <c>UpInBlue.Leonardo.Gallery.Wpf.exe --theme dark --density compact --animations off</c>.
/// With any of these options, settings are not stored.
/// </summary>
public sealed class StartupOptions
{
    private StartupOptions(string[] args)
    {
        for (var index = 1; index + 1 < args.Length; index += 2)
        {
            var value = args[index + 1];
            switch (args[index])
            {
                case "--theme" when Enum.TryParse<AppTheme>(value, ignoreCase: true, out var theme):
                    Theme = theme;
                    break;
                case "--density" when Enum.TryParse<ListDensity>(value, ignoreCase: true, out var density):
                    Density = density;
                    break;
                case "--animations":
                    Animations = value.ToLowerInvariant() switch
                    {
                        "on" => true,
                        "off" => false,
                        _ => null,
                    };
                    break;
            }
        }
    }

    /// <summary>
    /// The options of this process.
    /// </summary>
    public static StartupOptions Current { get; } = new(Environment.GetCommandLineArgs());

    /// <summary>The theme to use instead of the stored one.</summary>
    public AppTheme? Theme { get; }

    /// <summary>The density to use instead of the stored one.</summary>
    public ListDensity? Density { get; }

    /// <summary>Replaces the Windows animation setting, to test both motion paths.</summary>
    public bool? Animations { get; }

    /// <summary>
    /// Whether settings changes are stored. They are not when the sample was started with options.
    /// </summary>
    public bool KeepSettings => Theme is null && Density is null && Animations is null;

    /// <summary>
    /// Applies the theme and density options to the settings.
    /// </summary>
    /// <param name="settings">The settings to change.</param>
    public void ApplyTo(GallerySettings settings)
    {
        if (Theme is { } theme)
        {
            settings.Theme = theme;
        }

        if (Density is { } density)
        {
            settings.Density = density;
        }
    }
}
