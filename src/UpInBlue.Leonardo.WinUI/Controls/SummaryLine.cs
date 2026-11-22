using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace UpInBlue.Leonardo.WinUI.Controls;

/// <summary>
/// What just happened, as one sentence: "Since your last visit: 2 new · 1 checked out · 1 conflict".
/// Each phrase is a toggle filter with a command and an active state. Phrases with a count of 0 are left out,
/// and the control hides itself when all counts are 0. Screen readers read the whole sentence; each phrase is a
/// standard toggle button, reachable with Tab and toggled with Space.
/// </summary>
public sealed partial class SummaryLine : Control
{
    /// <summary>Identifies the <see cref="Prefix"/> property.</summary>
    public static readonly DependencyProperty PrefixProperty = DependencyProperty.Register(
        nameof(Prefix), typeof(string), typeof(SummaryLine), new PropertyMetadata(string.Empty, (sender, _) => ((SummaryLine)sender).Update()));

    /// <summary>Identifies the <see cref="Items"/> property.</summary>
    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
        nameof(Items), typeof(IList<SummaryLineItem>), typeof(SummaryLine), new PropertyMetadata(null, OnItemsChanged));

    private readonly Dictionary<SummaryLineItem, ToggleButton> _buttons = [];
    private readonly HashSet<SummaryLineItem> _observed = [];
    private SentencePanel? _panel;
    private string _spokenText = string.Empty;

    /// <summary>
    /// Creates a summary line with an empty list of phrases.
    /// </summary>
    public SummaryLine()
    {
        DefaultStyleKey = typeof(SummaryLine);
        DefaultStyleResourceUri = LeonardoControlResources.GenericUri;
        Items = new ObservableCollection<SummaryLineItem>();
        Update();
    }

    /// <summary>The start of the sentence, for example "Since your last visit:".</summary>
    public string Prefix
    {
        get => (string)GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    /// <summary>
    /// The phrases, in reading order. Changes to the collection and to the items update the sentence.
    /// </summary>
    public IList<SummaryLineItem> Items
    {
        get => (IList<SummaryLineItem>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    /// <summary>The sentence as screen readers read it, for example "Since your last visit: 2 new, 1 checked out, 1 conflict".</summary>
    public string SpokenText => _spokenText;

    /// <inheritdoc />
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (GetTemplateChild("PART_Host") is Border host)
        {
            _panel = new SentencePanel();
            host.Child = _panel;
        }

        _buttons.Clear();
        Update();
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer() => new SummaryLineAutomationPeer(this);

    private static void OnItemsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var line = (SummaryLine)sender;
        if (args.OldValue is INotifyCollectionChanged oldItems)
        {
            oldItems.CollectionChanged -= line.OnCollectionChanged;
        }

        if (args.NewValue is INotifyCollectionChanged newItems)
        {
            newItems.CollectionChanged += line.OnCollectionChanged;
        }

        line.Update();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Update();

    private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SummaryLineItem.IsActive) && sender is SummaryLineItem item && _buttons.TryGetValue(item, out var button))
        {
            button.IsChecked = item.IsActive;
            return;
        }

        Update();
    }

    /// <summary>
    /// Brings the sentence up to date. Buttons are reused per item, so focus stays on a phrase whose count changes.
    /// </summary>
    private void Update()
    {
        var items = Items ?? [];
        ObserveItems(items);

        var sentence = SummaryLineText.Compose(Prefix, items, CultureInfo.CurrentCulture);
        Visibility = sentence.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        var oldSpoken = _spokenText;
        _spokenText = sentence.SpokenText;

        if (_panel is not null)
        {
            var children = new List<UIElement>();
            if (!string.IsNullOrWhiteSpace(Prefix) && sentence.IsVisible)
            {
                children.Add(Word(Prefix.Trim()));
            }

            for (var index = 0; index < sentence.Phrases.Count; index++)
            {
                if (index > 0)
                {
                    children.Add(Word(SummaryLineText.Separator.Trim()));
                }

                children.Add(ButtonFor(sentence.Phrases[index]));
            }

            if (!children.SequenceEqual(_panel.Children, ReferenceOrTextComparer.Instance))
            {
                _panel.Children.Clear();
                foreach (var child in children)
                {
                    _panel.Children.Add(child);
                }
            }
        }

        if (oldSpoken != _spokenText && FrameworkElementAutomationPeer.FromElement(this) is { } peer)
        {
            peer.RaisePropertyChangedEvent(AutomationElementIdentifiers.NameProperty, oldSpoken, _spokenText);
            peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
        }
    }

    private void ObserveItems(IList<SummaryLineItem> items)
    {
        foreach (var item in _observed.Except(items).ToList())
        {
            item.PropertyChanged -= OnItemChanged;
            _observed.Remove(item);
            _buttons.Remove(item);
        }

        foreach (var item in items.Where(item => item is not null && _observed.Add(item)))
        {
            item.PropertyChanged += OnItemChanged;
        }
    }

    private ToggleButton ButtonFor(SummaryLineItem item)
    {
        var phrase = item.Phrase(CultureInfo.CurrentCulture);
        if (!_buttons.TryGetValue(item, out var button))
        {
            button = new ToggleButton();

            // Checked and Unchecked also follow toggles through UI Automation, which don't raise Click.
            // Updates from the item set IsChecked to the item's state, so they end here without running the command.
            void OnToggled(object sender, RoutedEventArgs e)
            {
                var active = button.IsChecked == true;
                if (item.IsActive == active)
                {
                    return;
                }

                item.IsActive = active;
                var parameter = item.CommandParameter ?? item;
                if (item.Command?.CanExecute(parameter) == true)
                {
                    item.Command.Execute(parameter);
                }
            }

            button.Checked += OnToggled;
            button.Unchecked += OnToggled;
            _buttons[item] = button;
        }

        button.Content = phrase;
        button.IsChecked = item.IsActive;
        AutomationProperties.SetName(button, phrase);
        return button;
    }

    private static TextBlock Word(string text)
    {
        var word = new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center };
        if (Application.Current.Resources.TryGetValue("LeonardoBodyTextStyle", out var style) && style is Style textStyle)
        {
            word.Style = textStyle;
        }

        // The control's own name is the whole sentence; the words would repeat it.
        AutomationProperties.SetAccessibilityView(word, AccessibilityView.Raw);
        return word;
    }

    /// <summary>
    /// Compares the wanted children with the current ones: buttons by reference, words by text.
    /// </summary>
    private sealed class ReferenceOrTextComparer : IEqualityComparer<UIElement>
    {
        public static readonly ReferenceOrTextComparer Instance = new();

        public bool Equals(UIElement? x, UIElement? y) => (x, y) switch
        {
            (TextBlock a, TextBlock b) => a.Text == b.Text,
            _ => ReferenceEquals(x, y),
        };

        public int GetHashCode(UIElement obj) => obj is TextBlock text ? text.Text.GetHashCode(StringComparison.Ordinal) : obj.GetHashCode();
    }
}

/// <summary>
/// Exposes a <see cref="SummaryLine"/> to UI Automation as a group whose name is the whole sentence. The phrases are its
/// children, as toggle buttons. Changes are announced politely.
/// </summary>
public sealed partial class SummaryLineAutomationPeer : FrameworkElementAutomationPeer
{
    /// <summary>
    /// Creates the peer.
    /// </summary>
    public SummaryLineAutomationPeer(SummaryLine owner)
        : base(owner)
    {
    }

    /// <inheritdoc />
    protected override string GetNameCore()
    {
        var name = base.GetNameCore();
        return string.IsNullOrEmpty(name) ? ((SummaryLine)Owner).SpokenText : name;
    }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Group;

    /// <inheritdoc />
    protected override AutomationLiveSetting GetLiveSettingCore() => AutomationLiveSetting.Polite;

    /// <inheritdoc />
    protected override string GetClassNameCore() => nameof(SummaryLine);
}
