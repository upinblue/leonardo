using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using UpInBlue.Leonardo.Tokens.Generation;
using UpInBlue.Leonardo.Tokens.Model;
using UpInBlue.Leonardo.Tokens.Parsing;

namespace UpInBlue.Leonardo.Tokens.Tests;

public sealed class GeneratorTests
{
    private const string WinUIXaml = "src/UpInBlue.Leonardo.WinUI/Themes/Generated/Tokens.xaml";
    private const string WinUIAccent = "src/UpInBlue.Leonardo.WinUI/Themes/Generated/AccentColors.xaml";
    private const string WinUICode = "src/UpInBlue.Leonardo.WinUI/Generated/LeonardoTokens.g.cs";
    private const string WpfLight = "src/UpInBlue.Leonardo.Wpf/Themes/Generated/Tokens.Light.xaml";
    private const string WpfDark = "src/UpInBlue.Leonardo.Wpf/Themes/Generated/Tokens.Dark.xaml";
    private const string WpfHighContrast = "src/UpInBlue.Leonardo.Wpf/Themes/Generated/Tokens.HighContrast.xaml";
    private const string WpfAccent = "src/UpInBlue.Leonardo.Wpf/Themes/Generated/AccentColors.xaml";
    private const string WpfCode = "src/UpInBlue.Leonardo.Wpf/Generated/LeonardoTokens.g.cs";
    private const string Swift = "generated/apple/LeonardoTokens.swift";
    private const string Catalog = "generated/apple/Leonardo.xcassets";
    private const string Css = "generated/web/leonardo.css";
    private const string Presentation = "generated/presentation/presentation-tokens.md";
    private const string SkillTokens = "claude/skills/leonardo/references/leonardo.tokens.json";

    [Fact]
    public void SkillCopiesAreIdenticalToTheirSources()
    {
        var files = GenerateFromRepository();

        Assert.Equal(Repository.TokensFile, files.Single(file => file.Path == TokenGenerator.SkillTokensPath).Content);
        Assert.Equal(Repository.DesignLanguageFile, files.Single(file => file.Path == TokenGenerator.SkillDesignLanguagePath).Content);
        Assert.Equal(Repository.TokensFile, File.ReadAllBytes(Repository.PathOf(TokenGenerator.SkillTokensPath)));
        Assert.Equal(Repository.DesignLanguageFile, File.ReadAllBytes(Repository.PathOf(TokenGenerator.SkillDesignLanguagePath)));
    }

    [Fact]
    public void GeneratedFilesInTheRepositoryAreUpToDate()
    {
        var report = new OutputWriter(Repository.Root).Compare(GenerateFromRepository());

        Assert.True(
            report.IsUpToDate,
            "Run 'dotnet run --project src/UpInBlue.Leonardo.Tokens -- generate'. Out of date: "
                + string.Join(", ", report.Changed.Concat(report.Stale)));
    }

    [Fact]
    public void SameInputProducesByteIdenticalOutput()
    {
        var first = GenerateFromRepository();
        var second = GenerateFromRepository();

        Assert.Equal(first.Select(file => file.Path), second.Select(file => file.Path));
        foreach (var (a, b) in first.Zip(second))
        {
            Assert.Equal(a.Content, b.Content);
        }
    }

    [Theory]
    [InlineData("de-DE")]
    [InlineData("ar-SA")]
    [InlineData("fr-CH")]
    public void OutputDoesNotDependOnTheCulture(string culture)
    {
        var invariant = WithCulture(CultureInfo.InvariantCulture, GenerateFromRepository);
        var localized = WithCulture(CultureInfo.GetCultureInfo(culture), GenerateFromRepository);

        foreach (var (a, b) in invariant.Zip(localized))
        {
            Assert.True(a.Content.AsSpan().SequenceEqual(b.Content), $"{a.Path} differs under {culture}.");
        }
    }

    [Fact]
    public void EveryGeneratedTextFileStartsWithTheHeader()
    {
        var exempt = new[] { ".json" };
        foreach (var file in GenerateFromRepository())
        {
            if (exempt.Any(extension => file.Path.EndsWith(extension, StringComparison.Ordinal))
                || file.Path == TokenGenerator.SkillDesignLanguagePath)
            {
                continue;
            }

            var firstLine = Encoding.UTF8.GetString(file.Content).Split('\n')[0];
            Assert.True(firstLine.Contains(Text.Header, StringComparison.Ordinal), $"{file.Path} starts with '{firstLine}'.");
        }
    }

    [Fact]
    public void GeneratedJsonIsValid()
    {
        foreach (var file in GenerateFromRepository().Where(file => file.Path.EndsWith(".json", StringComparison.Ordinal)))
        {
            using var document = JsonDocument.Parse(file.Content);
            Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);
        }
    }

    [Fact]
    public void GeneratedXamlIsWellFormed()
    {
        foreach (var file in GenerateFromRepository().Where(file => file.Path.EndsWith(".xaml", StringComparison.Ordinal)))
        {
            var document = System.Xml.Linq.XDocument.Parse(Encoding.UTF8.GetString(file.Content));
            Assert.Equal("ResourceDictionary", document.Root!.Name.LocalName);
        }
    }

    [Fact]
    public void ThemeColorsAppearInEveryOutputOfTheirPlatforms()
    {
        var files = GenerateFromRepository().ToDictionary(file => file.Path, file => Encoding.UTF8.GetString(file.Content));
        var tokens = TokenParser.Parse(Encoding.UTF8.GetString(Repository.TokensFile));

        foreach (var token in tokens.Children("theme.light").Concat(tokens.Children("theme.dark")))
        {
            var isDark = token.Path.StartsWith("theme.dark.", StringComparison.Ordinal);
            var name = Text.Pascal(token.Name);
            var colorKey = $"x:Key=\"Leonardo{name}Color\"";
            var windows = token.IsFor(OutputPlatform.Windows);

            Assert.Equal(windows, files[WinUIXaml].Contains(colorKey, StringComparison.Ordinal));
            Assert.Equal(windows, files[isDark ? WpfDark : WpfLight].Contains($"{colorKey}>{token.Color.XamlHex}<", StringComparison.Ordinal));
            Assert.Equal(windows, files[WpfHighContrast].Contains(colorKey, StringComparison.Ordinal));
            Assert.Equal(windows, files[WinUICode].Contains($" {name} = ", StringComparison.Ordinal));
            Assert.Equal(windows, files[WpfCode].Contains($" {name} = ", StringComparison.Ordinal));
            Assert.Equal(token.IsFor(OutputPlatform.Apple), files.ContainsKey($"{Catalog}/Leonardo/{name}.colorset/Contents.json"));
            Assert.Equal(token.IsFor(OutputPlatform.Apple), files[Swift].Contains($"Color(\"Leonardo/{name}\")", StringComparison.Ordinal));
            Assert.Equal(token.IsFor(OutputPlatform.Web), files[Css].Contains($"{WebGenerator.ColorProperty(token.Name)}: {token.Color.Hex};", StringComparison.Ordinal));
            Assert.Equal(token.IsFor(OutputPlatform.Presentation), files[Presentation].Contains($"| `{token.Name}` |", StringComparison.Ordinal));
        }
    }

    [Theory]
    [InlineData("statusCritical", new[] { "web", "presentation" })]
    [InlineData("statusCaution", new[] { "web", "presentation" })]
    [InlineData("statusSuccess", new[] { "web", "presentation" })]
    [InlineData("focus", new[] { "web" })]
    [InlineData("accentHover", new[] { "web" })]
    [InlineData("accentPressed", new[] { "web" })]
    [InlineData("chart", new[] { "windows", "apple", "web", "presentation" })]
    public void ThemeColorsHaveTheirDecidedPlatforms(string name, string[] platforms)
    {
        var tokens = TokenParser.Parse(Encoding.UTF8.GetString(Repository.TokensFile));
        var expected = platforms.Select(platform => Enum.Parse<OutputPlatform>(platform, ignoreCase: true)).Order();

        foreach (var theme in new[] { "light", "dark" })
        {
            var token = tokens[$"theme.{theme}.{name}"];
            Assert.Equal(expected, Enum.GetValues<OutputPlatform>().Where(token.IsFor).Order());
        }
    }

    [Fact]
    public void SizesAppearOnEveryPlatformThatUsesThem()
    {
        var files = GenerateFromRepository().ToDictionary(file => file.Path, file => Encoding.UTF8.GetString(file.Content));

        foreach (var xaml in new[] { WinUIXaml, WpfLight, WpfDark, WpfHighContrast })
        {
            Assert.Contains("x:Key=\"LeonardoSizeIcon\">16<", files[xaml], StringComparison.Ordinal);
            Assert.Contains("x:Key=\"LeonardoSizeStatusDot\">12<", files[xaml], StringComparison.Ordinal);
            Assert.DoesNotContain("FocusStroke", files[xaml], StringComparison.Ordinal);
        }

        foreach (var code in new[] { WinUICode, WpfCode })
        {
            Assert.Contains("public static class Size", files[code], StringComparison.Ordinal);
            Assert.Contains("public const double Icon = 16d;", files[code], StringComparison.Ordinal);
            Assert.Contains("public const double StatusDot = 12d;", files[code], StringComparison.Ordinal);
            Assert.DoesNotContain("FocusStroke", files[code], StringComparison.Ordinal);
        }

        Assert.Contains("public static let icon: CGFloat = 16", files[Swift], StringComparison.Ordinal);
        Assert.Contains("public static let statusDot: CGFloat = 12", files[Swift], StringComparison.Ordinal);
        Assert.DoesNotContain("focusStroke", files[Swift], StringComparison.Ordinal);
        Assert.Contains("--leonardo-size-icon: 16px;", files[Css], StringComparison.Ordinal);
        Assert.Contains("--leonardo-size-status-dot: 12px;", files[Css], StringComparison.Ordinal);
        Assert.Contains("--leonardo-size-focus-stroke: 2px;", files[Css], StringComparison.Ordinal);
    }

    [Fact]
    public void SwiftHasTheArriveHold()
    {
        var swift = Encoding.UTF8.GetString(GenerateFromRepository().Single(file => file.Path == Swift).Content);

        Assert.Contains("public static let arriveHold: TimeInterval = 2", swift, StringComparison.Ordinal);
    }

    [Fact]
    public void ChartIsTheHighlightColorInContrastThemes()
    {
        var files = GenerateFromRepository().ToDictionary(file => file.Path, file => Encoding.UTF8.GetString(file.Content));

        Assert.Contains("<StaticResource x:Key=\"LeonardoChartColor\" ResourceKey=\"SystemColorHighlightColor\" />", files[WinUIXaml], StringComparison.Ordinal);
        Assert.Contains("<x:Static x:Key=\"LeonardoChartColor\" Member=\"SystemColors.HighlightColor\" />", files[WpfHighContrast], StringComparison.Ordinal);
    }

    [Fact]
    public void PlatformsOnlyApplyToThemeColorsAndSizes()
    {
        var tokens = JsonNode.Parse(Repository.TokensFile)!;
        tokens["space"]!["m"]!["$extensions"] = JsonNode.Parse("""{ "platforms": ["web"] }""");

        var exception = Assert.Throws<TokenException>(() => Generate(tokens));

        Assert.Contains("space.m", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ThemeColorsHaveTheSamePlatformsInLightAndDark()
    {
        var tokens = JsonNode.Parse(Repository.TokensFile)!;
        tokens["theme"]!["dark"]!["link"]!["$extensions"] = JsonNode.Parse("""{ "platforms": ["web"] }""");

        var exception = Assert.Throws<TokenException>(() => Generate(tokens));

        Assert.Contains("link", exception.Message, StringComparison.Ordinal);
    }

    public static TheoryData<string, string, string[]> TokenChanges() => new()
    {
        {
            "space.m",
            """{ "value": 20, "unit": "px" }""",
            [WinUIXaml, WinUICode, WpfLight, WpfDark, WpfHighContrast, WpfCode, Swift, Css, SkillTokens]
        },
        {
            "theme.light.textTertiary",
            "\"#5A6474\"",
            [WinUIXaml, WinUICode, WpfLight, WpfCode, $"{Catalog}/Leonardo/TextTertiary.colorset/Contents.json", Css, Presentation, SkillTokens]
        },
        {
            "theme.dark.surface",
            "\"#1B2131\"",
            [WinUIXaml, WinUICode, WpfDark, WpfCode, $"{Catalog}/Leonardo/Surface.colorset/Contents.json", Css, Presentation, SkillTokens]
        },
        {
            "palette.brand",
            "\"#1A6BDF\"",
            [WinUIXaml, WinUIAccent, WinUICode, WpfLight, WpfDark, WpfHighContrast, WpfAccent, WpfCode, $"{Catalog}/AccentColor.colorset/Contents.json", $"{Catalog}/Leonardo/Accent.colorset/Contents.json", Css, Presentation, SkillTokens]
        },
        {
            "theme.dark.link",
            "\"#A8CBF5\"",
            [WinUIXaml, WinUIAccent, WinUICode, WpfDark, WpfAccent, WpfCode, $"{Catalog}/Leonardo/Link.colorset/Contents.json", Css, Presentation, SkillTokens]
        },
        {
            "motion.duration.normal",
            """{ "value": 300, "unit": "ms" }""",
            [WinUIXaml, WinUICode, WpfLight, WpfDark, WpfHighContrast, WpfCode, Css, SkillTokens]
        },
        {
            "motion.stagger.step",
            """{ "value": 60, "unit": "ms" }""",
            [WinUIXaml, WinUICode, WpfLight, WpfDark, WpfHighContrast, WpfCode, Swift, Css, SkillTokens]
        },
        {
            "motion.arrive.hold",
            """{ "value": 3000, "unit": "ms" }""",
            [WinUIXaml, WinUICode, WpfLight, WpfDark, WpfHighContrast, WpfCode, Swift, Css, SkillTokens]
        },
        {
            "theme.light.chart",
            "\"#0C4B9A\"",
            [WinUIXaml, WinUICode, WpfLight, WpfCode, $"{Catalog}/Leonardo/Chart.colorset/Contents.json", Css, Presentation, SkillTokens]
        },
        {
            "theme.light.statusCritical",
            "\"#C42B1D\"",
            [Css, Presentation, SkillTokens]
        },
        {
            "theme.dark.focus",
            "\"#80B3F1\"",
            [Css, SkillTokens]
        },
        {
            "theme.light.accentHover",
            "\"#1968D8\"",
            [Css, SkillTokens]
        },
        {
            "size.icon",
            """{ "value": 20, "unit": "px" }""",
            [WinUIXaml, WinUICode, WpfLight, WpfDark, WpfHighContrast, WpfCode, Swift, Css, SkillTokens]
        },
        {
            "size.focusStroke",
            """{ "value": 3, "unit": "px" }""",
            [Css, SkillTokens]
        },
        {
            "motion.ios.spring.durationMax",
            """{ "value": 450, "unit": "ms" }""",
            [Swift, SkillTokens]
        },
        {
            "motion.presentation.morph",
            """{ "value": 400, "unit": "ms" }""",
            [Presentation, SkillTokens]
        },
    };

    [Theory]
    [MemberData(nameof(TokenChanges))]
    public void ChangingOneTokenChangesEveryOutputThatUsesItAndNothingElse(string path, string newValue, string[] expectedChanges)
    {
        var original = JsonNode.Parse(Repository.TokensFile)!;
        var modified = JsonNode.Parse(Repository.TokensFile)!;
        var token = path.Split('.').Aggregate(modified, (node, segment) => node[segment]!);
        token["$value"] = JsonNode.Parse(newValue);

        var before = Generate(original);
        var after = Generate(modified);

        Assert.Equal(before.Select(file => file.Path), after.Select(file => file.Path));
        var changed = before.Zip(after)
            .Where(pair => !pair.First.Content.AsSpan().SequenceEqual(pair.Second.Content))
            .Select(pair => pair.First.Path)
            .Order(StringComparer.Ordinal);
        Assert.Equal(expectedChanges.Order(StringComparer.Ordinal), changed);
    }

    [Fact]
    public void UnknownTokenGroupIsAnError()
    {
        var tokens = JsonNode.Parse(Repository.TokensFile)!;
        tokens["shadow"] = JsonNode.Parse("""{ "card": { "$type": "number", "$value": 2 } }""");

        var exception = Assert.Throws<TokenException>(() => Generate(tokens));

        Assert.Contains("shadow.card", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ThemesMustDefineTheSameColors()
    {
        var tokens = JsonNode.Parse(Repository.TokensFile)!;
        tokens["theme"]!["dark"]!.AsObject().Remove("link");

        Assert.Throws<TokenException>(() => Generate(tokens));
    }

    private static IReadOnlyList<GeneratedFile> GenerateFromRepository()
        => TokenGenerator.Generate(Repository.TokensFile, Repository.DesignLanguageFile);

    private static IReadOnlyList<GeneratedFile> Generate(JsonNode tokens)
        => TokenGenerator.Generate(
            Encoding.UTF8.GetBytes(tokens.ToJsonString(new JsonSerializerOptions { WriteIndented = true })),
            Repository.DesignLanguageFile);

    private static T WithCulture<T>(CultureInfo culture, Func<T> action)
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = culture;
        try
        {
            return action();
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
