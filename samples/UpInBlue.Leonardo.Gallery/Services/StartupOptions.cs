using Microsoft.UI.Xaml;

namespace UpInBlue.Leonardo.Gallery.Services;

/// <summary>
/// Optional command-line options, used to open a page in a given theme, for example for screenshots:
/// <c>UpInBlue.Leonardo.Gallery.exe --page files --theme dark --width 1360 --height 860</c>.
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
                case "--page":
                    Page = value;
                    break;
                case "--theme":
                    Theme = value.ToLowerInvariant() switch
                    {
                        "light" => ElementTheme.Light,
                        "dark" => ElementTheme.Dark,
                        _ => ElementTheme.Default,
                    };
                    break;
                case "--density" when Enum.TryParse<ListDensity>(value, ignoreCase: true, out var density):
                    Density = density;
                    break;
                case "--width" when int.TryParse(value, out var width):
                    Width = width;
                    break;
                case "--height" when int.TryParse(value, out var height):
                    Height = height;
                    break;
            }
        }
    }

    /// <summary>
    /// The options of this process.
    /// </summary>
    public static StartupOptions Current { get; } = new(Environment.GetCommandLineArgs());

    /// <summary>The tag of the page to open first.</summary>
    public string? Page { get; }

    /// <summary>The theme to use instead of the stored one.</summary>
    public ElementTheme? Theme { get; }

    /// <summary>The density to use instead of the stored one.</summary>
    public ListDensity? Density { get; }

    /// <summary>The initial window width in pixels.</summary>
    public int? Width { get; }

    /// <summary>The initial window height in pixels.</summary>
    public int? Height { get; }

    /// <summary>
    /// Whether settings changes are stored. They are not when the gallery was started with options.
    /// </summary>
    public bool KeepSettings => Page is null && Theme is null && Density is null && Width is null && Height is null;

    /// <summary>
    /// Applies the theme and density options to the settings.
    /// </summary>
    public void ApplyTo(AppSettings settings)
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
