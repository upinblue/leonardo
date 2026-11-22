namespace UpInBlue.Leonardo.Tokens.Model;

/// <summary>
/// The outputs a token can be limited to with <c>$extensions.platforms</c>. The skill copy always carries every token.
/// </summary>
internal enum OutputPlatform
{
    /// <summary>WinUI 3 and WPF: XAML resources and the C# class <c>LeonardoTokens</c> (<c>windows</c>).</summary>
    Windows,

    /// <summary>iOS and macOS: the asset catalog and the Swift values (<c>apple</c>).</summary>
    Apple,

    /// <summary>Standalone web: <c>leonardo.css</c> (<c>web</c>).</summary>
    Web,

    /// <summary>PowerPoint: the presentation values (<c>presentation</c>).</summary>
    Presentation,
}
