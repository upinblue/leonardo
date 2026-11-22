using System.Text;
using System.Text.RegularExpressions;

namespace UpInBlue.Leonardo.Check;

/// <summary>
/// The text of a file: the original text, a copy in which comments are blanked out, and line numbers.
/// Blanking keeps every character offset and line break, so offsets in the masked text are offsets in the file.
/// </summary>
internal sealed partial class SourceText
{
    private readonly int[] _lineStarts;
    private readonly string[] _lines;

    public SourceText(string displayPath, string text, string masked, string? code = null)
    {
        if (text.Length != masked.Length || (code is not null && code.Length != text.Length))
        {
            throw new ArgumentException("The masked texts must have the length of the original text.", nameof(masked));
        }

        DisplayPath = displayPath;
        Text = text;
        Masked = masked;
        Code = code ?? masked;

        var starts = new List<int> { 0 };
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                starts.Add(i + 1);
            }
        }

        _lineStarts = [.. starts];
        _lines = text.Split('\n');
    }

    /// <summary>The path shown in the output.</summary>
    public string DisplayPath { get; }

    /// <summary>The original text.</summary>
    public string Text { get; }

    /// <summary>The text with comments replaced by spaces.</summary>
    public string Masked { get; }

    /// <summary>
    /// The text with comments and the contents of string literals replaced by spaces, for rules that look for API
    /// calls. For markup and CSS it is the same as <see cref="Masked"/>.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// The code from an offset to the end of its line, shortened for a message.
    /// </summary>
    public string Snippet(int offset)
    {
        var end = Masked.IndexOf('\n', offset);
        var text = (end < 0 ? Masked[offset..] : Masked[offset..end]).TrimEnd('\r');
        return Vocabulary.Excerpt(text);
    }

    /// <summary>
    /// The masked text of a 1-based line.
    /// </summary>
    public string MaskedLine(int line)
    {
        if (line < 1 || line > _lineStarts.Length)
        {
            return string.Empty;
        }

        var start = _lineStarts[line - 1];
        var end = line < _lineStarts.Length ? _lineStarts[line] : Masked.Length;
        return Masked[start..end].TrimEnd('\r', '\n');
    }

    /// <summary>
    /// The 1-based line of a character offset.
    /// </summary>
    public int LineOf(int offset)
    {
        var index = Array.BinarySearch(_lineStarts, offset);
        return (index >= 0 ? index : ~index - 1) + 1;
    }

    /// <summary>
    /// The text of a 1-based line, without the line break.
    /// </summary>
    public string Line(int line) => line >= 1 && line <= _lines.Length ? _lines[line - 1].TrimEnd('\r') : string.Empty;

    /// <summary>
    /// Whether a finding of <paramref name="rule"/> on <paramref name="line"/> is suppressed by a comment
    /// <c>leonardo-check: ignore LEO001 reason</c> on the same line or the line above. A reason is required.
    /// </summary>
    public bool IsSuppressed(int line, Rule rule) => Suppresses(Line(line), rule) || Suppresses(Line(line - 1), rule);

    private static bool Suppresses(string line, Rule rule)
    {
        foreach (Match match in SuppressionRegex().Matches(line))
        {
            var ids = match.Groups["ids"].Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var reason = match.Groups["reason"].Value.Replace("-->", string.Empty, StringComparison.Ordinal).Replace("*/", string.Empty, StringComparison.Ordinal);
            if (ids.Contains(rule.Id, StringComparer.OrdinalIgnoreCase) && reason.Count(char.IsLetter) >= 3)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Blanks out XML and HTML comments.
    /// </summary>
    public static string MaskMarkupComments(string text)
    {
        var builder = new StringBuilder(text);
        var start = text.IndexOf("<!--", StringComparison.Ordinal);
        while (start >= 0)
        {
            var end = text.IndexOf("-->", start + 4, StringComparison.Ordinal);
            end = end < 0 ? text.Length : end + 3;
            Blank(builder, start, end);
            start = end < text.Length ? text.IndexOf("<!--", end, StringComparison.Ordinal) : -1;
        }

        return builder.ToString();
    }

    /// <summary>
    /// Blanks out <c>//</c> and <c>/* */</c> comments of C-like languages (C#, Swift, TypeScript), leaving string
    /// literals alone. <paramref name="lineComments"/> is false for CSS, which only has block comments.
    /// </summary>
    public static string MaskCodeComments(string text, bool lineComments = true, bool blankStrings = false)
    {
        var builder = new StringBuilder(text);
        var i = 0;
        while (i < text.Length)
        {
            var c = text[i];
            var next = i + 1 < text.Length ? text[i + 1] : '\0';
            if (c == '/' && next == '*')
            {
                var end = text.IndexOf("*/", i + 2, StringComparison.Ordinal);
                end = end < 0 ? text.Length : end + 2;
                Blank(builder, i, end);
                i = end;
            }
            else if (lineComments && c == '/' && next == '/')
            {
                var end = text.IndexOf('\n', i);
                end = end < 0 ? text.Length : end;
                Blank(builder, i, end);
                i = end;
            }
            else if (c == '"' && i + 2 < text.Length && text[i + 1] == '"' && text[i + 2] == '"')
            {
                var end = text.IndexOf("\"\"\"", i + 3, StringComparison.Ordinal);
                end = end < 0 ? text.Length : end + 3;
                BlankInside(builder, i + 3, end - 3, blankStrings);
                i = end;
            }
            else if (c == '@' && next == '"')
            {
                var end = SkipVerbatimString(text, i + 2);
                BlankInside(builder, i + 2, end - 1, blankStrings);
                i = end;
            }
            else if (c is '"' or '\'' or '`')
            {
                var end = SkipString(text, i + 1, c);
                BlankInside(builder, i + 1, end - 1, blankStrings);
                i = end;
            }
            else
            {
                i++;
            }
        }

        return builder.ToString();
    }

    private static int SkipString(string text, int i, char quote)
    {
        while (i < text.Length)
        {
            var c = text[i];
            if (c == '\\')
            {
                i += 2;
            }
            else if (c == quote)
            {
                return i + 1;
            }
            else if (c == '\n' && quote != '`')
            {
                // An unterminated string (or an apostrophe in a Swift or TypeScript comment-free text): stop at the line end.
                return i;
            }
            else
            {
                i++;
            }
        }

        return i;
    }

    private static int SkipVerbatimString(string text, int i)
    {
        while (i < text.Length)
        {
            if (text[i] == '"')
            {
                if (i + 1 < text.Length && text[i + 1] == '"')
                {
                    i += 2;
                    continue;
                }

                return i + 1;
            }

            i++;
        }

        return i;
    }

    /// <summary>
    /// Keeps only the given ranges of a text and blanks everything else. Used to check the CSS inside HTML.
    /// </summary>
    public static string KeepOnly(string text, IEnumerable<(int Start, int End)> ranges)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            builder.Append(c is '\n' or '\r' ? c : ' ');
        }

        foreach (var (start, end) in ranges)
        {
            for (var i = start; i < end; i++)
            {
                builder[i] = text[i];
            }
        }

        return builder.ToString();
    }

    private static void BlankInside(StringBuilder builder, int start, int end, bool blank)
    {
        if (blank && end > start)
        {
            Blank(builder, start, end);
        }
    }

    private static void Blank(StringBuilder builder, int start, int end)
    {
        for (var i = start; i < end; i++)
        {
            if (builder[i] is not ('\n' or '\r'))
            {
                builder[i] = ' ';
            }
        }
    }

    [GeneratedRegex(@"leonardo-check:\s*ignore\s+(?<ids>LEO\d{3}(?:\s*,\s*LEO\d{3})*)(?<reason>.*)", RegexOptions.IgnoreCase)]
    private static partial Regex SuppressionRegex();
}
