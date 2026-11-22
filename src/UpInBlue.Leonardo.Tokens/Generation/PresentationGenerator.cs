using UpInBlue.Leonardo.Tokens.Model;

namespace UpInBlue.Leonardo.Tokens.Generation;

/// <summary>
/// Generates a readable table of the values for building the PowerPoint template.
/// </summary>
internal static class PresentationGenerator
{
    public const string Path = "generated/presentation/presentation-tokens.md";

    public static GeneratedFile Generate(TokenCatalog catalog)
    {
        var writer = new CodeWriter();
        writer.Line($"<!-- {Text.Header} -->");
        writer.Line();
        writer.Line("# Leonardo values for PowerPoint");
        writer.Line();
        writer.Line("Use these values to build the PowerPoint template. Values marked *proposal* are not final yet.");
        writer.Line();

        writer.Line("## Theme colors");
        writer.Line();
        writer.Line("| Color | Light | Dark | Note |");
        writer.Line("| --- | --- | --- | --- |");
        foreach (var (light, dark) in catalog.ThemePairsFor(OutputPlatform.Presentation))
        {
            writer.Line($"| `{light.Name}` | {Value(light.Color.Hex, light)} | {Value(dark.Color.Hex, dark)} | {Note(light.Description ?? dark.Description)} |");
        }

        writer.Line();
        writer.Line("## Blue scale");
        writer.Line();
        writer.Line("Charts use the blue scale.");
        writer.Line();
        writer.Line("| Color | Hex | Use |");
        writer.Line("| --- | --- | --- |");
        foreach (var token in catalog.Palette)
        {
            writer.Line($"| `{token.Name}` | {Value(token.Color.Hex, token)} | {Note(token.Description)} |");
        }

        writer.Line();
        writer.Line("## Fonts");
        writer.Line();
        writer.Line("| Role | Typeface | Size | Weight |");
        writer.Line("| --- | --- | --- | --- |");
        foreach (var role in catalog.TypeRoles)
        {
            writer.Line(role.Presentation is { } style
                ? $"| `{role.Name}` | {Text.MarkdownCell(style.FontFamily)} | {Text.Number(style.FontSizePt)} pt | {Text.FontWeightName(style.FontWeight)} ({style.FontWeight}) |"
                : $"| `{role.Name}` | Not used in presentations | – | – |");
        }

        writer.Line();
        writer.Line("## Durations");
        writer.Line();
        writer.Line("| Motion | Duration | Note |");
        writer.Line("| --- | --- | --- |");
        foreach (var token in catalog.PresentationMotion)
        {
            if (token.Type != TokenType.Duration)
            {
                throw new TokenException($"Token '{token.Path}' must be a duration.");
            }

            writer.Line($"| `{token.Name}` | {Value($"{Text.Number(token.Duration.Milliseconds / 1000)} s", token)} | {Note(token.Description)} |");
        }

        return GeneratedFile.FromText(Path, writer.ToString());
    }

    private static string Value(string value, Token token) => token.IsProposal ? $"{value} *proposal*" : value;

    private static string Note(string? description) => description is null ? string.Empty : Text.MarkdownCell(description);
}
