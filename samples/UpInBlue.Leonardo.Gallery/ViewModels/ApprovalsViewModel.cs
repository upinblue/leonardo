using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UpInBlue.Leonardo.Gallery.Models;

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

    /// <summary>"Jonas Weber · Brenner Industries · Nov 19, 2026".</summary>
    public string Meta => $"{Requester} · {Company} · {Formats.Date(Submitted)}";

    /// <summary>The amount for display, or empty.</summary>
    public string AmountText => Amount is { } amount ? Formats.Amount(amount) : string.Empty;

    /// <summary>The decision.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StateText), nameof(StateGlyph), nameof(IsWaiting), nameof(DecisionText), nameof(AccessibleName))]
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
    public string AccessibleName => $"{Title}, {StateText}, {Meta}{(Amount is null ? string.Empty : $", {AmountText}")}";
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
            new("Purchase order: 12 laptops", "Jonas Weber", "Brenner Industries", Day(19), 14_280m, "Replacement for the planning team. Three quotes compared; this is the lowest."),
            new("Contract renewal 2027", "Lea Schmid", "Hofmann Logistics", Day(18), 96_000m, "Framework agreement for 2027 with unchanged conditions and a new service level appendix."),
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
                }
            };
        }

        Selected = Requests[0];
    }

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
