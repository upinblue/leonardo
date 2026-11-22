using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UpInBlue.Leonardo.Gallery.Models;
using UpInBlue.Leonardo.WinUI.Controls;

namespace UpInBlue.Leonardo.Gallery.ViewModels;

/// <summary>
/// The state of a request.
/// </summary>
public enum ApprovalState
{
    /// <summary>Waiting for a decision.</summary>
    Waiting,

    /// <summary>Approved.</summary>
    Approved,

    /// <summary>Rejected.</summary>
    Rejected,
}

/// <summary>
/// A request to approve or reject. Invented sample content.
/// </summary>
public sealed partial class ApprovalRequest : ObservableObject
{
    /// <summary>
    /// Creates a waiting request.
    /// </summary>
    public ApprovalRequest(string title, string requester, string company, DateTimeOffset submitted, decimal? amount, string description)
    {
        Title = title;
        Requester = requester;
        Company = company;
        Submitted = submitted;
        Amount = amount;
        Description = description;
    }

    /// <summary>What is requested.</summary>
    public string Title { get; }

    /// <summary>Who asks.</summary>
    public string Requester { get; }

    /// <summary>The customer the request is about.</summary>
    public string Company { get; }

    /// <summary>When it was submitted.</summary>
    public DateTimeOffset Submitted { get; }

    /// <summary>The amount, if any.</summary>
    public decimal? Amount { get; }

    /// <summary>The justification.</summary>
    public string Description { get; }

    /// <summary>Whether the request arrived since the last visit.</summary>
    public bool IsNew { get; init; }

    /// <summary>Whether the row shows the "new" status dot: new and still waiting.</summary>
    public bool ShowsNew => IsNew && IsWaiting;

    /// <summary>"Jonas Weber · Brenner Industries · Nov 19, 2026".</summary>
    public string Meta => $"{Requester} · {Company} · {Formats.Date(Submitted)}";

    /// <summary>The amount for display, or empty.</summary>
    public string AmountText => Amount is { } amount ? Formats.Amount(amount) : string.Empty;

    /// <summary>The decision.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StateText), nameof(StateGlyph), nameof(IsWaiting), nameof(ShowsNew), nameof(DecisionText), nameof(AccessibleName))]
    public partial ApprovalState State { get; set; }

    /// <summary>The optional comment for the requester.</summary>
    [ObservableProperty]
    public partial string Comment { get; set; } = string.Empty;

    /// <summary>Whether the request still needs a decision.</summary>
    public bool IsWaiting => State == ApprovalState.Waiting;

    /// <summary>The state as text, so the state never depends on the icon alone.</summary>
    public string StateText => State switch
    {
        ApprovalState.Approved => "Approved",
        ApprovalState.Rejected => "Rejected",
        _ => "Waiting for you",
    };

    /// <summary>The icon of the state: a clock, a check mark or a cross.</summary>
    public string StateGlyph => State switch
    {
        ApprovalState.Approved => "",
        ApprovalState.Rejected => "",
        _ => "",
    };

    /// <summary>What just happened to this request, next to the request itself.</summary>
    public string DecisionText => State switch
    {
        ApprovalState.Approved => $"You approved this request. {Requester} will be notified.",
        ApprovalState.Rejected => $"You rejected this request. {Requester} will be notified.",
        _ => string.Empty,
    };

    /// <summary>The text a screen reader reads for the list row.</summary>
    public string AccessibleName => $"{Title}, {(ShowsNew ? "new, " : string.Empty)}{StateText}, {Meta}{(Amount is null ? string.Empty : $", {AmountText}")}";
}

/// <summary>
/// The Approvals scenario: an inbox of requests to approve or reject.
/// </summary>
public sealed partial class ApprovalsViewModel : ObservableObject
{
    /// <summary>
    /// Creates the view model with the sample requests and selects the first one.
    /// </summary>
    public ApprovalsViewModel()
    {
        Requests =
        [
            new("Purchase order: 12 laptops", "Jonas Weber", "Brenner Industries", Day(19), 14_280m, "Replacement for the planning team. Three quotes compared; this is the lowest.") { IsNew = true },
            new("Contract renewal 2027", "Lea Schmid", "Hofmann Logistics", Day(18), 96_000m, "Framework agreement for 2027 with unchanged conditions and a new service level appendix.") { IsNew = true },
            new("Vacation: December 22 to January 2", "Tim Kraus", "Internal", Day(17), null, "Team coverage is arranged with Anna Berg."),
            new("Travel: site visit in Leipzig", "Felix Okafor", "Kessler Construction", Day(16), 640.50m, "Train and one night at the hotel for the tender walkthrough."),
            new("Software subscription: diagram tool", "Mara Lindqvist", "Internal", Day(14), 1_188m, "Annual licence for five users."),
        ];
        foreach (var request in Requests)
        {
            request.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ApprovalRequest.State))
                {
                    OnPropertyChanged(nameof(WaitingSummary));
                    UpdateSummary();
                }
            };
        }

        Shown = new ObservableCollection<ApprovalRequest>(Requests);
        NewItem = new SummaryLineItem { Kind = SummaryKind.New, Command = FilterCommand };
        WaitingItem = new SummaryLineItem { Kind = SummaryKind.Custom, Label = "request waiting for you", PluralLabel = "requests waiting for you", Command = FilterCommand };
        Summary = [NewItem, WaitingItem];
        UpdateSummary();
        Selected = Requests[0];
    }

    /// <summary>
    /// Raised when a filter of the summary line was turned on or off. The view calls <see cref="ApplyFilter"/>, so that it can animate the change.
    /// </summary>
    public event EventHandler? FilterRequested;

    /// <summary>The requests that match the active filters.</summary>
    public ObservableCollection<ApprovalRequest> Shown { get; }

    /// <summary>"In your inbox: 2 new · 4 requests waiting for you". Each phrase filters the list.</summary>
    public ObservableCollection<SummaryLineItem> Summary { get; }

    /// <summary>The "new" phrase.</summary>
    public SummaryLineItem NewItem { get; }

    /// <summary>The "waiting for you" phrase.</summary>
    public SummaryLineItem WaitingItem { get; }

    /// <summary>Whether nothing is waiting, so the summary line hides itself and a sentence says so.</summary>
    public bool IsInboxEmpty => !SummaryLineText.IsVisible(Summary);

    /// <summary>
    /// Shows the requests that match the active filters, keeping their order. Without an active filter, all requests show.
    /// Decided requests stay until the filter changes again, so the request in the details pane never disappears from the list.
    /// </summary>
    public void ApplyFilter()
    {
        var wanted = Requests.Where(Matches).ToList();
        foreach (var request in Shown.Except(wanted).ToList())
        {
            Shown.Remove(request);
        }

        for (var index = 0; index < wanted.Count; index++)
        {
            if (index >= Shown.Count || !ReferenceEquals(Shown[index], wanted[index]))
            {
                Shown.Insert(index, wanted[index]);
            }
        }
    }

    private bool Matches(ApprovalRequest request)
    {
        if (!NewItem.IsActive && !WaitingItem.IsActive)
        {
            return true;
        }

        return (NewItem.IsActive && request.ShowsNew) || (WaitingItem.IsActive && request.IsWaiting);
    }

    private void UpdateSummary()
    {
        NewItem.Count = Requests.Count(request => request.ShowsNew);
        WaitingItem.Count = Requests.Count(request => request.IsWaiting);
        OnPropertyChanged(nameof(IsInboxEmpty));
    }

    [RelayCommand]
    private void Filter() => FilterRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>The requests, newest first.</summary>
    public ObservableCollection<ApprovalRequest> Requests { get; }

    /// <summary>The request in the details pane.</summary>
    [ObservableProperty]
    public partial ApprovalRequest Selected { get; set; }

    /// <summary>"3 requests waiting for you".</summary>
    public string WaitingSummary
    {
        get
        {
            var waiting = Requests.Count(request => request.IsWaiting);
            return waiting switch
            {
                0 => "Nothing is waiting for you.",
                1 => "1 request is waiting for you.",
                _ => $"{waiting} requests are waiting for you.",
            };
        }
    }

    [RelayCommand]
    private void Approve() => Selected.State = ApprovalState.Approved;

    [RelayCommand]
    private void Reject() => Selected.State = ApprovalState.Rejected;

    /// <summary>
    /// Takes the decision back. Undo comes before confirmation.
    /// </summary>
    [RelayCommand]
    private void Undo() => Selected.State = ApprovalState.Waiting;

    private static DateTimeOffset Day(int day) => new(2026, 11, day, 9, 0, 0, TimeSpan.FromHours(1));
}
