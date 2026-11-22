namespace UpInBlue.Leonardo.Tokens.Generation;

/// <summary>
/// Writes generated files into the repository, or checks that the repository is up to date.
/// </summary>
internal sealed class OutputWriter(string repositoryRoot)
{
    /// <summary>
    /// Writes every file whose content changed and deletes stale files in the generator's own directories.
    /// Line endings are ignored when comparing, because Git may check text files out with CRLF.
    /// </summary>
    public OutputReport Write(IReadOnlyList<GeneratedFile> files)
    {
        var report = Compare(files);
        foreach (var path in report.Changed)
        {
            var fullPath = FullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllBytes(fullPath, files.First(file => file.Path == path).Content);
        }

        foreach (var path in report.Stale)
        {
            File.Delete(FullPath(path));
        }

        foreach (var directory in TokenGenerator.OwnedDirectories)
        {
            DeleteEmptyDirectories(FullPath(directory));
        }

        return report;
    }

    /// <summary>
    /// Compares the generated files with the repository without changing anything.
    /// </summary>
    public OutputReport Compare(IReadOnlyList<GeneratedFile> files)
    {
        var changed = new List<string>();
        var unchanged = new List<string>();
        foreach (var file in files)
        {
            var fullPath = FullPath(file.Path);
            if (File.Exists(fullPath) && SameContent(File.ReadAllBytes(fullPath), file.Content))
            {
                unchanged.Add(file.Path);
            }
            else
            {
                changed.Add(file.Path);
            }
        }

        var expected = files.Select(file => file.Path).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var stale = TokenGenerator.OwnedDirectories
            .Select(FullPath)
            .Where(Directory.Exists)
            .SelectMany(directory => Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            .Select(path => Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/'))
            .Where(path => !expected.Contains(path))
            .Order(StringComparer.Ordinal)
            .ToList();

        return new OutputReport(changed, unchanged, stale);
    }

    /// <summary>
    /// Whether two files have the same content, ignoring the difference between CRLF and LF.
    /// </summary>
    public static bool SameContent(byte[] first, byte[] second)
        => first.AsSpan().SequenceEqual(second) || RemoveCarriageReturns(first).AsSpan().SequenceEqual(RemoveCarriageReturns(second));

    private static byte[] RemoveCarriageReturns(byte[] bytes) => bytes.Where(value => value != (byte)'\r').ToArray();

    private static void DeleteEmptyDirectories(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var child in Directory.EnumerateDirectories(directory))
        {
            DeleteEmptyDirectories(child);
        }

        if (!Directory.EnumerateFileSystemEntries(directory).Any())
        {
            Directory.Delete(directory);
        }
    }

    private string FullPath(string relativePath) => Path.GetFullPath(Path.Combine(repositoryRoot, relativePath));
}

/// <summary>
/// The result of comparing or writing generated files. Paths are relative to the repository root.
/// </summary>
internal sealed record OutputReport(IReadOnlyList<string> Changed, IReadOnlyList<string> Unchanged, IReadOnlyList<string> Stale)
{
    public bool IsUpToDate => Changed.Count == 0 && Stale.Count == 0;
}
