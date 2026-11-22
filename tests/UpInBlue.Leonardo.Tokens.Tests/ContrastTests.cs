using UpInBlue.Leonardo.Tokens.Model;
using UpInBlue.Leonardo.Tokens.Parsing;

namespace UpInBlue.Leonardo.Tokens.Tests;

/// <summary>
/// WCAG 2.2 contrast of the theme colors: 4.5:1 for text, 3:1 for non-text elements.
/// </summary>
/// <remarks>
/// The hover and pressed accents follow WinUI's AccentFillColorSecondaryBrush and AccentFillColorTertiaryBrush in
/// Microsoft.UI.Xaml generic.xaml: the accent fill (SystemAccentColorDark1 in light, SystemAccentColorLight2 in dark) at
/// 90 % and 80 % opacity. Dark composites it over the surface. Light composites it over SystemAccentColorDark2 (near),
/// because over the light surface white text would fall below 4.5:1.
/// </remarks>
public sealed class ContrastTests
{
    private static readonly string[] Themes = ["light", "dark"];
    private static readonly string[] TextColors = ["textPrimary", "textSecondary", "textTertiary", "link"];
    private static readonly string[] TextBackgrounds = ["surface", "surfaceAlt", "selection"];
    private static readonly string[] NonTextColors = ["strokeStrong", "chart", "statusCritical", "statusCaution", "statusSuccess"];
    private static readonly string[] FocusBackgrounds = ["surface", "surfaceAlt"];
    private static readonly string[] AccentStates = ["accent", "accentHover", "accentPressed"];

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

            foreach (var state in AccentStates)
            {
                data.Add(theme, "onAccent", state, 4.5);
            }

            foreach (var foreground in NonTextColors)
            {
                data.Add(theme, foreground, "surface", 3.0);
            }

            foreach (var background in FocusBackgrounds)
            {
                data.Add(theme, "focus", background, 3.0);
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
    [InlineData("light", "accentHover", 0.9, "palette.near")]
    [InlineData("light", "accentPressed", 0.8, "palette.near")]
    [InlineData("dark", "accentHover", 0.9, "theme.dark.surface")]
    [InlineData("dark", "accentPressed", 0.8, "theme.dark.surface")]
    public void AccentStatesAreTheFluentOpacitiesOfTheAccent(string theme, string state, double opacity, string backdrop)
    {
        var accent = Tokens.Value[$"theme.{theme}.accent"].Color;
        var behind = Tokens.Value[backdrop].Color;

        var expected = new RgbColor(
            Composite(accent.R, behind.R, opacity),
            Composite(accent.G, behind.G, opacity),
            Composite(accent.B, behind.B, opacity),
            255);

        Assert.Equal(expected.Hex, Tokens.Value[$"theme.{theme}.{state}"].Color.Hex);
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

    private static byte Composite(byte foreground, byte background, double opacity)
        => (byte)Math.Round((foreground * opacity) + (background * (1 - opacity)), MidpointRounding.AwayFromZero);
}
