namespace UpInBlue.Leonardo.Check.Tests;

public sealed class WebRuleTests
{
    private const string Css = "web/files.css";
    private const string Html = "web/index.html";
    private const string Tsx = "src/FileList.tsx";

    [Theory]
    [InlineData(".banner { background: #1b6ce0; }")]
    [InlineData(".banner { color: white; }")]
    [InlineData(".banner { border: 1px solid rgb(0 0 0 / 20%); }")]
    [InlineData(".banner { background: linear-gradient(90deg, #1b6ce0, #7fb2f0); }")]
    [InlineData(":root { --brand: #1b6ce0; }")]
    public void LiteralColorsAreReported(string css) => Check.Reports(Rules.LiteralColor, Css, css);

    [Theory]
    [InlineData(".banner { background: var(--leonardo-surface-alt); color: var(--leonardo-text-secondary); }")]
    [InlineData(".banner { border: 1px solid CanvasText; color: currentColor; background: transparent; }")]
    [InlineData(".row { white-space: nowrap; }")]
    [InlineData("/* .banner { background: #1b6ce0; } */")]
    public void PropertiesSystemColorsAndCommentsAreClean(string css) => Check.Clean(Css, css);

    [Fact]
    public void ScssVariablesAreChecked() => Check.Reports(Rules.LiteralColor, "web/files.scss", "$brand: #1b6ce0;");

    [Theory]
    [InlineData(".title { font-size: 22px; }")]
    [InlineData(".title { font-size: 1.25rem; }")]
    [InlineData(".title { font: 600 14px/1.4 Inter, sans-serif; }")]
    public void LiteralFontSizesAreReported(string css) => Check.Reports(Rules.LiteralFontSize, Css, css);

    [Theory]
    [InlineData(".title { font-size: var(--leonardo-section-font-size); }")]
    [InlineData(".title { font-size: inherit; font: inherit; }")]
    [InlineData("html { font-size: 100%; }")]
    public void TokenFontSizesAreClean(string css) => Check.Clean(Css, css);

    [Theory]
    [InlineData("* { outline: none; }")]
    [InlineData("button:focus { outline: 0; }")]
    public void OutlineNoneWithoutReplacementIsReported(string css) => Check.Reports(Rules.FocusVisual, Css, css);

    [Theory]
    [InlineData("button:focus { outline: none; box-shadow: 0 0 0 2px var(--leonardo-text-primary); }")]
    [InlineData("button:focus:not(:focus-visible) { outline: none; } button:focus-visible { outline: 2px solid var(--leonardo-text-primary); }")]
    [InlineData(".row { outline: none; } :focus-visible { outline: 2px solid var(--leonardo-text-primary); }")]
    public void OutlineNoneWithReplacementIsClean(string css) => Check.Clean(Css, css);

    [Theory]
    [InlineData(".button { padding: 5px 15px; }")]
    [InlineData(".panel { margin: 0 auto 2rem; }")]
    [InlineData(".list { gap: 12px; }")]
    [InlineData(".grid { row-gap: 0.75em; }")]
    [InlineData(".row { padding-inline-start: 40px; }")]
    [InlineData(".row { margin-top: calc(var(--leonardo-space-m) + 4px); }")]
    [InlineData("$gutter: 12px; .row { margin-block-end: 16px; }")]
    public void LiteralSpacingIsReported(string css) => Check.Reports(Rules.LiteralSpacing, Css, css);

    [Theory]
    [InlineData(".button { padding: var(--leonardo-space-xs) var(--leonardo-space-m); }")]
    [InlineData(".list { margin: 0; padding: 0; gap: 0px; }")]
    [InlineData(".visually-hidden { margin: -1px; padding: 0; }")]
    [InlineData(".pane { padding-top: calc(var(--leonardo-space-l) - 1px); }")]
    [InlineData(".page { margin: 0 auto; padding-inline: 5%; }")]
    [InlineData(".row { padding: calc(var(--leonardo-space-xl) - var(--leonardo-space-xs)); }")]
    [InlineData(":root { --files-nav-width: 16rem; }")]
    public void TokenZeroAndHairlineSpacingAreClean(string css) => Check.Clean(Css, css);

    [Fact]
    public void SpacingSuggestionNamesTheMatchingToken()
    {
        var finding = Assert.Single(Check.Run(Css, ".row { padding: 1.5rem; }"));
        Assert.Equal(Rules.LiteralSpacing, finding.Rule);
        Assert.Contains("var(--leonardo-space-l)", finding.Suggestion, StringComparison.Ordinal);
    }

    [Fact]
    public void LiteralSpacingIsReportedInHtmlStyles()
        => Check.Reports(Rules.LiteralSpacing, Html, """<p style="margin-top: 12px">Note</p>""");

    [Fact]
    public void LiteralTransitionWithoutReducedMotionIsReported()
    {
        var ids = Check.RuleIds(Css, ".row { transition: transform 0.3s ease-in-out; }");
        Assert.Contains(Rules.LiteralDuration.Id, ids);
        Assert.Contains(Rules.ReducedMotion.Id, ids);
    }

    [Fact]
    public void LiteralTransitionWithReducedMotionRuleIsOnlyALiteral()
    {
        var ids = Check.RuleIds(Css, """
            .row { transition: transform 0.3s ease-in-out; }
            @media (prefers-reduced-motion: reduce) { .row { transition: none; } }
            """);
        Assert.Contains(Rules.LiteralDuration.Id, ids);
        Assert.DoesNotContain(Rules.ReducedMotion.Id, ids);
    }

    [Fact]
    public void TransitionWithOtherCustomPropertiesNeedsAReducedMotionRule()
        => Check.Reports(Rules.ReducedMotion, Css, ".row { transition: opacity var(--my-duration); }");

    [Theory]
    [InlineData(".button { transition: background-color var(--leonardo-duration-fade) var(--leonardo-ease-fade); }")]
    [InlineData(".is-entering { animation: fade-in var(--leonardo-duration-fade) var(--leonardo-ease-fade) both, slide-in var(--leonardo-duration-normal) var(--leonardo-ease-enter) both; }")]
    [InlineData(".button:active { transition: none; }")]
    public void LeonardoMotionIsClean(string css) => Check.Clean(Css, css);

    [Fact]
    public void BackdropBlurIsReported() => Check.Reports(Rules.CustomGlass, Css, ".menu { backdrop-filter: blur(20px); }");

    [Theory]
    [InlineData("""<div class="row" onclick="openAppointment(1)">Anna Berg</div>""")]
    [InlineData("""<span role="button" onclick="toggle()">Menu</span>""")]
    public void ClickableDivsAreReportedInHtml(string html) => Check.Reports(Rules.ClickableDiv, Html, html);

    [Fact]
    public void ButtonsAndLinksAreClean()
        => Check.Clean(Html, """<button type="button" onclick="upload()">Upload files</button> <a href="#/files">Files</a>""");

    [Fact]
    public void CssInsideHtmlIsChecked()
    {
        Check.Reports(Rules.LiteralColor, Html, """<style> .banner { background: #1b6ce0; } </style>""");
        Check.Reports(Rules.LiteralFontSize, Html, """<p style="font-size: 12px">Note</p>""");
    }

    [Fact]
    public void TextOutsideCssIsNotCheckedInHtml() => Check.Clean(Html, "<p>color: red; font-size: 12px</p>");

    [Theory]
    [InlineData("""export const Row = () => <div className="row" onClick={() => open(id)}>Anna</div>;""")]
    [InlineData("""
        export const Row = () => (
            <div
                className="row"
                onClick={() => {
                    open(id);
                }}>
                Anna
            </div>);
        """)]
    public void ClickableDivsAreReportedInTsx(string tsx) => Check.Reports(Rules.ClickableDiv, Tsx, tsx);

    [Theory]
    [InlineData("""const style = { color: '#1b6ce0' };""")]
    [InlineData("""const Title = styled.h1` color: #1b6ce0; `;""")]
    public void LiteralColorsAreReportedInTsx(string tsx) => Check.Reports(Rules.LiteralColor, Tsx, tsx);

    [Fact]
    public void LiteralFontSizeIsReportedInTsx() => Check.Reports(Rules.LiteralFontSize, Tsx, """const style = { fontSize: 14 };""");

    [Fact]
    public void ThemeReferencesAreCleanInTsx()
        => Check.Clean(Tsx, """const style = { color: tokens.colorNeutralForeground1, background: theme.palette.red };""");
}
