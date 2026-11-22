using System.Globalization;

namespace UpInBlue.Leonardo.Tokens.Model;

/// <summary>
/// An sRGB color with 8 bits per channel.
/// </summary>
internal readonly record struct RgbColor(byte R, byte G, byte B, byte A)
{
    /// <summary>
    /// The color as <c>#RRGGBB</c>, or <c>#RRGGBBAA</c> if it is not opaque.
    /// </summary>
    public string Hex => A == 255 ? $"#{R:X2}{G:X2}{B:X2}" : $"#{R:X2}{G:X2}{B:X2}{A:X2}";

    /// <summary>
    /// The color as XAML expects it: <c>#AARRGGBB</c>.
    /// </summary>
    public string XamlHex => $"#{A:X2}{R:X2}{G:X2}{B:X2}";

    /// <summary>
    /// The relative luminance as defined by WCAG 2.2.
    /// </summary>
    public double RelativeLuminance => (0.2126 * Linear(R)) + (0.7152 * Linear(G)) + (0.0722 * Linear(B));

    /// <summary>
    /// Parses <c>#RRGGBB</c> or <c>#RRGGBBAA</c>.
    /// </summary>
    public static bool TryParse(string text, out RgbColor color)
    {
        color = default;
        if (text.Length is not (7 or 9) || text[0] != '#')
        {
            return false;
        }

        foreach (var character in text.AsSpan(1))
        {
            if (!char.IsAsciiHexDigit(character))
            {
                return false;
            }
        }

        var value = uint.Parse(text.AsSpan(1), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        color = text.Length == 7
            ? new RgbColor((byte)(value >> 16), (byte)(value >> 8), (byte)value, 255)
            : new RgbColor((byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value);
        return true;
    }

    /// <summary>
    /// The WCAG 2.2 contrast ratio between two opaque colors, from 1 to 21.
    /// </summary>
    public static double ContrastRatio(RgbColor first, RgbColor second)
    {
        var lighter = Math.Max(first.RelativeLuminance, second.RelativeLuminance);
        var darker = Math.Min(first.RelativeLuminance, second.RelativeLuminance);
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double Linear(byte channel)
    {
        var value = channel / 255.0;
        return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
    }
}

/// <summary>
/// A length with its unit, for example 16 px.
/// </summary>
internal readonly record struct Dimension(double Value, string Unit);

/// <summary>
/// A duration in milliseconds.
/// </summary>
internal readonly record struct TokenDuration(double Milliseconds);

/// <summary>
/// The control points of a cubic Bézier easing curve, as in CSS <c>cubic-bezier(x1, y1, x2, y2)</c>.
/// </summary>
internal readonly record struct CubicBezier(double X1, double Y1, double X2, double Y2);
