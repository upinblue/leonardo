using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace UpInBlue.Leonardo.WinUI.Motion;

/// <summary>
/// Dive in: I go one level deeper. The chosen element flies to its new place and becomes the place name, with the
/// enter curve in <c>motion.duration.long</c>; the rest of the page staggers in from the right. Going back reverses the
/// motion exactly: the place name flies back into the chosen element and the rest staggers in from the left.
/// With animations off, the place name and the rest crossfade in.
/// </summary>
/// <remarks>
/// Built on <c>ConnectedAnimationService</c> with the basic configuration, so that duration and curve come from the
/// tokens. Navigate with <c>SuppressNavigationTransitionInfo</c> so that the platform's page transition does not run as well.
/// <list type="number">
/// <item>Source page, before navigating forward: <see cref="Prepare(ListViewBase, object, string)"/> or <see cref="Prepare(UIElement)"/>.</item>
/// <item>Destination page, in <c>OnNavigatedTo</c>: <see cref="Start"/> with the place name and the rest of the page.</item>
/// <item>Destination page, in <c>OnNavigatingFrom</c> when going back: <see cref="PrepareBack"/> with the place name.</item>
/// <item>Source page, in <c>OnNavigatedTo</c> when coming back: <see cref="StartBackAsync"/> with the chosen list item.</item>
/// </list>
/// </remarks>
public static class DiveIn
{
    /// <summary>The key of the connected animation.</summary>
    public const string AnimationKey = "LeonardoDiveIn";

    /// <summary>
    /// Prepares the dive into a list item: the named element of its template, for example its name, will fly.
    /// </summary>
    public static void Prepare(ListViewBase list, object item, string elementName)
    {
        ArgumentNullException.ThrowIfNull(list);
        ConnectedMotion.Prepare(AnimationKey, list, item, elementName, MotionEasing.Enter, MotionTiming.Get(MotionDuration.Long));
    }

    /// <summary>
    /// Prepares the dive from any chosen element.
    /// </summary>
    public static void Prepare(UIElement chosen)
    {
        ArgumentNullException.ThrowIfNull(chosen);
        ConnectedMotion.Prepare(AnimationKey, chosen, MotionEasing.Enter, MotionTiming.Get(MotionDuration.Long));
    }

    /// <summary>
    /// Prepares the way back from the place name of the page that is left.
    /// </summary>
    public static void PrepareBack(UIElement placeName) => Prepare(placeName);

    /// <summary>
    /// Completes the dive on the destination: the chosen element becomes the place name, the rest staggers in from the right.
    /// Without a prepared animation, the page enters with Stagger alone.
    /// </summary>
    /// <param name="placeName">The place name of the destination.</param>
    /// <param name="rest">The other groups of the page in reading order, at most three.</param>
    public static MotionRun Start(UIElement placeName, IEnumerable<UIElement?> rest)
    {
        ArgumentNullException.ThrowIfNull(placeName);
        ArgumentNullException.ThrowIfNull(rest);
        return Complete(ConnectedMotion.TryStart(AnimationKey, placeName), placeName, rest, MotionDirection.Forward);
    }

    /// <summary>
    /// Completes the way back on the source page: the place name flies back into the chosen list item, the rest staggers in from the left.
    /// </summary>
    /// <param name="list">The list that holds the chosen item.</param>
    /// <param name="item">The item that was chosen when diving in.</param>
    /// <param name="elementName">The named element of the item template that flew.</param>
    /// <param name="rest">The other groups of the page in reading order, including the place name.</param>
    public static async Task<MotionRun> StartBackAsync(ListViewBase list, object item, string elementName, IEnumerable<UIElement?> rest)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(rest);
        list.ScrollIntoView(item);
        var animation = await ConnectedMotion.TryStartAsync(AnimationKey, list, item, elementName);
        return Complete(animation, null, rest, MotionDirection.Back);
    }

    /// <summary>
    /// Cancels a prepared dive, for example when the navigation was cancelled.
    /// </summary>
    public static void Cancel() => ConnectedMotion.Cancel(AnimationKey);

    private static MotionRun Complete(Microsoft.UI.Xaml.Media.Animation.ConnectedAnimation? animation, UIElement? placeName, IEnumerable<UIElement?> rest, MotionDirection direction)
    {
        var groups = rest.ToList();
        if (animation is null)
        {
            // No connected element: the whole page enters as one staggered view, or crossfades with animations off.
            return Stagger.Enter(placeName is null ? groups : groups.Prepend(placeName), direction);
        }

        return new CompositionMotion()
            .Connected(animation)
            .Include(Stagger.Enter(groups, direction, firstGroup: 1))
            .Start();
    }
}
