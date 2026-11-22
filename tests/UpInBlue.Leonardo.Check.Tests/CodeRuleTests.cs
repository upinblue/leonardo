namespace UpInBlue.Leonardo.Check.Tests;

public sealed class CSharpRuleTests
{
    private const string File = "Pages/SamplePage.xaml.cs";

    [Theory]
    [InlineData("var brush = new SolidColorBrush(Colors.Red);")]
    [InlineData("var color = Color.FromArgb(255, 27, 108, 224);")]
    [InlineData("var color = ColorHelper.FromArgb(0xFF, 0x1B, 0x6C, 0xE0);")]
    [InlineData("""var color = (Color)ColorConverter.ConvertFromString("#1B6CE0");""")]
    public void LiteralColorsAreReported(string code) => Check.Reports(Rules.LiteralColor, File, code);

    [Theory]
    [InlineData("var brush = new SolidColorBrush(Colors.Transparent);")]
    [InlineData("""var brush = (Brush)Application.Current.Resources["LeonardoAccentBrush"];""")]
    [InlineData("""const string Pattern = "AcrylicBrush and Colors.Red in a string";""")]
    [InlineData("// Colors.Red, new Thickness(12), FontSize = 20")]
    public void ResourcesStringsAndCommentsAreClean(string code) => Check.Clean(File, code);

    [Fact]
    public void LiteralFontSizeIsReported() => Check.Reports(Rules.LiteralFontSize, File, "title.FontSize = 28;");

    [Fact]
    public void ComparisonIsNotAnAssignment() => Check.Clean(File, "if (title.FontSize == 28) { }");

    [Fact]
    public void LiteralRadiusIsReported() => Check.Reports(Rules.LiteralCornerRadius, File, "border.CornerRadius = new CornerRadius(6);");

    [Theory]
    [InlineData("panel.Margin = new Thickness(12);")]
    [InlineData("panel.Spacing = 10;")]
    public void LiteralSpacingIsReported(string code) => Check.Reports(Rules.LiteralSpacing, File, code);

    [Fact]
    public void ZeroThicknessIsClean() => Check.Clean(File, "panel.Margin = new Thickness(0);");

    [Theory]
    [InlineData("animation.Duration = TimeSpan.FromMilliseconds(300);")]
    [InlineData("var fade = new DoubleAnimation { Duration = new Duration(TimeSpan.FromSeconds(0.4)) };")]
    public void LiteralAnimationTimesAreReported(string code) => Check.Reports(Rules.LiteralDuration, File, code);

    [Fact]
    public void NonAnimationTimeSpanIsClean() => Check.Clean(File, "var timeout = TimeSpan.FromSeconds(30);");

    [Fact]
    public void TokenDurationIsClean() => Check.Clean(File, "animation.Duration = MotionTiming.Get(MotionDuration.Normal);");

    [Theory]
    [InlineData("var easing = new CubicEase { EasingMode = EasingMode.EaseOut };")]
    [InlineData("var easing = compositor.CreateCubicBezierEasingFunction(new Vector2(0.1f, 0.9f), new Vector2(0.2f, 1f));")]
    [InlineData("var spline = new KeySpline { ControlPoint1 = new Point(0.1, 0.9) };")]
    public void LiteralEasingIsReported(string code) => Check.Reports(Rules.LiteralEasing, File, code);

    [Fact]
    public void EasingFromTokensIsClean()
        => Check.Clean(File, "var spline = new KeySpline { ControlPoint1 = new Windows.Foundation.Point(controlPoint1.X, controlPoint1.Y) };");

    [Fact]
    public void BounceEaseIsReported() => Check.Reports(Rules.SpringBounce, File, "var easing = new BounceEase();");

    [Theory]
    [InlineData("SystemBackdrop = new DesktopAcrylicBackdrop();")]
    [InlineData("var brush = compositor.CreateBackdropBrush();")]
    public void CustomGlassIsReported(string code) => Check.Reports(Rules.CustomGlass, File, code);

    [Theory]
    [InlineData("button.UseSystemFocusVisuals = false;")]
    [InlineData("button.FocusVisualStyle = null;")]
    public void RemovedFocusVisualsAreReported(string code) => Check.Reports(Rules.FocusVisual, File, code);
}

public sealed class SwiftRuleTests
{
    private const string File = "Sources/FilesView.swift";

    [Theory]
    [InlineData("Text(name).foregroundColor(Color(red: 0.6, green: 0.6, blue: 0.65))")]
    [InlineData("Text(name).foregroundStyle(Color(.sRGB, red: 0.1, green: 0.2, blue: 0.3, opacity: 1))")]
    [InlineData("Text(name).foregroundColor(Color(hex: \"#1B6CE0\"))")]
    [InlineData("extension Color { init(hex: String) { self.init(red: 0, green: 0, blue: 0) } }")]
    [InlineData("let tint = UIColor(red: 0.1, green: 0.4, blue: 0.9, alpha: 1)")]
    public void LiteralColorsAreReported(string code) => Check.Reports(Rules.LiteralColor, File, code);

    [Theory]
    [InlineData("Text(name).foregroundStyle(Leonardo.Colors.textPrimary)")]
    [InlineData("Image(systemName: \"circle\").foregroundStyle(.yellow)")]
    public void TokenAndSystemColorsAreClean(string code) => Check.Clean(File, code);

    [Theory]
    [InlineData("Text(name).font(.system(size: 15, weight: .bold))")]
    [InlineData("Text(name).font(.custom(\"Inter\", size: 15))")]
    [InlineData("label.font = UIFont.systemFont(ofSize: 13)")]
    public void FixedFontSizesAreReported(string code) => Check.Reports(Rules.LiteralFontSize, File, code);

    [Theory]
    [InlineData("Text(name).font(.system(Leonardo.TextStyle.section))")]
    [InlineData("Text(name).font(.headline)")]
    [InlineData("Text(name).font(.custom(\"Inter\", size: 15, relativeTo: .body))")]
    public void TextStylesAreClean(string code) => Check.Clean(File, code);

    [Theory]
    [InlineData("withAnimation(.easeInOut(duration: 0.4)) { expanded = true }")]
    [InlineData("withAnimation(.spring(duration: 0.3, bounce: 0)) { expanded = true }")]
    [InlineData(".animation(.linear(duration: 1).delay(0.2), value: progress)")]
    public void LiteralDurationsAreReported(string code) => Check.Reports(Rules.LiteralDuration, File, code);

    [Theory]
    [InlineData("withAnimation(.spring(response: 0.6, dampingFraction: 0.4)) { expanded = true }")]
    [InlineData("withAnimation(.spring(duration: Leonardo.Motion.Spring.durationMin, bounce: 0.3)) { expanded = true }")]
    [InlineData("withAnimation(.bouncy) { expanded = true }")]
    [InlineData("withAnimation(.interpolatingSpring(stiffness: 170, damping: 15)) { expanded = true }")]
    public void BouncingSpringsAreReported(string code) => Check.Reports(Rules.SpringBounce, File, code);

    [Fact]
    public void LeonardoSpringIsClean()
        => Check.Clean(File, "withAnimation(.spring(duration: Leonardo.Motion.Spring.durationMin, bounce: Leonardo.Motion.Spring.bounce)) { expanded = true }");
}
