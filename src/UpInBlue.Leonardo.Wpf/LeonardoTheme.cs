using System.ComponentModel;
using System.Windows;
using Microsoft.Win32;

namespace UpInBlue.Leonardo.Wpf;

/// <summary>
/// The appearance Leonardo resolves the theme mode to.
/// </summary>
public enum LeonardoAppearance
{
    /// <summary>The light theme with Leonardo's light colors and accent.</summary>
    Light,

    /// <summary>The dark theme with Leonardo's dark colors and accent.</summary>
    Dark,

    /// <summary>A Windows contrast theme: system colors only, no Leonardo colors and no accent override.</summary>
    HighContrast,
}

/// <summary>
/// Applies Leonardo on top of the WPF Fluent theme: sets <see cref="Application.ThemeMode"/> and merges the Leonardo
/// dictionary that matches the active theme into <see cref="Application.Resources"/>.
/// </summary>
/// <remarks>
/// <para>
/// The Fluent theme defines <c>SystemAccentColor</c>, <c>SystemAccentColorLight1</c> to <c>Light3</c> and
/// <c>SystemAccentColorDark1</c> to <c>Dark3</c> as aliases of <see cref="SystemColors.AccentColorKey"/> and its siblings, and
/// its accent brushes resolve these aliases with <c>StaticResource</c> when the Fluent dictionary loads. Leonardo therefore
/// defines the <see cref="SystemColors"/> keys in Application resources and then lets WPF rebuild the Fluent dictionary, so
/// every Fluent control uses the Leonardo accent. In a contrast theme the override is left out and the Fluent theme uses
/// system colors.
/// </para>
/// <para>
/// In a contrast theme, the Leonardo text styles set no <c>Foreground</c>, so text inherits the system color of its
/// container, for example of a selected list item (Microsoft, "Contrast themes"). Reference the text styles with
/// <c>DynamicResource</c>, so that they follow when the dictionary is swapped.
/// </para>
/// <para>
/// Light and dark colors live in separate dictionaries, <c>Themes/Leonardo.Light.xaml</c>, <c>Themes/Leonardo.Dark.xaml</c>
/// and <c>Themes/Leonardo.HighContrast.xaml</c>. With <see cref="ThemeMode.System"/>, Leonardo follows the Windows app mode
/// and contrast themes while the app runs. Call the members on the UI thread.
/// </para>
/// </remarks>
public static class LeonardoTheme
{
    private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string AppsUseLightThemeValue = "AppsUseLightTheme";
    private const string DictionaryPrefix = "pack://application:,,,/UpInBlue.Leonardo.Wpf;component/Themes/Leonardo.";

    private static Application? application;

    /// <summary>
    /// Occurs after Leonardo merged a different dictionary, because the theme mode, the Windows app mode or a contrast theme changed.
    /// </summary>
    public static event EventHandler? AppearanceChanged;

    /// <summary>
    /// Gets the appearance of the merged Leonardo dictionary, or <see langword="null"/> before <see cref="Apply"/> was called.
    /// </summary>
    public static LeonardoAppearance? Appearance { get; private set; }

    /// <summary>
    /// Sets the theme mode of the application and merges the matching Leonardo dictionary. Call it again to change the mode.
    /// </summary>
    /// <param name="app">The application, usually <see cref="Application.Current"/>.</param>
    /// <param name="mode">
    /// <see cref="ThemeMode.Light"/>, <see cref="ThemeMode.Dark"/> or <see cref="ThemeMode.System"/>. Leonardo builds on the
    /// Fluent theme, so <see cref="ThemeMode.None"/> is not supported.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="mode"/> is <see cref="ThemeMode.None"/>.</exception>
    public static void Apply(Application app, ThemeMode mode)
    {
        ArgumentNullException.ThrowIfNull(app);
        if (mode == ThemeMode.None)
        {
            throw new ArgumentException("Leonardo builds on the WPF Fluent theme. Use Light, Dark or System.", nameof(mode));
        }

        if (application is null)
        {
            SystemParameters.StaticPropertyChanged += OnSystemParameterChanged;
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }

        application = app;
        app.ThemeMode = mode;
        Refresh(rebuildFluent: true);
    }

    /// <summary>
    /// The appearance a theme mode resolves to under the current Windows settings.
    /// </summary>
    /// <param name="mode">The theme mode.</param>
    /// <returns><see cref="LeonardoAppearance.HighContrast"/> when a contrast theme is on, otherwise light or dark.</returns>
    public static LeonardoAppearance Resolve(ThemeMode mode)
    {
        if (SystemParameters.HighContrast)
        {
            return LeonardoAppearance.HighContrast;
        }

        if (mode == ThemeMode.Dark)
        {
            return LeonardoAppearance.Dark;
        }

        return mode == ThemeMode.Light || SystemUsesLightTheme() ? LeonardoAppearance.Light : LeonardoAppearance.Dark;
    }

    private static void Refresh(bool rebuildFluent)
    {
        if (application is not { } app)
        {
            return;
        }

        var appearance = Resolve(app.ThemeMode);
        var dictionaries = app.Resources.MergedDictionaries;
        var current = dictionaries.FirstOrDefault(IsLeonardoDictionary);
        var changed = appearance != Appearance || current is null;
        if (changed)
        {
            if (current is not null)
            {
                dictionaries.Remove(current);
            }

            // Added after the Fluent dictionary, which WPF keeps at the start, so Leonardo resources win over Fluent ones.
            dictionaries.Add(new ResourceDictionary { Source = new Uri(DictionaryPrefix + appearance + ".xaml", UriKind.Absolute) });
        }

        // The Fluent dictionary resolved the accent when it loaded. When the accent override came or went since then, for
        // example because a contrast theme was turned on, WPF must load it again: switching the mode away and back does that.
        var accentChanged = Appearance is { } before
            && (before == LeonardoAppearance.HighContrast) != (appearance == LeonardoAppearance.HighContrast);
        if (rebuildFluent || accentChanged)
        {
            var mode = app.ThemeMode;
            app.ThemeMode = mode == ThemeMode.Dark ? ThemeMode.Light : ThemeMode.Dark;
            app.ThemeMode = mode;
        }

        Appearance = appearance;
        if (changed)
        {
            AppearanceChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    private static bool IsLeonardoDictionary(ResourceDictionary dictionary)
        => dictionary.Source?.OriginalString.StartsWith(DictionaryPrefix, StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>
    /// The Windows app mode, the same registry value WPF reads for <see cref="ThemeMode.System"/>. Light when it is missing.
    /// </summary>
    private static bool SystemUsesLightTheme()
    {
        using var key = Registry.CurrentUser.OpenSubKey(PersonalizeKey);
        return key?.GetValue(AppsUseLightThemeValue) is not int value || value != 0;
    }

    private static void OnSystemParameterChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SystemParameters.HighContrast))
        {
            RefreshOnUIThread();
        }
    }

    private static void OnUserPreferenceChanged(object? sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category is UserPreferenceCategory.General or UserPreferenceCategory.Color or UserPreferenceCategory.Accessibility)
        {
            RefreshOnUIThread();
        }
    }

    private static void RefreshOnUIThread()
    {
        if (application?.Dispatcher is { } dispatcher)
        {
            // After WPF has applied the change to its own Fluent dictionaries.
            _ = dispatcher.BeginInvoke(() => Refresh(rebuildFluent: false), System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}
