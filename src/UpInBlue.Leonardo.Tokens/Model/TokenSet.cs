namespace UpInBlue.Leonardo.Tokens.Model;

/// <summary>
/// All tokens of the token file with resolved values, in file order.
/// </summary>
internal sealed class TokenSet
{
    private readonly Dictionary<string, Token> _byPath;
    private readonly IReadOnlyDictionary<string, string> _groupDescriptions;

    public TokenSet(IReadOnlyList<Token> tokens, IReadOnlyList<TypeRole> typeRoles, IReadOnlyDictionary<string, string> groupDescriptions)
    {
        Tokens = tokens;
        TypeRoles = typeRoles;
        _groupDescriptions = groupDescriptions;
        _byPath = tokens.ToDictionary(token => token.Path, StringComparer.Ordinal);
    }

    /// <summary>
    /// All tokens in file order. Composite typography values are in <see cref="TypeRoles"/>.
    /// </summary>
    public IReadOnlyList<Token> Tokens { get; }

    /// <summary>
    /// The type roles from the <c>typography</c> group, in file order.
    /// </summary>
    public IReadOnlyList<TypeRole> TypeRoles { get; }

    public Token this[string path]
        => _byPath.TryGetValue(path, out var token)
            ? token
            : throw new TokenException($"The token '{path}' does not exist.");

    public bool Contains(string path) => _byPath.ContainsKey(path);

    /// <summary>
    /// The tokens directly inside a group, for example <c>theme.light</c>, in file order.
    /// </summary>
    public IReadOnlyList<Token> Children(string groupPath)
    {
        var prefix = groupPath + ".";
        return Tokens
            .Where(token => token.Path.StartsWith(prefix, StringComparison.Ordinal) && token.Path.IndexOf('.', prefix.Length) < 0)
            .ToList();
    }

    /// <summary>
    /// All tokens anywhere inside a group, in file order.
    /// </summary>
    public IReadOnlyList<Token> Descendants(string groupPath)
    {
        var prefix = groupPath + ".";
        return Tokens.Where(token => token.Path.StartsWith(prefix, StringComparison.Ordinal)).ToList();
    }

    /// <summary>
    /// The <c>$description</c> of a group; the empty path is the file itself.
    /// </summary>
    public string? GroupDescription(string groupPath)
        => _groupDescriptions.TryGetValue(groupPath, out var description) ? description : null;
}
