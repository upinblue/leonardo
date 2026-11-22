using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace UpInBlue.Leonardo.Wpf;

/// <summary>
/// Leonardo motion for WPF: the system animation setting and the stagger. Every motion becomes a short crossfade when
/// animation effects are off in Windows. The meaning stays, the distance goes.
/// </summary>
/// <remarks>
/// The setting is read from <see cref="SystemParameters.ClientAreaAnimation"/>, which is the Windows setting
/// "Animation effects" (Settings, Accessibility, Visual effects; <c>SPI_GETCLIENTAREAANIMATION</c>). WPF raises
/// <see cref="SystemParameters.StaticPropertyChanged"/> when it changes, so a running app follows the setting.
/// Call the members on the UI thread.
/// </remarks>
public static class LeonardoMotion
{
    private static bool? animationsOverride;

    static LeonardoMotion()
    {
        SystemParameters.StaticPropertyChanged += OnSystemParameterChanged;
    }

    /// <summary>
    /// Occurs when <see cref="AnimationsEnabled"/> changes, because the system setting or <see cref="AnimationsOverride"/> changed.
    /// </summary>
    public static event EventHandler? AnimationsEnabledChanged;

    /// <summary>
    /// Gets a value indicating whether motion may move elements. When <see langword="false"/>, every motion is a short crossfade.
    /// </summary>
    public static bool AnimationsEnabled => animationsOverride ?? SystemParameters.ClientAreaAnimation;

    /// <summary>
    /// Gets or sets a value that replaces the system animation setting, for example to test both paths.
    /// <see langword="null"/>, the default, follows the system setting. Never use it to turn animations on against the user's choice.
    /// </summary>
    public static bool? AnimationsOverride
    {
        get => animationsOverride;
        set
        {
            if (animationsOverride == value)
            {
                return;
            }

            var before = AnimationsEnabled;
            animationsOverride = value;
            if (before != AnimationsEnabled)
            {
                AnimationsEnabledChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Stagger: the groups of a view enter shortly after each other, in reading order. Each group fades in linearly and
    /// glides in from the right with <c>motion.easing.enter</c>, offset by <c>motion.stagger.step</c> per group.
    /// With animation effects off, all groups crossfade at once.
    /// </summary>
    /// <param name="groups">
    /// The groups in reading order. Leonardo allows at most <see cref="LeonardoTokens.Motion.StaggerMaxGroups"/> groups;
    /// further groups enter together with the last allowed one.
    /// </param>
    /// <remarks>
    /// The motion never blocks: the elements stay interactive while they enter. A group moves only when its
    /// <see cref="UIElement.RenderTransform"/> is unset or a <see cref="TranslateTransform"/>; otherwise it only fades.
    /// </remarks>
    public static void Stagger(IEnumerable<UIElement> groups)
    {
        ArgumentNullException.ThrowIfNull(groups);

        var animate = AnimationsEnabled;
        var index = 0;
        foreach (var group in groups)
        {
            var step = Math.Min(index, LeonardoTokens.Motion.StaggerMaxGroups - 1);
            var begin = animate ? LeonardoTokens.Motion.StaggerStep * step : TimeSpan.Zero;
            Enter(group, begin, animate);
            index++;
        }
    }

    /// <summary>
    /// A key frame animation with one spline segment from <paramref name="from"/> to <paramref name="to"/>.
    /// </summary>
    /// <param name="from">The start value, held until <paramref name="begin"/>.</param>
    /// <param name="to">The end value.</param>
    /// <param name="begin">The delay before the motion starts.</param>
    /// <param name="duration">The duration of the motion, for example <see cref="LeonardoTokens.Motion.DurationNormal"/>.</param>
    /// <param name="controlPoint1">The first control point of the easing curve, for example <see cref="LeonardoTokens.Motion.EaseEnterControlPoint1"/>.</param>
    /// <param name="controlPoint2">The second control point of the easing curve, for example <see cref="LeonardoTokens.Motion.EaseEnterControlPoint2"/>.</param>
    /// <returns>A frozen animation.</returns>
    public static DoubleAnimationUsingKeyFrames Spline(double from, double to, TimeSpan begin, TimeSpan duration, Point controlPoint1, Point controlPoint2)
    {
        // The start value is a key frame instead of a local value, so the element never shows its end state before it begins.
        var animation = new DoubleAnimationUsingKeyFrames { FillBehavior = FillBehavior.Stop };
        animation.KeyFrames.Add(new DiscreteDoubleKeyFrame(from, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        if (begin > TimeSpan.Zero)
        {
            animation.KeyFrames.Add(new DiscreteDoubleKeyFrame(from, KeyTime.FromTimeSpan(begin)));
        }

        animation.KeyFrames.Add(new SplineDoubleKeyFrame(to, KeyTime.FromTimeSpan(begin + duration), new KeySpline(controlPoint1, controlPoint2)));
        animation.Freeze();
        return animation;
    }

    private static void Enter(UIElement element, TimeSpan begin, bool move)
    {
        var fade = Spline(
            0,
            1,
            begin,
            LeonardoTokens.Motion.DurationFade,
            LeonardoTokens.Motion.EaseFadeControlPoint1,
            LeonardoTokens.Motion.EaseFadeControlPoint2);
        element.BeginAnimation(UIElement.OpacityProperty, fade, HandoffBehavior.SnapshotAndReplace);

        if (!move || TranslationOf(element) is not { } translation)
        {
            return;
        }

        var glide = Spline(
            LeonardoTokens.Motion.DistanceEnterMin,
            0,
            begin,
            LeonardoTokens.Motion.DurationNormal,
            LeonardoTokens.Motion.EaseEnterControlPoint1,
            LeonardoTokens.Motion.EaseEnterControlPoint2);
        translation.BeginAnimation(TranslateTransform.XProperty, glide, HandoffBehavior.SnapshotAndReplace);
    }

    private static TranslateTransform? TranslationOf(UIElement element)
    {
        switch (element.RenderTransform)
        {
            case TranslateTransform { IsFrozen: false } translation:
                return translation;
            case null:
            case Transform when element.RenderTransform == Transform.Identity:
                var created = new TranslateTransform();
                element.RenderTransform = created;
                return created;
            default:
                return null;
        }
    }

    private static void OnSystemParameterChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SystemParameters.ClientAreaAnimation) && animationsOverride is null)
        {
            AnimationsEnabledChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
