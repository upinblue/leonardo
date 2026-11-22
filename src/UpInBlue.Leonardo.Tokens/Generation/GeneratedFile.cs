using System.Text;

namespace UpInBlue.Leonardo.Tokens.Generation;

/// <summary>
/// One output file.
/// </summary>
/// <param name="Path">The path relative to the repository root, with forward slashes.</param>
/// <param name="Content">The exact bytes of the file.</param>
internal sealed record GeneratedFile(string Path, byte[] Content)
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// A text file, encoded as UTF-8 without byte order mark.
    /// </summary>
    public static GeneratedFile FromText(string path, string text) => new(path, Utf8WithoutBom.GetBytes(text));
}
