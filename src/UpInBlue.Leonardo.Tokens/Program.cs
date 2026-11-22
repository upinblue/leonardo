using UpInBlue.Leonardo.Tokens.Generation;
using UpInBlue.Leonardo.Tokens.Model;

const string Usage = """
    Usage: dotnet run --project src/UpInBlue.Leonardo.Tokens -- <command> [--root <path>]

    Commands:
      generate   Regenerates every output from tokens/leonardo.tokens.json.
      check      Exits with code 1 if any output is out of date. Changes nothing.

    Options:
      --root     The repository root. Default: the nearest parent of the current directory
                 that contains tokens/leonardo.tokens.json.
    """;

if (args.Length == 0 || args[0] is not ("generate" or "check"))
{
    Console.Error.WriteLine(Usage);
    return 2;
}

string? root = null;
for (var index = 1; index < args.Length; index++)
{
    if (args[index] == "--root" && index + 1 < args.Length)
    {
        root = Path.GetFullPath(args[++index]);
    }
    else
    {
        Console.Error.WriteLine(Usage);
        return 2;
    }
}

root ??= FindRepositoryRoot(Directory.GetCurrentDirectory());
if (root is null || !File.Exists(Path.Combine(root, TokenGenerator.TokensPath)))
{
    Console.Error.WriteLine($"Could not find {TokenGenerator.TokensPath}. Run the command inside the repository or pass --root.");
    return 2;
}

IReadOnlyList<GeneratedFile> files;
try
{
    files = TokenGenerator.Generate(
        File.ReadAllBytes(Path.Combine(root, TokenGenerator.TokensPath)),
        File.ReadAllBytes(Path.Combine(root, TokenGenerator.DesignLanguagePath)));
}
catch (TokenException exception)
{
    Console.Error.WriteLine($"error: {exception.Message}");
    return 1;
}

var writer = new OutputWriter(root);
if (args[0] == "check")
{
    var report = writer.Compare(files);
    foreach (var path in report.Changed)
    {
        Console.WriteLine($"out of date: {path}");
    }

    foreach (var path in report.Stale)
    {
        Console.WriteLine($"stale:       {path}");
    }

    Console.WriteLine(report.IsUpToDate
        ? $"All {files.Count} generated files are up to date."
        : "Generated files are out of date. Run the generate command.");
    return report.IsUpToDate ? 0 : 1;
}

var result = writer.Write(files);
foreach (var path in result.Changed)
{
    Console.WriteLine($"written: {path}");
}

foreach (var path in result.Stale)
{
    Console.WriteLine($"deleted: {path}");
}

Console.WriteLine($"{result.Changed.Count} written, {result.Unchanged.Count} unchanged, {result.Stale.Count} deleted.");
return 0;

static string? FindRepositoryRoot(string start)
{
    for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
    {
        if (File.Exists(Path.Combine(directory.FullName, TokenGenerator.TokensPath)))
        {
            return directory.FullName;
        }
    }

    return null;
}
