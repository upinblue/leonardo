using UpInBlue.Leonardo.Tokens.Model;

namespace UpInBlue.Leonardo.Tokens.Generation;

/// <summary>
/// Generates the WinUI accent ramp and the text styles of the type roles.
/// </summary>
internal static class WinUIThemeGenerator
{
    public const string AccentColorsPath = "src/UpInBlue.Leonardo.WinUI/Themes/Generated/AccentColors.xaml";
    public const string TextStylesPath = "src/UpInBlue.Leonardo.WinUI/Themes/Generated/TextStyles.xaml";

    /// <summary>
    /// The weight of emphasis: the body role in semibold.
    /// </summary>
    private const int SemiboldWeight = 600;

    /// <summary>
    /// The WinUI accent ramp from the blue scale. Fluent fills accent surfaces with Dark1 in light theme and with Light2 in
    /// dark theme, and colors accent text with Dark2 and Dark3 (light) or Light3 (dark). The ramp therefore maps:
    /// Dark3 = depth, Dark2 = near (the light link), Dark1 = light accent (brand), Light2 = dark accent (far),
    /// Light3 = dark link. SystemAccentColor is brand as well, because Fluent puts white text on it (selected text),
    /// which needs 4.5:1; Light1 lies halfway between brand and far.
    /// </summary>
    public static GeneratedFile AccentColors(TokenCatalog catalog)
    {
        var writer = Begin();
        writer.Line("<!-- The Windows accent ramp, derived from the blue scale. Accent fills are brand in light theme and far in dark theme. -->");
        foreach (var (key, color, source) in AccentRamp(catalog))
        {
            writer.Line($"<!-- {Text.XmlComment(source)} -->");
            writer.Line($"<Color x:Key=\"{key}\">{color.XamlHex}</Color>");
        }

        return End(writer, AccentColorsPath);
    }

    /// <summary>
    /// The Windows accent ramp from the blue scale, shared by WinUI and the WPF Fluent theme, which use the same keys.
    /// </summary>
    public static IReadOnlyList<(string Key, RgbColor Color, string Source)> AccentRamp(TokenCatalog catalog)
    {
        var tokens = catalog.Tokens;
        var lightAccent = tokens["theme.light.accent"];
        var darkAccent = tokens["theme.dark.accent"];
        return
        [
            ("SystemAccentColorDark3", tokens["palette.depth"].Color, "palette.depth"),
            ("SystemAccentColorDark2", tokens["palette.near"].Color, "palette.near"),
            ("SystemAccentColorDark1", lightAccent.Color, "theme.light.accent"),
            ("SystemAccentColor", lightAccent.Color, "theme.light.accent: the base carries white text, for example selected text"),
            ("SystemAccentColorLight1", Mix(lightAccent.Color, darkAccent.Color, 0.5), "halfway from theme.light.accent to theme.dark.accent"),
            ("SystemAccentColorLight2", darkAccent.Color, "theme.dark.accent"),
            ("SystemAccentColorLight3", tokens["theme.dark.link"].Color, "theme.dark.link"),
        ];
    }
    /// <summary>
    /// The text styles of the type roles, plus emphasis (body in semibold) and variants with tabular figures for numbers in columns.
    /// They live in theme dictionaries, as Microsoft's guidance "Contrast themes" describes for list items with colored text:
    /// in light and dark (<c>Default</c>) they set the role's text color; in contrast themes (<c>HighContrast</c>) they set no
    /// <c>Foreground</c>, so text inherits it from its container. A selected <c>ListViewItem</c> then gives its text
    /// HighlightText on Highlight. Reference the styles with <c>ThemeResource</c> so they follow a theme change.
    /// </summary>
    public static GeneratedFile TextStyles(TokenCatalog catalog)
    {
        var writer = Begin();
        writer.Line("<!-- Text styles of the type roles. Text wraps instead of being clipped. Reference them with ThemeResource. -->");
        writer.Line("<ResourceDictionary.ThemeDictionaries>").Indent();
        writer.Line("<!-- Light and dark: every role sets its text color. -->");
        writer.Line("<ResourceDictionary x:Key=\"Default\">").Indent();
        WriteStyles(writer, catalog, withForeground: true);
        writer.Outdent().Line("</ResourceDictionary>");
        writer.Line();
        writer.Line("<!--");
        writer.Line("    Contrast themes: no Foreground, so text inherits the system color of its container, for example HighlightText in a");
        writer.Line("    selected list item (Microsoft, \"Contrast themes\", list items with colored text).");
        writer.Line("-->");
        writer.Line("<ResourceDictionary x:Key=\"HighContrast\">").Indent();
        WriteStyles(writer, catalog, withForeground: false);
        writer.Outdent().Line("</ResourceDictionary>");
        writer.Outdent().Line("</ResourceDictionary.ThemeDictionaries>");
        return End(writer, TextStylesPath);
    }

    private static void WriteStyles(CodeWriter writer, TokenCatalog catalog, bool withForeground)
    {
        var first = true;
        foreach (var role in catalog.TypeRoles)
        {
            if (role.Windows is not { } style)
            {
                continue;
            }

            if (!first)
            {
                writer.Line();
            }

            first = false;
            var name = Text.Pascal(role.Name);
            var foreground = withForeground ? XamlGenerator.BrushKey(style.Color ?? "textPrimary") : null;
            WriteStyle(writer, $"Leonardo{name}TextStyle", role.Name, style.FontWeight, foreground, tabular: false);

            if (role.Name == "body")
            {
                writer.Line();
                writer.Line("<!-- Emphasis: the body role in semibold, never a separate size. -->");
                WriteStyle(writer, "LeonardoBodyStrongTextStyle", role.Name, SemiboldWeight, foreground, tabular: false);
            }

            if (role.Name is "body" or "meta")
            {
                writer.Line();
                writer.Line($"<!-- The {role.Name} role with tabular figures, for numbers in columns. -->");
                WriteStyle(writer, $"Leonardo{name}TabularTextStyle", role.Name, style.FontWeight, foreground, tabular: true);
            }
        }
    }

    private static void WriteStyle(CodeWriter writer, string key, string role, int weight, string? foreground, bool tabular)
    {
        var prefix = $"Leonardo{Text.Pascal(role)}";
        writer.Line($"<Style x:Key=\"{key}\" TargetType=\"TextBlock\" BasedOn=\"{{StaticResource BodyTextBlockStyle}}\">").Indent();
        writer.Line($"<Setter Property=\"FontFamily\" Value=\"{{StaticResource {prefix}FontFamily}}\" />");
        writer.Line($"<Setter Property=\"FontSize\" Value=\"{{StaticResource {prefix}FontSize}}\" />");
        writer.Line($"<Setter Property=\"LineHeight\" Value=\"{{StaticResource {prefix}LineHeight}}\" />");
        writer.Line($"<Setter Property=\"FontWeight\" Value=\"{FontWeight(weight, key)}\" />");
        if (foreground is not null)
        {
            writer.Line($"<Setter Property=\"Foreground\" Value=\"{{ThemeResource {foreground}}}\" />");
        }

        writer.Line("<Setter Property=\"TextWrapping\" Value=\"Wrap\" />");
        if (tabular)
        {
            writer.Line("<Setter Property=\"Typography.NumeralAlignment\" Value=\"Tabular\" />");
        }

        writer.Outdent().Line("</Style>");
    }

    private static string FontWeight(int weight, string key) => weight switch
    {
        100 => "Thin",
        200 => "ExtraLight",
        300 => "Light",
        350 => "SemiLight",
        400 => "Normal",
        500 => "Medium",
        600 => "SemiBold",
        700 => "Bold",
        800 => "ExtraBold",
        900 => "Black",
        _ => throw new TokenException($"The font weight {weight} of {key} has no name in WinUI."),
    };

    private static RgbColor Mix(RgbColor from, RgbColor to, double amount)
    {
        static byte Channel(byte a, byte b, double t) => (byte)Math.Round(a + ((b - a) * t), MidpointRounding.AwayFromZero);
        return new RgbColor(Channel(from.R, to.R, amount), Channel(from.G, to.G, amount), Channel(from.B, to.B, amount), 255);
    }

    private static CodeWriter Begin()
    {
        var writer = new CodeWriter();
        writer.Line($"<!-- {Text.Header} -->");
        writer.Line("<ResourceDictionary");
        writer.Line("    xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
        writer.Line("    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">");
        writer.Line();
        return writer.Indent();
    }

    private static GeneratedFile End(CodeWriter writer, string path)
    {
        writer.Outdent().Line("</ResourceDictionary>");
        return GeneratedFile.FromText(path, writer.ToString());
    }
}
