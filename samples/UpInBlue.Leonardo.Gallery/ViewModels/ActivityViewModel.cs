using System.Collections.ObjectModel;

namespace UpInBlue.Leonardo.Gallery.ViewModels;

/// <summary>
/// An event in the timeline.
/// </summary>
public sealed record ActivityEvent(string Glyph, string Text, string Meta)
{
    /// <summary>Tells apart events with the same text, so that every event is its own list item.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>The text a screen reader reads.</summary>
    public string AccessibleName => $"{Text} {Meta}";
}

/// <summary>
/// The events of one day.
/// </summary>
public sealed record ActivityDay(string Day, ObservableCollection<ActivityEvent> Events);

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

    private static readonly ActivityEvent[] Arriving =
    [
        new(Upload, "Lea Schmid uploaded Delivery note 4471.pdf.", "Hofmann Logistics / Invoices · just now"),
        new(Comment, "Tim Kraus commented on Cost estimate.xlsx: “Please check the crane costs.”", "Brenner Industries / Plant expansion · just now"),
        new(Share, "Mara Lindqvist shared Tender documents.zip with Kessler Construction.", "Kessler Construction / Tenders · just now"),
        new(Edit, "Felix Okafor changed Onboarding checklist.docx.", "Mertens Medical · just now"),
    ];

    private int _arrived;

    /// <summary>The days with their events.</summary>
    public IReadOnlyList<ActivityDay> Days { get; } =
    [
        new("Today", new([
            new(Upload, "Mara Lindqvist uploaded Invoice 2026-119.pdf.", "Hofmann Logistics / Invoices · 8:40 AM"),
            new(Comment, "Jonas Weber commented on Price list 2027.xlsx: “Discount for 2027 confirmed.”", "Hofmann Logistics / Contracts · 8:12 AM"),
        ])),
        new("Yesterday", new([
            new(Edit, "Anna Berg changed Rahmenvertragsverlängerungsvereinbarung 2027.docx.", "Hofmann Logistics / Contracts · 4:05 PM"),
            new(Share, "Lea Schmid shared Framework agreement 2026.docx with Hofmann Logistics.", "Hofmann Logistics / Contracts · 11:20 AM"),
            new(Approve, "Tim Kraus approved the travel request of Felix Okafor.", "Approvals · 9:31 AM"),
        ])),
        new("November 18", new([
            new(Folder, "Felix Okafor created the folder Tenders.", "Kessler Construction · 2:44 PM"),
            new(Upload, "Felix Okafor uploaded Site plan hall 4.pdf.", "Brenner Industries / Plant expansion · 1:20 PM"),
        ])),
    ];

    /// <summary>
    /// Simulates a new event, as it would arrive from the server, at the top of today.
    /// </summary>
    public ActivityEvent AddEvent()
    {
        var template = Arriving[_arrived++ % Arriving.Length];
        var item = new ActivityEvent(template.Glyph, template.Text, template.Meta);
        Days[0].Events.Insert(0, item);
        return item;
    }
}
