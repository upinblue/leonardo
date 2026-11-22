using System.Text;
using UpInBlue.Leonardo.Tokens.Model;
using UpInBlue.Leonardo.Tokens.Parsing;

namespace UpInBlue.Leonardo.Tokens.Generation;

/// <summary>
/// Produces every output from the token file and the design language. Pure: no file system access.
/// </summary>
internal static class TokenGenerator
{
    public const string TokensPath = "tokens/leonardo.tokens.json";
    public const string DesignLanguagePath = "docs/design-language.md";
    public const string SkillTokensPath = "claude/skills/leonardo/references/leonardo.tokens.json";
    public const string SkillDesignLanguagePath = "claude/skills/leonardo/references/design-language.md";

    /// <summary>
    /// The directories whose content belongs entirely to the generator. Files in them that the generator
    /// no longer produces are deleted.
    /// </summary>
    public static readonly IReadOnlyList<string> OwnedDirectories =
    [
        "src/UpInBlue.Leonardo.WinUI/Themes/Generated",
        "src/UpInBlue.Leonardo.WinUI/Generated",
        "src/UpInBlue.Leonardo.Wpf/Themes/Generated",
        "src/UpInBlue.Leonardo.Wpf/Generated",
        "generated/apple",
        "generated/web",
        "generated/presentation",
    ];

    /// <summary>
    /// Generates all outputs, in a stable order.
    /// </summary>
    /// <param name="tokensFile">The bytes of <c>tokens/leonardo.tokens.json</c>.</param>
    /// <param name="designLanguageFile">The bytes of <c>docs/design-language.md</c>.</param>
    /// <exception cref="TokenException">The token file is invalid.</exception>
    public static IReadOnlyList<GeneratedFile> Generate(byte[] tokensFile, byte[] designLanguageFile)
    {
        var tokens = TokenParser.Parse(DecodeUtf8(tokensFile));
        var catalog = new TokenCatalog(tokens);

        var files = new List<GeneratedFile>
        {
            XamlGenerator.WinUI(catalog),
            CSharpGenerator.WinUI(catalog),
            XamlGenerator.Wpf(catalog, dark: false),
            XamlGenerator.Wpf(catalog, dark: true),
            CSharpGenerator.Wpf(catalog),
        };
        files.AddRange(AppleGenerator.Generate(catalog));
        files.Add(WebGenerator.Generate(catalog));
        files.Add(PresentationGenerator.Generate(catalog));
        files.Add(new GeneratedFile(SkillDesignLanguagePath, designLanguageFile));
        files.Add(new GeneratedFile(SkillTokensPath, tokensFile));
        return files;
    }

    private static string DecodeUtf8(byte[] bytes)
    {
        try
        {
            var text = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true).GetString(bytes);
            return text.TrimStart('﻿');
        }
        catch (DecoderFallbackException exception)
        {
            throw new TokenException("The token file is not valid UTF-8.", exception);
        }
    }
}
