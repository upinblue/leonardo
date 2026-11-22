using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Media.Animation;

namespace UpInBlue.Leonardo.WinUI.Motion;

/// <summary>
/// The easing curves of Leonardo motion.
/// </summary>
public enum MotionEasing
{
    /// <summary>Arrive and dive in: decelerates strongly. Token <c>motion.easing.enter</c>.</summary>
    Enter,

    /// <summary>Leave: accelerates. Always combined with a fade. Token <c>motion.easing.exit</c>.</summary>
    Exit,

    /// <summary>Transform and reflow: elements already on screen. Token <c>motion.easing.move</c>.</summary>
    Move,

    /// <summary>Linear, for opacity only. Token <c>motion.easing.fade</c>.</summary>
    Fade,
}

/// <summary>
/// Factories for the easing curves, for Composition and for Storyboards. All values come from <see cref="LeonardoTokens.Motion"/>.
/// </summary>
public static class MotionCurves
{
    /// <summary>
    /// The two control points of a curve, as in CSS <c>cubic-bezier(x1, y1, x2, y2)</c>.
    /// </summary>
    public static (Vector2 ControlPoint1, Vector2 ControlPoint2) GetControlPoints(MotionEasing easing) => easing switch
    {
        MotionEasing.Enter => (LeonardoTokens.Motion.EaseEnterControlPoint1, LeonardoTokens.Motion.EaseEnterControlPoint2),
        MotionEasing.Exit => (LeonardoTokens.Motion.EaseExitControlPoint1, LeonardoTokens.Motion.EaseExitControlPoint2),
        MotionEasing.Move => (LeonardoTokens.Motion.EaseMoveControlPoint1, LeonardoTokens.Motion.EaseMoveControlPoint2),
        MotionEasing.Fade => (LeonardoTokens.Motion.EaseFadeControlPoint1, LeonardoTokens.Motion.EaseFadeControlPoint2),
        _ => throw new ArgumentOutOfRangeException(nameof(easing), easing, null),
    };

    /// <summary>
    /// Creates a Composition easing function, for example for <c>Vector3KeyFrameAnimation.InsertKeyFrame</c>.
    /// </summary>
    /// <param name="compositor">The compositor of the visual to animate.</param>
    /// <param name="easing">The curve.</param>
    public static CubicBezierEasingFunction Create(Compositor compositor, MotionEasing easing)
    {
        ArgumentNullException.ThrowIfNull(compositor);
        var (controlPoint1, controlPoint2) = GetControlPoints(easing);
        return compositor.CreateCubicBezierEasingFunction(controlPoint1, controlPoint2);
    }

    /// <summary>
    /// Creates a <see cref="KeySpline"/> for a <c>SplineDoubleKeyFrame</c> or another spline key frame of a Storyboard.
    /// </summary>
    public static KeySpline CreateKeySpline(MotionEasing easing)
    {
        var (controlPoint1, controlPoint2) = GetControlPoints(easing);
        return new KeySpline
        {
            ControlPoint1 = new Windows.Foundation.Point(controlPoint1.X, controlPoint1.Y),
            ControlPoint2 = new Windows.Foundation.Point(controlPoint2.X, controlPoint2.Y),
        };
    }
}
