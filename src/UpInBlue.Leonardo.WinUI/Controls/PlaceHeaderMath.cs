namespace UpInBlue.Leonardo.WinUI.Controls;

/// <summary>
/// The scroll behavior of a <see cref="PlaceHeader"/>, testable without WinUI.
/// </summary>
/// <remarks>
/// The header sits above its scroll viewer. While the content scrolls up to the threshold, the place name moves up
/// at a fraction of the content speed (parallax). Beyond the threshold, the header shrinks to a compact line; the scroll
/// offset is reduced by the height the header gave up, so the content does not jump. Scrolling back to the very top
/// expands the header again. The threshold is larger than that height, so the reduced offset stays above zero and the
/// header cannot flip back and forth.
/// </remarks>
public static class PlaceHeaderMath
{
    /// <summary>
    /// The height the header gives up when it shrinks.
    /// </summary>
    public static double ShrinkDistance(double expandedHeight, double compactHeight)
        => Math.Max(0, expandedHeight - compactHeight);

    /// <summary>
    /// The scroll offset at which the header shrinks: the shrink distance plus a margin, so that the compensated offset stays positive.
    /// </summary>
    public static double Threshold(double shrinkDistance, double margin)
        => shrinkDistance <= 0 ? 0 : shrinkDistance + Math.Max(margin, 1);

    /// <summary>
    /// How far the place name moves up for a scroll offset: the offset times the parallax factor, up to the threshold.
    /// </summary>
    public static double ParallaxLift(double offset, double factor, double threshold)
        => Math.Clamp(offset, 0, Math.Max(threshold, 0)) * Math.Clamp(factor, 0, 1);

    /// <summary>
    /// Whether the header is compact after the content scrolled to <paramref name="offset"/>.
    /// </summary>
    /// <param name="isCompact">Whether it is compact now.</param>
    /// <param name="offset">The vertical scroll offset.</param>
    /// <param name="threshold">See <see cref="Threshold"/>. Zero means the header never shrinks.</param>
    public static bool NextIsCompact(bool isCompact, double offset, double threshold)
    {
        if (isCompact)
        {
            return offset > 0;
        }

        return threshold > 0 && offset >= threshold;
    }

    /// <summary>
    /// The scroll offset after shrinking, so that the content stays where it is on screen.
    /// </summary>
    public static double CompensatedOffset(double offset, double shrinkDistance)
        => Math.Max(0, offset - shrinkDistance);
}
