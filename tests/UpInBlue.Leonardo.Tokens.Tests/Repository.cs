using UpInBlue.Leonardo.Tokens.Generation;

namespace UpInBlue.Leonardo.Tokens.Tests;

/// <summary>
/// Access to the files of this repository.
/// </summary>
internal static class Repository
{
    private static readonly Lazy<string> LazyRoot = new(FindRoot);

    public static string Root => LazyRoot.Value;

    public static byte[] TokensFile => File.ReadAllBytes(PathOf(TokenGenerator.TokensPath));

    public static byte[] DesignLanguageFile => File.ReadAllBytes(PathOf(TokenGenerator.DesignLanguagePath));

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
