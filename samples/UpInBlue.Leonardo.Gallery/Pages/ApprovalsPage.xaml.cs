using Microsoft.UI.Xaml.Controls;
using UpInBlue.Leonardo.Gallery.ViewModels;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// The Approvals scenario: an inbox of requests to approve or reject.
/// </summary>
public sealed partial class ApprovalsPage : Page
{
    /// <summary>
    /// Initializes the page.
    /// </summary>
    public ApprovalsPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The state of the page.
    /// </summary>
    public ApprovalsViewModel ViewModel { get; } = new();

    private void OnRequestSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RequestList.SelectedItem is ApprovalRequest request)
        {
            ViewModel.Selected = request;
        }
    }
}
