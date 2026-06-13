using Avalonia.Controls;
using Avalonia.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.ViewModels;

namespace bizflow_desktop_app.Views;

public partial class CustomerListView : UserControl
{
    public CustomerListView()
    {
        InitializeComponent();
    }

    private void OnRowDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is DataGrid grid && grid.SelectedItem is CustomerResponse customer)
        {
            SelectCustomer(customer.Id);
        }
    }

    private void OnViewButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is CustomerResponse customer)
        {
            SelectCustomer(customer.Id);
        }
    }

    private void SelectCustomer(string id)
    {
        if (DataContext is CustomerListViewModel vm)
            vm.SelectCustomerCommand.Execute(id);
    }
}
