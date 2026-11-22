using System.Globalization;
using System.Xml.Linq;

namespace UpInBlue.Leonardo.Tokens.Tests;

/// <summary>
/// Proves that WPF and WinUI get the same Leonardo values: every resource key and value in the committed WinUI
/// dictionaries exists with the same value in the WPF dictionaries, for light, dark and high contrast.
/// </summary>
public sealed class WpfWinUIParityTests
{
    private const string WinUITokens = "src/UpInBlue.Leonardo.WinUI/Themes/Generated/Tokens.xaml";
    private const string WinUIAccent = "src/UpInBlue.Leonardo.WinUI/Themes/Generated/AccentColors.xaml";
    private const string WinUITextStyles = "src/UpInBlue.Leonardo.WinUI/Themes/Generated/TextStyles.xaml";
    private const string WpfThemes = "src/UpInBlue.Leonardo.Wpf/Themes/Generated/";

    private static readonly XNamespace X = "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly XNamespace Presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

    /// <summary>
    /// WPF adds durations as <c>KeyTime</c> and <c>TimeSpan</c>, which WinUI covers with one string resource.
    /// </summary>
    private static readonly string[] WpfOnlySuffixes = ["KeyTime", "TimeSpan"];

    [Theory]
    [InlineData("Light")]
    [InlineData("Dark")]
    public void ThemeColorsAreTheSame(string theme)
    {
        var winUI = Resources(ThemeDictionary(Load(WinUITokens), theme));
        var wpf = Resources(Load(WpfThemes + $"Tokens.{theme}.xaml").Root!);

        Assert.NotEmpty(winUI);
        foreach (var (key, value) in winUI)
        {
            Assert.True(wpf.TryGetValue(key, out var wpfValue), $"{theme}: WPF lacks {key}.");
            Assert.Equal(value, wpfValue);
        }
    }

    [Theory]
    [InlineData("Light")]
    [InlineData("Dark")]
    [InlineData("HighContrast")]
    public void SharedValuesAreTheSame(string theme)
    {
        var winUIRoot = Load(WinUITokens).Root!;
        var winUI = Resources(winUIRoot);
        var wpf = Resources(Load(WpfThemes + $"Tokens.{theme}.xaml").Root!);
        var wpfColors = theme == "HighContrast"
            ? Resources(ThemeDictionary(Load(WinUITokens), "HighContrast")).Keys
            : Resources(ThemeDictionary(Load(WinUITokens), theme)).Keys;

        Assert.NotEmpty(winUI);
        foreach (var (key, value) in winUI)
        {
            Assert.True(wpf.TryGetValue(key, out var wpfValue), $"{theme}: WPF lacks {key}.");
            Assert.Equal(value, wpfValue);
        }

        var extra = wpf.Keys
            .Except(winUI.Keys)
            .Except(wpfColors)
            .Where(key => !WpfOnlySuffixes.Any(suffix => key.EndsWith(suffix, StringComparison.Ordinal)));
        Assert.Empty(extra);
    }

    [Fact]
    public void WpfOnlyDurationsMatchTheirDuration()
    {
        var wpf = Resources(Load(WpfThemes + "Tokens.Light.xaml").Root!);
        foreach (var (key, value) in wpf.Where(pair => WpfOnlySuffixes.Any(suffix => pair.Key.EndsWith(suffix, StringComparison.Ordinal))))
        {
            var durationKey = WpfOnlySuffixes.Aggregate(key, (current, suffix) => current.EndsWith(suffix, StringComparison.Ordinal) ? current[..^suffix.Length] : current);
            Assert.Equal(wpf[durationKey], value);
        }

        Assert.Contains("LeonardoStaggerStepTimeSpan", wpf.Keys);
    }

    [Fact]
    public void HighContrastUsesTheSameSystemColors()
    {
        var winUI = ThemeDictionary(Load(WinUITokens), "HighContrast")
            .Elements()
            .Where(element => element.Name.LocalName == "StaticResource")
            .ToDictionary(element => (string)element.Attribute(X + "Key")!, element => (string)element.Attribute("ResourceKey")!);
        var wpf = Load(WpfThemes + "Tokens.HighContrast.xaml").Root!
            .Elements(X + "Static")
            .ToDictionary(element => (string)element.Attribute(X + "Key")!, element => (string)element.Attribute("Member")!);

        Assert.NotEmpty(winUI);
        Assert.Equal(winUI.Keys.Order(StringComparer.Ordinal), wpf.Keys.Order(StringComparer.Ordinal));
        foreach (var (key, systemColor) in winUI)
        {
            var name = systemColor["SystemColor".Length..^"Color".Length];
            var expected = $"SystemColors.{(name == "Hotlight" ? "HotTrack" : name)}Color";
            Assert.Equal(expected, wpf[key]);
        }
    }

    [Fact]
    public void AccentRampIsTheSame()
    {
        var winUI = Load(WinUIAccent).Root!.Elements(Presentation + "Color")
            .ToDictionary(element => (string)element.Attribute(X + "Key")!, element => element.Value);
        var wpf = Load(WpfThemes + "AccentColors.xaml").Root!.Elements(Presentation + "Color")
            .ToDictionary(element => SystemAccentKey((string)element.Attribute(X + "Key")!), element => element.Value);

        Assert.Equal(7, winUI.Count);
        Assert.Equal(winUI.OrderBy(pair => pair.Key, StringComparer.Ordinal), wpf.OrderBy(pair => pair.Key, StringComparer.Ordinal));
    }

    [Theory]
    [InlineData("Default", "TextStyles.xaml")]
    [InlineData("HighContrast", "TextStyles.HighContrast.xaml")]
    public void TextStylesAreTheSame(string winUITheme, string wpfFile)
    {
        var winUI = Styles(ThemeDictionary(Load(WinUITextStyles), winUITheme));
        var wpf = Styles(Load(WpfThemes + wpfFile).Root!);

        Assert.Equal(8, winUI.Count);
        Assert.Equal(winUI.Keys.Order(StringComparer.Ordinal), wpf.Keys.Order(StringComparer.Ordinal));
        foreach (var (key, setters) in winUI)
        {
            foreach (var (property, value) in setters)
            {
                Assert.True(wpf[key].TryGetValue(property, out var wpfValue), $"{key}: WPF lacks the setter {property}.");
                Assert.Equal(value, wpfValue);
            }

            Assert.Equal(setters.ContainsKey("Foreground"), wpf[key].ContainsKey("Foreground"));
        }
    }

    /// <summary>
    /// Microsoft, "Contrast themes", list items with colored text: in contrast themes, text styles must not set
    /// <c>Foreground</c>, so that text in a selected list item inherits HighlightText. In light and dark they set it.
    /// </summary>
    [Fact]
    public void TextStylesLeaveTheForegroundToContrastThemes()
    {
        var winUI = Load(WinUITextStyles);
        Assert.Empty(winUI.Root!.Elements(Presentation + "Style"));
        Assert.All(Styles(ThemeDictionary(winUI, "Default")).Values, setters => Assert.True(setters.ContainsKey("Foreground")));
        Assert.All(Styles(ThemeDictionary(winUI, "HighContrast")).Values, setters => Assert.False(setters.ContainsKey("Foreground")));
        Assert.All(Styles(Load(WpfThemes + "TextStyles.xaml").Root!).Values, setters => Assert.True(setters.ContainsKey("Foreground")));
        Assert.All(Styles(Load(WpfThemes + "TextStyles.HighContrast.xaml").Root!).Values, setters => Assert.False(setters.ContainsKey("Foreground")));
    }

    private static XDocument Load(string path) => XDocument.Load(Repository.PathOf(path));

    private static XElement ThemeDictionary(XDocument document, string theme)
        => document.Root!
            .Element(Presentation + "ResourceDictionary.ThemeDictionaries")!
            .Elements(Presentation + "ResourceDictionary")
            .Single(element => (string?)element.Attribute(X + "Key") == theme);

    /// <summary>
    /// The resources directly inside a dictionary, keyed by <c>x:Key</c>, with values normalized across the platforms.
    /// </summary>
    private static Dictionary<string, string> Resources(XElement dictionary)
        => dictionary.Elements()
            .Where(element => element.Attribute(X + "Key") is not null)
            .ToDictionary(element => (string)element.Attribute(X + "Key")!, Normalize, StringComparer.Ordinal);

    private static string Normalize(XElement element) => element.Name.LocalName switch
    {
        "SolidColorBrush" when element.Attribute("Color") is { } color => "brush " + color.Value,
        "SolidColorBrush" => "brush",
        "StaticResource" or "Static" => "system color",
        "Double" or "Int32" => double.Parse(element.Value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
        _ => element.Value.Trim(),
    };

    /// <summary>
    /// The setters of every style, with resource references reduced to their key and font weights to their OpenType value.
    /// </summary>
    private static Dictionary<string, Dictionary<string, string>> Styles(XElement dictionary)
        => dictionary.Elements(Presentation + "Style").ToDictionary(
            style => (string)style.Attribute(X + "Key")!,
            style => style.Elements(Presentation + "Setter").ToDictionary(
                setter => (string)setter.Attribute("Property")!,
                setter => SetterValue((string)setter.Attribute("Property")!, (string)setter.Attribute("Value")!)),
            StringComparer.Ordinal);

    private static string SetterValue(string property, string value)
    {
        foreach (var prefix in new[] { "{StaticResource ", "{DynamicResource ", "{ThemeResource " })
        {
            if (value.StartsWith(prefix, StringComparison.Ordinal))
            {
                return "resource " + value[prefix.Length..^1];
            }
        }

        return property == "FontWeight" ? Weight(value) : value;
    }

    private static string Weight(string value) => value switch
    {
        "Thin" => "100",
        "ExtraLight" => "200",
        "Light" => "300",
        "SemiLight" => "350",
        "Normal" => "400",
        "Medium" => "500",
        "SemiBold" => "600",
        "Bold" => "700",
        "ExtraBold" => "800",
        "Black" => "900",
        _ => value,
    };

    private static string SystemAccentKey(string wpfKey)
    {
        const string Prefix = "{x:Static SystemColors.";
        Assert.StartsWith(Prefix, wpfKey, StringComparison.Ordinal);
        Assert.EndsWith("Key}", wpfKey, StringComparison.Ordinal);
        return "System" + wpfKey[Prefix.Length..^"Key}".Length];
    }
}
