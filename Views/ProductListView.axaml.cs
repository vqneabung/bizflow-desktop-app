using Avalonia.Controls;
using Avalonia.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.ViewModels;

namespace bizflow_desktop_app.Views;

public partial class ProductListView : UserControl
{
    public ProductListView()
    {
        InitializeComponent();
    }

    private void OnRowDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is DataGrid grid && grid.SelectedItem is ProductResponse product)
        {
            if (DataContext is ProductListViewModel vm)
            {
                vm.SelectProductCommand.Execute(product.Id);
            }
        }
    }

    private void OnViewButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is ProductResponse product)
        {
            if (DataContext is ProductListViewModel vm)
            {
                vm.SelectProductCommand.Execute(product.Id);
            }
        }
    }
}
