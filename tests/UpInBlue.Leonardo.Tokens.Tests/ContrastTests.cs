using UpInBlue.Leonardo.Tokens.Model;
using UpInBlue.Leonardo.Tokens.Parsing;

namespace UpInBlue.Leonardo.Tokens.Tests;

/// <summary>
/// WCAG 2.2 contrast of the theme colors: 4.5:1 for text, 3:1 for non-text elements.
/// </summary>
public sealed class ContrastTests
{
    private static readonly string[] Themes = ["light", "dark"];
    private static readonly string[] TextColors = ["textPrimary", "textSecondary", "textTertiary", "link"];
    private static readonly string[] TextBackgrounds = ["surface", "surfaceAlt", "selection"];
    private static readonly string[] NonTextColors = ["strokeStrong", "statusCritical", "statusCaution", "statusSuccess"];

    private static readonly Lazy<TokenSet> Tokens = new(() => TokenParser.Parse(File.ReadAllText(Repository.PathOf("tokens/leonardo.tokens.json"))));

    public static TheoryData<string, string, string, double> Pairs()
    {
        var data = new TheoryData<string, string, string, double>();
        foreach (var theme in Themes)
        {
            foreach (var foreground in TextColors)
            {
                foreach (var background in TextBackgrounds)
                {
                    data.Add(theme, foreground, background, 4.5);
                }
            }

            data.Add(theme, "onAccent", "accent", 4.5);

            foreach (var foreground in NonTextColors)
            {
                data.Add(theme, foreground, "surface", 3.0);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Pairs))]
    public void ThemeColorsHaveEnoughContrast(string theme, string foreground, string background, double minimum)
    {
        var foregroundColor = Tokens.Value[$"theme.{theme}.{foreground}"].Color;
        var backgroundColor = Tokens.Value[$"theme.{theme}.{background}"].Color;
        Assert.Equal(255, foregroundColor.A);
        Assert.Equal(255, backgroundColor.A);

        var ratio = RgbColor.ContrastRatio(foregroundColor, backgroundColor);

        Assert.True(
            ratio >= minimum,
            $"{theme}: {foreground} {foregroundColor.Hex} on {background} {backgroundColor.Hex} is {ratio:0.00}:1, needs {minimum}:1.");
    }

    [Theory]
    [InlineData("#000000", "#FFFFFF", 21.0)]
    [InlineData("#FFFFFF", "#FFFFFF", 1.0)]
    [InlineData("#1B6CE0", "#FFFFFF", 4.92)]
    [InlineData("#767676", "#FFFFFF", 4.54)]
    public void ContrastRatioMatchesWcag(string first, string second, double expected)
    {
        Assert.True(RgbColor.TryParse(first, out var firstColor));
        Assert.True(RgbColor.TryParse(second, out var secondColor));

        Assert.Equal(expected, RgbColor.ContrastRatio(firstColor, secondColor), precision: 2);
        Assert.Equal(expected, RgbColor.ContrastRatio(secondColor, firstColor), precision: 2);
    }
}
