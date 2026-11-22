using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace UpInBlue.Leonardo.WinUI.Controls;

/// <summary>
/// What a phrase of a <see cref="SummaryLine"/> counts.
/// </summary>
public enum SummaryKind
{
    /// <summary>New items: "2 new".</summary>
    New,

    /// <summary>Items checked out by others: "1 checked out".</summary>
    CheckedOut,

    /// <summary>Items with a conflict: "1 conflict", "2 conflicts".</summary>
    Conflict,

    /// <summary>Anything else; set <see cref="SummaryLineItem.Label"/> and, if it differs, <see cref="SummaryLineItem.PluralLabel"/>.</summary>
    Custom,
}

/// <summary>
/// One phrase of a <see cref="SummaryLine"/>, for example "2 new". It is a toggle filter: activating it runs
/// <see cref="Command"/> and shows the active state.
/// </summary>
public sealed class SummaryLineItem : INotifyPropertyChanged
{
    private int _count;
    private SummaryKind _kind;
    private string? _label;
    private string? _pluralLabel;
    private bool _isActive;
    private ICommand? _command;
    private object? _commandParameter;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>How many items the phrase counts. Phrases with 0 are not shown.</summary>
    public int Count
    {
        get => _count;
        set => Set(ref _count, Math.Max(0, value));
    }

    /// <summary>What is counted. Defines the default label.</summary>
    public SummaryKind Kind
    {
        get => _kind;
        set => Set(ref _kind, value);
    }

    /// <summary>The label after the count when it is 1, for example "conflict". Empty uses the default label of <see cref="Kind"/>.</summary>
    public string? Label
    {
        get => _label;
        set => Set(ref _label, value);
    }

    /// <summary>The label after other counts, for example "conflicts". Empty uses <see cref="Label"/> or the default plural of <see cref="Kind"/>.</summary>
    public string? PluralLabel
    {
        get => _pluralLabel;
        set => Set(ref _pluralLabel, value);
    }

    /// <summary>Whether the filter of this phrase is on.</summary>
    public bool IsActive
    {
        get => _isActive;
        set => Set(ref _isActive, value);
    }

    /// <summary>Runs when the phrase is toggled, after <see cref="IsActive"/> has changed. Receives <see cref="CommandParameter"/>.</summary>
    public ICommand? Command
    {
        get => _command;
        set => Set(ref _command, value);
    }

    /// <summary>The parameter of <see cref="Command"/>. Null passes the item itself.</summary>
    public object? CommandParameter
    {
        get => _commandParameter;
        set => Set(ref _commandParameter, value);
    }

    /// <summary>
    /// The phrase, for example "2 new" or "1 conflict".
    /// </summary>
    public string Phrase(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        return $"{Count.ToString("N0", culture)} {CurrentLabel()}";
    }

    /// <summary>
    /// The label that fits <see cref="Count"/>.
    /// </summary>
    public string CurrentLabel()
    {
        var singular = string.IsNullOrWhiteSpace(Label) ? DefaultLabel(Kind, plural: false) : Label.Trim();
        if (Count == 1)
        {
            return singular;
        }

        if (!string.IsNullOrWhiteSpace(PluralLabel))
        {
            return PluralLabel.Trim();
        }

        return string.IsNullOrWhiteSpace(Label) ? DefaultLabel(Kind, plural: true) : singular;
    }

    /// <summary>
    /// The default label of a kind. "new" and "checked out" don't change in the plural; "conflict" does.
    /// </summary>
    public static string DefaultLabel(SummaryKind kind, bool plural) => kind switch
    {
        SummaryKind.New => "new",
        SummaryKind.CheckedOut => "checked out",
        SummaryKind.Conflict => plural ? "conflicts" : "conflict",
        _ => plural ? "items" : "item",
    };

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
