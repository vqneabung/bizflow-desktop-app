using Avalonia.Controls;
using Avalonia.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.ViewModels;

namespace bizflow_desktop_app.Views;

public partial class StockImportListView : UserControl
{
    public StockImportListView()
    {
        InitializeComponent();
    }

    private void OnRowDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is DataGrid grid && grid.SelectedItem is StockImportSummaryResponse stockImport)
        {
            if (DataContext is StockImportListViewModel vm)
            {
                vm.SelectStockImportCommand.Execute(stockImport.Id);
            }
        }
    }

    private void OnViewButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is StockImportSummaryResponse stockImport)
        {
            if (DataContext is StockImportListViewModel vm)
            {
                vm.SelectStockImportCommand.Execute(stockImport.Id);
            }
        }
    }
}
