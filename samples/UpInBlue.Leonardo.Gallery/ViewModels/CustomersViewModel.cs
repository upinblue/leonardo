using System.Collections.ObjectModel;
using System.Net.Mail;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UpInBlue.Leonardo.Gallery.Models;

namespace UpInBlue.Leonardo.Gallery.ViewModels;

/// <summary>
/// A customer with its saved values and a draft that the form edits. Each customer keeps its own draft,
/// so switching between customers never loses what the user typed.
/// </summary>
public sealed partial class CustomerItem : ObservableObject
{
    private Snapshot _saved;

    /// <summary>
    /// Creates a customer from saved values.
    /// </summary>
    public CustomerItem(string name, string city, string contact, string email, string phone, string status, DateTimeOffset since, string notes)
    {
        City = city;
        _saved = new Snapshot(name, contact, email, phone, status, since, notes);
        Load(_saved);
    }

    /// <summary>The status options.</summary>
    public static IReadOnlyList<string> Statuses { get; } = ["Active", "Prospect", "Paused"];

    /// <summary>The position of <see cref="Status"/> in <see cref="Statuses"/>, for the combo box.</summary>
    public int StatusIndex
    {
        get => Math.Max(0, Statuses.ToList().IndexOf(Status));
        set
        {
            if (value >= 0 && value < Statuses.Count)
            {
                Status = Statuses[value];
            }
        }
    }

    /// <summary>The city, shown in the list.</summary>
    public string City { get; }

    /// <summary>The company name as saved, shown in the list.</summary>
    public string SavedName => _saved.Name;

    /// <summary>The company name in the form. Required.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial string Name { get; set; } = string.Empty;

    /// <summary>The contact person. Optional.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial string Contact { get; set; } = string.Empty;

    /// <summary>The email address. Required.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial string Email { get; set; } = string.Empty;

    /// <summary>The phone number. Optional.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial string Phone { get; set; } = string.Empty;

    /// <summary>The status: Active, Prospect or Paused.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    [NotifyPropertyChangedFor(nameof(StatusIndex))]
    public partial string Status { get; set; } = string.Empty;

    /// <summary>The date the customer relationship started.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial DateTimeOffset? Since { get; set; }

    /// <summary>Notes. Optional.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial string Notes { get; set; } = string.Empty;

    /// <summary>The validation message of the company name; empty if valid.</summary>
    [ObservableProperty]
    public partial string NameError { get; set; } = string.Empty;

    /// <summary>The validation message of the email address; empty if valid.</summary>
    [ObservableProperty]
    public partial string EmailError { get; set; } = string.Empty;

    /// <summary>Whether the draft differs from the saved values.</summary>
    public bool HasChanges => Current() != _saved;

    /// <summary>The text a screen reader reads for the list row.</summary>
    public string AccessibleName => HasChanges ? $"{SavedName}, {City}, unsaved changes" : $"{SavedName}, {City}";

    /// <summary>Validates the company name, for example when the field is left.</summary>
    public bool ValidateName()
    {
        NameError = string.IsNullOrWhiteSpace(Name) ? "Enter the company name." : string.Empty;
        return NameError.Length == 0;
    }

    /// <summary>Validates the email address, for example when the field is left.</summary>
    public bool ValidateEmail()
    {
        EmailError = string.IsNullOrWhiteSpace(Email)
            ? "Enter an email address."
            : IsEmail(Email) ? string.Empty : "Enter an email address like name@example.com.";
        return EmailError.Length == 0;
    }

    /// <summary>Saves the draft. The values stay in the form when validation fails.</summary>
    public bool Save()
    {
        var nameValid = ValidateName();
        var emailValid = ValidateEmail();
        if (!nameValid || !emailValid)
        {
            return false;
        }

        _saved = Current();
        OnPropertyChanged(nameof(SavedName));
        OnPropertyChanged(nameof(HasChanges));
        OnPropertyChanged(nameof(AccessibleName));
        return true;
    }

    /// <summary>Discards the draft and returns to the saved values.</summary>
    public void Discard()
    {
        Load(_saved);
        NameError = string.Empty;
        EmailError = string.Empty;
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(HasChanges))
        {
            base.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(AccessibleName)));
        }
    }

    private static bool IsEmail(string text)
        => MailAddress.TryCreate(text.Trim(), out var address) && address.Host.Contains('.', StringComparison.Ordinal);

    private Snapshot Current() => new(Name, Contact, Email, Phone, Status, Since ?? default, Notes);

    private void Load(Snapshot values)
    {
        Name = values.Name;
        Contact = values.Contact;
        Email = values.Email;
        Phone = values.Phone;
        Status = values.Status;
        Since = values.Since;
        Notes = values.Notes;
    }

    private sealed record Snapshot(string Name, string Contact, string Email, string Phone, string Status, DateTimeOffset Since, string Notes);
}

/// <summary>
/// The Customers scenario: a list of customers and a form to edit the selected one.
/// </summary>
public sealed partial class CustomersViewModel : ObservableObject
{
    /// <summary>
    /// Creates the view model with the sample customers and selects the first one.
    /// </summary>
    public CustomersViewModel()
    {
        Customers =
        [
            new("Hofmann Logistics", "Hamburg", "Lea Schmid", "lea.schmid@hofmann-logistics.example", "+49 40 5550 1200", "Active", Date(2019, 3, 1), "Framework agreement renews every January."),
            new("Brenner Industries", "Stuttgart", "Felix Okafor", "f.okafor@brenner-industries.example", "+49 711 5550 340", "Active", Date(2021, 9, 15), string.Empty),
            new("Lakeside Utilities", "Konstanz", "Mara Lindqvist", "mara.lindqvist@lakeside-utilities.example", string.Empty, "Prospect", Date(2026, 10, 2), "Interested in the document workflow."),
            new("Kessler Construction", "Leipzig", "Tim Kraus", "tim.kraus@kessler-construction.example", "+49 341 5550 77", "Paused", Date(2018, 5, 20), string.Empty),
            new("Mertens Medical", "Münster", "Jonas Weber", "jonas.weber@mertens-medical.example", "+49 251 5550 900", "Active", Date(2024, 1, 8), "Data processing agreement signed."),
        ];
        Selected = Customers[0];
    }

    /// <summary>The customers.</summary>
    public ObservableCollection<CustomerItem> Customers { get; }

    /// <summary>The status options.</summary>
    public IReadOnlyList<string> Statuses => CustomerItem.Statuses;

    /// <summary>The customer in the form.</summary>
    [ObservableProperty]
    public partial CustomerItem Selected { get; set; }

    /// <summary>What just happened, for example "Changes saved."; empty if nothing.</summary>
    [ObservableProperty]
    public partial string Feedback { get; set; } = string.Empty;

    /// <summary>Raised when saving failed, so the view can move focus to the first invalid field.</summary>
    public event EventHandler? ValidationFailed;

    [RelayCommand]
    private void Save()
    {
        if (Selected.Save())
        {
            Feedback = "Changes saved.";
            return;
        }

        Feedback = string.Empty;
        ValidationFailed?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Discard()
    {
        Selected.Discard();
        Feedback = "Changes discarded.";
    }

    partial void OnSelectedChanged(CustomerItem oldValue, CustomerItem newValue)
    {
        if (oldValue is not null)
        {
            oldValue.PropertyChanged -= OnCustomerChanged;
        }

        newValue.PropertyChanged += OnCustomerChanged;
        Feedback = string.Empty;
    }

    private void OnCustomerChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CustomerItem.HasChanges) && Selected.HasChanges)
        {
            Feedback = string.Empty;
        }
    }

    private static DateTimeOffset Date(int year, int month, int day) => new(year, month, day, 0, 0, 0, TimeSpan.FromHours(1));
}
