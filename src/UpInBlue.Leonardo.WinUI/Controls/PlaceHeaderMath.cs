namespace UpInBlue.Leonardo.WinUI.Controls;

/// <summary>
/// The scroll behavior of a <see cref="PlaceHeader"/>, testable without WinUI.
/// </summary>
/// <remarks>
/// <para>
/// The header follows the scroll position; there is no timed animation. In layout, the header is always as high as its
/// compact form. The scrolling content starts the shrink distance lower, and the expanded header draws over that gap.
/// While the content scrolls through the shrink distance, every part of the header is a function of the scroll offset:
/// the summary slot moves with the content, the meta line moves with it and fades, and the title moves up and scales
/// from the place name size to the compact size. At the end of the distance, the header switches to its compact
/// layout, which looks exactly like the last frame. Scrolling back reverses every step.
/// </para>
/// <para>
/// The methods here compute the same values as the expressions that run on the compositor, so the behavior can be
/// tested frame by frame.
/// </para>
/// </remarks>
public static class PlaceHeaderMath
{
    /// <summary>
    /// The height the header gives up when it shrinks: meta line and place name, minus the compact title.
    /// </summary>
    public static double ShrinkDistance(double expandedHeight, double compactHeight)
        => Math.Max(0, expandedHeight - compactHeight);

    /// <summary>
    /// How far through the shrink distance the content has scrolled, from 0 (expanded) to 1 (compact).
    /// A distance of zero means the header never shrinks.
    /// </summary>
    public static double Progress(double offset, double shrinkDistance)
        => shrinkDistance <= 0 ? 0 : Math.Clamp(offset / shrinkDistance, 0, 1);

    /// <summary>
    /// Whether the header uses its compact layout at a scroll offset: from the end of the shrink distance on.
    /// </summary>
    public static bool IsCompact(double offset, double shrinkDistance)
        => shrinkDistance > 0 && offset >= shrinkDistance;

    /// <summary>
    /// How far the summary slot and the meta line move up, relative to their place in the current layout:
    /// exactly as far as the content, up to the shrink distance. In the compact layout they are already there.
    /// </summary>
    public static double ContentLift(double offset, double shrinkDistance, bool isCompact)
        => isCompact ? 0 : Math.Clamp(offset, 0, Math.Max(shrinkDistance, 0));

    /// <summary>
    /// The opacity of the meta line: it fades while the content scrolls by its own height.
    /// </summary>
    public static double MetaOpacity(double offset, double metaHeight)
        => metaHeight <= 0 ? 0 : 1 - Math.Clamp(offset / metaHeight, 0, 1);

    /// <summary>
    /// How far the titles move up, relative to their place in the current layout. Over the whole shrink distance they
    /// rise by the height of the meta line, into the place the compact title has once the meta line is gone. The title
    /// therefore moves more slowly than the content.
    /// </summary>
    public static double TitleLift(double progress, double metaHeight, bool isCompact)
        => isCompact ? 0 : Math.Clamp(progress, 0, 1) * Math.Max(metaHeight, 0);

    /// <summary>
    /// The scale of the large title: from 1 down to <paramref name="ratio"/>, the compact size over the place name size.
    /// </summary>
    public static double LargeTitleScale(double progress, double ratio)
        => Lerp(1, ratio, progress);

    /// <summary>
    /// The scale of the compact title: from the place name size (1 / <paramref name="ratio"/>) down to 1.
    /// </summary>
    public static double CompactTitleScale(double progress, double ratio)
        => ratio <= 0 ? 1 : Lerp(1 / ratio, 1, progress);

    /// <summary>
    /// The share of the shrink distance over which the two titles crossfade, at its end. Opacity is faster than
    /// movement, so the share is the fade duration over the move duration.
    /// </summary>
    public static double CrossfadeShare(TimeSpan fade, TimeSpan move)
        => move <= TimeSpan.Zero ? 1 : Math.Clamp(fade / move, 0.01, 1);

    /// <summary>
    /// The opacity of the compact title: 0 until the crossfade starts, 1 at the end of the shrink distance.
    /// </summary>
    public static double CompactTitleOpacity(double progress, double crossfadeShare)
    {
        var share = Math.Clamp(crossfadeShare, 0.01, 1);
        return Math.Clamp((Math.Clamp(progress, 0, 1) - (1 - share)) / share, 0, 1);
    }

    /// <summary>
    /// The opacity of the large title: the inverse of <see cref="CompactTitleOpacity"/>.
    /// </summary>
    public static double LargeTitleOpacity(double progress, double crossfadeShare)
        => 1 - CompactTitleOpacity(progress, crossfadeShare);

    private static double Lerp(double from, double to, double progress)
        => from + ((to - from) * Math.Clamp(progress, 0, 1));
}
