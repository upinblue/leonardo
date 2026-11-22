using System.Text.RegularExpressions;

namespace UpInBlue.Leonardo.Check.Checkers;

/// <summary>
/// Checks CSS, SCSS, HTML and TSX: literal colors, font sizes and spacing (margin, padding, gap) instead of
/// <c>--leonardo-*</c> properties, literal motion timing, transitions without a <c>prefers-reduced-motion</c> rule, <c>outline: none</c> without a
/// replacement, custom glass, and clickable <c>div</c> elements.
/// </summary>
internal sealed partial class WebChecker(WebLanguage language) : IChecker
{
    private static readonly HashSet<string> ColorProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "color", "background", "background-color", "background-image", "border", "border-color", "border-top",
        "border-right", "border-bottom", "border-left", "border-block", "border-inline", "border-block-start",
        "border-block-end", "border-inline-start", "border-inline-end", "border-top-color", "border-right-color",
        "border-bottom-color", "border-left-color", "outline", "outline-color", "fill", "stroke", "box-shadow",
        "text-shadow", "text-decoration", "text-decoration-color", "caret-color", "accent-color", "column-rule",
        "column-rule-color", "scrollbar-color",
    };

    private static readonly HashSet<string> MotionProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "transition", "transition-duration", "transition-delay", "transition-timing-function",
        "animation", "animation-duration", "animation-delay", "animation-timing-function",
    };

    private const string ColorSuggestion =
        "Use a Leonardo custom property, for example var(--leonardo-text-primary), var(--leonardo-accent) or var(--leonardo-status-critical); in forced colors, system colors such as CanvasText.";

    private static string ColorSuggestionFor(string property)
    {
        var example = property.ToLowerInvariant() switch
        {
            "color" => "var(--leonardo-text-primary), var(--leonardo-text-secondary) or var(--leonardo-text-tertiary)",
            "background" or "background-color" => "var(--leonardo-surface) or var(--leonardo-surface-alt)",
            var p when p.StartsWith("border", StringComparison.Ordinal) || p.StartsWith("outline", StringComparison.Ordinal)
                => "var(--leonardo-stroke-subtle) or var(--leonardo-stroke-strong)",
            _ => null,
        };

        return example is null
            ? ColorSuggestion
            : $"Use a Leonardo custom property such as {example}; status colors are var(--leonardo-status-*); in forced colors, system colors such as CanvasText.";
    }

    private const string FontSuggestion =
        "Use a Leonardo type role: font-size: var(--leonardo-body-font-size) (or place-name, view, section, meta) together with its line-height and font-weight.";

    private const string MotionSuggestion =
        "Use var(--leonardo-duration-normal) (or fade, short, long) and var(--leonardo-ease-enter) (or exit, move, fade); leonardo.css turns them into a crossfade under prefers-reduced-motion.";

    public void Check(SourceText source, FindingList findings)
    {
        var css = language switch
        {
            WebLanguage.Html => SourceText.KeepOnly(source.Masked, StyleRanges(source.Masked)),
            WebLanguage.Tsx => SourceText.KeepOnly(source.Masked, TemplateRanges(source.Masked)),
            _ => source.Masked,
        };

        var hasReducedMotionRule = source.Masked.Contains("prefers-reduced-motion", StringComparison.OrdinalIgnoreCase);
        var blocks = BlockRegex().Matches(css).Select(b => (Selector: b.Groups["selector"].Value, Body: b.Groups["body"].Value, Start: b.Groups["body"].Index, End: b.Groups["body"].Index + b.Groups["body"].Length)).ToList();
        var hasFocusVisibleReplacement = blocks.Any(b => b.Selector.Contains(":focus", StringComparison.OrdinalIgnoreCase)
            && VisibleFocusRegex().IsMatch(b.Body));

        foreach (Match declaration in DeclarationRegex().Matches(css))
        {
            var property = declaration.Groups["property"].Value;
            var value = declaration.Groups["value"].Value.Trim();
            CheckDeclaration(property, value, declaration.Groups["value"].Index, hasReducedMotionRule, hasFocusVisibleReplacement, blocks, findings);
        }

        if (language is WebLanguage.Tsx)
        {
            CheckStyleObjects(source.Masked, hasReducedMotionRule, findings);
        }

        if (language is WebLanguage.Html or WebLanguage.Tsx)
        {
            foreach (Match tag in ClickableTagRegex().Matches(source.Masked))
            {
                findings.Add(tag.Index, Rules.ClickableDiv, $"<{tag.Groups["name"].Value}> handles clicks ({tag.Groups["handler"].Value}).",
                    "Use <button type=\"button\"> for actions and <a href> for navigation; they come with keyboard support, focus and the right role.");
            }
        }
    }

    private static void CheckDeclaration(string property, string value, int offset, bool hasReducedMotionRule, bool hasFocusVisibleReplacement,
        List<(string Selector, string Body, int Start, int End)> blocks, FindingList findings)
    {
        var isCustomProperty = property.StartsWith("--", StringComparison.Ordinal) || property.StartsWith('$');
        var withoutVariables = VariableRegex().Replace(value, " ");

        if ((ColorProperties.Contains(property) || isCustomProperty) && FindColor(withoutVariables) is { } color)
        {
            findings.Add(offset, Rules.LiteralColor, $"{property}: {Vocabulary.Excerpt(value)} uses the literal color {color}.", ColorSuggestionFor(property));
        }

        if ((property.Equals("font-size", StringComparison.OrdinalIgnoreCase) && LengthRegex().IsMatch(withoutVariables))
            || (property.Equals("font", StringComparison.OrdinalIgnoreCase) && FontShorthandSizeRegex().IsMatch(withoutVariables)))
        {
            findings.Add(offset, Rules.LiteralFontSize, $"{property}: {Vocabulary.Excerpt(value)} is a literal font size.", FontSuggestion);
        }

        if (IsSpacingProperty(property) && FindSpacingLiteral(withoutVariables) is { } spacing)
        {
            findings.Add(offset, Rules.LiteralSpacing, $"{property}: {Vocabulary.Excerpt(value)} uses the literal spacing {spacing.Text}.",
                spacing.Token is null
                    ? "Use var(--leonardo-space-xs), -s, -m, -l or -xl, or calc() with them; spacing comes only from space.xs to space.xl. 0 and 1px hairlines are allowed."
                    : $"Use var(--leonardo-space-{spacing.Token}).");
        }

        if (property.Equals("outline", StringComparison.OrdinalIgnoreCase) && OutlineNoneRegex().IsMatch(value)
            || property.Equals("outline-style", StringComparison.OrdinalIgnoreCase) && value.Equals("none", StringComparison.OrdinalIgnoreCase)
            || property.Equals("outline-width", StringComparison.OrdinalIgnoreCase) && ZeroRegex().IsMatch(value))
        {
            var block = blocks.FirstOrDefault(b => offset >= b.Start && offset < b.End);
            var replacedHere = block.Body is not null
                && (block.Selector.Contains(":not(:focus-visible)", StringComparison.OrdinalIgnoreCase) || ReplacementRegex().IsMatch(block.Body));
            if (!replacedHere && !hasFocusVisibleReplacement)
            {
                findings.Add(offset, Rules.FocusVisual, $"{property}: {value} removes the focus outline without a replacement.",
                    "Keep the browser's focus ring. Where it has less than 3:1 contrast, replace it on :focus-visible with outline: var(--leonardo-size-focus-stroke) solid var(--leonardo-focus).");
            }
        }

        if (property.Contains("backdrop-filter", StringComparison.OrdinalIgnoreCase) && !value.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            findings.Add(offset, Rules.CustomGlass, $"{property}: {Vocabulary.Excerpt(value)} imitates glass.",
                "Use an opaque Leonardo surface such as var(--leonardo-surface-alt); the standalone web has no glass.");
        }

        if (MotionProperties.Contains(property) && !value.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            var literalTiming = LiteralTimeRegex().IsMatch(withoutVariables) || LiteralEasingRegex().IsMatch(withoutVariables);
            if (literalTiming)
            {
                findings.Add(offset, Rules.LiteralDuration, $"{property}: {Vocabulary.Excerpt(value)} uses literal timing.", MotionSuggestion);
            }

            var usesOnlyLeonardo = !literalTiming && !OtherVariableRegex().IsMatch(value);
            if (!hasReducedMotionRule && !usesOnlyLeonardo && (literalTiming || value.Contains("var(", StringComparison.Ordinal)))
            {
                findings.Add(offset, Rules.ReducedMotion, $"{property}: {Vocabulary.Excerpt(value)} has no prefers-reduced-motion rule.",
                    "Use the --leonardo-duration-* and --leonardo-ease-* properties, which leonardo.css reduces to a crossfade, or add @media (prefers-reduced-motion: reduce).");
            }
        }
    }

    private static void CheckStyleObjects(string text, bool hasReducedMotionRule, FindingList findings)
    {
        foreach (Match match in ObjectColorRegex().Matches(text))
        {
            if (FindColor(match.Groups["value"].Value) is { } color)
            {
                findings.Add(match.Groups["value"].Index, Rules.LiteralColor, $"{match.Groups["property"].Value}: '{match.Groups["value"].Value}' uses the literal color {color}.", ColorSuggestion);
            }
        }

        foreach (Match match in HexStringRegex().Matches(text))
        {
            findings.Add(match.Index, Rules.LiteralColor, $"{match.Value} is a literal color.", ColorSuggestion);
        }

        foreach (Match match in ObjectFontSizeRegex().Matches(text))
        {
            findings.Add(match.Index, Rules.LiteralFontSize, $"\"{Vocabulary.Excerpt(match.Value)}\" is a literal font size.", FontSuggestion);
        }

        foreach (Match match in ObjectTransitionRegex().Matches(text))
        {
            var value = match.Groups["value"].Value;
            if (LiteralTimeRegex().IsMatch(value))
            {
                findings.Add(match.Groups["value"].Index, Rules.LiteralDuration, $"{match.Groups["property"].Value}: '{value}' uses literal timing.", MotionSuggestion);
                if (!hasReducedMotionRule)
                {
                    findings.Add(match.Groups["value"].Index, Rules.ReducedMotion, $"{match.Groups["property"].Value}: '{value}' has no prefers-reduced-motion rule.",
                        "Use the --leonardo-duration-* and --leonardo-ease-* properties, or respect prefers-reduced-motion.");
                }
            }
        }
    }

    /// <summary>
    /// Margins, paddings and gaps, including their longhands (<c>margin-inline-start</c>, <c>padding-top</c>,
    /// <c>row-gap</c> …) and the older <c>grid-gap</c> names.
    /// </summary>
    private static bool IsSpacingProperty(string property)
    {
        var name = property.ToLowerInvariant();
        return name.StartsWith("margin", StringComparison.Ordinal)
            || name.StartsWith("padding", StringComparison.Ordinal)
            || name is "gap" or "row-gap" or "column-gap" or "grid-gap" or "grid-row-gap" or "grid-column-gap";
    }

    /// <summary>
    /// The first px, rem or em length in a spacing value that is neither 0 nor a 1px hairline, with the matching
    /// spacing token (<c>xs</c> … <c>xl</c>) if its value is one; or null.
    /// </summary>
    private static (string Text, string? Token)? FindSpacingLiteral(string value)
    {
        foreach (Match match in SpacingLengthRegex().Matches(value))
        {
            var number = double.Parse(match.Groups["number"].Value, System.Globalization.CultureInfo.InvariantCulture);
            var unit = match.Groups["unit"].Value.ToLowerInvariant();
            if (number == 0 || (unit == "px" && Vocabulary.IsAllowedSpacingLiteral(number)))
            {
                continue;
            }

            var pixels = unit == "px" ? Math.Abs(number) : Math.Abs(number) * 16;
            var token = Vocabulary.Spacing.TryGetValue(pixels, out var step) ? step.ToLowerInvariant() : null;
            return (match.Value, token);
        }

        return null;
    }

    private static string? FindColor(string value)
    {
        var hex = HexRegex().Match(value);
        if (hex.Success)
        {
            return hex.Value;
        }

        var function = ColorFunctionRegex().Match(value);
        if (function.Success)
        {
            return function.Value + "…)";
        }

        foreach (Match word in WordRegex().Matches(value))
        {
            if (Vocabulary.NamedColors.Contains(word.Value))
            {
                return word.Value;
            }
        }

        return null;
    }

    private static IEnumerable<(int Start, int End)> StyleRanges(string html)
    {
        foreach (Match match in StyleElementRegex().Matches(html))
        {
            yield return (match.Groups["css"].Index, match.Groups["css"].Index + match.Groups["css"].Length);
        }

        foreach (Match match in StyleAttributeRegex().Matches(html))
        {
            yield return (match.Groups["css"].Index, match.Groups["css"].Index + match.Groups["css"].Length);
        }
    }

    private static IEnumerable<(int Start, int End)> TemplateRanges(string code)
        => TemplateLiteralRegex().Matches(code).Select(m => (m.Groups["css"].Index, m.Groups["css"].Index + m.Groups["css"].Length));

    [GeneratedRegex(@"`(?<css>[^`]*)`", RegexOptions.Singleline)]
    private static partial Regex TemplateLiteralRegex();

    [GeneratedRegex(@"(?<![\w-])(?<property>--[\w-]+|\$[\w-]+|-?[a-zA-Z][\w-]*)\s*:\s*(?<value>[^;{}<>""]+?)\s*(?=;|}|""|$)", RegexOptions.Multiline)]
    private static partial Regex DeclarationRegex();

    [GeneratedRegex(@"(?<selector>[^{}]+)\{(?<body>[^{}]*)\}")]
    private static partial Regex BlockRegex();

    [GeneratedRegex(@"var\([^()]*(?:\([^()]*\)[^()]*)*\)")]
    private static partial Regex VariableRegex();

    [GeneratedRegex(@"var\(\s*--(?!leonardo-)")]
    private static partial Regex OtherVariableRegex();

    [GeneratedRegex(@"#(?:[0-9A-Fa-f]{8}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{3,4})\b")]
    private static partial Regex HexRegex();

    [GeneratedRegex(@"\b(?:rgba?|hsla?|hwb|lab|lch|oklab|oklch|color)\(", RegexOptions.IgnoreCase)]
    private static partial Regex ColorFunctionRegex();

    [GeneratedRegex(@"(?<![\w-])[A-Za-z]+(?![\w-])")]
    private static partial Regex WordRegex();

    [GeneratedRegex(@"(?<![\w-])\d*\.?\d+(?:px|pt|pc|rem|em|ex|ch|vw|vh|vmin|vmax|cm|mm|in)?(?![\w%-])")]
    private static partial Regex LengthRegex();

    [GeneratedRegex(@"(?<![\w-])\d*\.?\d+(?:px|pt|pc|rem|em|ex|ch|vw|vh)(?![\w-])")]
    private static partial Regex FontShorthandSizeRegex();

    [GeneratedRegex(@"(?<![\w.])-?(?<number>\d*\.?\d+)(?<unit>px|rem|em)(?![\w-])", RegexOptions.IgnoreCase)]
    private static partial Regex SpacingLengthRegex();

    [GeneratedRegex(@"^(?:none|0)(?:\s|$)", RegexOptions.IgnoreCase)]
    private static partial Regex OutlineNoneRegex();

    [GeneratedRegex(@"^0(?:px)?$")]
    private static partial Regex ZeroRegex();

    [GeneratedRegex(@"\bbox-shadow\s*:(?!\s*none\b)|\b(?:border(?:-[\w-]+)?|outline-(?:color|width|style|offset))\s*:(?!\s*(?:none|0)\b)", RegexOptions.IgnoreCase)]
    private static partial Regex ReplacementRegex();

    [GeneratedRegex(@"\bbox-shadow\s*:(?!\s*none\b)|\boutline\s*:(?!\s*(?:none|0)\b)", RegexOptions.IgnoreCase)]
    private static partial Regex VisibleFocusRegex();

    [GeneratedRegex(@"(?<![\w.-])(?:\d*\.\d+|[1-9]\d*)m?s(?![\w-])")]
    private static partial Regex LiteralTimeRegex();

    [GeneratedRegex(@"\bcubic-bezier\(|(?<![\w-])(?:ease|ease-in|ease-out|ease-in-out|linear|step-start|step-end)(?![\w-])|\bsteps\(", RegexOptions.IgnoreCase)]
    private static partial Regex LiteralEasingRegex();

    [GeneratedRegex(@"<style\b[^>]*>(?<css>.*?)</style>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex StyleElementRegex();

    [GeneratedRegex(@"\bstyle\s*=\s*""(?<css>[^""]*)""", RegexOptions.IgnoreCase)]
    private static partial Regex StyleAttributeRegex();

    [GeneratedRegex(@"<(?<name>div|span|li|td|tr|p|section|article|img|label)\b(?:[^>""'{]|""[^""]*""|'[^']*'|\{(?:[^{}]|\{[^{}]*\})*\})*?\b(?<handler>on[Cc]lick|onmousedown|onMouseDown|onpointerdown|onPointerDown)\s*=", RegexOptions.Singleline)]
    private static partial Regex ClickableTagRegex();

    [GeneratedRegex(@"\b(?<property>color|background|backgroundColor|borderColor|border|fill|stroke|outlineColor|boxShadow)\s*:\s*['""`](?<value>[^'""`]+)['""`]")]
    private static partial Regex ObjectColorRegex();

    [GeneratedRegex(@"['""`]#(?:[0-9A-Fa-f]{3,4}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})['""`]")]
    private static partial Regex HexStringRegex();

    [GeneratedRegex(@"\bfontSize\s*[:=]\s*(?:\{\s*)?['""`]?\d")]
    private static partial Regex ObjectFontSizeRegex();

    [GeneratedRegex(@"\b(?<property>transition|animation|transitionDuration|animationDuration)\s*:\s*['""`](?<value>[^'""`]+)['""`]")]
    private static partial Regex ObjectTransitionRegex();
}

/// <summary>
/// The web languages the <see cref="WebChecker"/> reads.
/// </summary>
internal enum WebLanguage
{
    /// <summary>CSS or SCSS.</summary>
    Css,

    /// <summary>HTML: the CSS in <c>style</c> elements and attributes, and clickable elements.</summary>
    Html,

    /// <summary>TSX: CSS in template literals, style objects, and clickable elements.</summary>
    Tsx,
}
