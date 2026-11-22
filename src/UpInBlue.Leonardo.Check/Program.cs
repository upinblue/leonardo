using UpInBlue.Leonardo.Check;

// leonardo-check: finds literal design values and common accessibility mistakes in UI code.
//
//   leonardo-check [paths...]   Checks files and folders (default: the current folder). Exit code 1 if there are findings.
//   leonardo-check --hook       Claude Code PostToolUse hook: reads the event from standard input and checks the
//                               edited file. Findings go to standard error with exit code 2.
//   leonardo-check --rules      Lists the rules.

if (args.Contains("--help") || args.Contains("-h"))
{
    Console.WriteLine("""
        leonardo-check – finds literal design values and common accessibility mistakes in UI code of up in blue apps.

        Usage:
          leonardo-check [paths...]   Check files and folders (default: the current folder).
          leonardo-check --hook       Run as a Claude Code PostToolUse hook (event JSON on standard input).
          leonardo-check --rules      List the rules.

        Checks .xaml, .cs, .swift, .css, .scss, .html and .tsx files. Skips generated files, hidden folders and
        bin, obj, Generated, node_modules and similar folders.
        Exit codes: 0 no findings, 1 findings, 3 a path does not exist. In hook mode: 0, or 2 with findings on standard error.
        A justified exception: a comment "leonardo-check: ignore LEO001 <reason>" on the line or the line above.
        """);
    return 0;
}

if (args.Contains("--rules"))
{
    foreach (var rule in Rules.All)
    {
        Console.WriteLine($"{rule.Id} {rule.Name}: {rule.Summary}");
    }

    return 0;
}

if (args.Contains("--hook"))
{
    var result = Hook.Run(Console.In.ReadToEnd(), path => File.Exists(path) ? File.ReadAllText(path) : null);
    if (result.Message.Length > 0)
    {
        Console.Error.WriteLine(result.Message);
    }

    return result.ExitCode;
}

var paths = args.Where(a => !a.StartsWith("--", StringComparison.Ordinal)).ToList();
if (paths.Count == 0)
{
    paths.Add(".");
}

IReadOnlyList<Finding> findings;
try
{
    findings = LeonardoChecker.CheckPaths(paths, Directory.GetCurrentDirectory());
}
catch (FileNotFoundException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 3;
}

foreach (var finding in findings)
{
    Console.WriteLine(finding.Format());
}

var files = findings.Select(f => f.Path).Distinct().Count();
Console.WriteLine(findings.Count == 0
    ? "Leonardo check: no findings."
    : $"Leonardo check: {findings.Count} finding{(findings.Count == 1 ? "" : "s")} in {files} file{(files == 1 ? "" : "s")}.");
return findings.Count == 0 ? 0 : 1;
