namespace UpInBlue.Leonardo.Check;

/// <summary>
/// A rule of the check tool.
/// </summary>
/// <param name="Id">The stable identifier, for example <c>LEO001</c>.</param>
/// <param name="Name">The short name, for example <c>literal-color</c>.</param>
/// <param name="Summary">What the rule finds, in one sentence.</param>
internal sealed record Rule(string Id, string Name, string Summary)
{
    /// <summary>
    /// The identifier and name as they appear in the output.
    /// </summary>
    public override string ToString() => $"{Id} {Name}";
}

/// <summary>
/// The rules. They follow the hard rules of the design language: no literal design values, no custom glass,
/// focus visuals stay, motion respects the system setting, and interactive elements are real controls.
/// </summary>
internal static class Rules
{
    public static readonly Rule LiteralColor = new("LEO001", "literal-color",
        "A color is written as a literal instead of coming from a Leonardo token, resource or system color.");

    public static readonly Rule LiteralFontSize = new("LEO002", "literal-font-size",
        "A font size is written as a number instead of using a Leonardo type role or a platform text style.");

    public static readonly Rule LiteralCornerRadius = new("LEO003", "literal-corner-radius",
        "A corner radius is written as a number instead of a Leonardo radius.");

    public static readonly Rule LiteralDuration = new("LEO004", "literal-duration",
        "An animation duration, delay or key time is written as a literal instead of a Leonardo duration.");

    public static readonly Rule LiteralEasing = new("LEO005", "literal-easing",
        "An easing curve or key spline is written as a literal, or a non-Leonardo easing function is used.");

    public static readonly Rule LiteralSpacing = new("LEO006", "literal-spacing",
        "A margin, padding or spacing is written as a number instead of a Leonardo spacing value.");

    public static readonly Rule CustomGlass = new("LEO007", "custom-glass",
        "A custom backdrop, acrylic or blur imitates glass; glass exists only where the platform provides it.");

    public static readonly Rule FocusVisual = new("LEO008", "focus-visual",
        "A focus visual is removed or restyled.");

    public static readonly Rule SpringBounce = new("LEO009", "spring-bounce",
        "A spring or easing overshoots; Leonardo motion never bounces.");

    public static readonly Rule ReducedMotion = new("LEO010", "reduced-motion",
        "A transition or animation has no prefers-reduced-motion rule and does not use Leonardo durations.");

    public static readonly Rule ClickableDiv = new("LEO011", "clickable-div",
        "A non-interactive element such as a div or span handles clicks instead of a button or link.");

    /// <summary>
    /// All rules, in order of their identifiers.
    /// </summary>
    public static IReadOnlyList<Rule> All { get; } =
    [
        LiteralColor, LiteralFontSize, LiteralCornerRadius, LiteralDuration, LiteralEasing, LiteralSpacing,
        CustomGlass, FocusVisual, SpringBounce, ReducedMotion, ClickableDiv,
    ];
}
