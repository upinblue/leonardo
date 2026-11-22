using UpInBlue.Leonardo.WinUI.Controls;

namespace UpInBlue.Leonardo.WinUI.Tests;

public sealed class StatusTextTests
{
    [Theory]
    [InlineData(StatusKind.New, "New")]
    [InlineData(StatusKind.CheckedOut, "Checked out")]
    [InlineData(StatusKind.Conflict, "Conflict")]
    public void EveryKindHasAName(StatusKind kind, string expected)
    {
        Assert.Equal(expected, StatusText.DefaultName(kind));
    }

    [Fact]
    public void TheDescriptionIsTheAccessibleName()
    {
        Assert.Equal("Checked out by Anna Berg", StatusText.Describe(StatusKind.CheckedOut, " Checked out by Anna Berg "));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithoutDescriptionTheNameIsNeverEmpty(string? description)
    {
        Assert.Equal("Conflict", StatusText.Describe(StatusKind.Conflict, description));
    }
}

public sealed class PlaceHeaderMathTests
{
    private const double Factor = 0.7;

    [Fact]
    public void ParallaxFactorIsTheToken()
    {
        Assert.Equal(Factor, Tokens.Number("motion.parallax.placeNameFactor"));
    }

    [Fact]
    public void TheTitleMovesSlowerThanTheContent()
    {
        Assert.Equal(0, PlaceHeaderMath.ParallaxLift(0, Factor, 60));
        Assert.Equal(28, PlaceHeaderMath.ParallaxLift(40, Factor, 60), 6);
        Assert.Equal(42, PlaceHeaderMath.ParallaxLift(500, Factor, 60), 6);
        Assert.Equal(0, PlaceHeaderMath.ParallaxLift(-30, Factor, 60));
    }

    [Fact]
    public void ShrinksOnlyBeyondTheThreshold()
    {
        var threshold = PlaceHeaderMath.Threshold(PlaceHeaderMath.ShrinkDistance(68, 28), 8);

        Assert.Equal(48, threshold);
        Assert.False(PlaceHeaderMath.NextIsCompact(false, 47, threshold));
        Assert.True(PlaceHeaderMath.NextIsCompact(false, 48, threshold));
    }

    [Fact]
    public void ExpandsOnlyAtTheTop()
    {
        Assert.True(PlaceHeaderMath.NextIsCompact(true, 1, 48));
        Assert.False(PlaceHeaderMath.NextIsCompact(true, 0, 48));
    }

    [Fact]
    public void TheCompensatedOffsetStaysAboveZeroSoTheHeaderCannotFlipBack()
    {
        var distance = PlaceHeaderMath.ShrinkDistance(68, 28);
        var threshold = PlaceHeaderMath.Threshold(distance, 8);

        var offset = PlaceHeaderMath.CompensatedOffset(threshold, distance);

        Assert.True(offset > 0);
        Assert.True(PlaceHeaderMath.NextIsCompact(true, offset, threshold));
    }

    [Fact]
    public void AHeaderWithNothingToGiveUpNeverShrinks()
    {
        var threshold = PlaceHeaderMath.Threshold(PlaceHeaderMath.ShrinkDistance(28, 28), 8);

        Assert.Equal(0, threshold);
        Assert.False(PlaceHeaderMath.NextIsCompact(false, 1000, threshold));
    }
}
