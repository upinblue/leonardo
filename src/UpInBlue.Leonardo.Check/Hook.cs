using System.Text;
using System.Text.Json;

namespace UpInBlue.Leonardo.Check;

/// <summary>
/// The Claude Code hook mode: reads a <c>PostToolUse</c> event from standard input and checks the file the tool
/// just wrote or edited.
/// </summary>
/// <remarks>
/// Claude Code shows the standard error of a <c>PostToolUse</c> hook to Claude when the hook exits with code 2, so
/// findings go to standard error with exit code 2. Everything else (no file, another file type, a generated file,
/// no findings) exits with 0 and prints nothing.
/// </remarks>
internal static class Hook
{
    /// <summary>The exit code that makes Claude Code show standard error to Claude.</summary>
    public const int FindingsExitCode = 2;

    /// <summary>
    /// The result of the hook: the exit code and the text for standard error.
    /// </summary>
    public sealed record Result(int ExitCode, string Message);

    /// <summary>
    /// Runs the hook for one event.
    /// </summary>
    /// <param name="eventJson">The JSON Claude Code passes on standard input.</param>
    /// <param name="readFile">Reads a file; a parameter so tests do not need files.</param>
    public static Result Run(string eventJson, Func<string, string?> readFile)
    {
        HookEvent hookEvent;
        try
        {
            hookEvent = HookEvent.Parse(eventJson);
        }
        catch (JsonException)
        {
            return new Result(0, string.Empty);
        }

        if (hookEvent.FilePath is not { Length: > 0 } filePath || !LeonardoChecker.IsCandidate(filePath))
        {
            return new Result(0, string.Empty);
        }

        var text = readFile(filePath);
        if (text is null)
        {
            return new Result(0, string.Empty);
        }

        var display = DisplayPath(filePath, hookEvent.WorkingDirectory);
        var all = LeonardoChecker.Check(display, text);
        if (all.Count == 0)
        {
            return new Result(0, string.Empty);
        }

        // For edits, report what the edit introduced; findings elsewhere in the file are only counted.
        var changedLines = ChangedLines(text, hookEvent.NewTexts);
        var current = changedLines is null ? all : [.. all.Where(f => changedLines.Contains(f.Line))];
        var elsewhere = all.Count - current.Count;
        if (current.Count == 0)
        {
            return new Result(0, string.Empty);
        }

        var message = new StringBuilder();
        message.AppendLine(current.Count == 1
            ? $"Leonardo check: 1 problem in {display} where you just changed it."
            : $"Leonardo check: {current.Count} problems in {display} where you just changed it.");
        foreach (var finding in current)
        {
            message.AppendLine(finding.Format());
        }

        if (elsewhere > 0)
        {
            message.AppendLine($"{elsewhere} more in other lines of this file (run leonardo-check {display}); leave them unless the task is about them.");
        }

        message.Append("Fix these with Leonardo tokens and resources (skill leonardo), or, if an exception is needed, add a comment `leonardo-check: ignore <rule id> <reason>`.");
        return new Result(FindingsExitCode, message.ToString());
    }

    /// <summary>
    /// The 1-based lines of <paramref name="text"/> that contain one of the new texts of an edit, or null when the
    /// whole file is new or the edit cannot be found (then every line counts).
    /// </summary>
    internal static HashSet<int>? ChangedLines(string text, IReadOnlyList<string>? newTexts)
    {
        if (newTexts is null)
        {
            return null;
        }

        var lines = new HashSet<int>();
        foreach (var newText in newTexts.Where(t => t.Trim().Length > 0))
        {
            var normalized = newText.Replace("\r\n", "\n");
            var haystack = text.Replace("\r\n", "\n");
            var found = false;
            for (var index = haystack.IndexOf(normalized, StringComparison.Ordinal); index >= 0;
                 index = haystack.IndexOf(normalized, index + Math.Max(1, normalized.Length), StringComparison.Ordinal))
            {
                found = true;
                var first = haystack.AsSpan(0, index).Count('\n') + 1;
                var count = normalized.TrimEnd('\n').Count(c => c == '\n') + 1;
                for (var line = first; line < first + count; line++)
                {
                    lines.Add(line);
                }
            }

            if (!found)
            {
                return null;
            }
        }

        return lines;
    }

    private static string DisplayPath(string filePath, string? workingDirectory)
    {
        if (workingDirectory is { Length: > 0 })
        {
            var relative = Path.GetRelativePath(workingDirectory, filePath);
            if (!relative.StartsWith("..", StringComparison.Ordinal) && !Path.IsPathRooted(relative))
            {
                return relative.Replace('\\', '/');
            }
        }

        return filePath.Replace('\\', '/');
    }

    /// <summary>
    /// The fields of a <c>PostToolUse</c> event the hook needs.
    /// </summary>
    /// <param name="FilePath">The file the tool wrote or edited.</param>
    /// <param name="WorkingDirectory">The working directory of the session, for relative paths in the output.</param>
    /// <param name="NewTexts">The new texts of an edit, or null for a tool that wrote the whole file.</param>
    internal sealed record HookEvent(string? FilePath, string? WorkingDirectory, IReadOnlyList<string>? NewTexts)
    {
        public static HookEvent Parse(string json)
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var cwd = String(root, "cwd");
            if (!root.TryGetProperty("tool_input", out var input) || input.ValueKind != JsonValueKind.Object)
            {
                return new HookEvent(null, cwd, null);
            }

            var path = String(input, "file_path");
            if (path is not null && !Path.IsPathRooted(path) && cwd is not null)
            {
                path = Path.Combine(cwd, path);
            }

            List<string>? newTexts = null;
            if (String(input, "new_string") is { } newString)
            {
                newTexts = [newString];
            }
            else if (input.TryGetProperty("edits", out var edits) && edits.ValueKind == JsonValueKind.Array)
            {
                newTexts = [.. edits.EnumerateArray().Select(edit => String(edit, "new_string")).OfType<string>()];
            }

            return new HookEvent(path, cwd, newTexts);
        }

        private static string? String(JsonElement element, string name)
            => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
    }
}
