using UpInBlue.Leonardo.Tokens.Model;

namespace UpInBlue.Leonardo.Tokens.Generation;

/// <summary>
/// The tokens sorted into the categories Leonardo defines. Every token must belong to a category,
/// so that a new token never silently disappears from the outputs.
/// </summary>
internal sealed class TokenCatalog
{
    private const string MotionIos = "motion.ios";
    private const string MotionPresentation = "motion.presentation";

    public TokenCatalog(TokenSet tokens)
    {
        Tokens = tokens;
        Palette = OfType(tokens.Children("palette"), TokenType.Color);
        Light = OfType(tokens.Children("theme.light"), TokenType.Color);
        Dark = OfType(tokens.Children("theme.dark"), TokenType.Color);
        Space = Pixels(tokens.Children("space"));
        Radius = Pixels(tokens.Children("radius"));
        Density = Pixels(tokens.Children("density"));
        Size = Pixels(tokens.Children("size"));
        IosSpring = tokens.Children($"{MotionIos}.spring");
        PresentationMotion = tokens.Children(MotionPresentation);
        Motion = tokens.Descendants("motion")
            .Where(token => !IsInside(token, MotionIos) && !IsInside(token, MotionPresentation))
            .Select(token =>
            {
                var segments = token.Path.Split('.');
                return segments.Length == 3
                    ? new MotionToken(segments[1], token)
                    : throw new TokenException($"Motion token '{token.Path}' must be directly inside a group such as motion.duration.");
            })
            .ToList();

        ValidateThemes();
        ValidateCoverage();
        ValidatePlatforms();
    }

    public TokenSet Tokens { get; }

    /// <summary>The blue scale (<c>palette.*</c>).</summary>
    public IReadOnlyList<Token> Palette { get; }

    /// <summary>Theme colors for light mode (<c>theme.light.*</c>).</summary>
    public IReadOnlyList<Token> Light { get; }

    /// <summary>Theme colors for dark mode (<c>theme.dark.*</c>), in the same order as <see cref="Light"/>.</summary>
    public IReadOnlyList<Token> Dark { get; }

    public IReadOnlyList<Token> Space { get; }

    public IReadOnlyList<Token> Radius { get; }

    public IReadOnlyList<Token> Density { get; }

    /// <summary>Fixed sizes of small elements (<c>size.*</c>), for every platform. Use <see cref="SizeFor"/> in an output.</summary>
    public IReadOnlyList<Token> Size { get; }

    /// <summary>Motion shared by Windows and the web: easing, durations, stagger, distances, parallax.</summary>
    public IReadOnlyList<MotionToken> Motion { get; }

    /// <summary>Spring parameters for iOS and macOS (<c>motion.ios.spring.*</c>).</summary>
    public IReadOnlyList<Token> IosSpring { get; }

    /// <summary>PowerPoint durations (<c>motion.presentation.*</c>).</summary>
    public IReadOnlyList<Token> PresentationMotion { get; }

    public IReadOnlyList<TypeRole> TypeRoles => Tokens.TypeRoles;

    /// <summary>
    /// The pairs of light and dark tokens for each theme color.
    /// </summary>
    public IEnumerable<(Token Light, Token Dark)> ThemePairs => Light.Zip(Dark);

    /// <summary>The light theme colors that appear in the outputs of a platform.</summary>
    public IReadOnlyList<Token> LightFor(OutputPlatform platform) => Light.Where(token => token.IsFor(platform)).ToList();

    /// <summary>The dark theme colors that appear in the outputs of a platform, in the same order as <see cref="LightFor"/>.</summary>
    public IReadOnlyList<Token> DarkFor(OutputPlatform platform) => Dark.Where(token => token.IsFor(platform)).ToList();

    /// <summary>The names of the theme colors that appear in the outputs of a platform.</summary>
    public IEnumerable<string> ThemeColorNamesFor(OutputPlatform platform) => LightFor(platform).Select(token => token.Name);

    /// <summary>The pairs of light and dark tokens for each theme color that appears in the outputs of a platform.</summary>
    public IEnumerable<(Token Light, Token Dark)> ThemePairsFor(OutputPlatform platform) => LightFor(platform).Zip(DarkFor(platform));

    /// <summary>The sizes that appear in the outputs of a platform.</summary>
    public IReadOnlyList<Token> SizeFor(OutputPlatform platform) => Size.Where(token => token.IsFor(platform)).ToList();

    private static bool IsInside(Token token, string group)
        => token.Path.StartsWith(group + ".", StringComparison.Ordinal);

    private static IReadOnlyList<Token> OfType(IReadOnlyList<Token> tokens, TokenType type)
    {
        foreach (var token in tokens)
        {
            if (token.Type != type)
            {
                throw new TokenException($"Token '{token.Path}' must be of type {type}.");
            }
        }

        return tokens;
    }

    private static IReadOnlyList<Token> Pixels(IReadOnlyList<Token> tokens)
    {
        foreach (var token in OfType(tokens, TokenType.Dimension))
        {
            if (token.Dimension.Unit != "px")
            {
                throw new TokenException($"Token '{token.Path}' must be in px.");
            }
        }

        return tokens;
    }

    private void ValidateThemes()
    {
        var light = Light.Select(token => token.Name).ToList();
        var dark = Dark.Select(token => token.Name).ToList();
        if (!light.SequenceEqual(dark, StringComparer.Ordinal))
        {
            throw new TokenException("theme.light and theme.dark must define the same colors in the same order.");
        }
    }

    /// <summary>
    /// Only theme colors and sizes can be limited to some platforms, and a theme color has the same platforms in light and dark.
    /// </summary>
    private void ValidatePlatforms()
    {
        var limitable = Light.Concat(Dark).Concat(Size).Select(token => token.Path).ToHashSet(StringComparer.Ordinal);
        foreach (var token in Tokens.Tokens)
        {
            if (token.Platforms is not null && !limitable.Contains(token.Path))
            {
                throw new TokenException($"Token '{token.Path}' has $extensions.platforms, which only theme colors and sizes support.");
            }
        }

        foreach (var (light, dark) in ThemePairs)
        {
            var lightPlatforms = Enum.GetValues<OutputPlatform>().Where(light.IsFor);
            var darkPlatforms = Enum.GetValues<OutputPlatform>().Where(dark.IsFor);
            if (!lightPlatforms.SequenceEqual(darkPlatforms))
            {
                throw new TokenException($"The theme color '{light.Name}' must have the same $extensions.platforms in light and dark.");
            }
        }
    }

    private void ValidateCoverage()
    {
        var covered = Palette
            .Concat(Light)
            .Concat(Dark)
            .Concat(Space)
            .Concat(Radius)
            .Concat(Density)
            .Concat(Size)
            .Concat(Motion.Select(motion => motion.Token))
            .Concat(IosSpring)
            .Concat(PresentationMotion)
            .Select(token => token.Path)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var token in Tokens.Tokens)
        {
            if (!covered.Contains(token.Path))
            {
                throw new TokenException($"Token '{token.Path}' is not in a group the generator knows. Add the group to the generator first.");
            }
        }

        foreach (var token in Motion.Select(motion => motion.Token))
        {
            if (token.Type == TokenType.Dimension && token.Dimension.Unit != "px")
            {
                throw new TokenException($"Token '{token.Path}' must be in px.");
            }
        }
    }
}

/// <summary>
/// A motion token with its group, for example <c>duration</c> for <c>motion.duration.normal</c>.
/// </summary>
internal sealed record MotionToken(string Group, Token Token);
