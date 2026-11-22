using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UpInBlue.Leonardo.Gallery.ViewModels;
using UpInBlue.Leonardo.WinUI.Motion;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// The Approvals scenario: an inbox of requests to approve or reject. The pressed decision button transforms into
/// the panel that confirms the decision, and Undo transforms the panel back into the buttons.
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

    private void OnApproveClicked(object sender, RoutedEventArgs e) => Decide(ApproveButton, ViewModel.ApproveCommand);

    private void OnRejectClicked(object sender, RoutedEventArgs e) => Decide(RejectButton, ViewModel.RejectCommand);

    private void Decide(Button pressed, System.Windows.Input.ICommand command)
    {
        Transform.Run(pressed, DecisionPanel, () => command.Execute(null));
        UndoDecisionButton.Focus(FocusState.Programmatic);
    }

    private void OnUndoClicked(object sender, RoutedEventArgs e)
    {
        Transform.Run(DecisionPanel, WaitingPanel, () => ViewModel.UndoCommand.Execute(null));
        ApproveButton.Focus(FocusState.Programmatic);
    }
}
