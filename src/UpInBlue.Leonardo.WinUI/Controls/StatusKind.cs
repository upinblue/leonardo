namespace UpInBlue.Leonardo.WinUI.Controls;

/// <summary>
/// The states a <see cref="StatusDot"/> shows. Each has its own shape, so the state never depends on color alone.
/// </summary>
/// <remarks>
/// The kinds map by meaning, not by the word in their name. Pick the kind whose meaning fits the app's state:
/// <see cref="New"/> means new since the last visit, <see cref="CheckedOut"/> means attention (someone else holds it, or
/// it waits for someone), and <see cref="Conflict"/> means broken or overdue. Always say the actual state in the
/// <c>StatusDot.Description</c>, for example "Waiting for Lea Schmid since Monday" for an approval that uses
/// <see cref="CheckedOut"/>. The colors are Fluent's status brushes, so they follow the theme and contrast themes.
/// </remarks>
public enum StatusKind
{
    /// <summary>New since the last visit: a filled dot in the accent color.</summary>
    New,

    /// <summary>
    /// Attention: a ring in Fluent's caution color (<c>SystemFillColorCautionBrush</c>). Named after its first use, a file
    /// checked out by another person; it fits every state that needs attention but is not broken, such as waiting.
    /// </summary>
    CheckedOut,

    /// <summary>
    /// Broken or overdue: a filled circle with an exclamation mark in Fluent's critical color
    /// (<c>SystemFillColorCriticalBrush</c>). Named after its first use, a sync conflict; it fits every state that is
    /// broken or overdue.
    /// </summary>
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
