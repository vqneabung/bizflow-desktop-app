using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.Services;
using Jeek.Avalonia.Localization;

namespace bizflow_desktop_app.ViewModels;

/// <summary>
/// CustomerListViewModel — danh sách khách hàng với search, pagination.
/// </summary>
public partial class CustomerListViewModel : ViewModelBase
{
    private readonly ICustomerService _customerService;

    [ObservableProperty]
    private ObservableCollection<CustomerResponse> _customers = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private bool _hasPrevious;

    [ObservableProperty]
    private bool _hasNext;

    public event Action<string>? CustomerSelected;
    public event Action? CreateClicked;

    public CustomerListViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        IsLoading = true;
        HasError = false;

        try
        {
            var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
            var result = await _customerService.GetCustomersAsync(CurrentPage, 20, search);

            Customers = new ObservableCollection<CustomerResponse>(result.Data);
            TotalCount = (int)result.Pagination.TotalElements;
            TotalPages = result.Pagination.TotalPages;
            HasPrevious = CurrentPage > 1;
            HasNext = CurrentPage < TotalPages;
            IsEmpty = Customers.Count == 0;
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"{Localizer.Get("customer.list.errorLoad")}: {ex.Message}";
            Customers = [];
            IsEmpty = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        CurrentPage = 1;
        await LoadCustomersAsync();
    }

    [RelayCommand]
    private void DoSearch()
    {
        CurrentPage = 1;
        LoadCustomersCommand.Execute(null);
    }

    [RelayCommand]
    private void GoToPage(int page)
    {
        if (page < 1 || page > TotalPages) return;
        CurrentPage = page;
        LoadCustomersCommand.Execute(null);
    }

    [RelayCommand]
    private void SelectCustomer(string? id)
    {
        if (id is not null)
            CustomerSelected?.Invoke(id);
    }

    [RelayCommand]
    private void CreateNew()
    {
        CreateClicked?.Invoke();
    }
}
