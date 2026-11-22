using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// The States scenario: a file list in the empty, loading, error and offline states.
/// </summary>
public sealed partial class StatesPage : Page, INotifyPropertyChanged
{
    private string _state = "Empty";
    private string _feedback = string.Empty;

    /// <summary>
    /// Initializes the page.
    /// </summary>
    public StatesPage()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Whether the empty state is shown.</summary>
    public bool IsEmpty => _state == "Empty";

    /// <summary>Whether the loading state is shown.</summary>
    public bool IsLoading => _state == "Loading";

    /// <summary>Whether the error state is shown.</summary>
    public bool IsError => _state == "Error";

    /// <summary>Whether the offline state is shown.</summary>
    public bool IsOffline => _state == "Offline";

    /// <summary>What just happened; empty if nothing.</summary>
    public string Feedback => _feedback;

    private void OnStateChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (sender.SelectedItem?.Tag is string state)
        {
            SetState(state, string.Empty);
        }
    }

    private void OnTryAgainClicked(object sender, RoutedEventArgs e)
    {
        foreach (var item in StateSelector.Items)
        {
            item.IsSelected = item.Tag as string == "Loading";
        }

        SetState("Loading", "Trying again.");
    }

    private void OnSampleActionClicked(object sender, RoutedEventArgs e)
        => SetState(_state, "Uploading isn't part of this sample.");

    private void SetState(string state, string feedback)
    {
        _state = state;
        _feedback = feedback;
        foreach (var name in new[] { nameof(IsEmpty), nameof(IsLoading), nameof(IsError), nameof(IsOffline), nameof(Feedback) })
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
