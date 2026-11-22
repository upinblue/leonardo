using UpInBlue.Leonardo.Tokens.Model;

namespace UpInBlue.Leonardo.Tokens.Generation;

/// <summary>
/// Generates <c>leonardo.css</c> with custom properties for standalone web.
/// </summary>
internal static class WebGenerator
{
    public const string Path = "generated/web/leonardo.css";

    private const string Prefix = "--leonardo-";

    public static string ColorProperty(string name) => $"{Prefix}{Text.Kebab(name)}";

    public static string MotionProperty(MotionToken motion)
        => $"{Prefix}{(motion.Group == "easing" ? "ease" : Text.Kebab(motion.Group))}-{Text.Kebab(motion.Token.Name)}";

    public static GeneratedFile Generate(TokenCatalog catalog)
    {
        var writer = new CodeWriter();
        writer.Line($"/* {Text.Header} */");
        writer.Line();
        writer.Line("/* Light theme and all values that do not depend on the theme. */");
        writer.Line(":root {").Indent();
        writer.Line("color-scheme: light;");
        writer.Line();
        writer.Line("/* Blue scale */");
        foreach (var token in catalog.Palette)
        {
            Property(writer, $"{Prefix}palette-{Text.Kebab(token.Name)}", token.Color.Hex, token);
        }

        writer.Line();
        writer.Line("/* Theme colors */");
        WriteThemeColors(writer, catalog.LightFor(OutputPlatform.Web));

        writer.Line();
        writer.Line("/* Spacing */");
        foreach (var token in catalog.Space)
        {
            Property(writer, $"{Prefix}space-{Text.Kebab(token.Name)}", Pixels(token), token);
        }

        writer.Line();
        writer.Line("/* Corner radii */");
        foreach (var token in catalog.Radius)
        {
            Property(writer, $"{Prefix}radius-{Text.Kebab(token.Name)}", Pixels(token), token);
        }

        writer.Line();
        writer.Line("/* List row heights */");
        foreach (var token in catalog.Density)
        {
            Property(writer, $"{Prefix}density-{Text.Kebab(token.Name)}", Pixels(token), token);
        }

        writer.Line();
        writer.Line("/* Fixed sizes */");
        foreach (var token in catalog.SizeFor(OutputPlatform.Web))
        {
            Property(writer, $"{Prefix}size-{Text.Kebab(token.Name)}", Pixels(token), token);
        }

        writer.Line();
        writer.Line("/* Type roles */");
        foreach (var role in catalog.TypeRoles)
        {
            if (role.Web is not { } style)
            {
                continue;
            }

            var prefix = $"{Prefix}{Text.Kebab(role.Name)}";
            writer.Line($"{prefix}-font-family: {FontFamily(style.FontFamily)};");
            writer.Line($"{prefix}-font-size: {Text.Number(style.FontSizeRem)}rem;");
            writer.Line($"{prefix}-line-height: {Text.Number(style.LineHeight)};");
            writer.Line($"{prefix}-font-weight: {style.FontWeight};");
            if (style.Color is { } color)
            {
                writer.Line($"{prefix}-color: var({ColorProperty(color)});");
            }
        }

        writer.Line();
        writer.Line("/* Motion */");
        foreach (var motion in catalog.Motion)
        {
            Property(writer, MotionProperty(motion), MotionValue(motion.Token), motion.Token);
        }

        writer.Outdent().Line("}");
        writer.Line();

        writer.Line("/* Dark theme: follows the system unless the page sets data-theme=\"light\". */");
        writer.Line("@media (prefers-color-scheme: dark) {").Indent();
        writer.Line(":root:not([data-theme=\"light\"]) {").Indent();
        writer.Line("color-scheme: dark;");
        WriteThemeColors(writer, catalog.DarkFor(OutputPlatform.Web));
        writer.Outdent().Line("}");
        writer.Outdent().Line("}");
        writer.Line();

        writer.Line("/* Dark theme chosen by the page. */");
        writer.Line("[data-theme=\"dark\"] {").Indent();
        writer.Line("color-scheme: dark;");
        WriteThemeColors(writer, catalog.DarkFor(OutputPlatform.Web));
        writer.Outdent().Line("}");
        writer.Line();

        WriteReducedMotion(writer, catalog);
        return GeneratedFile.FromText(Path, writer.ToString());
    }

    /// <summary>
    /// With reduced motion, every motion becomes a short crossfade: no distance, no parallax, no stagger,
    /// and every movement takes the fade duration with the linear fade curve.
    /// </summary>
    private static void WriteReducedMotion(CodeWriter writer, TokenCatalog catalog)
    {
        var fadeDuration = Find(catalog, "duration", "fade");
        var fadeEasing = Find(catalog, "easing", "fade");

        writer.Line("/* Reduced motion: every motion becomes a short crossfade. The meaning stays, the distance goes. */");
        writer.Line("@media (prefers-reduced-motion: reduce) {").Indent();
        writer.Line(":root {").Indent();
        foreach (var motion in catalog.Motion)
        {
            var property = MotionProperty(motion);
            var value = motion.Group switch
            {
                "easing" when motion != fadeEasing => $"var({MotionProperty(fadeEasing)})",
                "duration" when motion != fadeDuration => $"var({MotionProperty(fadeDuration)})",
                "distance" => "0px",
                "stagger" when motion.Token.Type == TokenType.Duration => "0ms",
                "parallax" => "1",
                _ => null,
            };

            if (value is not null)
            {
                writer.Line($"{property}: {value};");
            }
        }

        writer.Outdent().Line("}");
        writer.Outdent().Line("}");
    }

    private static MotionToken Find(TokenCatalog catalog, string group, string name)
        => catalog.Motion.FirstOrDefault(motion => motion.Group == group && motion.Token.Name == name)
            ?? throw new TokenException($"The token 'motion.{group}.{name}' is required for reduced motion on the web.");

    private static void WriteThemeColors(CodeWriter writer, IReadOnlyList<Token> tokens)
    {
        foreach (var token in tokens)
        {
            Property(writer, ColorProperty(token.Name), token.Color.Hex, token);
        }
    }

    private static void Property(CodeWriter writer, string property, string value, Token token)
        => writer.Line(token.IsProposal ? $"{property}: {value}; /* proposal */" : $"{property}: {value};");

    private static string Pixels(Token token) => $"{Text.Number(token.Dimension.Value)}px";

    private static string MotionValue(Token token) => token.Type switch
    {
        TokenType.CubicBezier => $"cubic-bezier({Text.Number(token.CubicBezier.X1)}, {Text.Number(token.CubicBezier.Y1)}, {Text.Number(token.CubicBezier.X2)}, {Text.Number(token.CubicBezier.Y2)})",
        TokenType.Duration => $"{Text.Number(token.Duration.Milliseconds)}ms",
        TokenType.Dimension => Pixels(token),
        TokenType.Number => Text.Number(token.Number),
        _ => throw new TokenException($"Motion token '{token.Path}' has the unsupported type {token.Type}."),
    };

    private static string FontFamily(string family) => $"\"{family}\", sans-serif";
}
