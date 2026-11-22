using System.Text.Json;

namespace UpInBlue.Leonardo.Tokens.Tests;

/// <summary>
/// Smoke tests for the inputs the token generator relies on.
/// </summary>
public sealed class RepositoryLayoutTests
{
    [Fact]
    public void TokenFileExistsAndIsValidJson()
    {
        var path = Path.Combine(FindRepositoryRoot(), "tokens", "leonardo.tokens.json");

        Assert.True(File.Exists(path), $"Token file not found: {path}");

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);
    }

    private static string FindRepositoryRoot()
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
