namespace UpInBlue.Leonardo.Check.Tests;

/// <summary>
/// The acceptance criterion of stage 8: adding a literal color to a gallery page, a SwiftUI view and the web sample
/// produces a reported violation each. The real gallery page is read from the repository and changed in memory.
/// </summary>
public sealed class AcceptanceTests
{
    [Fact]
    public void LiteralColorAddedToAGalleryPageIsReported()
    {
        const string path = "samples/UpInBlue.Leonardo.Gallery/Pages/FilesPage.xaml";
        var original = File.ReadAllText(Repository.PathOf(path));
        const string anchor = "Background=\"{ThemeResource LeonardoSurfaceAltBrush}\"";
        Assert.Contains(anchor, original, StringComparison.Ordinal);

        Check.Clean(path, original);

        var changed = original.Replace(anchor, "Background=\"#F3F7FC\"", StringComparison.Ordinal);
        var finding = Assert.Single(Check.Run(path, changed));
        Assert.Equal(Rules.LiteralColor, finding.Rule);
        Assert.Equal(LineOf(changed, "Background=\"#F3F7FC\""), finding.Line);
    }

    [Fact]
    public void LiteralColorAddedToASwiftUiViewIsReported()
    {
        const string path = "samples/apple/Shared/FileRow.swift";
        const string view = """
            import SwiftUI

            struct FileRow: View {
                let file: FileItem

                var body: some View {
                    VStack(alignment: .leading, spacing: Leonardo.Space.xs) {
                        Text(file.name)
                            .font(.system(Leonardo.TextStyle.body))
                        Text(file.modifiedBy)
                            .font(.system(Leonardo.TextStyle.meta))
                            .foregroundStyle(Leonardo.Colors.textTertiary)
                    }
                }
            }
            """;

        Check.Clean(path, view);

        var changed = view.Replace("Leonardo.Colors.textTertiary", "Color(red: 0.42, green: 0.47, blue: 0.55)", StringComparison.Ordinal);
        var finding = Assert.Single(Check.Run(path, changed));
        Assert.Equal(Rules.LiteralColor, finding.Rule);
        Assert.Equal(12, finding.Line);
    }

    [Fact]
    public void LiteralColorAddedToTheWebSampleIsReported()
    {
        const string path = "samples/web/files.css";
        const string css = """
            .banner {
                padding: var(--leonardo-space-s) var(--leonardo-space-l);
                background: var(--leonardo-surface-alt);
            }

            .banner-title {
                color: var(--leonardo-text-secondary);
            }
            """;

        Check.Clean(path, css);

        var changed = css.Replace("var(--leonardo-text-secondary)", "#5B6B82", StringComparison.Ordinal);
        var finding = Assert.Single(Check.Run(path, changed));
        Assert.Equal(Rules.LiteralColor, finding.Rule);
        Assert.Equal(7, finding.Line);
    }

    private static int LineOf(string text, string value) => text[..text.IndexOf(value, StringComparison.Ordinal)].Count(c => c == '\n') + 1;
}
