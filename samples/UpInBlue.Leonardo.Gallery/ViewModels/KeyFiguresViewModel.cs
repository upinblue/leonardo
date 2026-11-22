using Microsoft.UI.Xaml;
using UpInBlue.Leonardo.Gallery.Models;

namespace UpInBlue.Leonardo.Gallery.ViewModels;

/// <summary>
/// A key figure: the number is the headline, the label says what it counts.
/// </summary>
public sealed record KeyFigure(string Value, string Label, string Context)
{
    /// <summary>The text a screen reader reads.</summary>
    public string AccessibleName => $"{Label}: {Value}. {Context}";
}

/// <summary>
/// A bar of the chart.
/// </summary>
public sealed record ChartBar(string Month, int Value, double Height)
{
    /// <summary>The value for display.</summary>
    public string ValueText => Formats.Number(Value);

    /// <summary>The text a screen reader reads.</summary>
    public string AccessibleName => $"{Month}: {ValueText} files shared";
}

/// <summary>
/// The Key figures scenario. Invented sample numbers.
/// </summary>
public sealed class KeyFiguresViewModel
{
    /// <summary>
    /// Creates the figures and scales the bars to the chart height.
    /// </summary>
    public KeyFiguresViewModel()
    {
        var chartHeight = (double)Application.Current.Resources["GalleryChartHeight"];
        (string Month, int Value)[] months = [("Jun", 182), ("Jul", 205), ("Aug", 196), ("Sep", 231), ("Oct", 214), ("Nov", 248)];
        var max = months.Max(month => month.Value);
        Bars = months.Select(month => new ChartBar(month.Month, month.Value, chartHeight * month.Value / max)).ToList();
    }

    /// <summary>The key figures.</summary>
    public IReadOnlyList<KeyFigure> Figures { get; } =
    [
        new(Formats.Number(248), "Files shared", "In November, 16 % more than in October."),
        new(Formats.Number(3), "Approvals waiting", "The oldest one is 4 days old."),
        new("61 %", "Storage used", "390 GB of 640 GB."),
        new(Formats.Number(5), "Active customers", "Lakeside Utilities joined in October."),
    ];

    /// <summary>Files shared per month, from June to November.</summary>
    public IReadOnlyList<ChartBar> Bars { get; }

    /// <summary>The chart as one sentence for screen readers and as a text alternative.</summary>
    public string ChartSummary => $"Files shared per month, June to November 2026: {string.Join(", ", Bars.Select(bar => $"{bar.Month} {bar.ValueText}"))}.";
}
