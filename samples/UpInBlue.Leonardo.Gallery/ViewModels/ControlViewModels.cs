using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UpInBlue.Leonardo.WinUI.Controls;

namespace UpInBlue.Leonardo.Gallery.ViewModels;

/// <summary>
/// A status kind on the Status dot page.
/// </summary>
public sealed record StatusSample(StatusKind Kind, string Name, string Description, string Shape, string Color)
{
    /// <summary>The text a screen reader reads for the row.</summary>
    public string AccessibleName => $"{Name}: {Shape} in {Color}. Example: {Description}";
}

/// <summary>
/// A file row with a status, on the Status dot page.
/// </summary>
public sealed record StatusRow(string Name, string Glyph, StatusKind? Status, string StatusDescription)
{
    /// <summary>Whether the row has a status.</summary>
    public bool HasStatus => Status is not null;

    /// <summary>The status for the dot; only read when <see cref="HasStatus"/> is true.</summary>
    public StatusKind StatusValue => Status ?? StatusKind.New;

    /// <summary>The text a screen reader reads for the row, including the status.</summary>
    public string AccessibleName => HasStatus ? $"{Name}, {StatusDescription}" : Name;
}

/// <summary>
/// The Status dot page.
/// </summary>
public sealed class StatusDotViewModel
{
    /// <summary>The three kinds.</summary>
    public IReadOnlyList<StatusSample> Kinds { get; } =
    [
        new(StatusKind.New, "New", "New since your last visit", "Filled dot", "the accent color"),
        new(StatusKind.CheckedOut, "Checked out", "Checked out by Anna Berg", "Ring", "the system caution color"),
        new(StatusKind.Conflict, "Conflict", "Sync conflict: Lea Schmid and you changed this file", "Filled circle with an exclamation mark", "the system critical color"),
    ];

    /// <summary>Rows of a file list with and without status.</summary>
    public IReadOnlyList<StatusRow> Rows { get; } =
    [
        new("Price list 2027.xlsx", "", StatusKind.New, "New since your last visit"),
        new("Rahmenvertragsverlängerungsvereinbarung 2027.docx", "", StatusKind.CheckedOut, "Checked out by Anna Berg"),
        new("Service level appendix.docx", "", StatusKind.Conflict, "Sync conflict: Lea Schmid and you changed this file"),
        new("Signed framework agreement 2025.pdf", "", null, string.Empty),
    ];
}

/// <summary>
/// The Summary line page: a summary line to try with its own counts, and examples of every state.
/// </summary>
public sealed partial class SummaryLineViewModel : ObservableObject
{
    /// <summary>
    /// Creates the view model with "2 new · 1 checked out · 1 conflict".
    /// </summary>
    public SummaryLineViewModel()
    {
        New = new SummaryLineItem { Kind = SummaryKind.New, Count = 2, Command = FilterChangedCommand };
        CheckedOut = new SummaryLineItem { Kind = SummaryKind.CheckedOut, Count = 1, Command = FilterChangedCommand };
        Conflict = new SummaryLineItem { Kind = SummaryKind.Conflict, Count = 1, Command = FilterChangedCommand };
        Items = [New, CheckedOut, Conflict];
        NewCount = New.Count;
        CheckedOutCount = CheckedOut.Count;
        ConflictCount = Conflict.Count;
    }

    /// <summary>The phrases of the summary line to try.</summary>
    public ObservableCollection<SummaryLineItem> Items { get; }

    /// <summary>The "new" phrase.</summary>
    public SummaryLineItem New { get; }

    /// <summary>The "checked out" phrase.</summary>
    public SummaryLineItem CheckedOut { get; }

    /// <summary>The "conflict" phrase.</summary>
    public SummaryLineItem Conflict { get; }

    /// <summary>The count of new items, edited in a number box.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHidden))]
    public partial double NewCount { get; set; }

    /// <summary>The count of checked-out items.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHidden))]
    public partial double CheckedOutCount { get; set; }

    /// <summary>The count of conflicts.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHidden))]
    public partial double ConflictCount { get; set; }

    /// <summary>Whether all counts are 0, so the summary line hides itself.</summary>
    public bool IsHidden => !SummaryLineText.IsVisible(Items);

    /// <summary>What the active filters do, for example "Showing new and conflict only."</summary>
    [ObservableProperty]
    public partial string FilterText { get; set; } = "No filter is on: everything is shown.";

    /// <summary>Every phrase, nothing active.</summary>
    public ObservableCollection<SummaryLineItem> AllPhrases { get; } =
    [
        new() { Kind = SummaryKind.New, Count = 2 },
        new() { Kind = SummaryKind.CheckedOut, Count = 1 },
        new() { Kind = SummaryKind.Conflict, Count = 1 },
    ];

    /// <summary>A filter is on.</summary>
    public ObservableCollection<SummaryLineItem> ActivePhrase { get; } =
    [
        new() { Kind = SummaryKind.New, Count = 2 },
        new() { Kind = SummaryKind.Conflict, Count = 3, IsActive = true },
    ];

    /// <summary>Several filters are on: checked toggles count as selection, so more than one may be active.</summary>
    public ObservableCollection<SummaryLineItem> ActivePhrases { get; } =
    [
        new() { Kind = SummaryKind.New, Count = 2, IsActive = true },
        new() { Kind = SummaryKind.CheckedOut, Count = 1 },
        new() { Kind = SummaryKind.Conflict, Count = 3, IsActive = true },
    ];

    /// <summary>Custom phrases with their own labels.</summary>
    public ObservableCollection<SummaryLineItem> CustomPhrases { get; } =
    [
        new() { Kind = SummaryKind.New, Count = 2 },
        new() { Kind = SummaryKind.Custom, Count = 4, Label = "request waiting for you", PluralLabel = "requests waiting for you" },
    ];

    partial void OnNewCountChanged(double value) => New.Count = AsCount(value);

    partial void OnCheckedOutCountChanged(double value) => CheckedOut.Count = AsCount(value);

    partial void OnConflictCountChanged(double value) => Conflict.Count = AsCount(value);

    [RelayCommand]
    private void FilterChanged()
    {
        var active = Items.Where(item => item.IsActive).Select(item => item.CurrentLabel()).ToList();
        FilterText = active.Count == 0
            ? "No filter is on: everything is shown."
            : $"Showing {string.Join(" and ", active)} only.";
    }

    private static int AsCount(double value) => double.IsNaN(value) ? 0 : (int)Math.Clamp(Math.Round(value), 0, 999);
}

/// <summary>
/// A row of the long list on the Place header page.
/// </summary>
public sealed record PlaceHeaderRow(string Name, string Meta);

/// <summary>
/// The Place header page: enough content to scroll, so that the header shows parallax and shrinks.
/// </summary>
public sealed class PlaceHeaderViewModel
{
    private static readonly string[] Companies = ["Hofmann Logistics", "Brenner Industries", "Lakeside Utilities", "Kessler Construction", "Mertens Medical"];
    private static readonly string[] People = ["Anna Berg", "Jonas Weber", "Lea Schmid", "Tim Kraus", "Mara Lindqvist", "Felix Okafor"];
    private static readonly string[] Documents = ["Framework agreement", "Price list", "Meeting notes", "Cost estimate", "Service level appendix", "Delivery note"];

    /// <summary>The summary line in the header.</summary>
    public ObservableCollection<SummaryLineItem> Summary { get; } =
    [
        new() { Kind = SummaryKind.New, Count = 2 },
        new() { Kind = SummaryKind.Conflict, Count = 1 },
    ];

    /// <summary>Rows to scroll through.</summary>
    public IReadOnlyList<PlaceHeaderRow> Rows { get; } = Enumerable.Range(0, 30)
        .Select(index => new PlaceHeaderRow(
            $"{Documents[index % Documents.Length]} {2020 + (index % 8)}.docx",
            $"{Companies[index % Companies.Length]} · changed by {People[index % People.Length]}"))
        .ToList();
}
