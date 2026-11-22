using System.Text.RegularExpressions;

namespace UpInBlue.Leonardo.Check.Checkers;

/// <summary>
/// Checks XAML of WinUI 3 and WPF: literal colors, font sizes, corner radii, durations, key times, key splines and
/// spacing; easing functions outside the Leonardo curves; custom glass; removed or restyled focus visuals.
/// </summary>
internal sealed partial class XamlChecker : IChecker
{
    private static readonly HashSet<string> ColorProperties = new(StringComparer.Ordinal)
    {
        "Background", "Foreground", "Fill", "Stroke", "BorderBrush", "Color", "OpacityMask",
    };

    private static readonly HashSet<string> SpacingProperties = new(StringComparer.Ordinal)
    {
        "Margin", "Padding", "Spacing", "ColumnSpacing", "RowSpacing",
    };

    private static readonly HashSet<string> TimeProperties = new(StringComparer.Ordinal) { "Duration", "BeginTime", "KeyTime" };

    private static readonly HashSet<string> SplineProperties = new(StringComparer.Ordinal) { "KeySpline", "ControlPoint1", "ControlPoint2" };

    private static readonly HashSet<string> FocusProperties = new(StringComparer.Ordinal)
    {
        "FocusVisualPrimaryThickness", "FocusVisualSecondaryThickness", "FocusVisualPrimaryBrush",
        "FocusVisualSecondaryBrush", "FocusVisualMargin", "FocusVisualStyle",
    };

    private static readonly HashSet<string> GlassProperties = new(StringComparer.Ordinal)
    {
        "TintOpacity", "TintLuminosityOpacity", "BackgroundSource",
    };

    private static readonly HashSet<string> GlassElements = new(StringComparer.Ordinal)
    {
        "AcrylicBrush", "DesktopAcrylicBackdrop", "AcrylicBackdrop",
    };

    private static readonly HashSet<string> BouncingEasings = new(StringComparer.Ordinal) { "BackEase", "BounceEase", "ElasticEase" };

    private static readonly HashSet<string> OtherEasings = new(StringComparer.Ordinal)
    {
        "CubicEase", "SineEase", "QuadraticEase", "ExponentialEase", "PowerEase", "CircleEase", "QuarticEase", "QuinticEase",
    };

    private const string ColorSuggestion =
        "Use a Leonardo color resource, for example {ThemeResource LeonardoTextPrimaryBrush} (WinUI) or {DynamicResource LeonardoTextPrimaryBrush} (WPF), or a Fluent theme brush.";

    private static string ColorSuggestionFor(string property)
    {
        var brush = property switch
        {
            "Background" => "LeonardoSurfaceBrush or LeonardoSurfaceAltBrush",
            "Foreground" => "LeonardoTextPrimaryBrush, LeonardoTextSecondaryBrush or LeonardoTextTertiaryBrush",
            "BorderBrush" or "Stroke" => "LeonardoStrokeSubtleBrush or LeonardoStrokeStrongBrush",
            _ => null,
        };

        return brush is null
            ? ColorSuggestion
            : $"Use a Leonardo color resource such as {brush}, with ThemeResource in WinUI and DynamicResource in WPF, or a Fluent theme brush.";
    }

    private const string DurationSuggestion =
        "Use a motion helper of UpInBlue.Leonardo.WinUI.Motion, or a Leonardo duration such as {StaticResource LeonardoDurationNormal}.";

    private const string EasingSuggestion =
        "Use a motion helper, or a spline key frame with KeySpline=\"{StaticResource LeonardoEaseEnter}\" (or LeonardoEaseExit, LeonardoEaseMove).";

    public void Check(SourceText source, FindingList findings)
    {
        var text = source.Masked;

        foreach (Match tag in TagRegex().Matches(text))
        {
            var element = LocalName(tag.Groups["name"].Value);
            var attributes = AttributeRegex().Matches(tag.Groups["attrs"].Value)
                .Select(a => (Name: a.Groups["attr"].Value, Value: a.Groups["value"].Value, Offset: tag.Groups["attrs"].Index + a.Groups["value"].Index))
                .ToList();

            CheckElement(element, tag.Index, findings);

            if (element == "Setter")
            {
                var property = attributes.FirstOrDefault(a => a.Name == "Property");
                var value = attributes.FirstOrDefault(a => a.Name == "Value");
                if (property.Name is not null && value.Name is not null)
                {
                    CheckProperty(LocalName(property.Value), value.Value, value.Offset, findings);
                }

                continue;
            }

            foreach (var (name, value, offset) in attributes)
            {
                CheckProperty(LocalName(name), value, offset, findings);
            }
        }

        foreach (Match content in ColorContentRegex().Matches(text))
        {
            var value = content.Groups["value"].Value.Trim();
            if (IsLiteralColor(value))
            {
                findings.Add(content.Groups["value"].Index, Rules.LiteralColor, $"<{content.Groups["name"].Value}> defines the literal color \"{value}\".", ColorSuggestion);
            }
        }
    }

    private static void CheckElement(string element, int offset, FindingList findings)
    {
        if (GlassElements.Contains(element) || element.EndsWith("BlurEffect", StringComparison.Ordinal))
        {
            findings.Add(offset, Rules.CustomGlass, $"<{element}> creates a custom glass or blur.",
                "Use MicaBackdrop on the window and Fluent's own flyouts and menus, which bring acrylic where Fluent uses it.");
        }
        else if (BouncingEasings.Contains(element))
        {
            findings.Add(offset, Rules.SpringBounce, $"<{element}> overshoots or bounces.", EasingSuggestion);
        }
        else if (OtherEasings.Contains(element))
        {
            findings.Add(offset, Rules.LiteralEasing, $"<{element}> is not a Leonardo easing curve.", EasingSuggestion);
        }
    }

    private static void CheckProperty(string property, string value, int offset, FindingList findings)
    {
        var trimmed = value.Trim();
        if (property == "FocusVisualStyle" || FocusProperties.Contains(property))
        {
            findings.Add(offset, Rules.FocusVisual, $"{property}=\"{trimmed}\" restyles or removes the focus visual.",
                "Keep the platform's focus visuals: remove this setting.");
            return;
        }

        if (property == "UseSystemFocusVisuals" && trimmed.Equals("False", StringComparison.OrdinalIgnoreCase))
        {
            findings.Add(offset, Rules.FocusVisual, "UseSystemFocusVisuals=\"False\" removes the system focus visual.",
                "Keep the system focus visuals: remove this setting.");
            return;
        }

        if (trimmed.StartsWith('{') || trimmed.Length == 0)
        {
            return;
        }

        if (GlassProperties.Contains(property))
        {
            findings.Add(offset, Rules.CustomGlass, $"{property}=\"{trimmed}\" configures a custom acrylic.",
                "Use MicaBackdrop on the window and Fluent's own flyouts and menus.");
        }
        else if (IsColorProperty(property) && IsLiteralColor(trimmed))
        {
            findings.Add(offset, Rules.LiteralColor, $"{property}=\"{trimmed}\" is a literal color.", ColorSuggestionFor(property));
        }
        else if (property == "FontSize" && Vocabulary.TryParseNumbers(trimmed, out _))
        {
            findings.Add(offset, Rules.LiteralFontSize, $"FontSize=\"{trimmed}\" is a literal font size.",
                "Use a Leonardo text style, for example Style=\"{StaticResource LeonardoBodyTextStyle}\" or LeonardoSectionTextStyle.");
        }
        else if (property == "CornerRadius" && Vocabulary.TryParseNumbers(trimmed, out var radii) && radii.Any(r => r != 0))
        {
            var resource = Vocabulary.RadiusResource(radii);
            findings.Add(offset, Rules.LiteralCornerRadius, $"CornerRadius=\"{trimmed}\" is a literal radius.",
                resource is null
                    ? "Use {StaticResource LeonardoRadiusS}, LeonardoRadiusM or LeonardoRadiusL; inner radius = outer radius - padding."
                    : $"Use {{StaticResource {resource}}}.");
        }
        else if (TimeProperties.Contains(property) && IsNonZeroTime(trimmed))
        {
            findings.Add(offset, Rules.LiteralDuration, $"{property}=\"{trimmed}\" is a literal time.", DurationSuggestion);
        }
        else if (SplineProperties.Contains(property) && trimmed.Any(char.IsDigit))
        {
            findings.Add(offset, Rules.LiteralEasing, $"{property}=\"{trimmed}\" is a literal easing curve.", EasingSuggestion);
        }
        else if (SpacingProperties.Contains(property) && Vocabulary.TryParseNumbers(trimmed, out var spacing) && spacing.Any(s => s != 0))
        {
            var resource = Vocabulary.SpacingResource(spacing);
            var suggestion = property is "Margin" or "Padding"
                ? resource is null
                    ? "Use a Leonardo spacing resource such as {StaticResource LeonardoSpaceMThickness} or its Left, Top, Right, Bottom, Horizontal and Vertical variants; spacing comes only from space.xs to space.xl."
                    : $"Use {{StaticResource {resource}Thickness}} or one of its Left, Top, Right, Bottom, Horizontal and Vertical variants."
                : resource is null
                    ? "Use {StaticResource LeonardoSpaceXS}, LeonardoSpaceS, LeonardoSpaceM, LeonardoSpaceL or LeonardoSpaceXL."
                    : $"Use {{StaticResource {resource}}}.";
            findings.Add(offset, Rules.LiteralSpacing, $"{property}=\"{trimmed}\" is literal spacing.", suggestion);
        }
    }

    private static bool IsColorProperty(string property)
        => ColorProperties.Contains(property) || property.EndsWith("Brush", StringComparison.Ordinal) || property.EndsWith("Color", StringComparison.Ordinal);

    private static bool IsLiteralColor(string value)
        => HexColorRegex().IsMatch(value) || Vocabulary.NamedColors.Contains(value);

    private static bool IsNonZeroTime(string value)
    {
        var match = TimeRegex().Match(value);
        return match.Success && value.Any(c => c is >= '1' and <= '9');
    }

    private static string LocalName(string name)
    {
        var colon = name.LastIndexOf(':');
        var local = colon >= 0 ? name[(colon + 1)..] : name;
        var dot = local.LastIndexOf('.');
        return dot >= 0 ? local[(dot + 1)..] : local;
    }

    [GeneratedRegex(@"<(?<name>[A-Za-z_][\w:.]*)(?<attrs>(?:\s+[\w:.]+\s*=\s*(?:""[^""]*""|'[^']*'))*)\s*/?>")]
    private static partial Regex TagRegex();

    [GeneratedRegex(@"(?<attr>[\w:.]+)\s*=\s*(?:""(?<value>[^""]*)""|'(?<value>[^']*)')")]
    private static partial Regex AttributeRegex();

    [GeneratedRegex(@"<(?<name>(?:\w+:)?(?:Color|SolidColorBrush))\b[^>]*>(?<value>[^<]+)</")]
    private static partial Regex ColorContentRegex();

    [GeneratedRegex(@"^#(?:[0-9A-Fa-f]{3,4}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$")]
    private static partial Regex HexColorRegex();

    [GeneratedRegex(@"^(?:\d+:\d+:\d+(?:\.\d+)?|\d+(?:\.\d+)?)$")]
    private static partial Regex TimeRegex();
}
