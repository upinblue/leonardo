namespace UpInBlue.Leonardo.Check;

/// <summary>
/// A violation found in a file.
/// </summary>
/// <param name="Path">The path of the file, as it is shown in the output.</param>
/// <param name="Line">The 1-based line number.</param>
/// <param name="Rule">The rule that was violated.</param>
/// <param name="Message">What was found, quoting the offending code.</param>
/// <param name="Suggestion">What to use instead.</param>
internal sealed record Finding(string Path, int Line, Rule Rule, string Message, string Suggestion)
{
    /// <summary>
    /// The finding in the output format: file, line, rule, message, and the suggestion on the next line.
    /// </summary>
    public string Format() => $"{Path}:{Line}: {Rule}: {Message}{Environment.NewLine}    Suggestion: {Suggestion}";
}

/// <summary>
/// Collects the findings of one file. Keeps one finding per line and rule.
/// </summary>
internal sealed class FindingList(SourceText source)
{
    private readonly List<Finding> _findings = [];
    private readonly HashSet<(int Line, string RuleId)> _seen = [];

    public SourceText Source { get; } = source;

    public IReadOnlyList<Finding> Items => _findings;

    /// <summary>
    /// Adds a finding at a character offset of the file, unless the line already has one for this rule or carries
    /// a suppression comment for it.
    /// </summary>
    public void Add(int offset, Rule rule, string message, string suggestion)
    {
        var line = Source.LineOf(offset);
        if (Source.IsSuppressed(line, rule) || !_seen.Add((line, rule.Id)))
        {
            return;
        }

        _findings.Add(new Finding(Source.DisplayPath, line, rule, message, suggestion));
    }
}
