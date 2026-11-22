using System.Globalization;
using UpInBlue.Leonardo.WinUI;

namespace UpInBlue.Leonardo.Gallery.ViewModels;

/// <summary>
/// Describes the type roles with the values from the generated tokens.
/// </summary>
public sealed class TypographyViewModel
{
    /// <summary>The place name role.</summary>
    public string PlaceNameSpec { get; } = Spec(
        LeonardoTokens.Typography.PlaceName.FontFamily,
        LeonardoTokens.Typography.PlaceName.FontSize,
        LeonardoTokens.Typography.PlaceName.LineHeight,
        LeonardoTokens.Typography.PlaceName.FontWeight);

    /// <summary>The view role.</summary>
    public string ViewSpec { get; } = Spec(
        LeonardoTokens.Typography.View.FontFamily,
        LeonardoTokens.Typography.View.FontSize,
        LeonardoTokens.Typography.View.LineHeight,
        LeonardoTokens.Typography.View.FontWeight);

    /// <summary>The section role.</summary>
    public string SectionSpec { get; } = Spec(
        LeonardoTokens.Typography.Section.FontFamily,
        LeonardoTokens.Typography.Section.FontSize,
        LeonardoTokens.Typography.Section.LineHeight,
        LeonardoTokens.Typography.Section.FontWeight);

    /// <summary>The body role.</summary>
    public string BodySpec { get; } = Spec(
        LeonardoTokens.Typography.Body.FontFamily,
        LeonardoTokens.Typography.Body.FontSize,
        LeonardoTokens.Typography.Body.LineHeight,
        LeonardoTokens.Typography.Body.FontWeight);

    /// <summary>Emphasis: the body role in semibold.</summary>
    public string BodyStrongSpec { get; } = Spec(
        LeonardoTokens.Typography.Body.FontFamily,
        LeonardoTokens.Typography.Body.FontSize,
        LeonardoTokens.Typography.Body.LineHeight,
        null) + " · Semibold";

    /// <summary>The meta role.</summary>
    public string MetaSpec { get; } = Spec(
        LeonardoTokens.Typography.Meta.FontFamily,
        LeonardoTokens.Typography.Meta.FontSize,
        LeonardoTokens.Typography.Meta.LineHeight,
        LeonardoTokens.Typography.Meta.FontWeight) + " · textTertiary";

    /// <summary>Amounts to compare proportional and tabular figures.</summary>
    public IReadOnlyList<string> Amounts { get; } =
        new[] { 1111.11, 48_210.50, 7_404.00, 111.10, 98_765.43 }
            .Select(amount => amount.ToString("N2", CultureInfo.GetCultureInfo("en-US")))
            .ToList();

    private static string Spec(string family, double size, double lineHeight, ushort? weight)
    {
        var text = string.Create(CultureInfo.InvariantCulture, $"{family} · {size}/{lineHeight} epx");
        return weight is { } value ? $"{text} · {WeightName(value)}" : text;
    }

    private static string WeightName(ushort weight) => weight switch
    {
        300 => "Light",
        350 => "Semilight",
        400 => "Regular",
        600 => "Semibold",
        700 => "Bold",
        _ => weight.ToString(CultureInfo.InvariantCulture),
    };
}
