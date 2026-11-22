namespace UpInBlue.Leonardo.Check.Tests;

public sealed class XamlRuleTests
{
    private const string Page = "Pages/SamplePage.xaml";

    [Theory]
    [InlineData("""<Border Background="#FF0000" />""")]
    [InlineData("""<Border Background="#F00" />""")]
    [InlineData("""<TextBlock Foreground="Red" />""")]
    [InlineData("""<Rectangle Fill="#80112233" />""")]
    [InlineData("""<Setter Property="BorderBrush" Value="White" />""")]
    [InlineData("""<SolidColorBrush x:Key="MyBrush" Color="#1B6CE0" />""")]
    [InlineData("""<Color x:Key="MyColor">#1B6CE0</Color>""")]
    public void LiteralColorsAreReported(string xaml) => Check.Reports(Rules.LiteralColor, Page, xaml);

    [Theory]
    [InlineData("""<Border Background="{ThemeResource LeonardoSurfaceAltBrush}" />""")]
    [InlineData("""<Border Background="Transparent" />""")]
    [InlineData("""<TextBlock Text="#1 in sales" />""")]
    [InlineData("""<FontIcon Glyph="&#xE8B7;" />""")]
    [InlineData("""<SolidColorBrush x:Key="GalleryChartBrush" Color="{StaticResource LeonardoPaletteNearColor}" />""")]
    public void ResourcesAndNonColorsAreClean(string xaml) => Check.Clean(Page, xaml);

    [Fact]
    public void LiteralFontSizeIsReported()
        => Check.Reports(Rules.LiteralFontSize, Page, """<TextBlock FontSize="20" Text="Contracts" />""");

    [Fact]
    public void FontSizeFromResourceIsClean()
        => Check.Clean(Page, """<FontIcon FontSize="{StaticResource GalleryIconSize}" Glyph="&#xE8B7;" />""");

    [Theory]
    [InlineData("""<Border CornerRadius="6" />""")]
    [InlineData("""<Border CornerRadius="8,8,0,0" />""")]
    public void LiteralRadiusIsReported(string xaml) => Check.Reports(Rules.LiteralCornerRadius, Page, xaml);

    [Fact]
    public void ZeroRadiusIsClean() => Check.Clean(Page, """<Border CornerRadius="0" />""");

    [Fact]
    public void RadiusSuggestionNamesTheMatchingToken()
    {
        var finding = Assert.Single(Check.Run(Page, """<Border CornerRadius="8" />"""));
        Assert.Contains("LeonardoRadiusM", finding.Suggestion, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("""<DoubleAnimation Duration="0:0:0.3" To="1" />""")]
    [InlineData("""<SplineDoubleKeyFrame KeyTime="0:0:0.25" Value="0" KeySpline="{StaticResource LeonardoEaseEnter}" />""")]
    [InlineData("""<DoubleAnimation BeginTime="0:0:0.05" />""")]
    public void LiteralTimesAreReported(string xaml) => Check.Reports(Rules.LiteralDuration, Page, xaml);

    [Theory]
    [InlineData("""<SplineDoubleKeyFrame KeyTime="0:0:0" Value="0" />""")]
    [InlineData("""<SplineDoubleKeyFrame KeyTime="{StaticResource LeonardoDurationNormal}" KeySpline="{StaticResource LeonardoEaseEnter}" />""")]
    public void ZeroAndResourceTimesAreClean(string xaml) => Check.Clean(Page, xaml);

    [Theory]
    [InlineData("""<SplineDoubleKeyFrame KeySpline="0.1,0.9 0.2,1" />""")]
    [InlineData("""<DoubleAnimation><DoubleAnimation.EasingFunction><CubicEase EasingMode="EaseOut" /></DoubleAnimation.EasingFunction></DoubleAnimation>""")]
    public void LiteralEasingIsReported(string xaml) => Check.Reports(Rules.LiteralEasing, Page, xaml);

    [Fact]
    public void BouncingEasingIsReported()
        => Check.Reports(Rules.SpringBounce, Page, """<DoubleAnimation><DoubleAnimation.EasingFunction><BounceEase /></DoubleAnimation.EasingFunction></DoubleAnimation>""");

    [Theory]
    [InlineData("""<StackPanel Margin="12" />""")]
    [InlineData("""<StackPanel Padding="24,0,24,0" />""")]
    [InlineData("""<StackPanel Spacing="10" />""")]
    [InlineData("""<Grid ColumnSpacing="16" />""")]
    [InlineData("""<Setter Property="Padding" Value="8" />""")]
    public void LiteralSpacingIsReported(string xaml) => Check.Reports(Rules.LiteralSpacing, Page, xaml);

    [Theory]
    [InlineData("""<StackPanel Margin="0" />""")]
    [InlineData("""<StackPanel Margin="{StaticResource LeonardoSpaceXLHorizontalThickness}" Spacing="{StaticResource LeonardoSpaceS}" />""")]
    [InlineData("""<Border BorderThickness="1" />""")]
    [InlineData("""<x:Double x:Key="GalleryIconSize">16</x:Double>""")]
    public void ZeroResourceAndNonSpacingValuesAreClean(string xaml) => Check.Clean(Page, xaml);

    [Fact]
    public void SpacingSuggestionNamesTheMatchingToken()
    {
        var finding = Assert.Single(Check.Run(Page, """<StackPanel Padding="16" />"""));
        Assert.Contains("LeonardoSpaceMThickness", finding.Suggestion, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("""<Border><Border.Background><AcrylicBrush TintOpacity="0.4" /></Border.Background></Border>""")]
    [InlineData("""<Window.SystemBackdrop><DesktopAcrylicBackdrop /></Window.SystemBackdrop>""")]
    public void CustomGlassIsReported(string xaml) => Check.Reports(Rules.CustomGlass, Page, xaml);

    [Fact]
    public void MicaIsClean() => Check.Clean(Page, """<Window.SystemBackdrop><MicaBackdrop /></Window.SystemBackdrop>""");

    [Theory]
    [InlineData("""<Button UseSystemFocusVisuals="False" />""")]
    [InlineData("""<Button FocusVisualPrimaryThickness="0" />""")]
    [InlineData("""<Button FocusVisualStyle="{x:Null}" />""")]
    [InlineData("""<Setter Property="FocusVisualPrimaryBrush" Value="{ThemeResource LeonardoAccentBrush}" />""")]
    public void RemovedOrRestyledFocusVisualsAreReported(string xaml) => Check.Reports(Rules.FocusVisual, Page, xaml);

    [Fact]
    public void CommentsAreIgnored()
        => Check.Clean(Page, """
            <!-- Incorrect: <Border Background="#FF0000" FontSize="20" CornerRadius="6" /> -->
            <Border />
            """);

    [Fact]
    public void AttributesOnSeveralLinesAreReportedOnTheirLine()
    {
        var finding = Assert.Single(Check.Run(Page, """
            <Border
                x:Name="Pane"
                Background="#FF0000" />
            """));
        Assert.Equal(3, finding.Line);
    }
}
