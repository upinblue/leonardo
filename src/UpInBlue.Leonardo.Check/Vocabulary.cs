using System.Globalization;

namespace UpInBlue.Leonardo.Check;

/// <summary>
/// Names and values the checkers need to recognize literals and to suggest the matching Leonardo value.
/// The spacing and radius steps mirror <c>space.*</c> and <c>radius.*</c> in <c>tokens/leonardo.tokens.json</c>;
/// a test keeps them in sync.
/// </summary>
internal static class Vocabulary
{
    /// <summary>Spacing tokens by value: <c>space.xs</c> to <c>space.xl</c>.</summary>
    public static readonly IReadOnlyDictionary<double, string> Spacing = new Dictionary<double, string>
    {
        [4] = "XS",
        [8] = "S",
        [16] = "M",
        [24] = "L",
        [40] = "XL",
    };

    /// <summary>Radius tokens by value: <c>radius.s</c> to <c>radius.l</c>.</summary>
    public static readonly IReadOnlyDictionary<double, string> Radius = new Dictionary<double, string>
    {
        [4] = "S",
        [8] = "M",
        [16] = "L",
    };

    /// <summary>
    /// Color names of XAML, WPF, SwiftUI, UIKit, AppKit and CSS that name a fixed color. <c>Transparent</c> and the
    /// semantic system colors are not included: they are not design values.
    /// </summary>
    public static readonly IReadOnlySet<string> NamedColors = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "AliceBlue", "AntiqueWhite", "Aqua", "Aquamarine", "Azure", "Beige", "Bisque", "Black", "BlanchedAlmond", "Blue",
        "BlueViolet", "Brown", "BurlyWood", "CadetBlue", "Chartreuse", "Chocolate", "Coral", "CornflowerBlue", "Cornsilk",
        "Crimson", "Cyan", "DarkBlue", "DarkCyan", "DarkGoldenrod", "DarkGray", "DarkGrey", "DarkGreen", "DarkKhaki",
        "DarkMagenta", "DarkOliveGreen", "DarkOrange", "DarkOrchid", "DarkRed", "DarkSalmon", "DarkSeaGreen",
        "DarkSlateBlue", "DarkSlateGray", "DarkSlateGrey", "DarkTurquoise", "DarkViolet", "DeepPink", "DeepSkyBlue",
        "DimGray", "DimGrey", "DodgerBlue", "Firebrick", "FloralWhite", "ForestGreen", "Fuchsia", "Gainsboro", "GhostWhite",
        "Gold", "Goldenrod", "Gray", "Grey", "Green", "GreenYellow", "Honeydew", "HotPink", "IndianRed", "Indigo", "Ivory",
        "Khaki", "Lavender", "LavenderBlush", "LawnGreen", "LemonChiffon", "LightBlue", "LightCoral", "LightCyan",
        "LightGoldenrodYellow", "LightGray", "LightGrey", "LightGreen", "LightPink", "LightSalmon", "LightSeaGreen",
        "LightSkyBlue", "LightSlateGray", "LightSlateGrey", "LightSteelBlue", "LightYellow", "Lime", "LimeGreen", "Linen",
        "Magenta", "Maroon", "MediumAquamarine", "MediumBlue", "MediumOrchid", "MediumPurple", "MediumSeaGreen",
        "MediumSlateBlue", "MediumSpringGreen", "MediumTurquoise", "MediumVioletRed", "MidnightBlue", "MintCream",
        "MistyRose", "Moccasin", "NavajoWhite", "Navy", "OldLace", "Olive", "OliveDrab", "Orange", "OrangeRed", "Orchid",
        "PaleGoldenrod", "PaleGreen", "PaleTurquoise", "PaleVioletRed", "PapayaWhip", "PeachPuff", "Peru", "Pink", "Plum",
        "PowderBlue", "Purple", "RebeccaPurple", "Red", "RosyBrown", "RoyalBlue", "SaddleBrown", "Salmon", "SandyBrown",
        "SeaGreen", "SeaShell", "Sienna", "Silver", "SkyBlue", "SlateBlue", "SlateGray", "SlateGrey", "Snow", "SpringGreen",
        "SteelBlue", "Tan", "Teal", "Thistle", "Tomato", "Turquoise", "Violet", "Wheat", "White", "WhiteSmoke", "Yellow",
        "YellowGreen",
    };

    /// <summary>
    /// Parses a list of numbers such as <c>16</c>, <c>16,8</c> or <c>0 0 4 0</c>.
    /// </summary>
    public static bool TryParseNumbers(string value, out double[] numbers)
    {
        var parts = value.Split([',', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        numbers = new double[parts.Length];
        if (parts.Length == 0)
        {
            return false;
        }

        for (var i = 0; i < parts.Length; i++)
        {
            if (!double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out numbers[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// The Leonardo spacing resource for a uniform value, for example <c>LeonardoSpaceM</c> for 16, or null.
    /// </summary>
    public static string? SpacingResource(double[] numbers)
    {
        var distinct = numbers.Where(n => n != 0).Distinct().ToArray();
        return distinct.Length == 1 && Spacing.TryGetValue(distinct[0], out var step) ? $"LeonardoSpace{step}" : null;
    }

    /// <summary>
    /// The Leonardo radius resource for a uniform value, for example <c>LeonardoRadiusM</c> for 8, or null.
    /// </summary>
    public static string? RadiusResource(double[] numbers)
    {
        var distinct = numbers.Where(n => n != 0).Distinct().ToArray();
        return distinct.Length == 1 && Radius.TryGetValue(distinct[0], out var step) ? $"LeonardoRadius{step}" : null;
    }

    /// <summary>
    /// A short excerpt of code for a message: trimmed and at most 80 characters.
    /// </summary>
    public static string Excerpt(string code)
    {
        var text = string.Join(' ', code.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return text.Length <= 80 ? text : text[..77] + "...";
    }
}
