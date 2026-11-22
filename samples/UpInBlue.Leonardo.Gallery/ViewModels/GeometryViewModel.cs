using System.Globalization;
using Microsoft.UI.Xaml;
using UpInBlue.Leonardo.WinUI;

namespace UpInBlue.Leonardo.Gallery.ViewModels;

/// <summary>
/// A spacing, radius or row height value with its use.
/// </summary>
public sealed record GeometrySample(string Name, double Value, string Use)
{
    /// <summary>The value as text, for example "16 epx".</summary>
    public string ValueText => string.Create(CultureInfo.InvariantCulture, $"{Value} epx");

    /// <summary>The value as a uniform corner radius.</summary>
    public CornerRadius Radius => new(Value);

    /// <summary>The text a screen reader reads for the sample.</summary>
    public string AccessibleName => $"{Name}, {ValueText}. {Use}";
}

/// <summary>
/// The grid and geometry values from the generated tokens.
/// </summary>
public sealed class GeometryViewModel
{
    /// <summary>The spacing scale.</summary>
    public IReadOnlyList<GeometrySample> Spacing { get; } =
    [
        new("space.xs", LeonardoTokens.Space.XS, "Icon to label; how far list backplates reach toward the content edge"),
        new("space.s", LeonardoTokens.Space.S, "Within a group"),
        new("space.m", LeonardoTokens.Space.M, "Between groups"),
        new("space.l", LeonardoTokens.Space.L, "Page margin, details pane"),
        new("space.xl", LeonardoTokens.Space.XL, "Between place name and content; alignment line on pages with icon lists"),
    ];

    /// <summary>The sizes of small elements.</summary>
    public IReadOnlyList<GeometrySample> Sizes { get; } =
    [
        new("size.icon", LeonardoTokens.Size.Icon, "Icons in list rows and next to labels"),
        new("size.statusDot", LeonardoTokens.Size.StatusDot, "Diameter of the status dot"),
    ];

    /// <summary>The corner radii.</summary>
    public IReadOnlyList<GeometrySample> Radii { get; } =
    [
        new("radius.s", LeonardoTokens.Radius.S, "Small elements"),
        new("radius.m", LeonardoTokens.Radius.M, "Elements inside a card"),
        new("radius.l", LeonardoTokens.Radius.L, "Cards and large containers"),
    ];

    /// <summary>The list row heights.</summary>
    public IReadOnlyList<GeometrySample> Densities { get; } =
    [
        new("density.compact", LeonardoTokens.Density.Compact, "Compact"),
        new("density.standard", LeonardoTokens.Density.Standard, "Standard, the default"),
        new("density.comfortable", LeonardoTokens.Density.Comfortable, "Comfortable"),
    ];

    /// <summary>The concentric rule with the actual values.</summary>
    public string ConcentricRule { get; } = string.Create(
        CultureInfo.InvariantCulture,
        $"Inner radius = outer radius − padding: radius.l {LeonardoTokens.Radius.L} − space.s {LeonardoTokens.Space.S} = radius.m {LeonardoTokens.Radius.L - LeonardoTokens.Space.S}.");
}
