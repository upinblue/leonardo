namespace UpInBlue.Leonardo.Gallery.ViewModels;

/// <summary>
/// An event in the timeline.
/// </summary>
public sealed record ActivityEvent(string Glyph, string Text, string Meta)
{
    /// <summary>The text a screen reader reads.</summary>
    public string AccessibleName => $"{Text} {Meta}";
}

/// <summary>
/// The events of one day.
/// </summary>
public sealed record ActivityDay(string Day, IReadOnlyList<ActivityEvent> Events);

/// <summary>
/// The Activity scenario: a timeline of events, newest first. Invented sample content.
/// </summary>
public sealed class ActivityViewModel
{
    private const string Upload = "";
    private const string Edit = "";
    private const string Share = "";
    private const string Approve = "";
    private const string Comment = "";
    private const string Folder = "";

    /// <summary>The days with their events.</summary>
    public IReadOnlyList<ActivityDay> Days { get; } =
    [
        new("Today", [
            new(Upload, "Mara Lindqvist uploaded Invoice 2026-119.pdf.", "Hofmann Logistics / Invoices · 8:40 AM"),
            new(Comment, "Jonas Weber commented on Price list 2027.xlsx: “Discount for 2027 confirmed.”", "Hofmann Logistics / Contracts · 8:12 AM"),
        ]),
        new("Yesterday", [
            new(Edit, "Anna Berg changed Rahmenvertragsverlängerungsvereinbarung 2027.docx.", "Hofmann Logistics / Contracts · 4:05 PM"),
            new(Share, "Lea Schmid shared Framework agreement 2026.docx with Hofmann Logistics.", "Hofmann Logistics / Contracts · 11:20 AM"),
            new(Approve, "Tim Kraus approved the travel request of Felix Okafor.", "Approvals · 9:31 AM"),
        ]),
        new("November 18", [
            new(Folder, "Felix Okafor created the folder Tenders.", "Kessler Construction · 2:44 PM"),
            new(Upload, "Felix Okafor uploaded Site plan hall 4.pdf.", "Brenner Industries / Plant expansion · 1:20 PM"),
        ]),
    ];
}
