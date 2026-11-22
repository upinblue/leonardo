using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using UpInBlue.Leonardo.WinUI.Motion;

namespace UpInBlue.Leonardo.WinUI.Controls;

/// <summary>
/// The place name of a view: a meta line, the title large and light like a station sign, and a slot for a
/// <see cref="SummaryLine"/>. The title is a level 1 heading for assistive technology.
/// </summary>
/// <remarks>
/// <para>
/// Place the header above the scrolling content and set <see cref="ScrollSource"/> to the <see cref="ScrollViewer"/>,
/// or to an element that contains one, such as a <see cref="ListView"/>. While the content scrolls, the title moves up
/// at <c>motion.parallax.placeNameFactor</c> of the content speed; further down, the header shrinks to a compact line
/// and the content keeps its place on screen. Scrolling back to the top expands it again.
/// </para>
/// <para>
/// With animations off, there is no parallax and the header shrinks and expands without animation.
/// </para>
/// </remarks>
[TemplatePart(Name = "PART_Root", Type = typeof(Grid))]
[TemplatePart(Name = "PART_Meta", Type = typeof(TextBlock))]
[TemplatePart(Name = "PART_Title", Type = typeof(TextBlock))]
[TemplatePart(Name = "PART_CompactTitle", Type = typeof(TextBlock))]
public sealed partial class PlaceHeader : ContentControl
{
    /// <summary>Identifies the <see cref="Title"/> property.</summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(PlaceHeader), new PropertyMetadata(string.Empty, (sender, _) => ((PlaceHeader)sender).UpdateTexts()));

    /// <summary>Identifies the <see cref="Meta"/> property.</summary>
    public static readonly DependencyProperty MetaProperty = DependencyProperty.Register(
        nameof(Meta), typeof(string), typeof(PlaceHeader), new PropertyMetadata(string.Empty, (sender, _) => ((PlaceHeader)sender).UpdateTexts()));

    /// <summary>Identifies the <see cref="ScrollSource"/> property.</summary>
    public static readonly DependencyProperty ScrollSourceProperty = DependencyProperty.Register(
        nameof(ScrollSource), typeof(UIElement), typeof(PlaceHeader), new PropertyMetadata(null, (sender, _) => ((PlaceHeader)sender).Attach()));

    /// <summary>Identifies the <see cref="IsCompact"/> property.</summary>
    public static readonly DependencyProperty IsCompactProperty = DependencyProperty.Register(
        nameof(IsCompact), typeof(bool), typeof(PlaceHeader), new PropertyMetadata(false));

    private Grid? _root;
    private TextBlock? _meta;
    private TextBlock? _title;
    private TextBlock? _compactTitle;
    private ScrollViewer? _scrollViewer;
    private ExpressionAnimation? _parallax;
    private double _threshold;
    private double _shrinkDistance;

    /// <summary>
    /// Creates a place header.
    /// </summary>
    public PlaceHeader()
    {
        DefaultStyleKey = typeof(PlaceHeader);
        DefaultStyleResourceUri = LeonardoControlResources.GenericUri;
        Loaded += (_, _) =>
        {
            MotionSettings.Changed += OnMotionChanged;
            Attach();
        };
        Unloaded += (_, _) =>
        {
            MotionSettings.Changed -= OnMotionChanged;
            Detach();
        };
        SizeChanged += (_, _) => UpdateShrinkDistance();
    }

    /// <summary>The place name, for example the name of the open folder.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The meta line above the title, for example "Customers / Hofmann Logistics". Empty hides it.</summary>
    public string Meta
    {
        get => (string)GetValue(MetaProperty);
        set => SetValue(MetaProperty, value);
    }

    /// <summary>
    /// The scrolling content below the header: a <see cref="ScrollViewer"/> or an element that contains one.
    /// Null keeps the header large.
    /// </summary>
    public UIElement? ScrollSource
    {
        get => (UIElement?)GetValue(ScrollSourceProperty);
        set => SetValue(ScrollSourceProperty, value);
    }

    /// <summary>Whether the header has shrunk to its compact line.</summary>
    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        private set => SetValue(IsCompactProperty, value);
    }

    /// <summary>
    /// The element that shows the title now, for connected animations such as <see cref="DiveIn"/>.
    /// </summary>
    public UIElement? TitleElement => IsCompact ? _compactTitle : _title;

    /// <inheritdoc />
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _root = GetTemplateChild("PART_Root") as Grid;
        _meta = GetTemplateChild("PART_Meta") as TextBlock;
        _title = GetTemplateChild("PART_Title") as TextBlock;
        _compactTitle = GetTemplateChild("PART_CompactTitle") as TextBlock;

        if (_root is not null)
        {
            // The title moves up inside the header; it never draws outside it.
            var visual = ElementCompositionPreview.GetElementVisual(_root);
            visual.Clip = visual.Compositor.CreateInsetClip();
        }

        UpdateTexts();
        ApplyCompact(IsCompact, animate: false);
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer() => new PlaceHeaderAutomationPeer(this);

    private void UpdateTexts()
    {
        if (_meta is not null)
        {
            _meta.Visibility = string.IsNullOrWhiteSpace(Meta) || IsCompact ? Visibility.Collapsed : Visibility.Visible;
        }

        UpdateShrinkDistance();
    }

    private void Attach()
    {
        Detach();
        if (!IsLoaded || ScrollSource is not { } source)
        {
            return;
        }

        _scrollViewer = source as ScrollViewer ?? FindScrollViewer(source);
        if (_scrollViewer is null)
        {
            // A list creates its scroll viewer with its template; try again once it is loaded.
            if (source is FrameworkElement { IsLoaded: false } element)
            {
                element.Loaded += OnSourceLoaded;
            }

            return;
        }

        _scrollViewer.ViewChanged += OnViewChanged;
        UpdateShrinkDistance();
        StartParallax();
    }

    private void OnSourceLoaded(object sender, RoutedEventArgs e)
    {
        ((FrameworkElement)sender).Loaded -= OnSourceLoaded;
        Attach();
    }

    private void Detach()
    {
        StopParallax();
        if (_scrollViewer is not null)
        {
            _scrollViewer.ViewChanged -= OnViewChanged;
            _scrollViewer = null;
        }
    }

    private void OnMotionChanged(object? sender, EventArgs e)
    {
        StopParallax();
        StartParallax();
    }

    /// <summary>
    /// Measures how much height the header gives up when it shrinks, and the scroll offset at which it does.
    /// </summary>
    private void UpdateShrinkDistance()
    {
        if (_title is null || _compactTitle is null || IsCompact)
        {
            return;
        }

        var meta = _meta is { Visibility: Visibility.Visible } ? _meta.ActualHeight : 0;
        _shrinkDistance = PlaceHeaderMath.ShrinkDistance(meta + _title.ActualHeight, _compactTitle.ActualHeight);
        _threshold = PlaceHeaderMath.Threshold(_shrinkDistance, LeonardoTokens.Space.S);
        _parallax?.SetScalarParameter("threshold", (float)_threshold);
    }

    private void OnViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
    {
        if (_scrollViewer is null)
        {
            return;
        }

        var offset = _scrollViewer.VerticalOffset;
        var compact = PlaceHeaderMath.NextIsCompact(IsCompact, offset, _threshold);
        if (compact == IsCompact)
        {
            return;
        }

        if (compact)
        {
            var distance = _shrinkDistance;
            ApplyCompact(true, MotionSettings.AnimationsEnabled);

            // The header gave up height; scroll back by the same amount so the content stays where it is.
            _scrollViewer.ChangeView(null, PlaceHeaderMath.CompensatedOffset(offset, distance), null, disableAnimation: true);
        }
        else
        {
            ApplyCompact(false, MotionSettings.AnimationsEnabled);
            if (MotionSettings.AnimationsEnabled && ScrollSource is { } source)
            {
                // Expanding pushes the content down: it glides there instead of jumping (Reflow).
                new CompositionMotion()
                    .Translate(source, new Vector3(0f, (float)-_shrinkDistance, 0f), Vector3.Zero, MotionEasing.Move, MotionTiming.Get(MotionDuration.Normal))
                    .Start();
            }
        }
    }

    private void ApplyCompact(bool compact, bool animate)
    {
        IsCompact = compact;
        if (_title is null || _compactTitle is null)
        {
            return;
        }

        if (compact)
        {
            StopParallax();
        }

        _title.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        _compactTitle.Opacity = compact ? 1 : 0;
        _compactTitle.IsHitTestVisible = compact;

        // Exactly one of the two titles is the level 1 heading.
        AutomationProperties.SetHeadingLevel(_title, compact ? AutomationHeadingLevel.None : AutomationHeadingLevel.Level1);
        AutomationProperties.SetHeadingLevel(_compactTitle, compact ? AutomationHeadingLevel.Level1 : AutomationHeadingLevel.None);
        AutomationProperties.SetAccessibilityView(_compactTitle, compact ? AccessibilityView.Content : AccessibilityView.Raw);
        UpdateTexts();

        if (animate)
        {
            // The title shrinks into the compact line, or grows back into the place name (Transform).
            var large = (float)_title.FontSize;
            var small = (float)_compactTitle.FontSize;
            var fade = MotionTiming.Get(MotionDuration.Fade);
            var move = MotionTiming.Get(MotionDuration.Normal);
            var motion = new CompositionMotion();
            if (compact)
            {
                motion.Scale(_compactTitle, large / small, 1f, MotionEasing.Move, move).Fade(_compactTitle, 0f, 1f, fade);
            }
            else
            {
                motion.Scale(_title, small / large, 1f, MotionEasing.Move, move).Fade(_title, 0f, 1f, fade);
            }

            motion.Start();
        }

        if (!compact)
        {
            StartParallax();
        }
    }

    /// <summary>
    /// The only depth effect of Leonardo: while the content scrolls, the title moves up more slowly than the content.
    /// Driven by the scroll viewer on the compositor, so it follows touch and wheel without delay.
    /// </summary>
    private void StartParallax()
    {
        if (_parallax is not null || _title is null || _scrollViewer is null || IsCompact || !MotionSettings.AnimationsEnabled)
        {
            return;
        }

        var scroll = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(_scrollViewer);
        ElementCompositionPreview.SetIsTranslationEnabled(_title, true);
        var visual = ElementCompositionPreview.GetElementVisual(_title);
        _parallax = visual.Compositor.CreateExpressionAnimation("-Clamp(-scroll.Translation.Y, 0, threshold) * factor");
        _parallax.SetReferenceParameter("scroll", scroll);
        _parallax.SetScalarParameter("threshold", (float)_threshold);
        _parallax.SetScalarParameter("factor", (float)LeonardoTokens.Motion.ParallaxPlaceNameFactor);
        visual.StartAnimation("Translation.Y", _parallax);
    }

    private void StopParallax()
    {
        if (_parallax is null || _title is null)
        {
            return;
        }

        var visual = ElementCompositionPreview.GetElementVisual(_title);
        visual.StopAnimation("Translation.Y");
        visual.Properties.InsertVector3("Translation", Vector3.Zero);
        _parallax = null;
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject root)
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < count; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is ScrollViewer viewer)
            {
                return viewer;
            }

            if (FindScrollViewer(child) is { } found)
            {
                return found;
            }
        }

        return null;
    }
}

/// <summary>
/// Exposes a <see cref="PlaceHeader"/> to UI Automation as a group named after the place. Its title is the
/// level 1 heading of the view.
/// </summary>
public sealed partial class PlaceHeaderAutomationPeer : FrameworkElementAutomationPeer
{
    /// <summary>
    /// Creates the peer.
    /// </summary>
    public PlaceHeaderAutomationPeer(PlaceHeader owner)
        : base(owner)
    {
    }

    /// <inheritdoc />
    protected override string GetNameCore()
    {
        var name = base.GetNameCore();
        return string.IsNullOrEmpty(name) ? ((PlaceHeader)Owner).Title : name;
    }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Group;

    /// <inheritdoc />
    protected override string GetClassNameCore() => nameof(PlaceHeader);
}
