using Avalonia.Controls;
using Avalonia.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.ViewModels;

namespace bizflow_desktop_app.Views;

public partial class OrderListView : UserControl
{
    public OrderListView()
    {
        InitializeComponent();
    }

    private void OnRowDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is DataGrid grid && grid.SelectedItem is OrderSummaryResponse order)
        {
            if (DataContext is OrderListViewModel vm)
            {
                vm.SelectOrderCommand.Execute(order.Id);
            }
        }
    }

    private void OnViewButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is OrderSummaryResponse order)
        {
            if (DataContext is OrderListViewModel vm)
            {
                vm.SelectOrderCommand.Execute(order.Id);
            }
        }
    }
}
