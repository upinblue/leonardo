namespace UpInBlue.Leonardo.Gallery.Wpf.Models;

/// <summary>
/// A customer contract in the sample list. All content is invented.
/// </summary>
/// <param name="Customer">The fictional customer.</param>
/// <param name="Name">The name of the contract.</param>
/// <param name="Owner">The fictional person responsible for it.</param>
/// <param name="AnnualValue">The annual value in euros.</param>
/// <param name="Renewal">The renewal date.</param>
public sealed record Contract(string Customer, string Name, string Owner, decimal AnnualValue, DateOnly Renewal);

/// <summary>
/// Invented sample content. Only the fictional companies and people of the Leonardo repository appear.
/// </summary>
public static class SampleData
{
    /// <summary>
    /// The contracts shown in the list.
    /// </summary>
    public static IReadOnlyList<Contract> Contracts { get; } =
    [
        new("Hofmann Logistics", "Framework agreement", "Lea Schmid", 248_310m, new(2026, 12, 31)),
        new("Hofmann Logistics", "Warehouse services", "Anna Berg", 61_740m, new(2027, 3, 31)),
        new("Brenner Industries", "Plant expansion support", "Felix Okafor", 1_284_002m, new(2027, 6, 30)),
        new("Brenner Industries", "Maintenance", "Jonas Weber", 88_412m, new(2027, 1, 15)),
        new("Lakeside Utilities", "Meter data platform", "Mara Lindqvist", 402_118m, new(2027, 9, 30)),
        new("Kessler Construction", "Site documentation", "Tim Kraus", 120_554m, new(2027, 2, 28)),
        new("Mertens Medical", "Data processing agreement", "Lea Schmid", 36_002m, new(2027, 4, 1)),
        new("Mertens Medical", "Onboarding", "Tim Kraus", 9_870m, new(2026, 12, 15)),
    ];
}
