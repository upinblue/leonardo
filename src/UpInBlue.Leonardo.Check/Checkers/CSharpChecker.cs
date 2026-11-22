using System.Text.RegularExpressions;

namespace UpInBlue.Leonardo.Check.Checkers;

/// <summary>
/// Checks C# code-behind and view code of WinUI 3 and WPF for the same mistakes as XAML: literal colors, font sizes,
/// corner radii, durations, easing curves and spacing; custom glass; removed focus visuals.
/// </summary>
internal sealed partial class CSharpChecker : IChecker
{
    private const string ColorSuggestion =
        "Use a Leonardo color resource from the theme (for example LeonardoTextPrimaryBrush), a Fluent theme brush, or a system color in high contrast; never LeonardoTokens colors for UI that must follow the theme.";

    private const string DurationSuggestion =
        "Use a motion helper of UpInBlue.Leonardo.WinUI.Motion, or MotionTiming.Get(MotionDuration.Normal) / LeonardoTokens.Motion.Duration… values.";

    private const string EasingSuggestion =
        "Use MotionCurves.Create(compositor, MotionEasing.Enter) (or Exit, Move), MotionCurves.CreateKeySpline(…), or a motion helper.";

    public void Check(SourceText source, FindingList findings)
    {
        var text = source.Code;

        foreach (Match match in ColorApiRegex().Matches(text))
        {
            findings.Add(match.Index, Rules.LiteralColor, $"\"{Vocabulary.Excerpt(match.Value)}\" creates a literal color.", ColorSuggestion);
        }

        foreach (Match match in NamedColorRegex().Matches(text))
        {
            if (Vocabulary.NamedColors.Contains(match.Groups["name"].Value))
            {
                findings.Add(match.Index, Rules.LiteralColor, $"\"{match.Value}\" is a fixed color.", ColorSuggestion);
            }
        }

        foreach (Match match in HexStringRegex().Matches(source.Masked))
        {
            findings.Add(match.Index, Rules.LiteralColor, $"{match.Value} is a literal color.", ColorSuggestion);
        }

        foreach (Match match in FontSizeRegex().Matches(text))
        {
            findings.Add(match.Index, Rules.LiteralFontSize, $"\"{Vocabulary.Excerpt(match.Value)}\" sets a literal font size.",
                "Set a Leonardo text style instead, for example Style = (Style)Application.Current.Resources[\"LeonardoBodyTextStyle\"].");
        }

        foreach (Match match in CornerRadiusRegex().Matches(text))
        {
            if (Vocabulary.TryParseNumbers(match.Groups["values"].Value, out var radii) && radii.Any(r => r != 0))
            {
                findings.Add(match.Index, Rules.LiteralCornerRadius, $"\"{Vocabulary.Excerpt(match.Value)}\" is a literal radius.",
                    "Use the LeonardoRadiusS, LeonardoRadiusM or LeonardoRadiusL resource.");
            }
        }

        foreach (Match match in ThicknessRegex().Matches(text))
        {
            if (Vocabulary.TryParseNumbers(match.Groups["values"].Value, out var spacing) && spacing.Any(s => s != 0))
            {
                findings.Add(match.Index, Rules.LiteralSpacing, $"\"{Vocabulary.Excerpt(match.Value)}\" is literal spacing.",
                    "Use a LeonardoSpace…Thickness resource, or LeonardoTokens.Space values.");
            }
        }

        foreach (Match match in SpacingPropertyRegex().Matches(text))
        {
            if (Vocabulary.TryParseNumbers(match.Groups["value"].Value, out var spacing) && spacing.Any(s => s != 0))
            {
                findings.Add(match.Index, Rules.LiteralSpacing, $"\"{Vocabulary.Excerpt(match.Value)}\" is literal spacing.",
                    "Use LeonardoTokens.Space.XS … XL or the LeonardoSpace… resources.");
            }
        }

        foreach (Match match in TimeSpanRegex().Matches(text))
        {
            var lineText = source.MaskedLine(source.LineOf(match.Index));
            if (AnimationContextRegex().IsMatch(lineText) && match.Groups["value"].Value.Any(c => c is >= '1' and <= '9'))
            {
                findings.Add(match.Index, Rules.LiteralDuration, $"\"{Vocabulary.Excerpt(match.Value)}\" is a literal animation time.", DurationSuggestion);
            }
        }

        foreach (Match match in EasingRegex().Matches(text))
        {
            var rule = match.Groups["bounce"].Success ? Rules.SpringBounce : Rules.LiteralEasing;
            findings.Add(match.Index, rule, $"\"{Vocabulary.Excerpt(match.Value)}\" is not a Leonardo easing curve.", EasingSuggestion);
        }

        foreach (Match match in GlassRegex().Matches(text))
        {
            findings.Add(match.Index, Rules.CustomGlass, $"\"{match.Value}\" creates a custom glass or blur.",
                "Use MicaBackdrop on the window and Fluent's own flyouts and menus.");
        }

        foreach (Match match in FocusRegex().Matches(text))
        {
            findings.Add(match.Index, Rules.FocusVisual, $"\"{Vocabulary.Excerpt(match.Value)}\" removes or restyles the focus visual.",
                "Keep the platform's focus visuals: remove this setting.");
        }
    }


    [GeneratedRegex(@"\b(?:Color|ColorHelper)\.From(?:Argb|Rgb|ScRgb)\s*\(\s*(?:0x|\d|\(byte\))[^;]*|\bnew\s+(?:Windows\.UI\.|System\.Windows\.Media\.)?Color\s*\{\s*[ARGB]\s*=\s*(?:0x|\d)")]
    private static partial Regex ColorApiRegex();

    [GeneratedRegex(@"\b(?:Microsoft\.UI\.|Windows\.UI\.|System\.Windows\.Media\.)?Colors\.(?<name>[A-Z]\w*)\b")]
    private static partial Regex NamedColorRegex();

    [GeneratedRegex(@"""#(?:[0-9A-Fa-f]{3,4}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})""")]
    private static partial Regex HexStringRegex();

    [GeneratedRegex(@"\bFontSize\s*=(?!=)\s*-?\d+(?:\.\d+)?")]
    private static partial Regex FontSizeRegex();

    [GeneratedRegex(@"\bnew\s+CornerRadius\s*\((?<values>[\d.\s,]+)\)|\bCornerRadius\s*=(?!=)\s*(?<values>\d+(?:\.\d+)?)\b")]
    private static partial Regex CornerRadiusRegex();

    [GeneratedRegex(@"\bnew\s+Thickness\s*\((?<values>[-\d.\s,]+)\)")]
    private static partial Regex ThicknessRegex();

    [GeneratedRegex(@"\b(?:Spacing|ColumnSpacing|RowSpacing)\s*=(?!=)\s*(?<value>\d+(?:\.\d+)?)\b")]
    private static partial Regex SpacingPropertyRegex();

    [GeneratedRegex(@"\bTimeSpan\.From(?:Milliseconds|Seconds|Ticks)\s*\(\s*(?<value>\d+(?:\.\d+)?)[dfm]?\s*\)")]
    private static partial Regex TimeSpanRegex();

    [GeneratedRegex(@"Duration|BeginTime|DelayTime|KeyTime|KeyFrame|Animation|Storyboard|Transition")]
    private static partial Regex AnimationContextRegex();

    [GeneratedRegex(@"\bnew\s+(?:(?<bounce>BackEase|BounceEase|ElasticEase)|CubicEase|SineEase|QuadraticEase|ExponentialEase|PowerEase|CircleEase|QuarticEase|QuinticEase)\b|\bnew\s+KeySpline\s*\{[^}]*?(?:Point|Vector2)\s*\(\s*-?\d|\bControlPoint[12]\s*=\s*new\s+(?:Point|Windows\.Foundation\.Point|Vector2)\s*\(\s*-?\d|\bCreateCubicBezierEasingFunction\s*\(\s*new\s+Vector2\s*\(\s*-?\d")]
    private static partial Regex EasingRegex();

    [GeneratedRegex(@"\b(?:AcrylicBrush|DesktopAcrylicController|DesktopAcrylicBackdrop|GaussianBlurEffect)\b|\bCreate(?:Host)?BackdropBrush\s*\(")]
    private static partial Regex GlassRegex();

    [GeneratedRegex(@"\bUseSystemFocusVisuals\s*=\s*false\b|\bFocusVisualStyle\s*=\s*null\b|\bFocusVisual(?:Primary|Secondary)(?:Thickness|Brush)\s*=(?!=)|\bFocusVisualMargin\s*=(?!=)")]
    private static partial Regex FocusRegex();
}
