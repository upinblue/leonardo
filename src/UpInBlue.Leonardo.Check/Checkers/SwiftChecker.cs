using System.Globalization;
using System.Text.RegularExpressions;

namespace UpInBlue.Leonardo.Check.Checkers;

/// <summary>
/// Checks SwiftUI, UIKit and AppKit code: literal colors (<c>Color(red:…)</c>, hex helpers), fixed font sizes instead
/// of text styles, literal spacing (<c>.padding(15)</c>, <c>spacing: 12</c>, <c>EdgeInsets(top: 8, …)</c>), literal
/// animation durations, and springs or animations with bounce.
/// </summary>
internal sealed partial class SwiftChecker : IChecker
{
    private const string ColorSuggestion =
        "Use Leonardo.Colors (for example Leonardo.Colors.textPrimary) from the generated asset catalog, the app's accent color, or a system color for status (.red, .yellow, .green).";

    private const string DurationSuggestion =
        "Use a spring without bounce: .spring(duration: Leonardo.Motion.Spring.durationMin, bounce: Leonardo.Motion.Spring.bounce); stagger with Leonardo.Motion.staggerStep.";

    public void Check(SourceText source, FindingList findings)
    {
        var text = source.Code;

        foreach (Match match in ColorRegex().Matches(source.Masked))
        {
            findings.Add(match.Index, Rules.LiteralColor, $"\"{source.Snippet(match.Index)}\" is a literal color.", ColorSuggestion);
        }

        foreach (Match match in FontRegex().Matches(text))
        {
            findings.Add(match.Index, Rules.LiteralFontSize, $"\"{source.Snippet(match.Index)}\" is a fixed font size.",
                "Use a text style so the size follows Dynamic Type: .font(.system(Leonardo.TextStyle.body)), or .font(.body); for a custom font, .custom(name, size:, relativeTo:).");
        }

        foreach (Match match in SpacingRegex().Matches(text))
        {
            var end = match.Index + match.Length;
            var closed = match.Value.Contains('(') && text.AsSpan(end).TrimStart().StartsWith(")") ? match.Value + ")" : match.Value;
            AddSpacing(match.Groups["value"], match.Index, findings, $"\"{Vocabulary.Excerpt(closed)}\"");
        }

        foreach (Match match in InsetsRegex().Matches(text))
        {
            foreach (Match argument in InsetArgumentRegex().Matches(match.Groups["args"].Value))
            {
                AddSpacing(argument.Groups["value"], match.Index, findings, $"\"{Vocabulary.Excerpt(match.Value)}\"");
            }
        }

        foreach (Match match in DurationRegex().Matches(text))
        {
            findings.Add(match.Index, Rules.LiteralDuration, $"\"{Vocabulary.Excerpt(match.Value)}\" is a literal animation duration.", DurationSuggestion);
        }

        foreach (Match match in BounceRegex().Matches(text))
        {
            if (match.Groups["damping"].Success
                && double.TryParse(match.Groups["damping"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var damping)
                && damping >= 1)
            {
                continue;
            }

            if (match.Groups["bounce"].Success
                && double.TryParse(match.Groups["bounce"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var bounce)
                && bounce <= 0)
            {
                continue;
            }

            findings.Add(match.Index, Rules.SpringBounce, $"\"{Vocabulary.Excerpt(match.Value)}\" bounces.",
                "Use a spring without bounce: .spring(duration: Leonardo.Motion.Spring.durationMin, bounce: Leonardo.Motion.Spring.bounce).");
        }
    }

    private static void AddSpacing(Group value, int offset, FindingList findings, string quote)
    {
        if (!double.TryParse(value.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
            || Vocabulary.IsAllowedSpacingLiteral(number))
        {
            return;
        }

        var step = Vocabulary.Spacing.TryGetValue(Math.Abs(number), out var name) ? $"Leonardo.Space.{name.ToLowerInvariant()}" : null;
        findings.Add(offset, Rules.LiteralSpacing, $"{quote} is literal spacing.",
            step is null
                ? "Use Leonardo.Space.xs, .s, .m, .l or .xl; spacing comes only from space.xs to space.xl. Inside system lists, keep the system insets."
                : $"Use {step}.");
    }

    [GeneratedRegex(@"\b(?:Color|UIColor|NSColor|CGColor)\s*\(\s*(?:\.\w+\s*,\s*)?(?:red|hue|white|displayP3Red|srgbRed|calibratedRed|deviceRed|genericGrayGamma2_2Gray)\s*:|\b(?:Color|UIColor|NSColor)\s*\(\s*hex\s*:|\bColor\s*\(\s*#colorLiteral|\binit\s*\(\s*hex\s*:|""#(?:[0-9A-Fa-f]{3,4}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})""|\b0x[0-9A-Fa-f]{6}\b(?=[^\n]*\b(?:Color|hex)\b)")]
    private static partial Regex ColorRegex();

    [GeneratedRegex(@"\.system\s*\(\s*size\s*:|\bFont\.system\s*\(\s*size\s*:|\.custom\s*\(\s*""[^""]*""\s*,\s*(?:fixedSize\s*:|size\s*:\s*[\d.]+\s*\))|\b(?:UIFont|NSFont)\.(?:systemFont|boldSystemFont|monospacedSystemFont)\s*\(\s*ofSize\s*:|\b(?:UIFont|NSFont)\s*\(\s*name\s*:[^)]*size\s*:")]
    private static partial Regex FontRegex();

    /// <summary>
    /// <c>.padding(15)</c>, <c>.padding(.horizontal, 12)</c>, <c>.padding([.top, .leading], 8)</c>, the same for
    /// <c>.safeAreaPadding</c> and <c>.contentMargins</c>, and <c>spacing: 12</c> in stacks, grids and insets.
    /// </summary>
    [GeneratedRegex(@"\.(?:padding|safeAreaPadding|contentMargins)\s*\(\s*(?:(?:\.\w+|\[[^\]]*\])\s*,\s*)?(?<value>-?\d+(?:\.\d+)?)(?=\s*[,)])|\b(?:spacing|horizontalSpacing|verticalSpacing)\s*:\s*(?<value>-?\d+(?:\.\d+)?)(?=\s*[,)])")]
    private static partial Regex SpacingRegex();

    /// <summary>
    /// Insets with literal values: <c>EdgeInsets(top: 8, leading: 16, bottom: 8, trailing: 16)</c> (also in
    /// <c>.listRowInsets</c>), <c>UIEdgeInsets</c>, <c>NSEdgeInsets</c> and <c>NSDirectionalEdgeInsets</c>.
    /// </summary>
    [GeneratedRegex(@"\b(?:EdgeInsets|UIEdgeInsets|NSEdgeInsets|NSDirectionalEdgeInsets)\s*\((?<args>[^()]*)\)")]
    private static partial Regex InsetsRegex();

    [GeneratedRegex(@"\w+\s*:\s*(?<value>-?\d+(?:\.\d+)?)(?=\s*(?:,|$))")]
    private static partial Regex InsetArgumentRegex();

    [GeneratedRegex(@"\.(?:easeIn|easeOut|easeInOut|linear|timingCurve)\s*\([^)]*?duration\s*:\s*\d+(?:\.\d+)?|\.(?:spring|smooth|snappy|bouncy)\s*\(\s*(?:duration|response)\s*:\s*\d+(?:\.\d+)?|\.delay\s*\(\s*\d+(?:\.\d+)?\s*\)|\bwithAnimation\s*\(\s*\.(?:easeIn|easeOut|easeInOut|linear)\s*\(\s*\d")]
    private static partial Regex DurationRegex();

    [GeneratedRegex(@"\.(?:spring|interactiveSpring)\s*\([^)]*?dampingFraction\s*:\s*(?<damping>\d+(?:\.\d+)?)|\.spring\s*\([^)]*?bounce\s*:\s*(?<bounce>-?\d+(?:\.\d+)?)|\.interpolatingSpring\s*\(|\.bouncy\b|\.snappy\s*\([^)]*extraBounce\s*:\s*(?<bounce>-?\d+(?:\.\d+)?)|\.smooth\s*\([^)]*extraBounce\s*:\s*(?<bounce>-?\d+(?:\.\d+)?)")]
    private static partial Regex BounceRegex();
}
