using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;

namespace UpInBlue.Leonardo.WinUI.Motion;

/// <summary>
/// A running motion. Every motion can be interrupted: <see cref="SkipToEnd"/> jumps to the end state at once.
/// </summary>
public sealed class MotionRun
{
    private static readonly HashSet<MotionRun> Active = [];
    private static readonly ConditionalWeakTable<UIElement, MotionRun> ByElement = [];

    private readonly List<Action> _finishers = [];
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _pendingParts;
    private bool _sealed;

    internal MotionRun()
    {
    }

    /// <summary>
    /// A motion that has already ended, for example because there was nothing to animate.
    /// </summary>
    public static MotionRun Completed
    {
        get
        {
            var run = new MotionRun();
            run.Complete();
            return run;
        }
    }

    /// <summary>Whether the motion has ended.</summary>
    public bool IsCompleted => _completion.Task.IsCompleted;

    /// <summary>Completes when the motion has ended, by itself or through <see cref="SkipToEnd"/>.</summary>
    public Task Completion => _completion.Task;

    /// <summary>
    /// Jumps to the end state: every animated property gets its final value at once.
    /// </summary>
    public void SkipToEnd()
    {
        if (IsCompleted)
        {
            return;
        }

        foreach (var finish in _finishers.ToArray())
        {
            finish();
        }

        Complete();
    }

    /// <summary>
    /// Ends every running motion. Used by <see cref="MotionInterruption"/>.
    /// </summary>
    internal static void SkipAllToEnd()
    {
        foreach (var run in Active.ToArray())
        {
            run.SkipToEnd();
        }
    }

    /// <summary>
    /// Takes an element over from a motion still running on it: that motion jumps to its end first.
    /// </summary>
    internal void Claim(UIElement element)
    {
        if (ByElement.TryGetValue(element, out var previous) && previous != this)
        {
            previous.SkipToEnd();
        }

        ByElement.AddOrUpdate(element, this);
        Active.Add(this);
    }

    /// <summary>Registers what to do when the motion is skipped: stop the animation and set the final value.</summary>
    internal void OnSkip(Action finish) => _finishers.Add(finish);

    /// <summary>Registers a part that completes asynchronously, for example a Composition batch or a Storyboard.</summary>
    internal void AddPart() => _pendingParts++;

    /// <summary>Marks a part as completed.</summary>
    internal void PartCompleted()
    {
        _pendingParts--;
        if (_sealed && _pendingParts <= 0)
        {
            Complete();
        }
    }

    /// <summary>All parts are registered; the run completes when they have.</summary>
    internal MotionRun Seal()
    {
        _sealed = true;
        if (_pendingParts <= 0)
        {
            Complete();
        }
        else
        {
            Active.Add(this);
        }

        return this;
    }

    private void Complete()
    {
        Active.Remove(this);
        _completion.TrySetResult();
    }
}
