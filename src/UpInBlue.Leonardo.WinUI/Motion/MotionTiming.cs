using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;

namespace UpInBlue.Leonardo.WinUI.Motion;

/// <summary>
/// The durations of Leonardo motion.
/// </summary>
public enum MotionDuration
{
    /// <summary>Opacity: <c>motion.duration.fade</c>.</summary>
    Fade,

    /// <summary>Short distances and all exits: <c>motion.duration.short</c>.</summary>
    Short,

    /// <summary>Medium distances: <c>motion.duration.normal</c>.</summary>
    Normal,

    /// <summary>Long distances and navigation: <c>motion.duration.long</c>.</summary>
    Long,
}

/// <summary>
/// Durations from the tokens, stretched by <see cref="MotionSettings.Speed"/>. Use these instead of the raw token values
/// so that the slow-down for inspection applies everywhere.
/// </summary>
public static class MotionTiming
{
    /// <summary>
    /// A duration of the vocabulary.
    /// </summary>
    public static TimeSpan Get(MotionDuration duration) => Scale(Raw(duration));

    /// <summary>The offset between stagger groups, <c>motion.stagger.step</c>.</summary>
    public static TimeSpan StaggerStep => Scale(LeonardoTokens.Motion.StaggerStep);

    /// <summary>How long the Arrive marker stays, <c>motion.arrive.hold</c>.</summary>
    public static TimeSpan ArriveHold => Scale(LeonardoTokens.Motion.ArriveHold);

    /// <summary>
    /// The entrance duration for a distance in epx: normal up to <c>motion.distance.enterMax</c>, long beyond.
    /// Short is reserved for exits, so that leaving is always shorter than arriving.
    /// </summary>
    public static TimeSpan Enter(double distance) => Scale(MotionMath.EnterDuration(
        distance,
        LeonardoTokens.Motion.DistanceEnterMax,
        LeonardoTokens.Motion.DurationNormal,
        LeonardoTokens.Motion.DurationLong));

    /// <summary>
    /// The duration of a movement of elements already on screen: normal up to <c>motion.distance.enterMax</c>, long beyond.
    /// </summary>
    public static TimeSpan Move(double distance) => Scale(MotionMath.MoveDuration(
        distance,
        LeonardoTokens.Motion.DistanceEnterMax,
        LeonardoTokens.Motion.DurationNormal,
        LeonardoTokens.Motion.DurationLong));

    /// <summary>
    /// Stretches any duration by <see cref="MotionSettings.Speed"/>.
    /// </summary>
    public static TimeSpan Scale(TimeSpan duration) => MotionMath.Scale(duration, MotionSettings.Speed);

    /// <summary>
    /// Creates a Storyboard animation of a double property with one spline key frame, for example for opacity.
    /// </summary>
    /// <param name="target">The object to animate.</param>
    /// <param name="property">The property path, for example <c>"Opacity"</c>.</param>
    /// <param name="to">The end value.</param>
    /// <param name="easing">The curve.</param>
    /// <param name="duration">The duration; use <see cref="Get"/> or another member of this class.</param>
    /// <param name="beginTime">The delay before the animation starts.</param>
    public static DoubleAnimationUsingKeyFrames CreateDoubleAnimation(DependencyObject target, string property, double to, MotionEasing easing, TimeSpan duration, TimeSpan beginTime = default)
    {
        var animation = new DoubleAnimationUsingKeyFrames { BeginTime = beginTime };
        animation.KeyFrames.Add(new SplineDoubleKeyFrame
        {
            KeyTime = KeyTime.FromTimeSpan(duration),
            KeySpline = MotionCurves.CreateKeySpline(easing),
            Value = to,
        });
        Storyboard.SetTarget(animation, target);
        Storyboard.SetTargetProperty(animation, property);
        return animation;
    }

    private static TimeSpan Raw(MotionDuration duration) => duration switch
    {
        MotionDuration.Fade => LeonardoTokens.Motion.DurationFade,
        MotionDuration.Short => LeonardoTokens.Motion.DurationShort,
        MotionDuration.Normal => LeonardoTokens.Motion.DurationNormal,
        MotionDuration.Long => LeonardoTokens.Motion.DurationLong,
        _ => throw new ArgumentOutOfRangeException(nameof(duration), duration, null),
    };
}
