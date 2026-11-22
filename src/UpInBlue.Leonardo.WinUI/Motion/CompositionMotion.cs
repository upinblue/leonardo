using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media.Animation;

namespace UpInBlue.Leonardo.WinUI.Motion;

/// <summary>
/// Builds one <see cref="MotionRun"/> from Composition animations of translation and opacity on XAML elements.
/// Translation is used instead of Offset because XAML layout owns Offset.
/// </summary>
internal sealed class CompositionMotion
{
    private readonly MotionRun _run = new();
    private CompositionScopedBatch? _batch;

    /// <summary>
    /// Moves an element from an offset to another one, relative to its layout position.
    /// </summary>
    public CompositionMotion Translate(UIElement element, Vector3 from, Vector3 to, MotionEasing easing, TimeSpan duration, TimeSpan delay = default)
    {
        var visual = Prepare(element);
        ElementCompositionPreview.SetIsTranslationEnabled(element, true);
        var compositor = visual.Compositor;
        var animation = compositor.CreateVector3KeyFrameAnimation();
        animation.InsertKeyFrame(0f, from);
        animation.InsertKeyFrame(1f, to, MotionCurves.Create(compositor, easing));
        animation.Duration = AtLeastOneMillisecond(duration);
        animation.DelayTime = delay;
        animation.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
        animation.Target = "Translation";
        visual.Properties.InsertVector3("Translation", from);
        visual.StartAnimation("Translation", animation);
        _run.OnSkip(() =>
        {
            visual.StopAnimation("Translation");
            visual.Properties.InsertVector3("Translation", to);
        });
        return this;
    }

    /// <summary>
    /// Fades an element linearly, as opacity always is.
    /// </summary>
    public CompositionMotion Fade(UIElement element, float from, float to, TimeSpan duration, TimeSpan delay = default)
    {
        var visual = Prepare(element);
        var compositor = visual.Compositor;
        var animation = compositor.CreateScalarKeyFrameAnimation();
        animation.InsertKeyFrame(0f, from);
        animation.InsertKeyFrame(1f, to, MotionCurves.Create(compositor, MotionEasing.Fade));
        animation.Duration = AtLeastOneMillisecond(duration);
        animation.DelayTime = delay;
        animation.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
        visual.Opacity = from;
        visual.StartAnimation("Opacity", animation);
        _run.OnSkip(() =>
        {
            visual.StopAnimation("Opacity");
            visual.Opacity = to;
        });
        return this;
    }

    /// <summary>
    /// Adds a Storyboard to the run, for properties that XAML owns, such as the opacity of a marker in a template.
    /// </summary>
    public CompositionMotion Storyboard(Storyboard storyboard)
    {
        _run.AddPart();
        storyboard.Completed += (_, _) => _run.PartCompleted();
        _run.OnSkip(storyboard.SkipToFill);
        storyboard.Begin();
        return this;
    }

    /// <summary>
    /// Adds a connected animation to the run. Skipping cancels it, which shows the destination element in place.
    /// </summary>
    public CompositionMotion Connected(ConnectedAnimation animation)
    {
        _run.AddPart();
        animation.Completed += (_, _) => _run.PartCompleted();
        _run.OnSkip(animation.Cancel);
        return this;
    }

    /// <summary>
    /// Makes another run part of this one: skipping this run skips the other, and this run ends when the other has.
    /// </summary>
    public CompositionMotion Include(MotionRun other)
    {
        if (other.IsCompleted)
        {
            return this;
        }

        _run.AddPart();
        _run.OnSkip(other.SkipToEnd);
        _ = CompleteWhenDoneAsync(other);
        return this;
    }

    /// <summary>
    /// Claims an element that the run changes without animating it, so that a motion still running on it ends first.
    /// </summary>
    public CompositionMotion Claim(UIElement element)
    {
        _run.Claim(element);
        return this;
    }

    /// <summary>
    /// Starts nothing more; the run completes when all animations have.
    /// </summary>
    public MotionRun Start()
    {
        if (_batch is not null)
        {
            _batch.End();
        }

        return _run.Seal();
    }

    /// <summary>
    /// Sets an element back to its resting state: no translation, fully opaque, no running animation.
    /// </summary>
    public static void Reset(UIElement element)
    {
        // "Translation" exists on the visual only once translation is enabled for the element.
        ElementCompositionPreview.SetIsTranslationEnabled(element, true);
        var visual = ElementCompositionPreview.GetElementVisual(element);
        visual.StopAnimation("Opacity");
        visual.StopAnimation("Translation");
        visual.Opacity = 1f;
        visual.Properties.InsertVector3("Translation", Vector3.Zero);
    }

    private Visual Prepare(UIElement element)
    {
        _run.Claim(element);
        var visual = ElementCompositionPreview.GetElementVisual(element);
        if (_batch is null)
        {
            _batch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            _run.AddPart();
            _batch.Completed += (_, _) => _run.PartCompleted();
        }

        return visual;
    }

    private async Task CompleteWhenDoneAsync(MotionRun other)
    {
        // Resumes on the UI thread, where the run's state lives.
        await other.Completion;
        _run.PartCompleted();
    }

    private static TimeSpan AtLeastOneMillisecond(TimeSpan duration)
        => duration < TimeSpan.FromMilliseconds(1) ? TimeSpan.FromMilliseconds(1) : duration;
}
