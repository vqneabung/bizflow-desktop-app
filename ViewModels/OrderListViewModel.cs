using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.Services;
using bizflow_desktop_app.ViewModels.Base;
using Microsoft.Extensions.Logging;

namespace bizflow_desktop_app.ViewModels;

public partial class OrderListViewModel : PagedListViewModelBase<OrderSummaryResponse>
{
    private readonly IOrderService _orderService;
    private readonly INavigationService _nav;

    [ObservableProperty]
    private string? _selectedStatus;

    protected override string ErrorLoadKey => "order.list.errorLoad";

    public OrderListViewModel(
        IOrderService orderService,
        INavigationService nav,
        ILogger<OrderListViewModel> logger)
    {
        _orderService = orderService;
        _nav = nav;
    }

    protected override async Task<PaginatedResponse<OrderSummaryResponse>> FetchAsync(int page, int size, string? search)
    {
        return await _orderService.GetOrdersAsync(page, size, SelectedStatus);
    }

    [RelayCommand]
    private void SelectOrder(string? id)
    {
        if (id is not null)
            _nav.NavigateToFresh<OrderDetailViewModel>(vm => _ = vm.LoadAsync(id));
    }

    [RelayCommand]
    private void CreateNew()
    {
        _nav.NavigateToFresh<OrderCreateViewModel>(vm => vm.LoadForCreate());
    }

    partial void OnSelectedStatusChanged(string? value)
    {
        RefreshCommand.Execute(null);
    }
}
