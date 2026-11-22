using UpInBlue.Leonardo.Tokens.Model;

namespace UpInBlue.Leonardo.Tokens.Generation;

/// <summary>
/// Generates the asset catalog and the Swift values for iOS and macOS.
/// </summary>
internal static class AppleGenerator
{
    public const string Directory = "generated/apple";
    public const string CatalogPath = $"{Directory}/Leonardo.xcassets";
    public const string SwiftPath = $"{Directory}/LeonardoTokens.swift";
    public const string ReadmePath = $"{Directory}/README.md";

    /// <summary>
    /// The SwiftUI <c>Font.TextStyle</c> cases a role may map to.
    /// </summary>
    private static readonly HashSet<string> TextStyles = new(StringComparer.Ordinal)
    {
        "largeTitle", "title", "title2", "title3", "headline", "subheadline", "body", "callout", "footnote", "caption", "caption2",
    };

    /// <summary>
    /// Not a text style: the role is the window title in the toolbar, drawn by the system.
    /// </summary>
    private const string WindowTitle = "windowTitle";

    public static IEnumerable<GeneratedFile> Generate(TokenCatalog catalog)
    {
        yield return GeneratedFile.FromText(ReadmePath, Readme());
        yield return GeneratedFile.FromText($"{CatalogPath}/Contents.json", Contents(providesNamespace: false));
        yield return GeneratedFile.FromText($"{CatalogPath}/Leonardo/Contents.json", Contents(providesNamespace: true));

        var accent = catalog.ThemePairs.FirstOrDefault(pair => pair.Light.Name == "accent");
        if (accent.Light is null)
        {
            throw new TokenException("The theme color 'accent' is required for the AccentColor of the asset catalog.");
        }

        yield return GeneratedFile.FromText($"{CatalogPath}/AccentColor.colorset/Contents.json", ColorSet(accent.Light.Color, accent.Dark.Color));

        foreach (var (light, dark) in catalog.ThemePairs)
        {
            yield return GeneratedFile.FromText($"{CatalogPath}/Leonardo/{Text.Pascal(light.Name)}.colorset/Contents.json", ColorSet(light.Color, dark.Color));
        }

        yield return GeneratedFile.FromText(SwiftPath, Swift(catalog));
    }

    private static string Readme()
        => new CodeWriter()
            .Line($"<!-- {Text.Header} -->")
            .Line()
            .Line("# Leonardo for iOS and macOS")
            .Line()
            .Line("- `Leonardo.xcassets`: one color set per theme color in the folder `Leonardo` (for example `Leonardo/Accent`), with light and dark appearance, plus `AccentColor`. Add the catalog to the app target. JSON files cannot carry the generated-file header; this note stands for them.")
            .Line("- `LeonardoTokens.swift`: the namespace enum `Leonardo` with colors, spacing, radii, densities, spring parameters and the type roles as SwiftUI text styles.")
            .ToString();

    private static string Contents(bool providesNamespace)
    {
        var writer = new CodeWriter()
            .Line("{")
            .Line("  \"info\" : {")
            .Line("    \"author\" : \"xcode\",")
            .Line("    \"version\" : 1")
            .Line(providesNamespace ? "  }," : "  }");
        if (providesNamespace)
        {
            writer
                .Line("  \"properties\" : {")
                .Line("    \"provides-namespace\" : true")
                .Line("  }");
        }

        return writer.Line("}").ToString();
    }

    private static string ColorSet(RgbColor light, RgbColor dark)
    {
        var writer = new CodeWriter()
            .Line("{")
            .Line("  \"colors\" : [")
            .Line("    {");
        ColorEntry(writer, light);
        writer
            .Line("      \"idiom\" : \"universal\"")
            .Line("    },")
            .Line("    {")
            .Line("      \"appearances\" : [")
            .Line("        {")
            .Line("          \"appearance\" : \"luminosity\",")
            .Line("          \"value\" : \"dark\"")
            .Line("        }")
            .Line("      ],");
        ColorEntry(writer, dark);
        return writer
            .Line("      \"idiom\" : \"universal\"")
            .Line("    }")
            .Line("  ],")
            .Line("  \"info\" : {")
            .Line("    \"author\" : \"xcode\",")
            .Line("    \"version\" : 1")
            .Line("  }")
            .Line("}")
            .ToString();
    }

    private static void ColorEntry(CodeWriter writer, RgbColor color)
        => writer
            .Line("      \"color\" : {")
            .Line("        \"color-space\" : \"srgb\",")
            .Line("        \"components\" : {")
            .Line($"          \"alpha\" : \"{(color.A / 255.0).ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)}\",")
            .Line($"          \"blue\" : \"0x{color.B:X2}\",")
            .Line($"          \"green\" : \"0x{color.G:X2}\",")
            .Line($"          \"red\" : \"0x{color.R:X2}\"")
            .Line("        }")
            .Line("      },");

    private static string Swift(TokenCatalog catalog)
    {
        var writer = new CodeWriter();
        writer.Line($"// {Text.Header}");
        writer.Line();
        writer.Line("import SwiftUI");
        writer.Line();
        writer.Line("/// The values of the Leonardo design tokens for iOS and macOS.");
        writer.Line("public enum Leonardo {").Indent();

        writer.Line("/// Theme colors from the asset catalog `Leonardo.xcassets`, with light and dark appearance.");
        writer.Line("public enum Colors {").Indent();
        foreach (var token in catalog.Light)
        {
            Comment(writer, null, $"theme.*.{token.Name}");
            writer.Line($"public static let {token.Name} = Color(\"Leonardo/{Text.Pascal(token.Name)}\")");
        }

        writer.Outdent().Line("}");
        writer.Line();

        PointEnum(writer, "Space", "Spacing in points. " + catalog.Tokens.GroupDescription("space"), catalog.Space);
        writer.Line();
        PointEnum(writer, "Radius", "Corner radii in points. " + catalog.Tokens.GroupDescription("radius"), catalog.Radius);
        writer.Line();
        PointEnum(writer, "Density", "List row heights in points. " + catalog.Tokens.GroupDescription("density"), catalog.Density);
        writer.Line();

        writer.Line("/// Motion on iOS and macOS: springs without bounce.");
        writer.Line("public enum Motion {").Indent();
        writer.Line("/// Spring parameters, for example `.spring(duration: Leonardo.Motion.Spring.durationMin, bounce: Leonardo.Motion.Spring.bounce)`.");
        writer.Line("public enum Spring {").Indent();
        foreach (var token in catalog.IosSpring)
        {
            Comment(writer, token.Description, token.Path, token.IsProposal);
            writer.Line(token.Type switch
            {
                TokenType.Duration => $"public static let {token.Name}: TimeInterval = {Text.Number(token.Duration.Milliseconds / 1000)}",
                TokenType.Number => $"public static let {token.Name}: Double = {Text.Number(token.Number)}",
                _ => throw new TokenException($"Token '{token.Path}' must be a duration or a number."),
            });
        }

        writer.Outdent().Line("}");
        foreach (var motion in catalog.Motion.Where(motion => motion.Group == "stagger"))
        {
            var token = motion.Token;
            var name = "stagger" + Text.Pascal(token.Name);
            writer.Line();
            Comment(writer, token.Description, token.Path, token.IsProposal);
            writer.Line(token.Type switch
            {
                TokenType.Duration => $"public static let {name}: TimeInterval = {Text.Number(token.Duration.Milliseconds / 1000)}",
                TokenType.Number when Text.IsWhole(token.Number) => $"public static let {name}: Int = {Text.Number(token.Number)}",
                TokenType.Number => $"public static let {name}: Double = {Text.Number(token.Number)}",
                _ => throw new TokenException($"Token '{token.Path}' must be a duration or a number."),
            });
        }

        writer.Outdent().Line("}");
        writer.Line();

        writer.Line("/// The type roles as system text styles. The system decides the size, so roles follow Dynamic Type.");
        writer.Line("public enum TextStyle {").Indent();
        var first = true;
        foreach (var role in catalog.TypeRoles)
        {
            if (!first)
            {
                writer.Line();
            }

            first = false;
            WriteTextStyle(writer, role);
        }

        writer.Outdent().Line("}");
        writer.Outdent().Line("}");
        return writer.ToString();
    }

    private static void WriteTextStyle(CodeWriter writer, TypeRole role)
    {
        var ios = role.Ios?.TextStyle;
        var macos = role.MacOS?.TextStyle;
        Validate(role, ios);
        Validate(role, macos);

        if (ios == macos)
        {
            Declare(writer, role.Name, ios, role.Ios?.Description);
            return;
        }

        writer.Line("#if os(macOS)");
        Declare(writer, role.Name, macos, role.MacOS?.Description);
        writer.Line("#else");
        Declare(writer, role.Name, ios, role.Ios?.Description);
        writer.Line("#endif");
    }

    private static void Declare(CodeWriter writer, string role, string? textStyle, string? description)
    {
        var source = $"typography.{role}";
        if (textStyle is null)
        {
            writer.Line($"// {role}: not used on this platform. Token: {source}.");
            return;
        }

        if (textStyle == WindowTitle)
        {
            var explanation = description ?? "The window title in the toolbar, drawn by the system.";
            writer.Line($"// {role}: {explanation} No text style. Token: {source}.");
            return;
        }

        Comment(writer, description, source);
        writer.Line($"public static let {role}: Font.TextStyle = .{textStyle}");
    }

    private static void Validate(TypeRole role, string? textStyle)
    {
        if (textStyle is not null && textStyle != WindowTitle && !TextStyles.Contains(textStyle))
        {
            throw new TokenException($"The type role '{role.Name}' uses '{textStyle}', which is not a SwiftUI text style.");
        }
    }

    private static void PointEnum(CodeWriter writer, string name, string description, IReadOnlyList<Token> tokens)
    {
        writer.Line($"/// {description.Trim()}");
        writer.Line($"public enum {name} {{").Indent();
        foreach (var token in tokens)
        {
            Comment(writer, token.Description, token.Path, token.IsProposal);
            writer.Line($"public static let {token.Name}: CGFloat = {Text.Number(token.Dimension.Value)}");
        }

        writer.Outdent().Line("}");
    }

    private static void Comment(CodeWriter writer, string? description, string source, bool isProposal = false)
    {
        var text = description is null ? $"Token: `{source}`." : $"{description} Token: `{source}`.";
        writer.Line($"/// {text}{(isProposal ? " Proposal, not final yet." : string.Empty)}");
    }
}
