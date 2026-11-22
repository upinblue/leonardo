using System.Text.Json;

namespace UpInBlue.Leonardo.Check.Tests;

public sealed class HookTests
{
    private const string Cwd = "C:/repo";
    private const string PagePath = "C:/repo/samples/App/Pages/FilesPage.xaml";

    private static string Event(string toolName, object toolInput) => JsonSerializer.Serialize(new
    {
        session_id = "test",
        cwd = Cwd,
        hook_event_name = "PostToolUse",
        tool_name = toolName,
        tool_input = toolInput,
        tool_response = new { success = true },
    });

    private static Hook.Result Run(string json, string? fileText) => Hook.Run(json, _ => fileText);

    [Fact]
    public void WriteWithALiteralColorReportsWithExitCode2()
    {
        var page = "<Page>\n    <Border Background=\"#FF0000\" />\n</Page>\n";
        var result = Run(Event("Write", new { file_path = PagePath, content = page }), page);

        Assert.Equal(Hook.FindingsExitCode, result.ExitCode);
        Assert.Contains("samples/App/Pages/FilesPage.xaml:2: LEO001 literal-color", result.Message, StringComparison.Ordinal);
        Assert.Contains("Suggestion:", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CleanFileExitsWithZeroAndNoOutput()
    {
        var page = "<Page>\n    <Border Background=\"{ThemeResource LeonardoSurfaceBrush}\" />\n</Page>\n";
        var result = Run(Event("Write", new { file_path = PagePath, content = page }), page);

        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.Message);
    }

    [Fact]
    public void EditReportsOnlyTheLinesItChanged()
    {
        var page = "<Page>\n    <Border Background=\"#00FF00\" />\n    <TextBlock FontSize=\"20\" />\n</Page>\n";
        var result = Run(Event("Edit", new { file_path = PagePath, old_string = "<TextBlock />", new_string = "<TextBlock FontSize=\"20\" />" }), page);

        Assert.Equal(Hook.FindingsExitCode, result.ExitCode);
        Assert.Contains("LEO002", result.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("LEO001", result.Message, StringComparison.Ordinal);
        Assert.Contains("1 more in other lines", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EditThatFixesALineIsQuietEvenWhenOtherLinesHaveFindings()
    {
        var page = "<Page>\n    <Border Background=\"#00FF00\" />\n    <TextBlock Style=\"{StaticResource LeonardoBodyTextStyle}\" />\n</Page>\n";
        var result = Run(Event("Edit", new { file_path = PagePath, old_string = "<TextBlock FontSize=\"20\" />", new_string = "<TextBlock Style=\"{StaticResource LeonardoBodyTextStyle}\" />" }), page);

        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public void MultiEditChecksEveryNewText()
    {
        var css = ".a { color: red; }\n.b { margin: 0; }\n.c { font-size: 12px; }\n";
        var result = Run(Event("MultiEdit", new
        {
            file_path = "C:/repo/web/app.css",
            edits = new[] { new { old_string = "x", new_string = ".a { color: red; }" }, new { old_string = "y", new_string = ".c { font-size: 12px; }" } },
        }), css);

        Assert.Equal(Hook.FindingsExitCode, result.ExitCode);
        Assert.Contains("LEO001", result.Message, StringComparison.Ordinal);
        Assert.Contains("LEO002", result.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("C:/repo/README.md")]
    [InlineData("C:/repo/src/App/Themes/Generated/Tokens.xaml")]
    [InlineData("C:/repo/tests/App.Tests/ColorTests.cs")]
    public void OtherAndGeneratedFilesAreIgnored(string path)
        => Assert.Equal(0, Run(Event("Write", new { file_path = path, content = "x" }), "<Border Background=\"#FF0000\" />").ExitCode);

    [Theory]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("""{"tool_name":"Bash","tool_input":{"command":"ls"}}""")]
    public void UnexpectedInputIsIgnored(string json) => Assert.Equal(0, Run(json, "<Border Background=\"#FF0000\" />").ExitCode);

    [Fact]
    public void MissingFileIsIgnored() => Assert.Equal(0, Run(Event("Write", new { file_path = PagePath, content = "" }), null).ExitCode);

    [Fact]
    public void WindowsPathsAreShownRelativeToTheWorkingDirectory()
    {
        var json = JsonSerializer.Serialize(new
        {
            cwd = @"C:\repo",
            tool_name = "Write",
            tool_input = new { file_path = @"C:\repo\web\app.css", content = "" },
        });

        var result = Run(json, ".a { color: red; }");

        Assert.Contains("web/app.css:1:", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ChangedLinesFallBackToTheWholeFileWhenTheEditIsNotFound()
        => Assert.Null(Hook.ChangedLines("a\nb\n", ["c"]));
}
