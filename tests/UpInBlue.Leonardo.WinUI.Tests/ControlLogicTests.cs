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
    // An example header: meta line 20, place name 56, compact title 28; the compact font is half the place name font.
    private const double Meta = 20;
    private const double Title = 56;
    private const double Compact = 28;
    private const double Ratio = 0.5;
    private static readonly double Distance = PlaceHeaderMath.ShrinkDistance(Meta + Title, Compact);

    private static double Share => PlaceHeaderMath.CrossfadeShare(
        Tokens.Duration("motion.duration.fade"),
        Tokens.Duration("motion.duration.normal"));

    [Fact]
    public void TheHeaderGivesUpMetaLineAndPlaceNameMinusTheCompactTitle()
    {
        Assert.Equal(48, Distance);
    }

    [Theory]
    [InlineData(-30, 0)]
    [InlineData(0, 0)]
    [InlineData(12, 0.25)]
    [InlineData(48, 1)]
    [InlineData(500, 1)]
    public void ProgressFollowsTheScrollOffsetOverTheShrinkDistance(double offset, double expected)
    {
        Assert.Equal(expected, PlaceHeaderMath.Progress(offset, Distance), 6);
    }

    [Fact]
    public void TheSummaryMovesExactlyWithTheContent()
    {
        Assert.Equal(0, PlaceHeaderMath.ContentLift(-30, Distance, isCompact: false));
        Assert.Equal(30, PlaceHeaderMath.ContentLift(30, Distance, isCompact: false));
        Assert.Equal(Distance, PlaceHeaderMath.ContentLift(500, Distance, isCompact: false));
    }

    [Fact]
    public void TheMetaLineFadesOverItsOwnHeightInsteadOfBeingCovered()
    {
        Assert.Equal(1, PlaceHeaderMath.MetaOpacity(0, Meta));
        Assert.Equal(0.5, PlaceHeaderMath.MetaOpacity(10, Meta), 6);
        Assert.Equal(0, PlaceHeaderMath.MetaOpacity(Meta, Meta));
        Assert.Equal(0, PlaceHeaderMath.MetaOpacity(0, 0));
    }

    [Fact]
    public void TheTitleMovesMoreSlowlyThanTheContent()
    {
        for (var offset = 1.0; offset <= Distance; offset++)
        {
            var progress = PlaceHeaderMath.Progress(offset, Distance);
            Assert.True(PlaceHeaderMath.TitleLift(progress, Meta, isCompact: false) < PlaceHeaderMath.ContentLift(offset, Distance, isCompact: false));
        }
    }

    [Fact]
    public void TheTitleScalesContinuouslyFromThePlaceNameToTheCompactSize()
    {
        Assert.Equal(1, PlaceHeaderMath.LargeTitleScale(0, Ratio));
        Assert.Equal(0.75, PlaceHeaderMath.LargeTitleScale(0.5, Ratio), 6);
        Assert.Equal(Ratio, PlaceHeaderMath.LargeTitleScale(1, Ratio));
        Assert.Equal(2, PlaceHeaderMath.CompactTitleScale(0, Ratio));
        Assert.Equal(1, PlaceHeaderMath.CompactTitleScale(1, Ratio));
    }

    [Fact]
    public void OpacityIsFasterThanMovementSoTheTitlesCrossfadeAtTheEnd()
    {
        Assert.Equal(83.0 / 250.0, Share, 6);
        Assert.Equal(0, PlaceHeaderMath.CompactTitleOpacity(1 - Share, Share), 6);
        Assert.Equal(1, PlaceHeaderMath.CompactTitleOpacity(1, Share), 6);
        Assert.Equal(1, PlaceHeaderMath.LargeTitleOpacity(0.5, Share), 6);
        Assert.Equal(0, PlaceHeaderMath.LargeTitleOpacity(1, Share), 6);
    }

    [Fact]
    public void AtTheEndOfTheDistanceTheExpandedLayoutLooksExactlyLikeTheCompactLayout()
    {
        // Positions relative to the top of the header content, expanded layout with its transforms ...
        var progress = PlaceHeaderMath.Progress(Distance, Distance);
        var titleExpanded = Meta - PlaceHeaderMath.TitleLift(progress, Meta, isCompact: false);
        var summaryExpanded = Meta + Title - PlaceHeaderMath.ContentLift(Distance, Distance, isCompact: false);

        // ... and the compact layout, where the meta line and the large title are gone.
        var titleCompact = 0 - PlaceHeaderMath.TitleLift(progress, Meta, isCompact: true);
        var summaryCompact = Compact - PlaceHeaderMath.ContentLift(Distance, Distance, isCompact: true);

        Assert.Equal(titleCompact, titleExpanded, 6);
        Assert.Equal(summaryCompact, summaryExpanded, 6);
        Assert.Equal(1, PlaceHeaderMath.CompactTitleScale(progress, Ratio));
        Assert.Equal(1, PlaceHeaderMath.CompactTitleOpacity(progress, Share), 6);
    }

    [Fact]
    public void TheHeaderIsCompactFromTheEndOfTheDistanceOn()
    {
        Assert.False(PlaceHeaderMath.IsCompact(47.9, Distance));
        Assert.True(PlaceHeaderMath.IsCompact(48, Distance));
    }

    [Fact]
    public void ScrollingBackReversesEveryStepExactly()
    {
        // Every value is a function of the offset alone, so the way back passes the same frames.
        var down = Enumerable.Range(0, 60).Select(offset => Frame(offset)).ToArray();
        var up = Enumerable.Range(0, 60).Reverse().Select(offset => Frame(offset)).Reverse().ToArray();

        Assert.Equal(down, up);
    }

    [Fact]
    public void AHeaderWithNothingToGiveUpNeverShrinks()
    {
        var distance = PlaceHeaderMath.ShrinkDistance(Compact, Compact);

        Assert.Equal(0, distance);
        Assert.Equal(0, PlaceHeaderMath.Progress(1000, distance));
        Assert.False(PlaceHeaderMath.IsCompact(1000, distance));
    }

    private static (double Summary, double Title, double Scale, double Meta, double Opacity) Frame(double offset)
    {
        var compact = PlaceHeaderMath.IsCompact(offset, Distance);
        var progress = PlaceHeaderMath.Progress(offset, Distance);
        return (
            PlaceHeaderMath.ContentLift(offset, Distance, compact),
            PlaceHeaderMath.TitleLift(progress, Meta, compact),
            PlaceHeaderMath.LargeTitleScale(progress, Ratio),
            PlaceHeaderMath.MetaOpacity(offset, Meta),
            PlaceHeaderMath.CompactTitleOpacity(progress, Share));
    }
}
