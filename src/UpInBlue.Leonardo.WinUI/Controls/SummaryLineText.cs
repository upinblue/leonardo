using System.Globalization;

namespace UpInBlue.Leonardo.WinUI.Controls;

/// <summary>
/// The sentence of a <see cref="SummaryLine"/>: what it shows and what a screen reader reads.
/// </summary>
/// <param name="Phrases">The phrases with a count above 0, in their order.</param>
/// <param name="Text">The visible sentence, for example "Since your last visit: 2 new · 1 checked out · 1 conflict".</param>
/// <param name="SpokenText">The sentence for screen readers, with commas instead of middle dots.</param>
public sealed record SummarySentence(IReadOnlyList<SummaryLineItem> Phrases, string Text, string SpokenText)
{
    /// <summary>Whether there is anything to say. A summary line without phrases hides itself.</summary>
    public bool IsVisible => Phrases.Count > 0;
}

/// <summary>
/// Composes the sentence of a <see cref="SummaryLine"/>. Used by the control and testable without WinUI.
/// </summary>
public static class SummaryLineText
{
    /// <summary>The separator between phrases in the visible sentence: a middle dot with spaces.</summary>
    public const string Separator = " · ";

    /// <summary>The separator between phrases in the spoken sentence.</summary>
    public const string SpokenSeparator = ", ";

    /// <summary>
    /// Composes the sentence. Phrases with a count of 0 are left out.
    /// </summary>
    /// <param name="prefix">The start of the sentence, for example "Since your last visit:". May be empty.</param>
    /// <param name="items">The phrases.</param>
    /// <param name="culture">The culture for the numbers.</param>
    public static SummarySentence Compose(string? prefix, IEnumerable<SummaryLineItem?>? items, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        var phrases = (items ?? []).OfType<SummaryLineItem>().Where(item => item.Count > 0).ToList();
        if (phrases.Count == 0)
        {
            return new SummarySentence(phrases, string.Empty, string.Empty);
        }

        var words = phrases.Select(item => item.Phrase(culture)).ToList();
        var start = string.IsNullOrWhiteSpace(prefix) ? string.Empty : prefix.Trim() + " ";
        return new SummarySentence(
            phrases,
            start + string.Join(Separator, words),
            start + string.Join(SpokenSeparator, words));
    }

    /// <summary>
    /// The visibility rule: a summary line shows only if at least one count is above 0.
    /// </summary>
    public static bool IsVisible(IEnumerable<SummaryLineItem?>? items)
        => (items ?? []).OfType<SummaryLineItem>().Any(item => item.Count > 0);
}
