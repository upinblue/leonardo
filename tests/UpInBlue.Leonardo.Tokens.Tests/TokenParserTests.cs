using UpInBlue.Leonardo.Tokens.Model;
using UpInBlue.Leonardo.Tokens.Parsing;

namespace UpInBlue.Leonardo.Tokens.Tests;

public sealed class TokenParserTests
{
    [Fact]
    public void ResolvesReference()
    {
        var tokens = TokenParser.Parse("""
            {
              "palette": { "brand": { "$type": "color", "$value": "#1B6CE0" } },
              "theme": { "accent": { "$type": "color", "$value": "{palette.brand}" } }
            }
            """);

        var accent = tokens["theme.accent"];
        Assert.Equal(new RgbColor(0x1B, 0x6C, 0xE0, 0xFF), accent.Color);
        Assert.Equal("palette.brand", accent.Reference);
    }

    [Fact]
    public void ResolvesChainedReferencesInAnyOrder()
    {
        var tokens = TokenParser.Parse("""
            {
              "a": { "$type": "dimension", "$value": "{b}" },
              "b": { "$type": "dimension", "$value": "{c}" },
              "c": { "$type": "dimension", "$value": { "value": 16, "unit": "px" } }
            }
            """);

        Assert.Equal(new Dimension(16, "px"), tokens["a"].Dimension);
        Assert.Equal(new Dimension(16, "px"), tokens["b"].Dimension);
    }

    [Fact]
    public void UnknownReferenceIsAnError()
    {
        var exception = Assert.Throws<TokenException>(() => TokenParser.Parse("""
            { "theme": { "accent": { "$type": "color", "$value": "{palette.missing}" } } }
            """));

        Assert.Contains("theme.accent", exception.Message, StringComparison.Ordinal);
        Assert.Contains("{palette.missing}", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ReferenceCycleIsAnError()
    {
        var exception = Assert.Throws<TokenException>(() => TokenParser.Parse("""
            {
              "a": { "$type": "color", "$value": "{b}" },
              "b": { "$type": "color", "$value": "{c}" },
              "c": { "$type": "color", "$value": "{a}" }
            }
            """));

        Assert.Contains("a -> b -> c -> a", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SelfReferenceIsAnError()
    {
        var exception = Assert.Throws<TokenException>(() => TokenParser.Parse("""
            { "a": { "$type": "color", "$value": "{a}" } }
            """));

        Assert.Contains("a -> a", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ReferenceToTokenOfOtherTypeIsAnError()
    {
        var exception = Assert.Throws<TokenException>(() => TokenParser.Parse("""
            {
              "size": { "$type": "dimension", "$value": { "value": 4, "unit": "px" } },
              "color": { "$type": "color", "$value": "{size}" }
            }
            """));

        Assert.Contains("type", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("""{ "a": { "$type": "color", "$value": "blue" } }""")]
    [InlineData("""{ "a": { "$type": "color", "$value": "#12345" } }""")]
    [InlineData("""{ "a": { "$type": "color", "$value": "#GG0000" } }""")]
    [InlineData("""{ "a": { "$type": "dimension", "$value": { "value": 4, "unit": "em" } } }""")]
    [InlineData("""{ "a": { "$type": "duration", "$value": { "value": 4, "unit": "min" } } }""")]
    [InlineData("""{ "a": { "$type": "cubicBezier", "$value": [0, 0, 1] } }""")]
    [InlineData("""{ "a": { "$type": "cubicBezier", "$value": [2, 0, 0, 1] } }""")]
    [InlineData("""{ "a": { "$type": "number", "$value": "4" } }""")]
    [InlineData("""{ "a": { "$type": "shadow", "$value": 4 } }""")]
    [InlineData("""{ "a": { "$value": 4 } }""")]
    [InlineData("""{ "a": 4 }""")]
    [InlineData("""not json""")]
    public void InvalidTokensAreErrors(string json)
        => Assert.Throws<TokenException>(() => TokenParser.Parse(json));

    [Fact]
    public void ReadsAllTypesDescriptionsAndStatus()
    {
        var tokens = TokenParser.Parse("""
            {
              "motion": {
                "$type": "duration",
                "fast": { "$value": { "value": 0.5, "unit": "s" }, "$description": "Inherits the group type." },
                "curve": { "$type": "cubicBezier", "$value": [0.55, 0.55, 0, 1] },
                "factor": { "$type": "number", "$value": 0.7, "$extensions": { "status": "proposal" } }
              },
              "translucent": { "$type": "color", "$value": "#11223380" }
            }
            """);

        Assert.Equal(new TokenDuration(500), tokens["motion.fast"].Duration);
        Assert.Equal("Inherits the group type.", tokens["motion.fast"].Description);
        Assert.Equal(new CubicBezier(0.55, 0.55, 0, 1), tokens["motion.curve"].CubicBezier);
        Assert.Equal(0.7, tokens["motion.factor"].Number);
        Assert.True(tokens["motion.factor"].IsProposal);
        Assert.False(tokens["motion.fast"].IsProposal);
        Assert.Equal(0x80, tokens["translucent"].Color.A);
    }

    [Fact]
    public void TypeRoleColorMustBeAThemeColor()
    {
        var exception = Assert.Throws<TokenException>(() => TokenParser.Parse("""
            {
              "typography": {
                "meta": { "windows": { "fontFamily": "Segoe UI Variable Small", "fontSize": 12, "lineHeight": 16, "fontWeight": 400, "color": "textQuaternary" } }
              }
            }
            """));

        Assert.Contains("textQuaternary", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ParsesTheRepositoryTokenFile()
    {
        var tokens = TokenParser.Parse(File.ReadAllText(Repository.PathOf("tokens/leonardo.tokens.json")));

        Assert.Equal(tokens["palette.brand"].Color, tokens["theme.light.accent"].Color);
        Assert.Equal(tokens["palette.far"].Color, tokens["theme.dark.accent"].Color);
        Assert.Equal(["placeName", "view", "section", "body", "meta"], tokens.TypeRoles.Select(role => role.Name));
        Assert.Null(tokens.TypeRoles.Single(role => role.Name == "view").Presentation);
    }
}
