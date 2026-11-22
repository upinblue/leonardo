using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UpInBlue.Leonardo.WinUI.Controls;

namespace UpInBlue.Leonardo.Gallery.ViewModels;

/// <summary>
/// An event in the timeline.
/// </summary>
public sealed record ActivityEvent(string Glyph, string Text, string Meta)
{
    /// <summary>Tells apart events with the same text, so that every event is its own list item.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>The status of the event, or null: new since the last visit, or a conflict.</summary>
    public StatusKind? Status { get; init; }

    /// <summary>What the status means, for example "New since your last visit".</summary>
    public string StatusDescription { get; init; } = string.Empty;

    /// <summary>Whether the event has a status.</summary>
    public bool HasStatus => Status is not null;

    /// <summary>The status for the dot; only read when <see cref="HasStatus"/> is true.</summary>
    public StatusKind StatusValue => Status ?? StatusKind.New;

    /// <summary>The text a screen reader reads.</summary>
    public string AccessibleName => HasStatus ? $"{StatusDescription}: {Text} {Meta}" : $"{Text} {Meta}";
}

/// <summary>
/// The events of one day. <see cref="Events"/> holds the ones that match the active filters.
/// </summary>
public sealed partial class ActivityDay : ObservableObject
{
    /// <summary>
    /// Creates a day with its events, newest first.
    /// </summary>
    public ActivityDay(string day, IEnumerable<ActivityEvent> events)
    {
        Day = day;
        All = [.. events];
        Events = new ObservableCollection<ActivityEvent>(All);
    }

    /// <summary>The day, for example "Today".</summary>
    public string Day { get; }

    /// <summary>All events of the day.</summary>
    public List<ActivityEvent> All { get; }

    /// <summary>The events shown.</summary>
    public ObservableCollection<ActivityEvent> Events { get; }

    /// <summary>Whether the day shows any event; a day without events hides its heading.</summary>
    public bool HasEvents => Events.Count > 0;

    /// <summary>
    /// Shows the events that match, keeping their order.
    /// </summary>
    public void Filter(Func<ActivityEvent, bool> matches)
    {
        var wanted = All.Where(matches).ToList();
        foreach (var item in Events.Except(wanted).ToList())
        {
            Events.Remove(item);
        }

        for (var index = 0; index < wanted.Count; index++)
        {
            if (index >= Events.Count || !ReferenceEquals(Events[index], wanted[index]))
            {
                Events.Insert(index, wanted[index]);
            }
        }

        OnPropertyChanged(nameof(HasEvents));
    }
}

/// <summary>
/// The Activity scenario: a timeline of events, newest first, with a summary of what happened since the last visit.
/// Invented sample content.
/// </summary>
public sealed partial class ActivityViewModel : ObservableObject
{
    private const string Upload = "";
    private const string Edit = "";
    private const string Share = "";
    private const string Approve = "";
    private const string Comment = "";
    private const string Folder = "";
    private const string Sync = "";
    private const string NewSinceLastVisit = "New since your last visit";

    private static readonly ActivityEvent[] Arriving =
    [
        new(Upload, "Lea Schmid uploaded Delivery note 4471.pdf.", "Hofmann Logistics / Invoices · just now"),
        new(Comment, "Tim Kraus commented on Cost estimate.xlsx: “Please check the crane costs.”", "Brenner Industries / Plant expansion · just now"),
        new(Share, "Mara Lindqvist shared Tender documents.zip with Kessler Construction.", "Kessler Construction / Tenders · just now"),
        new(Edit, "Felix Okafor changed Onboarding checklist.docx.", "Mertens Medical · just now"),
    ];

    private int _arrived;

    /// <summary>
    /// Creates the timeline and its summary.
    /// </summary>
    public ActivityViewModel()
    {
        NewItem = new SummaryLineItem { Kind = SummaryKind.New, Command = FilterCommand };
        ConflictItem = new SummaryLineItem { Kind = SummaryKind.Conflict, Command = FilterCommand };
        Summary = [NewItem, ConflictItem];
        var all = Days.SelectMany(day => day.All).ToList();
        NewItem.Count = all.Count(item => item.Status == StatusKind.New);
        ConflictItem.Count = all.Count(item => item.Status == StatusKind.Conflict);
    }

    /// <summary>
    /// Raised when a filter of the summary line was turned on or off. The view calls <see cref="ApplyFilter"/>, so that it can animate the change.
    /// </summary>
    public event EventHandler? FilterRequested;

    /// <summary>The days with their events.</summary>
    public IReadOnlyList<ActivityDay> Days { get; } =
    [
        new("Today", [
            new(Upload, "Mara Lindqvist uploaded Invoice 2026-119.pdf.", "Hofmann Logistics / Invoices · 8:40 AM") { Status = StatusKind.New, StatusDescription = NewSinceLastVisit },
            new(Comment, "Jonas Weber commented on Price list 2027.xlsx: “Discount for 2027 confirmed.”", "Hofmann Logistics / Contracts · 8:12 AM") { Status = StatusKind.New, StatusDescription = NewSinceLastVisit },
        ]),
        new("Yesterday", [
            new(Edit, "Anna Berg changed Rahmenvertragsverlängerungsvereinbarung 2027.docx.", "Hofmann Logistics / Contracts · 4:05 PM"),
            new(Sync, "Lea Schmid and Jonas Weber changed Service level appendix.docx at the same time. Choose which version to keep.", "Hofmann Logistics / Contracts · 2:10 PM") { Status = StatusKind.Conflict, StatusDescription = "Sync conflict" },
            new(Share, "Lea Schmid shared Framework agreement 2026.docx with Hofmann Logistics.", "Hofmann Logistics / Contracts · 11:20 AM"),
            new(Approve, "Tim Kraus approved the travel request of Felix Okafor.", "Approvals · 9:31 AM"),
        ]),
        new("November 18", [
            new(Folder, "Felix Okafor created the folder Tenders.", "Kessler Construction · 2:44 PM"),
            new(Upload, "Felix Okafor uploaded Site plan hall 4.pdf.", "Brenner Industries / Plant expansion · 1:20 PM"),
        ]),
    ];

    /// <summary>"Since your last visit: 2 new · 1 conflict". Each phrase filters the timeline.</summary>
    public ObservableCollection<SummaryLineItem> Summary { get; }

    /// <summary>The "new" phrase.</summary>
    public SummaryLineItem NewItem { get; }

    /// <summary>The "conflict" phrase.</summary>
    public SummaryLineItem ConflictItem { get; }

    /// <summary>
    /// Shows the events that match the active filters. Without an active filter, all events show.
    /// </summary>
    public void ApplyFilter()
    {
        foreach (var day in Days)
        {
            day.Filter(Matches);
        }
    }

    /// <summary>
    /// Simulates a new event, as it would arrive from the server, at the top of today. Returns null if a filter hides it.
    /// </summary>
    public ActivityEvent? AddEvent()
    {
        var template = Arriving[_arrived++ % Arriving.Length];
        var item = new ActivityEvent(template.Glyph, template.Text, template.Meta);
        Days[0].All.Insert(0, item);
        if (!Matches(item))
        {
            return null;
        }

        Days[0].Events.Insert(0, item);
        return item;
    }

    private bool Matches(ActivityEvent item)
    {
        if (!NewItem.IsActive && !ConflictItem.IsActive)
        {
            return true;
        }

        return (NewItem.IsActive && item.Status == StatusKind.New) || (ConflictItem.IsActive && item.Status == StatusKind.Conflict);
    }

    [RelayCommand]
    private void Filter() => FilterRequested?.Invoke(this, EventArgs.Empty);
}
