using UpInBlue.Leonardo.WinUI.Motion;

namespace UpInBlue.Leonardo.WinUI.Tests;

public sealed class MotionMathTests
{
    private static readonly TimeSpan Fade = Tokens.Duration("motion.duration.fade");
    private static readonly TimeSpan Short = Tokens.Duration("motion.duration.short");
    private static readonly TimeSpan Normal = Tokens.Duration("motion.duration.normal");
    private static readonly TimeSpan Long = Tokens.Duration("motion.duration.long");
    private static readonly TimeSpan Step = Tokens.Duration("motion.stagger.step");
    private static readonly int MaxGroups = (int)Tokens.Number("motion.stagger.maxGroups");
    private static readonly double EnterMin = Tokens.Dimension("motion.distance.enterMin");
    private static readonly double EnterMax = Tokens.Dimension("motion.distance.enterMax");

    [Theory]
    [InlineData("motion.easing.enter")]
    [InlineData("motion.easing.exit")]
    [InlineData("motion.easing.move")]
    public void MovementIsNeverLinear(string path)
    {
        var curve = Tokens.CubicBezier(path);

        Assert.False(MotionMath.IsLinear(curve[0], curve[1], curve[2], curve[3]));
    }

    [Fact]
    public void OnlyOpacityIsLinear()
    {
        var curve = Tokens.CubicBezier("motion.easing.fade");

        Assert.True(MotionMath.IsLinear(curve[0], curve[1], curve[2], curve[3]));
    }

    [Fact]
    public void ArrivingDeceleratesAndLeavingAccelerates()
    {
        var enter = Tokens.CubicBezier("motion.easing.enter");
        var exit = Tokens.CubicBezier("motion.easing.exit");

        // Decelerating: the curve rises above the diagonal at the start. Accelerating: it stays below.
        Assert.True(enter[1] >= enter[0] && enter[3] > enter[2]);
        Assert.True(exit[0] > exit[1] && exit[2] >= exit[3]);
    }

    [Fact]
    public void ExitsAreShorterThanEveryEntrance()
    {
        var exit = Short;

        Assert.True(exit < MotionMath.EnterDuration(0, EnterMax, Normal, Long));
        Assert.True(exit < MotionMath.EnterDuration(EnterMin, EnterMax, Normal, Long));
        Assert.True(exit < MotionMath.EnterDuration(EnterMax * 4, EnterMax, Normal, Long));
    }

    [Fact]
    public void OpacityIsFasterThanMovement()
    {
        Assert.True(Fade < Short);
        Assert.True(Fade < Normal);
        Assert.True(Fade < Long);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(4, 3)]
    [InlineData(10, 3)]
    [InlineData(-1, 0)]
    public void StaggerUsesAtMostTheMaximumNumberOfGroups(int index, int expectedSteps)
    {
        Assert.Equal(4, MaxGroups);

        Assert.Equal(Step * expectedSteps, MotionMath.StaggerDelay(index, Step, MaxGroups));
    }

    [Theory]
    [InlineData(0, 24)]
    [InlineData(24, 24)]
    [InlineData(32, 32)]
    [InlineData(40, 40)]
    [InlineData(120, 40)]
    [InlineData(-32, 32)]
    [InlineData(double.NaN, 24)]
    public void EntranceDistanceStaysBetweenMinimumAndMaximum(double distance, double expected)
    {
        Assert.Equal(expected, MotionMath.ClampDistance(distance, EnterMin, EnterMax));
    }

    [Fact]
    public void EntranceDurationGrowsWithDistance()
    {
        Assert.Equal(Normal, MotionMath.EnterDuration(EnterMin, EnterMax, Normal, Long));
        Assert.Equal(Normal, MotionMath.EnterDuration(EnterMax, EnterMax, Normal, Long));
        Assert.Equal(Long, MotionMath.EnterDuration(EnterMax + 1, EnterMax, Normal, Long));
    }

    [Fact]
    public void MoveDurationIsNormalOrLong()
    {
        Assert.Equal(Normal, MotionMath.MoveDuration(EnterMax, EnterMax, Normal, Long));
        Assert.Equal(Long, MotionMath.MoveDuration(-EnterMax * 3, EnterMax, Normal, Long));
    }

    [Theory]
    [InlineData(1, 250)]
    [InlineData(0.5, 500)]
    [InlineData(0.1, 2500)]
    [InlineData(0.01, 2500)]
    [InlineData(2, 250)]
    [InlineData(double.NaN, 250)]
    public void SlowingDownStretchesDurationsUpToTenTimes(double speed, double expectedMilliseconds)
    {
        Assert.Equal(TimeSpan.FromMilliseconds(expectedMilliseconds), MotionMath.Scale(Normal, speed));
    }

    [Fact]
    public void ExitFadeEndsTogetherWithTheMovement()
    {
        var delay = MotionMath.FadeOutDelay(Short, Fade);

        Assert.Equal(Short, delay + Fade);
        Assert.Equal(TimeSpan.Zero, MotionMath.FadeOutDelay(Fade, Short));
    }

    [Fact]
    public void ReflowStartsFromTheOldPlace()
    {
        Assert.Equal((0d, 80d), MotionMath.ReflowOffset(4, 120, 4, 40));
        Assert.Equal((0d, -40d), MotionMath.ReflowOffset(4, 0, 4, 40));
    }

    [Fact]
    public void ReflowIgnoresItemsThatDidNotMove()
    {
        Assert.Null(MotionMath.ReflowOffset(4, 40, 4.2, 40.3));
    }
}
