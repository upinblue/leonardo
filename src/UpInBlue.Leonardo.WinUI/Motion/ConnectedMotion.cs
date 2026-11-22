using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media.Animation;

namespace UpInBlue.Leonardo.WinUI.Motion;

/// <summary>
/// Connected animations with Leonardo timing. The basic configuration is the only one that uses the service's
/// default duration and easing, so both can come from the tokens; the gravity configuration adds a dip with its own
/// curve, and the direct configuration has a fixed duration and curve.
/// </summary>
internal static class ConnectedMotion
{
    private static ConnectedAnimationService Service => ConnectedAnimationService.GetForCurrentView();

    /// <summary>
    /// Prepares a connected animation from a source element, or cancels a stale one when animations are off.
    /// </summary>
    public static void Prepare(string key, UIElement source, MotionEasing easing, TimeSpan duration)
    {
        if (!MotionSettings.AnimationsEnabled)
        {
            Cancel(key);
            return;
        }

        Configure(source, easing, duration);
        Service.PrepareToAnimate(key, source).Configuration = new BasicConnectedAnimationConfiguration();
    }

    /// <summary>
    /// Prepares a connected animation from an element in a list item.
    /// </summary>
    public static void Prepare(string key, ListViewBase list, object item, string elementName, MotionEasing easing, TimeSpan duration)
    {
        if (!MotionSettings.AnimationsEnabled)
        {
            Cancel(key);
            return;
        }

        Configure(list, easing, duration);
        list.PrepareConnectedAnimation(key, item, elementName).Configuration = new BasicConnectedAnimationConfiguration();
    }

    /// <summary>
    /// Starts a prepared animation on the destination element. Returns null when there is none or it could not start.
    /// </summary>
    public static ConnectedAnimation? TryStart(string key, UIElement destination)
    {
        var animation = Service.GetAnimation(key);
        if (animation is null)
        {
            return null;
        }

        if (!MotionSettings.AnimationsEnabled)
        {
            animation.Cancel();
            return null;
        }

        return animation.TryStart(destination) ? animation : null;
    }

    /// <summary>
    /// Starts a prepared animation on an element in a list item, waiting for the item container if necessary.
    /// </summary>
    public static async Task<ConnectedAnimation?> TryStartAsync(string key, ListViewBase list, object item, string elementName)
    {
        var animation = Service.GetAnimation(key);
        if (animation is null)
        {
            return null;
        }

        if (!MotionSettings.AnimationsEnabled)
        {
            animation.Cancel();
            return null;
        }

        return await list.TryStartConnectedAnimationAsync(animation, item, elementName) ? animation : null;
    }

    /// <summary>
    /// Cancels a prepared animation, if any.
    /// </summary>
    public static void Cancel(string key) => Service.GetAnimation(key)?.Cancel();

    private static void Configure(UIElement element, MotionEasing easing, TimeSpan duration)
    {
        var compositor = ElementCompositionPreview.GetElementVisual(element).Compositor;
        Service.DefaultDuration = duration;
        Service.DefaultEasingFunction = MotionCurves.Create(compositor, easing);
    }
}
