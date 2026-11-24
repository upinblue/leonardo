using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using UpInBlue.Leonardo.WinUI.Motion;
using Windows.Foundation;

namespace UpInBlue.Leonardo.WinUI.Controls;

/// <summary>
/// The place name of a view: a meta line, the title large and light like a station sign, and a slot for a
/// <see cref="SummaryLine"/>. The title is a level 1 heading for assistive technology.
/// </summary>
/// <remarks>
/// <para>
/// Place the header above the scrolling content and set <see cref="ScrollSource"/> to the <see cref="ScrollViewer"/>,
/// or to an element that contains one, such as a <see cref="ListView"/>. The header then follows the scroll position:
/// while the content scrolls through the shrink distance, the meta line moves with the content and fades, and the
/// title moves up more slowly than the content and scales from the place name size to the compact size. There is no
/// timed animation; scrolling back reverses every step exactly.
/// </para>
/// <para>
/// To make this possible, the header reserves the shrink distance at the top of the scrolling content (as top margin
/// of the scroll viewer's content, or top padding of a list's items presenter) and draws its expanded form over that
/// gap. In layout, the header is always as high as its compact form, so the content never jumps.
/// </para>
/// <para>
/// With animations off, nothing scales or fades while scrolling: the header scrolls away like the content, and at the
/// end of the shrink distance the compact title crossfades in.
/// </para>
/// </remarks>
[TemplatePart(Name = "PART_Root", Type = typeof(Grid))]
[TemplatePart(Name = "PART_Meta", Type = typeof(TextBlock))]
[TemplatePart(Name = "PART_Title", Type = typeof(TextBlock))]
[TemplatePart(Name = "PART_CompactTitle", Type = typeof(TextBlock))]
[TemplatePart(Name = "PART_Content", Type = typeof(ContentPresenter))]
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

    // The scroll offset within the shrink distance, and the progress through it, as expression fragments.
    private const string Offset = "Clamp(-scroll.Translation.Y, 0, props.Distance)";
    private const string Progress = "Clamp(-scroll.Translation.Y / Max(props.Distance, 1), 0, 1)";

    private readonly List<(Visual Visual, string Property)> _tracking = [];
    private Grid? _root;
    private TextBlock? _meta;
    private TextBlock? _title;
    private TextBlock? _compactTitle;
    private ContentPresenter? _content;
    private ScrollViewer? _scrollViewer;
    private CompositionPropertySet? _scroll;
    private CompositionPropertySet? _props;
    private FrameworkElement? _insetTarget;
    private double _insetApplied;
    private double _distance;
    private double _metaHeight;

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

    /// <summary>The shrink distance while the header follows a scroll viewer; otherwise zero.</summary>
    private double ActiveDistance => _scrollViewer is null ? 0 : _distance;

    /// <inheritdoc />
    protected override void OnApplyTemplate()
    {
        StopTracking();
        base.OnApplyTemplate();
        _root = GetTemplateChild("PART_Root") as Grid;
        _meta = GetTemplateChild("PART_Meta") as TextBlock;
        _title = GetTemplateChild("PART_Title") as TextBlock;
        _compactTitle = GetTemplateChild("PART_CompactTitle") as TextBlock;
        _content = GetTemplateChild("PART_Content") as ContentPresenter;

        if (_root is not null)
        {
            // The meta line and the title move up inside the header; they never draw above it.
            var visual = ElementCompositionPreview.GetElementVisual(_root);
            visual.Clip = visual.Compositor.CreateInsetClip();
        }

        UpdateTexts();
        ApplyCompact(IsCompact, crossfade: false);
        StartTracking();
    }

    /// <summary>
    /// Measures the expanded header and reports the compact height: the expanded part draws over the gap that the
    /// header reserves at the top of the scrolling content.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);
        if (!IsCompact)
        {
            UpdateDistance();
        }

        return new Size(size.Width, Math.Max(0, size.Height - LayoutLift));
    }

    /// <summary>
    /// Arranges the template at its full height, which reaches below the header's own bounds while it is expanded.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        base.ArrangeOverride(new Size(finalSize.Width, finalSize.Height + LayoutLift));
        return finalSize;
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer() => new PlaceHeaderAutomationPeer(this);

    /// <summary>How much of the template reaches below the header's layout bounds.</summary>
    private double LayoutLift => IsCompact ? 0 : ActiveDistance;

    private void UpdateTexts()
    {
        if (_meta is not null)
        {
            _meta.Visibility = string.IsNullOrWhiteSpace(Meta) || IsCompact ? Visibility.Collapsed : Visibility.Visible;
        }

        InvalidateMeasure();
    }

    /// <summary>
    /// Takes the shrink distance from the measured parts, while the header is expanded. Reserves it at the top of the
    /// scrolling content and hands it to the expressions.
    /// </summary>
    private void UpdateDistance()
    {
        if (_title is null || _compactTitle is null)
        {
            return;
        }

        var meta = _meta is { Visibility: Visibility.Visible } ? _meta.DesiredSize.Height : 0;
        var distance = PlaceHeaderMath.ShrinkDistance(meta + _title.DesiredSize.Height, _compactTitle.DesiredSize.Height);
        if (distance == _distance && meta == _metaHeight)
        {
            return;
        }

        _distance = distance;
        _metaHeight = meta;
        ApplyInset(ActiveDistance);
        UpdateProperties();
    }

    private void UpdateProperties()
    {
        if (_props is null || _title is null || _compactTitle is null)
        {
            return;
        }

        _props.InsertScalar("Distance", (float)ActiveDistance);
        _props.InsertScalar("Meta", (float)_metaHeight);
        _props.InsertScalar("Ratio", (float)(_title.FontSize > 0 ? _compactTitle.FontSize / _title.FontSize : 1));
        _props.InsertScalar("Share", (float)PlaceHeaderMath.CrossfadeShare(
            MotionTiming.Get(MotionDuration.Fade),
            MotionTiming.Get(MotionDuration.Normal)));
        _props.InsertScalar("Compact", IsCompact ? 1f : 0f);
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

        // The expanded header draws over the top of the scroll viewer and must stay reachable there.
        if (ReadLocalValue(Canvas.ZIndexProperty) == DependencyProperty.UnsetValue)
        {
            Canvas.SetZIndex(this, Canvas.GetZIndex(_scrollViewer) + 1);
        }

        _scrollViewer.ViewChanged += OnViewChanged;
        ApplyInset(ActiveDistance);
        InvalidateMeasure();
        StartTracking();
        OnViewChanged(null, null);
    }

    private void OnSourceLoaded(object sender, RoutedEventArgs e)
    {
        ((FrameworkElement)sender).Loaded -= OnSourceLoaded;
        Attach();
    }

    private void Detach()
    {
        StopTracking();
        ApplyInset(0);
        _insetTarget = null;
        if (_scrollViewer is not null)
        {
            _scrollViewer.ViewChanged -= OnViewChanged;
            _scrollViewer = null;
        }

        if (IsCompact)
        {
            ApplyCompact(false, crossfade: false);
        }

        InvalidateMeasure();
    }

    private void OnMotionChanged(object? sender, EventArgs e)
    {
        StopTracking();
        ApplyCompact(IsCompact, crossfade: false);
        StartTracking();
    }

    private void OnViewChanged(object? sender, ScrollViewerViewChangedEventArgs? e)
    {
        if (_scrollViewer is null)
        {
            return;
        }

        var offset = _scrollViewer.VerticalOffset;
        var compact = PlaceHeaderMath.IsCompact(offset, ActiveDistance);
        if (compact != IsCompact)
        {
            ApplyCompact(compact, crossfade: !MotionSettings.AnimationsEnabled);
        }

        if (_root is not null)
        {
            // XAML hit testing does not follow compositor transforms. While the header is between its two layouts,
            // its parts are not where their layout says, so the header lets input through to the content.
            _root.IsHitTestVisible = IsCompact || offset <= 0;
        }
    }

    /// <summary>
    /// Switches between the expanded and the compact layout. With animations on, the last frame of the expanded layout
    /// looks exactly like the compact layout, so the switch is invisible.
    /// </summary>
    private void ApplyCompact(bool compact, bool crossfade)
    {
        var changed = compact != IsCompact;
        IsCompact = compact;
        _props?.InsertScalar("Compact", compact ? 1f : 0f);
        if (_title is null || _compactTitle is null)
        {
            return;
        }

        _title.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        _compactTitle.IsHitTestVisible = compact;

        // With the expressions running, they decide which title is visible; otherwise the layout does.
        var tracked = _tracking.Count > 0 && MotionSettings.AnimationsEnabled;
        _compactTitle.Opacity = tracked || compact ? 1 : 0;

        // Exactly one of the two titles is the level 1 heading.
        AutomationProperties.SetHeadingLevel(_title, compact ? AutomationHeadingLevel.None : AutomationHeadingLevel.Level1);
        AutomationProperties.SetHeadingLevel(_compactTitle, compact ? AutomationHeadingLevel.Level1 : AutomationHeadingLevel.None);
        AutomationProperties.SetAccessibilityView(_compactTitle, compact ? AccessibilityView.Content : AccessibilityView.Raw);
        UpdateTexts();

        if (crossfade && changed)
        {
            // Animations are off: the meaning stays, the distance goes.
            new CompositionMotion()
                .Fade(compact ? _compactTitle : _title, 0f, 1f, MotionTiming.Get(MotionDuration.Fade))
                .Start();
        }
    }

    /// <summary>
    /// Ties the header to the scroll position on the compositor, so it follows touch and wheel without delay.
    /// Each expression has a counterpart in <see cref="PlaceHeaderMath"/>.
    /// </summary>
    private void StartTracking()
    {
        if (_tracking.Count > 0 || _scrollViewer is null || _root is null || _meta is null || _title is null
            || _compactTitle is null || _content is null)
        {
            return;
        }

        var compositor = ElementCompositionPreview.GetElementVisual(_root).Compositor;
        _scroll = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(_scrollViewer);
        _props = compositor.CreatePropertySet();
        UpdateProperties();

        // ContentLift: the summary slot moves exactly with the content.
        Track(_content, "Translation.Y", $"-{Offset} * (1 - props.Compact)");

        // The meta line scrolls away with the content (it is collapsed in the compact layout).
        Track(_meta, "Translation.Y", $"-{Offset}");

        if (MotionSettings.AnimationsEnabled)
        {
            // MetaOpacity: fades over its own height instead of being covered.
            Track(_meta, "Opacity", "1 - Clamp(-scroll.Translation.Y / Max(props.Meta, 1), 0, 1)");

            // TitleLift, LargeTitleScale, CompactTitleScale: both titles rise into the compact place and scale between
            // the two sizes, anchored at the top leading corner, which sits on the alignment line.
            var lift = $"-{Progress} * props.Meta * (1 - props.Compact)";
            Track(_title, "Translation.Y", lift);
            Track(_compactTitle, "Translation.Y", lift);
            Track(_title, "Scale", $"Vector3(Lerp(1, props.Ratio, {Progress}), Lerp(1, props.Ratio, {Progress}), 1)");
            Track(_compactTitle, "Scale", $"Vector3(Lerp(1 / props.Ratio, 1, {Progress}), Lerp(1 / props.Ratio, 1, {Progress}), 1)");

            // LargeTitleOpacity, CompactTitleOpacity: the titles crossfade over the last part of the distance.
            var compactOpacity = $"Clamp(({Progress} - (1 - props.Share)) / props.Share, 0, 1)";
            Track(_title, "Opacity", $"1 - {compactOpacity}");
            Track(_compactTitle, "Opacity", compactOpacity);
        }
        else
        {
            // Nothing scales or fades: the title scrolls away like the content, and the compact title waits in its place.
            Track(_title, "Translation.Y", $"-{Offset}");
            Track(_compactTitle, "Translation.Y", "-props.Meta * (1 - props.Compact)");
        }

        ApplyCompact(IsCompact, crossfade: false);
    }

    private void Track(UIElement element, string property, string expression)
    {
        ElementCompositionPreview.SetIsTranslationEnabled(element, true);
        var visual = ElementCompositionPreview.GetElementVisual(element);
        visual.CenterPoint = Vector3.Zero;
        var animation = visual.Compositor.CreateExpressionAnimation(expression);
        animation.SetReferenceParameter("scroll", _scroll);
        animation.SetReferenceParameter("props", _props);
        visual.StartAnimation(property, animation);
        _tracking.Add((visual, property));
    }

    private void StopTracking()
    {
        foreach (var (visual, property) in _tracking)
        {
            visual.StopAnimation(property);
            visual.Opacity = 1f;
            visual.Scale = Vector3.One;
            visual.Properties.InsertVector3("Translation", Vector3.Zero);
        }

        _tracking.Clear();
        _props = null;
        _scroll = null;
        if (_root is not null)
        {
            _root.IsHitTestVisible = true;
        }
    }

    /// <summary>
    /// Reserves <paramref name="inset"/> at the top of the scrolling content, where the expanded header draws:
    /// top padding of a list's items presenter, or top margin of the scroll viewer's content.
    /// </summary>
    private void ApplyInset(double inset)
    {
        var target = _scrollViewer?.Content as FrameworkElement ?? _insetTarget;
        if (target is null)
        {
            return;
        }

        if (!ReferenceEquals(target, _insetTarget))
        {
            // The content was replaced; the old one keeps nothing of the header.
            if (_insetTarget is not null)
            {
                AddInset(_insetTarget, -_insetApplied);
            }

            _insetTarget = target;
            _insetApplied = 0;
        }

        AddInset(target, inset - _insetApplied);
        _insetApplied = inset;
    }

    private static void AddInset(FrameworkElement target, double delta)
    {
        if (delta == 0)
        {
            return;
        }

        if (target is ItemsPresenter presenter)
        {
            var padding = presenter.Padding;
            padding.Top += delta;
            presenter.Padding = padding;
        }
        else
        {
            var margin = target.Margin;
            margin.Top += delta;
            target.Margin = margin;
        }
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
