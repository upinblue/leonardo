using UpInBlue.Leonardo.Tokens.Model;

namespace UpInBlue.Leonardo.Tokens.Generation;

/// <summary>
/// Generates the XAML resource dictionaries for WinUI 3 and WPF.
/// </summary>
internal static class XamlGenerator
{
    public const string WinUIPath = "src/UpInBlue.Leonardo.WinUI/Themes/Generated/Tokens.xaml";
    public const string WpfLightPath = "src/UpInBlue.Leonardo.Wpf/Themes/Generated/Tokens.Light.xaml";
    public const string WpfDarkPath = "src/UpInBlue.Leonardo.Wpf/Themes/Generated/Tokens.Dark.xaml";
    public const string WpfHighContrastPath = "src/UpInBlue.Leonardo.Wpf/Themes/Generated/Tokens.HighContrast.xaml";

    /// <summary>
    /// The Windows system color each theme color becomes in high contrast. High contrast uses system colors only.
    /// </summary>
    private static readonly Dictionary<string, string> HighContrastColors = new(StringComparer.Ordinal)
    {
        ["accent"] = "SystemColorHighlightColor",
        ["onAccent"] = "SystemColorHighlightTextColor",
        ["link"] = "SystemColorHotlightColor",
        ["selection"] = "SystemColorHighlightColor",
        ["surface"] = "SystemColorWindowColor",
        ["surfaceAlt"] = "SystemColorWindowColor",
        ["textPrimary"] = "SystemColorWindowTextColor",
        ["textSecondary"] = "SystemColorWindowTextColor",
        ["textTertiary"] = "SystemColorWindowTextColor",
        ["strokeSubtle"] = "SystemColorWindowTextColor",
        ["strokeStrong"] = "SystemColorWindowTextColor",
        ["statusCritical"] = "SystemColorWindowTextColor",
        ["statusCaution"] = "SystemColorWindowTextColor",
        ["statusSuccess"] = "SystemColorWindowTextColor",
    };

    public static string ColorKey(string name) => $"Leonardo{Text.Pascal(name)}Color";

    public static string BrushKey(string name) => $"Leonardo{Text.Pascal(name)}Brush";

    /// <summary>
    /// The name of a motion value without prefix: <c>motion.easing.enter</c> → <c>EaseEnter</c>, <c>motion.duration.normal</c> → <c>DurationNormal</c>.
    /// </summary>
    public static string MotionName(MotionToken motion)
        => (motion.Group == "easing" ? "Ease" : Text.Pascal(motion.Group)) + Text.Pascal(motion.Token.Name);

    public static GeneratedFile WinUI(TokenCatalog catalog)
    {
        var writer = Begin(isWpf: false);
        writer.Line("<ResourceDictionary.ThemeDictionaries>").Indent();

        WriteThemeDictionary(writer, "Light", catalog.Light);
        writer.Line();
        WriteThemeDictionary(writer, "Dark", catalog.Dark);
        writer.Line();

        writer.Line("<ResourceDictionary x:Key=\"HighContrast\">").Indent();
        writer.Line("<!-- High contrast uses system colors only; no token values apply. -->");
        foreach (var name in catalog.ThemeColorNames)
        {
            var systemColor = HighContrastColor(name);
            writer.Line($"<StaticResource x:Key=\"{ColorKey(name)}\" ResourceKey=\"{systemColor}\" />");
            writer.Line($"<SolidColorBrush x:Key=\"{BrushKey(name)}\" Color=\"{{ThemeResource {systemColor}}}\" />");
        }

        writer.Outdent().Line("</ResourceDictionary>");
        writer.Outdent().Line("</ResourceDictionary.ThemeDictionaries>");
        writer.Line();

        WriteSharedResources(writer, catalog, isWpf: false);
        return End(writer, WinUIPath);
    }

    public static GeneratedFile Wpf(TokenCatalog catalog, bool dark)
    {
        var writer = Begin(isWpf: true);
        foreach (var token in dark ? catalog.Dark : catalog.Light)
        {
            WriteColor(writer, token);
        }

        writer.Line();
        WriteSharedResources(writer, catalog, isWpf: true);
        return End(writer, dark ? WpfDarkPath : WpfLightPath);
    }

    /// <summary>
    /// The WPF dictionary for contrast themes: the same keys as Light and Dark, mapped to the system colors of
    /// <c>System.Windows.SystemColors</c>. Brushes follow the system colors dynamically; colors are read when the dictionary loads.
    /// </summary>
    public static GeneratedFile WpfHighContrast(TokenCatalog catalog)
    {
        var writer = Begin(isWpf: true);
        writer.Line("<!-- High contrast uses system colors only; no token values apply. -->");
        foreach (var name in catalog.ThemeColorNames)
        {
            var systemColor = WpfSystemColor(HighContrastColor(name));
            writer.Line($"<x:Static x:Key=\"{ColorKey(name)}\" Member=\"SystemColors.{systemColor}Color\" />");
            writer.Line($"<SolidColorBrush x:Key=\"{BrushKey(name)}\" Color=\"{{DynamicResource {{x:Static SystemColors.{systemColor}ColorKey}}}}\" />");
        }

        writer.Line();
        WriteSharedResources(writer, catalog, isWpf: true);
        return End(writer, WpfHighContrastPath);
    }

    private static string HighContrastColor(string name)
        => HighContrastColors.TryGetValue(name, out var systemColor)
            ? systemColor
            : throw new TokenException($"The theme color '{name}' has no high contrast system color. Add it to {nameof(XamlGenerator)}.");

    /// <summary>
    /// The WPF name of a WinUI system color: <c>SystemColorHighlightColor</c> → <c>Highlight</c>. WPF calls the hotlight color <c>HotTrack</c>.
    /// </summary>
    private static string WpfSystemColor(string winUIColor)
    {
        var name = winUIColor["SystemColor".Length..^"Color".Length];
        return name == "Hotlight" ? "HotTrack" : name;
    }

    private static CodeWriter Begin(bool isWpf)
    {
        var writer = new CodeWriter();
        writer.Line($"<!-- {Text.Header} -->");
        writer.Line("<ResourceDictionary");
        writer.Line("    xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
        writer.Line(isWpf
            ? "    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\""
            : "    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">");
        if (isWpf)
        {
            writer.Line("    xmlns:sys=\"clr-namespace:System;assembly=System.Runtime\">");
        }

        writer.Line();
        return writer.Indent();
    }

    private static GeneratedFile End(CodeWriter writer, string path)
    {
        writer.Outdent().Line("</ResourceDictionary>");
        return GeneratedFile.FromText(path, writer.ToString());
    }

    private static void WriteThemeDictionary(CodeWriter writer, string theme, IReadOnlyList<Token> tokens)
    {
        writer.Line($"<ResourceDictionary x:Key=\"{theme}\">").Indent();
        foreach (var token in tokens)
        {
            WriteColor(writer, token);
        }

        writer.Outdent().Line("</ResourceDictionary>");
    }

    private static void WriteColor(CodeWriter writer, Token token)
    {
        WriteSource(writer, token);
        writer.Line($"<Color x:Key=\"{ColorKey(token.Name)}\">{token.Color.XamlHex}</Color>");
        writer.Line($"<SolidColorBrush x:Key=\"{BrushKey(token.Name)}\" Color=\"{token.Color.XamlHex}\" />");
    }

    private static void WriteSharedResources(CodeWriter writer, TokenCatalog catalog, bool isWpf)
    {
        var doubleElement = isWpf ? "sys:Double" : "x:Double";
        var intElement = isWpf ? "sys:Int32" : "x:Int32";

        writer.Line("<!-- Blue scale. The same in every theme: use it only where a theme color does not fit, such as charts, and never in high contrast. -->");
        foreach (var token in catalog.Palette)
        {
            WriteSource(writer, token);
            writer.Line($"<Color x:Key=\"LeonardoPalette{Text.Pascal(token.Name)}Color\">{token.Color.XamlHex}</Color>");
        }

        writer.Line();
        writer.Line("<!-- Spacing in epx: all sides, one side, or both sides of one axis. -->");
        foreach (var token in catalog.Space)
        {
            var key = $"LeonardoSpace{Text.Pascal(token.Name)}";
            var value = Text.Number(token.Dimension.Value);
            WriteSource(writer, token);
            writer.Line($"<{doubleElement} x:Key=\"{key}\">{value}</{doubleElement}>");
            writer.Line($"<Thickness x:Key=\"{key}Thickness\">{value}</Thickness>");
            writer.Line($"<Thickness x:Key=\"{key}LeftThickness\">{value},0,0,0</Thickness>");
            writer.Line($"<Thickness x:Key=\"{key}TopThickness\">0,{value},0,0</Thickness>");
            writer.Line($"<Thickness x:Key=\"{key}RightThickness\">0,0,{value},0</Thickness>");
            writer.Line($"<Thickness x:Key=\"{key}BottomThickness\">0,0,0,{value}</Thickness>");
            writer.Line($"<Thickness x:Key=\"{key}HorizontalThickness\">{value},0,{value},0</Thickness>");
            writer.Line($"<Thickness x:Key=\"{key}VerticalThickness\">0,{value},0,{value}</Thickness>");
        }

        writer.Line();
        writer.Line("<!-- Corner radii in epx. -->");
        foreach (var token in catalog.Radius)
        {
            WriteSource(writer, token);
            writer.Line($"<CornerRadius x:Key=\"LeonardoRadius{Text.Pascal(token.Name)}\">{Text.Number(token.Dimension.Value)}</CornerRadius>");
        }

        writer.Line();
        writer.Line("<!-- List row heights in epx. -->");
        foreach (var token in catalog.Density)
        {
            WriteSource(writer, token);
            writer.Line($"<{doubleElement} x:Key=\"LeonardoDensity{Text.Pascal(token.Name)}\">{Text.Number(token.Dimension.Value)}</{doubleElement}>");
        }

        writer.Line();
        writer.Line("<!-- Type roles: font family, font size and line height in epx. -->");
        foreach (var role in catalog.TypeRoles)
        {
            if (role.Windows is not { } style)
            {
                continue;
            }

            var prefix = $"Leonardo{Text.Pascal(role.Name)}";
            writer.Line($"<!-- typography.{role.Name}.windows -->");
            writer.Line($"<FontFamily x:Key=\"{prefix}FontFamily\">{Text.Xml(style.FontFamily)}</FontFamily>");
            writer.Line($"<{doubleElement} x:Key=\"{prefix}FontSize\">{Text.Number(style.FontSize)}</{doubleElement}>");
            writer.Line($"<{doubleElement} x:Key=\"{prefix}LineHeight\">{Text.Number(style.LineHeight)}</{doubleElement}>");
        }

        writer.Line();
        // WinUI keeps easing curves and durations as strings, like Fluent's own ControlFastAnimationDuration:
        // a typed Duration resource cannot be assigned to KeyTime or BeginTime, a string converts to all three.
        writer.Line(isWpf
            ? "<!-- Motion: easing curves as KeySpline; durations as Duration, as KeyTime for key frames with a KeySpline and as TimeSpan for BeginTime; distances in epx and factors. -->"
            : "<!-- Motion: easing curves and durations as strings, like Fluent's own resources, so they convert to KeySpline, KeyTime, Duration and BeginTime. Distances in epx and factors. -->");
        foreach (var motion in catalog.Motion)
        {
            var token = motion.Token;
            var key = $"Leonardo{MotionName(motion)}";
            WriteSource(writer, token);
            switch (token.Type)
            {
                case TokenType.CubicBezier:
                    var element = isWpf ? "KeySpline" : "x:String";
                    writer.Line($"<{element} x:Key=\"{key}\">{KeySpline(token.CubicBezier)}</{element}>");
                    break;
                case TokenType.Duration:
                    var durationElement = isWpf ? "Duration" : "x:String";
                    writer.Line($"<{durationElement} x:Key=\"{key}\">{XamlDuration(token)}</{durationElement}>");
                    if (isWpf)
                    {
                        writer.Line($"<KeyTime x:Key=\"{key}KeyTime\">{XamlDuration(token)}</KeyTime>");
                        writer.Line($"<sys:TimeSpan x:Key=\"{key}TimeSpan\">{XamlDuration(token)}</sys:TimeSpan>");
                    }

                    break;
                case TokenType.Dimension:
                    writer.Line($"<{doubleElement} x:Key=\"{key}\">{Text.Number(token.Dimension.Value)}</{doubleElement}>");
                    break;
                case TokenType.Number when Text.IsWhole(token.Number):
                    writer.Line($"<{intElement} x:Key=\"{key}\">{Text.Number(token.Number)}</{intElement}>");
                    break;
                case TokenType.Number:
                    writer.Line($"<{doubleElement} x:Key=\"{key}\">{Text.Number(token.Number)}</{doubleElement}>");
                    break;
                default:
                    throw new TokenException($"Motion token '{token.Path}' has the unsupported type {token.Type}.");
            }
        }
    }

    private static void WriteSource(CodeWriter writer, Token token)
        => writer.Line(token.IsProposal ? $"<!-- {token.Path} (proposal) -->" : $"<!-- {token.Path} -->");

    private static string KeySpline(CubicBezier curve)
        => $"{Text.Number(curve.X1)},{Text.Number(curve.Y1)},{Text.Number(curve.X2)},{Text.Number(curve.Y2)}";

    private static string XamlDuration(Token token)
    {
        var milliseconds = token.Duration.Milliseconds;
        return milliseconds < 60_000
            ? $"0:0:{Text.Number(milliseconds / 1000)}"
            : throw new TokenException($"Token '{token.Path}' is longer than one minute.");
    }
}
