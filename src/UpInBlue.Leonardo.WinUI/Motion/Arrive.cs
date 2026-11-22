using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Media.Animation;

namespace UpInBlue.Leonardo.WinUI.Motion;

/// <summary>
/// Arrive: something new is here. The new item appears in its place, its opacity rising with <c>motion.duration.fade</c>.
/// A short accent marker appears with it, stays for <c>motion.arrive.hold</c> and fades away linearly within
/// <c>motion.duration.long</c>. Screen readers hear the announcement as a notification.
/// With animations off, the marker appears at once and disappears with <c>motion.duration.fade</c>.
/// </summary>
public static class Arrive
{
    /// <summary>The activity ID of the notifications, so that a screen reader can group them.</summary>
    public const string NotificationActivityId = "UpInBlue.Leonardo.Arrive";

    /// <summary>
    /// Shows that an item has arrived.
    /// </summary>
    /// <param name="item">The new item, already in its place, for example the container of a new list item.</param>
    /// <param name="marker">
    /// The accent marker, an element of the item whose opacity is 0 at rest, for example an accent dot that hangs to the
    /// left of the alignment line. Null for no marker.
    /// </param>
    /// <param name="announcement">What a screen reader announces, for example the text of the new event. Null for no announcement.</param>
    public static MotionRun Run(UIElement item, UIElement? marker, string? announcement)
    {
        ArgumentNullException.ThrowIfNull(item);
        var animate = MotionSettings.AnimationsEnabled;
        var fade = MotionTiming.Get(MotionDuration.Fade);
        var motion = new CompositionMotion().Fade(item, 0f, 1f, fade);

        if (marker is not null)
        {
            motion.Claim(marker);
            var appear = animate ? fade : TimeSpan.Zero;
            var hold = appear + MotionTiming.ArriveHold;
            var end = hold + (animate ? MotionTiming.Get(MotionDuration.Long) : fade);
            var keyFrames = new DoubleAnimationUsingKeyFrames();
            keyFrames.KeyFrames.Add(new DiscreteDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = animate ? 0 : 1 });
            keyFrames.KeyFrames.Add(Linear(appear, 1));
            keyFrames.KeyFrames.Add(Linear(hold, 1));
            keyFrames.KeyFrames.Add(Linear(end, 0));
            Storyboard.SetTarget(keyFrames, marker);
            Storyboard.SetTargetProperty(keyFrames, nameof(UIElement.Opacity));
            var storyboard = new Storyboard();
            storyboard.Children.Add(keyFrames);
            motion.Storyboard(storyboard);
        }

        if (!string.IsNullOrWhiteSpace(announcement))
        {
            var peer = FrameworkElementAutomationPeer.FromElement(item) ?? FrameworkElementAutomationPeer.CreatePeerForElement(item);
            peer?.RaiseNotificationEvent(AutomationNotificationKind.ItemAdded, AutomationNotificationProcessing.All, announcement, NotificationActivityId);
        }

        return motion.Start();
    }

    private static SplineDoubleKeyFrame Linear(TimeSpan time, double value) => new()
    {
        KeyTime = KeyTime.FromTimeSpan(time),
        KeySpline = MotionCurves.CreateKeySpline(MotionEasing.Fade),
        Value = value,
    };
}
