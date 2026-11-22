using Microsoft.UI.Xaml;

namespace UpInBlue.Leonardo.WinUI.Motion;

/// <summary>
/// Transform: this thing becomes that. A source element, for example a button, changes shape, size and content into the
/// element it becomes, for example the panel it opens, with the move curve in <c>motion.duration.normal</c>, instead of
/// disappearing and reappearing. With animations off, the target crossfades in.
/// </summary>
/// <remarks>
/// Built on <c>ConnectedAnimationService</c> with the basic configuration, so that duration and curve come from the tokens.
/// The connected animation crossfades a snapshot of the source into the target while it moves and scales.
/// </remarks>
public static class Transform
{
    /// <summary>The key of the connected animation.</summary>
    public const string AnimationKey = "LeonardoTransform";

    /// <summary>
    /// Lets a source element become a target element.
    /// </summary>
    /// <param name="source">The element that is visible now, for example the button that was pressed.</param>
    /// <param name="target">The element it becomes. It may be collapsed now; <paramref name="change"/> makes it visible.</param>
    /// <param name="change">Switches the state, for example hides the button and shows the panel.</param>
    public static MotionRun Run(UIElement source, UIElement target, Action change)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(change);

        MotionInterruption.SkipAllToEnd();
        var canConnect = source.Visibility == Visibility.Visible && source.ActualSize.X > 0 && source.ActualSize.Y > 0;
        if (canConnect)
        {
            ConnectedMotion.Prepare(AnimationKey, source, MotionEasing.Move, MotionTiming.Get(MotionDuration.Normal));
        }

        change();
        target.UpdateLayout();

        var animation = canConnect ? ConnectedMotion.TryStart(AnimationKey, target) : null;
        if (animation is null)
        {
            return new CompositionMotion().Fade(target, 0f, 1f, MotionTiming.Get(MotionDuration.Fade)).Start();
        }

        return new CompositionMotion().Claim(target).Connected(animation).Start();
    }
}
