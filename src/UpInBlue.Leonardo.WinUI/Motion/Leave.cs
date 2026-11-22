using System.Numerics;
using Microsoft.UI.Xaml;

namespace UpInBlue.Leonardo.WinUI.Motion;

/// <summary>
/// Leave: this is gone. The element accelerates out with the exit curve in <c>motion.duration.short</c> and fades while
/// doing so; it is shorter than any entrance. With animations off, it only fades with <c>motion.duration.fade</c>.
/// </summary>
public static class Leave
{
    /// <summary>
    /// Lets an element leave. It stays transparent at the end; remove or collapse it, then call <see cref="Reset"/>
    /// if the element is reused, for example a recycled list container. <see cref="RunAsync"/> does both.
    /// </summary>
    /// <param name="element">The element that leaves.</param>
    /// <param name="direction">Forward leaves to the left, making room for what comes from the right; back leaves to the right.</param>
    public static MotionRun Run(UIElement element, MotionDirection direction = MotionDirection.Forward)
    {
        ArgumentNullException.ThrowIfNull(element);
        var motion = new CompositionMotion();
        var fade = MotionTiming.Get(MotionDuration.Fade);
        if (!MotionSettings.AnimationsEnabled)
        {
            return motion.Fade(element, 1f, 0f, fade).Start();
        }

        var duration = MotionTiming.Get(MotionDuration.Short);
        var travel = (float)LeonardoTokens.Motion.DistanceEnterMin;
        var to = new Vector3(direction == MotionDirection.Forward ? -travel : travel, 0f, 0f);
        return motion
            .Translate(element, Vector3.Zero, to, MotionEasing.Exit, duration)
            .Fade(element, 1f, 0f, fade, MotionMath.FadeOutDelay(duration, fade))
            .Start();
    }

    /// <summary>
    /// Lets an element leave, then removes it and resets it, so that a recycled container is visible again.
    /// </summary>
    /// <param name="element">The element that leaves, for example a list item container.</param>
    /// <param name="remove">Removes the element, for example by removing its item from the collection.</param>
    /// <param name="direction">See <see cref="Run"/>.</param>
    public static async Task RunAsync(UIElement element, Action remove, MotionDirection direction = MotionDirection.Forward)
    {
        ArgumentNullException.ThrowIfNull(remove);
        await Run(element, direction).Completion;
        remove();
        Reset(element);
    }

    /// <summary>
    /// Makes an element visible again in its layout position after it has left.
    /// </summary>
    public static void Reset(UIElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        CompositionMotion.Reset(element);
    }
}
