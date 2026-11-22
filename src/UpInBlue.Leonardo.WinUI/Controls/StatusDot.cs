using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace UpInBlue.Leonardo.WinUI.Controls;

/// <summary>
/// A status dot: new (a filled dot in the accent color), checked out (a ring in the system caution color) or conflict
/// (a filled circle with an exclamation mark in the system critical color). Shape, color and text always go together:
/// set <see cref="Description"/>, for example "Checked out by Anna Berg"; it is the tooltip and the name screen readers read.
/// </summary>
/// <remarks>
/// The dot hangs to the left of the alignment line, next to the icon of a row. It is not focusable, so tooltips don't
/// reach keyboard users: put the status into the row's accessible name and the details as well.
/// </remarks>
public sealed partial class StatusDot : Control
{
    /// <summary>Identifies the <see cref="Kind"/> property.</summary>
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
        nameof(Kind), typeof(StatusKind), typeof(StatusDot), new PropertyMetadata(StatusKind.New, OnChanged));

    /// <summary>Identifies the <see cref="Description"/> property.</summary>
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(StatusDot), new PropertyMetadata(string.Empty, OnChanged));

    /// <summary>
    /// Creates a status dot.
    /// </summary>
    public StatusDot()
    {
        DefaultStyleKey = typeof(StatusDot);
        DefaultStyleResourceUri = LeonardoControlResources.GenericUri;
        UpdateText();
    }

    /// <summary>The status to show.</summary>
    public StatusKind Kind
    {
        get => (StatusKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>
    /// Required. What the status means for this item, for example "Checked out by Anna Berg" or "Sync conflict: two people changed this file".
    /// Without it, the tooltip and the accessible name fall back to the name of the kind.
    /// </summary>
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>The text of the tooltip and the accessible name.</summary>
    public string Text => StatusText.Describe(Kind, Description);

    /// <inheritdoc />
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateState();
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer() => new StatusDotAutomationPeer(this);

    private static void OnChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var dot = (StatusDot)sender;
        dot.UpdateState();
        dot.UpdateText();
        if (FrameworkElementAutomationPeer.FromElement(dot) is { } peer)
        {
            peer.RaisePropertyChangedEvent(Microsoft.UI.Xaml.Automation.AutomationElementIdentifiers.NameProperty, string.Empty, dot.Text);
        }
    }

    private void UpdateState() => VisualStateManager.GoToState(this, Kind.ToString(), false);

    private void UpdateText() => ToolTipService.SetToolTip(this, Text);
}

/// <summary>
/// Exposes a <see cref="StatusDot"/> to UI Automation as an image named after its status, for example "Checked out by Anna Berg".
/// </summary>
public sealed partial class StatusDotAutomationPeer : FrameworkElementAutomationPeer
{
    /// <summary>
    /// Creates the peer.
    /// </summary>
    public StatusDotAutomationPeer(StatusDot owner)
        : base(owner)
    {
    }

    /// <inheritdoc />
    protected override string GetNameCore()
    {
        var name = base.GetNameCore();
        return string.IsNullOrEmpty(name) ? ((StatusDot)Owner).Text : name;
    }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Image;

    /// <inheritdoc />
    protected override string GetLocalizedControlTypeCore() => "status";

    /// <inheritdoc />
    protected override string GetClassNameCore() => nameof(StatusDot);
}
