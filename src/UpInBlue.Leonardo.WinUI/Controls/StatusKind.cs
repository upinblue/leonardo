namespace UpInBlue.Leonardo.WinUI.Controls;

/// <summary>
/// The states a <see cref="StatusDot"/> shows. Each has its own shape, so the state never depends on color alone.
/// </summary>
public enum StatusKind
{
    /// <summary>New since the last visit: a filled dot in the accent color.</summary>
    New,

    /// <summary>Checked out by another person: a ring in the system caution color.</summary>
    CheckedOut,

    /// <summary>Something is broken, for example a sync conflict: a filled circle with an exclamation mark in the system critical color.</summary>
    Conflict,
}

/// <summary>
/// Texts of the status kinds.
/// </summary>
public static class StatusText
{
    /// <summary>
    /// The name of a kind, used when a <see cref="StatusDot"/> has no description: "New", "Checked out" or "Conflict".
    /// </summary>
    public static string DefaultName(StatusKind kind) => kind switch
    {
        StatusKind.New => "New",
        StatusKind.CheckedOut => "Checked out",
        StatusKind.Conflict => "Conflict",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };

    /// <summary>
    /// The accessible name and tooltip of a status: the description, for example "Checked out by Anna Berg",
    /// or the name of the kind if the description is missing.
    /// </summary>
    public static string Describe(StatusKind kind, string? description)
        => string.IsNullOrWhiteSpace(description) ? DefaultName(kind) : description.Trim();
}
