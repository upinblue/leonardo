using System.Globalization;
using System.Text.RegularExpressions;

namespace UpInBlue.Leonardo.Check.Checkers;

/// <summary>
/// Checks SwiftUI, UIKit and AppKit code: literal colors (<c>Color(red:…)</c>, hex helpers), fixed font sizes instead
/// of text styles, literal animation durations, and springs or animations with bounce.
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

    [GeneratedRegex(@"\b(?:Color|UIColor|NSColor|CGColor)\s*\(\s*(?:\.\w+\s*,\s*)?(?:red|hue|white|displayP3Red|srgbRed|calibratedRed|deviceRed|genericGrayGamma2_2Gray)\s*:|\b(?:Color|UIColor|NSColor)\s*\(\s*hex\s*:|\bColor\s*\(\s*#colorLiteral|\binit\s*\(\s*hex\s*:|""#(?:[0-9A-Fa-f]{3,4}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})""|\b0x[0-9A-Fa-f]{6}\b(?=[^\n]*\b(?:Color|hex)\b)")]
    private static partial Regex ColorRegex();

    [GeneratedRegex(@"\.system\s*\(\s*size\s*:|\bFont\.system\s*\(\s*size\s*:|\.custom\s*\(\s*""[^""]*""\s*,\s*(?:fixedSize\s*:|size\s*:\s*[\d.]+\s*\))|\b(?:UIFont|NSFont)\.(?:systemFont|boldSystemFont|monospacedSystemFont)\s*\(\s*ofSize\s*:|\b(?:UIFont|NSFont)\s*\(\s*name\s*:[^)]*size\s*:")]
    private static partial Regex FontRegex();

    [GeneratedRegex(@"\.(?:easeIn|easeOut|easeInOut|linear|timingCurve)\s*\([^)]*?duration\s*:\s*\d+(?:\.\d+)?|\.(?:spring|smooth|snappy|bouncy)\s*\(\s*(?:duration|response)\s*:\s*\d+(?:\.\d+)?|\.delay\s*\(\s*\d+(?:\.\d+)?\s*\)|\bwithAnimation\s*\(\s*\.(?:easeIn|easeOut|easeInOut|linear)\s*\(\s*\d")]
    private static partial Regex DurationRegex();

    [GeneratedRegex(@"\.(?:spring|interactiveSpring)\s*\([^)]*?dampingFraction\s*:\s*(?<damping>\d+(?:\.\d+)?)|\.spring\s*\([^)]*?bounce\s*:\s*(?<bounce>-?\d+(?:\.\d+)?)|\.interpolatingSpring\s*\(|\.bouncy\b|\.snappy\s*\([^)]*extraBounce\s*:\s*(?<bounce>-?\d+(?:\.\d+)?)|\.smooth\s*\([^)]*extraBounce\s*:\s*(?<bounce>-?\d+(?:\.\d+)?)")]
    private static partial Regex BounceRegex();
}
