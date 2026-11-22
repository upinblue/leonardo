using System.Numerics;
using Microsoft.UI.Xaml;

namespace UpInBlue.Leonardo.WinUI.Motion;

/// <summary>
/// The direction of a motion. Forward comes from the chosen element or from the right; back reverses exactly that.
/// </summary>
public enum MotionDirection
{
    /// <summary>One level deeper, or the normal entrance: content comes from the right.</summary>
    Forward,

    /// <summary>Back: content comes from the left.</summary>
    Back,
}

/// <summary>
/// Stagger: these belong together, in this order. The groups of a view enter shortly after each other, in reading order:
/// at most <c>motion.stagger.maxGroups</c> groups, <c>motion.stagger.step</c> apart, travelling a distance between
/// <c>motion.distance.enterMin</c> and <c>enterMax</c> with the enter curve, while their opacity rises linearly with
/// <c>motion.duration.fade</c>. With animations off, the groups crossfade in together.
/// </summary>
public static class Stagger
{
    /// <summary>
    /// Lets groups enter in reading order.
    /// </summary>
    /// <param name="groups">The groups in reading order, for example heading, summary and list. Null entries are skipped.</param>
    /// <param name="direction">Where the groups come from.</param>
    /// <param name="distance">How far they travel, clamped to <c>motion.distance.enterMin</c> … <c>enterMax</c>. Defaults to the minimum.</param>
    /// <param name="firstGroup">The stagger position of the first group, for example 1 when another element entered at position 0.</param>
    public static MotionRun Enter(IEnumerable<UIElement?> groups, MotionDirection direction = MotionDirection.Forward, double distance = LeonardoTokens.Motion.DistanceEnterMin, int firstGroup = 0)
    {
        ArgumentNullException.ThrowIfNull(groups);
        var elements = groups.OfType<UIElement>().ToList();
        if (elements.Count == 0)
        {
            return MotionRun.Completed;
        }

        var motion = new CompositionMotion();
        var fade = MotionTiming.Get(MotionDuration.Fade);
        if (!MotionSettings.AnimationsEnabled)
        {
            foreach (var element in elements)
            {
                motion.Fade(element, 0f, 1f, fade);
            }

            return motion.Start();
        }

        var travel = (float)MotionMath.ClampDistance(distance, LeonardoTokens.Motion.DistanceEnterMin, LeonardoTokens.Motion.DistanceEnterMax);
        var from = new Vector3(direction == MotionDirection.Forward ? travel : -travel, 0f, 0f);
        var duration = MotionTiming.Enter(travel);
        for (var index = 0; index < elements.Count; index++)
        {
            var delay = MotionTiming.Scale(MotionMath.StaggerDelay(firstGroup + index, LeonardoTokens.Motion.StaggerStep, LeonardoTokens.Motion.StaggerMaxGroups));
            motion.Translate(elements[index], from, Vector3.Zero, MotionEasing.Enter, duration, delay)
                .Fade(elements[index], 0f, 1f, fade, delay);
        }

        return motion.Start();
    }
}
