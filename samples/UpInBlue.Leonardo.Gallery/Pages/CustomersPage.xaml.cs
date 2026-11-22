using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UpInBlue.Leonardo.Gallery.ViewModels;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// The Customers scenario: list and details with an editable form and validation.
/// </summary>
public sealed partial class CustomersPage : Page
{
    /// <summary>
    /// Initializes the page.
    /// </summary>
    public CustomersPage()
    {
        InitializeComponent();
        Loaded += (_, _) => ViewModel.ValidationFailed += OnValidationFailed;
        Unloaded += (_, _) => ViewModel.ValidationFailed -= OnValidationFailed;
    }

    /// <summary>
    /// The state of the page.
    /// </summary>
    public CustomersViewModel ViewModel { get; } = new();

    private void OnCustomerSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CustomerList.SelectedItem is CustomerItem customer)
        {
            ViewModel.Selected = customer;
        }
    }

    private void OnNameLostFocus(object sender, RoutedEventArgs e) => ViewModel.Selected.ValidateName();

    private void OnEmailLostFocus(object sender, RoutedEventArgs e) => ViewModel.Selected.ValidateEmail();

    /// <summary>
    /// Moves focus to the first field that needs attention. The entered values stay as they are.
    /// </summary>
    private void OnValidationFailed(object? sender, EventArgs e)
    {
        var field = ViewModel.Selected.NameError.Length > 0 ? NameBox : EmailBox;
        field.Focus(FocusState.Programmatic);
    }
}
