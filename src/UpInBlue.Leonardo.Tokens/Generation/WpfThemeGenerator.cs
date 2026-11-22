using UpInBlue.Leonardo.Tokens.Model;

namespace UpInBlue.Leonardo.Tokens.Generation;

/// <summary>
/// Generates the accent override for the WPF Fluent theme and the WPF text styles of the type roles.
/// </summary>
internal static class WpfThemeGenerator
{
    public const string AccentColorsPath = "src/UpInBlue.Leonardo.Wpf/Themes/Generated/AccentColors.xaml";
    public const string TextStylesPath = "src/UpInBlue.Leonardo.Wpf/Themes/Generated/TextStyles.xaml";

    /// <summary>
    /// The weight of emphasis: the body role in semibold. The same as in WinUI.
    /// </summary>
    private const int SemiboldWeight = 600;

    /// <summary>
    /// The accent ramp for the WPF Fluent theme, with the same values as in WinUI, see <see cref="WinUIThemeGenerator.AccentRamp"/>.
    /// The Fluent theme defines <c>SystemAccentColor…</c> as aliases of the <c>SystemColors.AccentColor…Key</c> resources and
    /// resolves them when it loads, so the override uses the <c>SystemColors</c> keys. Brushes for the same keys follow, so
    /// that <c>SystemColors.AccentColor…BrushKey</c> also gives the Leonardo accent.
    /// </summary>
    public static GeneratedFile AccentColors(TokenCatalog catalog)
    {
        var writer = Begin();
        writer.Line("<!-- The Windows accent ramp, derived from the blue scale; the same values as in WinUI. Accent fills are brand in light theme and far in dark theme. -->");
        writer.Line("<!-- The Fluent theme reads these keys when it loads its dictionary, so LeonardoTheme merges this one first. Never merge it in a contrast theme. -->");
        foreach (var (key, color, source) in WinUIThemeGenerator.AccentRamp(catalog))
        {
            var name = SystemColorsName(key);
            writer.Line($"<!-- {key}: {Text.XmlComment(source)} -->");
            writer.Line($"<Color x:Key=\"{{x:Static SystemColors.{name}Key}}\">{color.XamlHex}</Color>");
            writer.Line($"<SolidColorBrush x:Key=\"{{x:Static SystemColors.{name}BrushKey}}\" Color=\"{color.XamlHex}\" />");
        }

        return End(writer, AccentColorsPath);
    }

    /// <summary>
    /// The name of an accent color in <c>System.Windows.SystemColors</c>: <c>SystemAccentColorDark1</c> → <c>AccentColorDark1</c>.
    /// </summary>
    public static string SystemColorsName(string winUIKey)
        => winUIKey.StartsWith("System", StringComparison.Ordinal)
            ? winUIKey["System".Length..]
            : throw new TokenException($"The accent key '{winUIKey}' has no SystemColors equivalent.");

    /// <summary>
    /// The text styles of the type roles, with the same keys, values and variants as in WinUI. WPF styles do not build on a
    /// Fluent style, so every property that WinUI inherits from <c>BodyTextBlockStyle</c> is set here. The values come from
    /// the token dictionary next to this one, which WPF only finds through <c>DynamicResource</c>.
    /// </summary>
    public static GeneratedFile TextStyles(TokenCatalog catalog)
    {
        var writer = Begin();
        writer.Line("<!-- Text styles of the type roles. Text wraps instead of being clipped. Values are DynamicResource references to the token dictionary of the active theme. -->");
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
            var foreground = XamlGenerator.BrushKey(style.Color ?? "textPrimary");
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

        return End(writer, TextStylesPath);
    }

    private static void WriteStyle(CodeWriter writer, string key, string role, int weight, string foreground, bool tabular)
    {
        var prefix = $"Leonardo{Text.Pascal(role)}";
        writer.Line($"<Style x:Key=\"{key}\" TargetType=\"TextBlock\">").Indent();
        writer.Line($"<Setter Property=\"FontFamily\" Value=\"{{DynamicResource {prefix}FontFamily}}\" />");
        writer.Line($"<Setter Property=\"FontSize\" Value=\"{{DynamicResource {prefix}FontSize}}\" />");
        writer.Line($"<Setter Property=\"LineHeight\" Value=\"{{DynamicResource {prefix}LineHeight}}\" />");
        writer.Line("<Setter Property=\"LineStackingStrategy\" Value=\"MaxHeight\" />");
        writer.Line($"<Setter Property=\"FontWeight\" Value=\"{FontWeight(weight)}\" />");
        writer.Line($"<Setter Property=\"Foreground\" Value=\"{{DynamicResource {foreground}}}\" />");
        writer.Line("<Setter Property=\"TextWrapping\" Value=\"Wrap\" />");
        writer.Line("<Setter Property=\"TextTrimming\" Value=\"None\" />");
        if (tabular)
        {
            writer.Line("<Setter Property=\"Typography.NumeralAlignment\" Value=\"Tabular\" />");
        }

        writer.Outdent().Line("</Style>");
    }

    /// <summary>
    /// The WPF name of a font weight. WPF has no name for 350 (semilight); its converter accepts the OpenType weight as a number instead.
    /// </summary>
    private static string FontWeight(int weight) => weight switch
    {
        100 => "Thin",
        200 => "ExtraLight",
        300 => "Light",
        400 => "Normal",
        500 => "Medium",
        600 => "SemiBold",
        700 => "Bold",
        800 => "ExtraBold",
        900 => "Black",
        >= 1 and <= 999 => Text.Number(weight),
        _ => throw new TokenException($"The font weight {weight} is outside the OpenType range 1 to 999."),
    };

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
