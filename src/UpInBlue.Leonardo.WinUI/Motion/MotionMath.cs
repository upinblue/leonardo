namespace UpInBlue.Leonardo.WinUI.Motion;

/// <summary>
/// The calculations behind the motion helpers: timing, stagger delays, distances and reflow offsets.
/// The methods take the token values as arguments and use no WinUI types, so they can be unit tested.
/// The helpers in this namespace call them with the values of <c>LeonardoTokens.Motion</c>.
/// </summary>
public static class MotionMath
{
    /// <summary>The slowest inspection speed: one tenth, so 250 ms take 2.5 s.</summary>
    public const double MinimumSpeed = 0.1;

    /// <summary>The normal speed.</summary>
    public const double NormalSpeed = 1;

    /// <summary>
    /// Clamps an inspection speed to <see cref="MinimumSpeed"/> … <see cref="NormalSpeed"/>. Invalid values mean normal speed.
    /// </summary>
    public static double ClampSpeed(double speed)
        => double.IsFinite(speed) ? Math.Clamp(speed, MinimumSpeed, NormalSpeed) : NormalSpeed;

    /// <summary>
    /// Stretches a duration for inspection. At speed 0.1 a duration takes ten times as long.
    /// </summary>
    /// <param name="duration">The duration at normal speed, from a token.</param>
    /// <param name="speed">The inspection speed, see <see cref="ClampSpeed"/>.</param>
    public static TimeSpan Scale(TimeSpan duration, double speed)
        => TimeSpan.FromTicks((long)Math.Round(duration.Ticks / ClampSpeed(speed)));

    /// <summary>
    /// The delay of a stagger group. Groups enter in reading order, one step apart. Groups beyond the maximum
    /// enter together with the last allowed group, so a view never staggers more than <paramref name="maxGroups"/> times.
    /// </summary>
    /// <param name="index">The position of the group in reading order, starting at 0.</param>
    /// <param name="step">The offset between groups, <c>motion.stagger.step</c>.</param>
    /// <param name="maxGroups">The number of groups, <c>motion.stagger.maxGroups</c>.</param>
    public static TimeSpan StaggerDelay(int index, TimeSpan step, int maxGroups)
    {
        if (index <= 0 || maxGroups <= 1)
        {
            return TimeSpan.Zero;
        }

        return step * Math.Min(index, maxGroups - 1);
    }

    /// <summary>
    /// Keeps an entrance distance between <c>motion.distance.enterMin</c> and <c>enterMax</c>.
    /// </summary>
    public static double ClampDistance(double distance, double min, double max)
        => double.IsFinite(distance) ? Math.Clamp(Math.Abs(distance), min, max) : min;

    /// <summary>
    /// The entrance duration for a distance: the normal duration up to <paramref name="max"/>, the long one beyond
    /// (navigation). The short duration belongs to exits only, because leaving MUST be shorter than arriving.
    /// </summary>
    public static TimeSpan EnterDuration(double distance, double max, TimeSpan normalDuration, TimeSpan longDuration)
        => Math.Abs(distance) <= max ? normalDuration : longDuration;

    /// <summary>
    /// The duration of a movement of elements already on screen (Reflow, Transform): the normal duration up to
    /// <paramref name="threshold"/>, the long one beyond.
    /// </summary>
    public static TimeSpan MoveDuration(double distance, double threshold, TimeSpan normalDuration, TimeSpan longDuration)
        => Math.Abs(distance) <= threshold ? normalDuration : longDuration;

    /// <summary>
    /// The delay of the fade that accompanies an exit. The exit curve accelerates, so the element moves at the end;
    /// the fade runs during that last part and ends together with the movement.
    /// </summary>
    public static TimeSpan FadeOutDelay(TimeSpan movement, TimeSpan fade)
        => movement > fade ? movement - fade : TimeSpan.Zero;

    /// <summary>
    /// The offset an element has to start from so that it seems to glide from its old place to its new one.
    /// Returns null when it did not move noticeably (less than half an effective pixel).
    /// </summary>
    public static (double X, double Y)? ReflowOffset(double oldX, double oldY, double newX, double newY)
    {
        var x = oldX - newX;
        var y = oldY - newY;
        return Math.Abs(x) < 0.5 && Math.Abs(y) < 0.5 ? null : (x, y);
    }

    /// <summary>
    /// The length of an offset, for choosing a duration.
    /// </summary>
    public static double Length(double x, double y) => Math.Sqrt((x * x) + (y * y));

    /// <summary>
    /// Whether a cubic Bézier curve is linear, that is, both control points lie on the diagonal.
    /// Leonardo allows linear curves for opacity only.
    /// </summary>
    public static bool IsLinear(double x1, double y1, double x2, double y2)
        => Math.Abs(x1 - y1) < 1e-9 && Math.Abs(x2 - y2) < 1e-9;
}
