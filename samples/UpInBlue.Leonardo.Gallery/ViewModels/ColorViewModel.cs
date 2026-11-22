using System.Globalization;
using Microsoft.UI.Xaml.Media;
using UpInBlue.Leonardo.WinUI;
using Windows.UI;

namespace UpInBlue.Leonardo.Gallery.ViewModels;

/// <summary>
/// A color of the blue scale.
/// </summary>
public sealed record PaletteSwatch(string Name, string Hex, Brush Brush, string Use);

/// <summary>
/// A theme color in light and dark with its contrast against the color it is used with.
/// </summary>
public sealed record ThemeColorRow(
    string Name,
    string Use,
    Brush LightBrush,
    string LightHex,
    string LightContrast,
    Brush DarkBrush,
    string DarkHex,
    string DarkContrast)
{
    /// <summary>The text a screen reader reads for the row.</summary>
    public string AccessibleName => $"{Name}. Light {LightHex}, {LightContrast}. Dark {DarkHex}, {DarkContrast}.";
}

/// <summary>
/// The colors of Leonardo, computed from the generated token values.
/// </summary>
public sealed class ColorViewModel
{
    private const string OnSurface = "surface";

    /// <summary>The blue scale, from near to far.</summary>
    public IReadOnlyList<PaletteSwatch> Palette { get; } =
    [
        Swatch("depth", LeonardoTokens.Palette.Depth, "Dark surfaces, text"),
        Swatch("near", LeonardoTokens.Palette.Near, "Links and blue text"),
        Swatch("brand", LeonardoTokens.Palette.Brand, "Logo, accent, buttons"),
        Swatch("far", LeonardoTokens.Palette.Far, "Lines, charts, accent on dark ground"),
        Swatch("haze", LeonardoTokens.Palette.Haze, "Selection, surfaces"),
        Swatch("sky", LeonardoTokens.Palette.Sky, "Background"),
    ];

    /// <summary>The theme colors in light and dark.</summary>
    public IReadOnlyList<ThemeColorRow> ThemeColors { get; } =
    [
        Row("accent", "Act here: primary action, selection", L => L.Accent, D => D.Accent, OnSurface),
        Row("onAccent", "Text and icons on the accent", L => L.OnAccent, D => D.OnAccent, "accent"),
        Row("link", "Leads somewhere: links in running text", L => L.Link, D => D.Link, OnSurface),
        Row("selection", "Selected rows and items", L => L.Selection, D => D.Selection, "textPrimary"),
        Row("surface", "Opaque content surface", L => L.Surface, D => D.Surface, "textPrimary"),
        Row("surfaceAlt", "Alternative surface", L => L.SurfaceAlt, D => D.SurfaceAlt, "textPrimary"),
        Row("textPrimary", "Text", L => L.TextPrimary, D => D.TextPrimary, OnSurface),
        Row("textSecondary", "Secondary text", L => L.TextSecondary, D => D.TextSecondary, OnSurface),
        Row("textTertiary", "Metadata", L => L.TextTertiary, D => D.TextTertiary, OnSurface),
        Row("strokeSubtle", "Decorative separators only", L => L.StrokeSubtle, D => D.StrokeSubtle, OnSurface),
        Row("strokeStrong", "Boundaries of interactive elements", L => L.StrokeStrong, D => D.StrokeStrong, OnSurface),
        Row("chart", "The one blue of a single-series chart", L => L.Chart, D => D.Chart, OnSurface),
    ];

    private static PaletteSwatch Swatch(string name, Color color, string use) => new(name, Hex(color), new SolidColorBrush(color), use);

    private static ThemeColorRow Row(string name, string use, Func<Theme, Color> light, Func<Theme, Color> dark, string against)
    {
        var lightColor = light(Theme.Light);
        var darkColor = dark(Theme.Dark);
        var lightAgainst = Lookup(Theme.Light, against);
        var darkAgainst = Lookup(Theme.Dark, against);
        var label = against == "textPrimary" ? "with textPrimary" : $"on {against}";
        return new ThemeColorRow(
            name,
            use,
            new SolidColorBrush(lightColor),
            Hex(lightColor),
            Ratio(Contrast(lightColor, lightAgainst), label),
            new SolidColorBrush(darkColor),
            Hex(darkColor),
            Ratio(Contrast(darkColor, darkAgainst), label));
    }

    private static string Ratio(double ratio, string label)
        => string.Create(CultureInfo.GetCultureInfo("en-US"), $"{ratio:0.0}:1 {label}");

    private static Color Lookup(Theme theme, string name) => name switch
    {
        "accent" => theme.Accent,
        "textPrimary" => theme.TextPrimary,
        _ => theme.Surface,
    };

    private static string Hex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    /// <summary>
    /// The WCAG 2.2 contrast ratio.
    /// </summary>
    private static double Contrast(Color first, Color second)
    {
        static double Channel(byte value)
        {
            var c = value / 255.0;
            return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        static double Luminance(Color c) => (0.2126 * Channel(c.R)) + (0.7152 * Channel(c.G)) + (0.0722 * Channel(c.B));

        var a = Luminance(first);
        var b = Luminance(second);
        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }

    /// <summary>
    /// The theme colors of one theme, read from the generated tokens.
    /// </summary>
    private sealed record Theme(
        Color Accent,
        Color OnAccent,
        Color Link,
        Color Selection,
        Color Surface,
        Color SurfaceAlt,
        Color TextPrimary,
        Color TextSecondary,
        Color TextTertiary,
        Color StrokeSubtle,
        Color StrokeStrong,
        Color Chart)
    {
        public static Theme Light { get; } = new(
            LeonardoTokens.Light.Accent,
            LeonardoTokens.Light.OnAccent,
            LeonardoTokens.Light.Link,
            LeonardoTokens.Light.Selection,
            LeonardoTokens.Light.Surface,
            LeonardoTokens.Light.SurfaceAlt,
            LeonardoTokens.Light.TextPrimary,
            LeonardoTokens.Light.TextSecondary,
            LeonardoTokens.Light.TextTertiary,
            LeonardoTokens.Light.StrokeSubtle,
            LeonardoTokens.Light.StrokeStrong,
            LeonardoTokens.Light.Chart);

        public static Theme Dark { get; } = new(
            LeonardoTokens.Dark.Accent,
            LeonardoTokens.Dark.OnAccent,
            LeonardoTokens.Dark.Link,
            LeonardoTokens.Dark.Selection,
            LeonardoTokens.Dark.Surface,
            LeonardoTokens.Dark.SurfaceAlt,
            LeonardoTokens.Dark.TextPrimary,
            LeonardoTokens.Dark.TextSecondary,
            LeonardoTokens.Dark.TextTertiary,
            LeonardoTokens.Dark.StrokeSubtle,
            LeonardoTokens.Dark.StrokeStrong,
            LeonardoTokens.Dark.Chart);
    }
}
