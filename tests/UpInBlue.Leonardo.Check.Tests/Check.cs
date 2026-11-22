namespace UpInBlue.Leonardo.Check.Tests;

/// <summary>
/// Helpers for the tests: run the checker on a snippet and look at the rule identifiers.
/// </summary>
internal static class Check
{
    public static IReadOnlyList<Finding> Run(string path, string text) => LeonardoChecker.Check(path, text);

    public static string[] RuleIds(string path, string text) => [.. Run(path, text).Select(f => f.Rule.Id)];

    public static void Reports(Rule rule, string path, string text)
        => Assert.True(RuleIds(path, text).Contains(rule.Id), $"Expected {rule} in:{Environment.NewLine}{text}{Environment.NewLine}Found: {string.Join(", ", RuleIds(path, text))}");

    public static void Clean(string path, string text)
    {
        var findings = Run(path, text);
        Assert.True(findings.Count == 0, $"Expected no findings in:{Environment.NewLine}{text}{Environment.NewLine}Found:{Environment.NewLine}{string.Join(Environment.NewLine, findings.Select(f => f.Format()))}");
    }
}

/// <summary>
/// Access to the files of this repository.
/// </summary>
internal static class Repository
{
    private static readonly Lazy<string> LazyRoot = new(FindRoot);

    public static string Root => LazyRoot.Value;

    public static string PathOf(string relativePath) => Path.Combine(Root, relativePath);

    private static string FindRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "UpInBlue.Leonardo.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Repository root with UpInBlue.Leonardo.slnx not found.");
    }
}
