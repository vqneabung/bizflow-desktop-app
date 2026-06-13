using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.Services;
using Jeek.Avalonia.Localization;

namespace bizflow_desktop_app.ViewModels;

/// <summary>
/// CustomerDetailViewModel — hiển thị chi tiết khách hàng, edit/deactivate.
/// </summary>
public partial class CustomerDetailViewModel : ViewModelBase
{
    private readonly ICustomerService _customerService;

    [ObservableProperty]
    private CustomerResponse? _customer;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = "";

    public event Action<string>? EditClicked;
    public event Action? BackClicked;

    // Computed display properties
    public string DebtDisplay => Customer is null ? "" : $"{Customer.TotalDebt:N0} đ";
    public string StatusText => Customer?.IsActive == true ? Localizer.Get("customer.detail.active") : Localizer.Get("customer.detail.inactive");
    public string CreatedDate => Customer?.CreatedAt.ToString("dd/MM/yyyy HH:mm") ?? "";
    public string UpdatedDate => Customer?.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "";

    public CustomerDetailViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task LoadAsync(string id)
    {
        IsLoading = true;
        HasError = false;

        try
        {
            Customer = await _customerService.GetCustomerAsync(id);

            if (Customer is null)
            {
                HasError = true;
                ErrorMessage = Localizer.Get("customer.detail.notFound");
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"{Localizer.Get("common.error")}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(DebtDisplay));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(CreatedDate));
            OnPropertyChanged(nameof(UpdatedDate));
        }
    }

    [RelayCommand]
    private void Edit()
    {
        if (Customer is not null)
            EditClicked?.Invoke(Customer.Id);
    }

    [RelayCommand]
    private async Task DeactivateAsync()
    {
        if (Customer is null) return;

        var success = await _customerService.DeactivateCustomerAsync(Customer.Id);
        if (success)
        {
            await LoadAsync(Customer.Id);
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        BackClicked?.Invoke();
    }
}
