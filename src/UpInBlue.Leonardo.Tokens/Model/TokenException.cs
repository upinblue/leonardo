namespace UpInBlue.Leonardo.Tokens.Model;

/// <summary>
/// An error in the token file, such as an invalid value, an unknown reference or a reference cycle.
/// </summary>
internal sealed class TokenException : Exception
{
    public TokenException(string message)
        : base(message)
    {
    }

    public TokenException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
