namespace UpInBlue.Leonardo.Tokens.Model;

/// <summary>
/// A token with its resolved value.
/// </summary>
/// <param name="Path">The dot-separated path, for example <c>theme.light.accent</c>.</param>
/// <param name="Type">The token type.</param>
/// <param name="Value">The resolved value: <see cref="RgbColor"/>, <see cref="Dimension"/>, <see cref="TokenDuration"/>, <see cref="CubicBezier"/> or <see cref="double"/>.</param>
/// <param name="Description">The <c>$description</c>, if any.</param>
/// <param name="Status">The value of <c>$extensions.status</c>, if any.</param>
/// <param name="Reference">The path of the referenced token if the value is a reference such as <c>{palette.brand}</c>.</param>
internal sealed record Token(string Path, TokenType Type, object Value, string? Description, string? Status, string? Reference)
{
    /// <summary>
    /// The last segment of the path, for example <c>accent</c>.
    /// </summary>
    public string Name => Path[(Path.LastIndexOf('.') + 1)..];

    /// <summary>
    /// Whether the value is a proposal that still has to be confirmed.
    /// </summary>
    public bool IsProposal => string.Equals(Status, "proposal", StringComparison.Ordinal);

    public RgbColor Color => As<RgbColor>();

    public Dimension Dimension => As<Dimension>();

    public TokenDuration Duration => As<TokenDuration>();

    public CubicBezier CubicBezier => As<CubicBezier>();

    public double Number => As<double>();

    private T As<T>()
        where T : struct
        => Value is T value
            ? value
            : throw new TokenException($"Token '{Path}' is a {Type} token, not a {typeof(T).Name}.");
}
