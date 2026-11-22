using System.ComponentModel;
using System.Globalization;
using Microsoft.UI.Xaml.Controls;
using UpInBlue.Leonardo.Gallery.ViewModels;
using UpInBlue.Leonardo.WinUI.Controls;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// The SummaryLine control: a summary line to try, and examples of every state.
/// </summary>
public sealed partial class SummaryLinePage : Page, INotifyPropertyChanged
{
    /// <summary>
    /// Initializes the page.
    /// </summary>
    public SummaryLinePage()
    {
        InitializeComponent();
        foreach (var item in ViewModel.Items)
        {
            item.PropertyChanged += (_, _) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpokenSentence)));
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// The state of the page.
    /// </summary>
    public SummaryLineViewModel ViewModel { get; } = new();

    /// <summary>
    /// What a screen reader reads for the summary line to try.
    /// </summary>
    public string SpokenSentence => SummaryLineText.Compose("Since your last visit:", ViewModel.Items, CultureInfo.CurrentCulture).SpokenText is { Length: > 0 } text
        ? $"“{text}”"
        : "Nothing: the summary line is hidden.";
}
