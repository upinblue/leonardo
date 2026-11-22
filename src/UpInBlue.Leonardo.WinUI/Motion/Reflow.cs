using System.Numerics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace UpInBlue.Leonardo.WinUI.Motion;

/// <summary>
/// Reflow: the order has changed. Items glide from their old place to their new one with the move curve, in
/// <c>motion.duration.normal</c>, or <c>motion.duration.long</c> when they travel further than <c>motion.distance.enterMax</c>.
/// With animations off, the moved items crossfade in their new place.
/// </summary>
/// <remarks>
/// Capture before changing the collection, play after it:
/// <code>
/// var reflow = Reflow.Capture(CustomerList);
/// viewModel.Sort();
/// reflow.Play();
/// </code>
/// Items are matched by their data item, so this also works when the list recreates containers, as
/// <see cref="ListView"/> does for a move in an <c>ObservableCollection</c>. Only realized containers move.
/// Turn off the list's own item container transitions so that the platform does not animate the same change a second time.
/// </remarks>
public static class Reflow
{
    /// <summary>
    /// Records where the items of one or more lists are now.
    /// </summary>
    public static ReflowSnapshot Capture(params ItemsControl[] lists)
    {
        ArgumentNullException.ThrowIfNull(lists);

        // Running motion ends first: a new change starts from the end state of the previous one.
        MotionRun.SkipAllToEnd();
        var entries = new List<ReflowSnapshot.Entry>();
        foreach (var list in lists)
        {
            for (var index = 0; index < list.Items.Count; index++)
            {
                if (list.ContainerFromIndex(index) is not UIElement container)
                {
                    continue;
                }

                var position = container.TransformToVisual(list).TransformPoint(default);
                entries.Add(new ReflowSnapshot.Entry(list, list.Items[index], position));
            }
        }

        return new ReflowSnapshot(entries);
    }
}

/// <summary>
/// The positions of list items before a change. See <see cref="Reflow"/>.
/// </summary>
public sealed class ReflowSnapshot
{
    private readonly IReadOnlyList<Entry> _entries;

    internal ReflowSnapshot(IReadOnlyList<Entry> entries) => _entries = entries;

    /// <summary>
    /// Lets the items glide from their recorded place to their new one. Call after the collection has changed.
    /// </summary>
    public MotionRun Play()
    {
        foreach (var list in _entries.Select(entry => entry.List).Distinct())
        {
            list.UpdateLayout();
        }

        var motion = new CompositionMotion();
        var animate = MotionSettings.AnimationsEnabled;
        foreach (var entry in _entries)
        {
            if (entry.List.ContainerFromItem(entry.Item) is not UIElement container)
            {
                continue;
            }

            var position = container.TransformToVisual(entry.List).TransformPoint(default);
            if (MotionMath.ReflowOffset(entry.Position.X, entry.Position.Y, position.X, position.Y) is not { } offset)
            {
                continue;
            }

            if (!animate)
            {
                motion.Fade(container, 0f, 1f, MotionTiming.Get(MotionDuration.Fade));
                continue;
            }

            var from = new Vector3((float)offset.X, (float)offset.Y, 0f);
            motion.Translate(container, from, Vector3.Zero, MotionEasing.Move, MotionTiming.Move(MotionMath.Length(offset.X, offset.Y)));
        }

        return motion.Start();
    }

    internal sealed record Entry(ItemsControl List, object Item, Windows.Foundation.Point Position);
}
