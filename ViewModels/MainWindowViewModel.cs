using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Services;
using Jeek.Avalonia.Localization;

namespace bizflow_desktop_app.ViewModels;

/// <summary>
/// MainWindowViewModel — điều khiển navigation sidebar + ContentControl.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IApiService _api;
    private readonly ISessionService _session;
    private readonly ProductListViewModel _productListVM;
    private readonly ProductDetailViewModel _productDetailVM;
    private readonly ProductFormViewModel _productFormVM;
    private readonly CustomerListViewModel _customerListVM;
    private readonly CustomerDetailViewModel _customerDetailVM;
    private readonly CustomerFormViewModel _customerFormVM;

    /// <summary>Emitted khi logout hoàn tất — View sẽ đóng MainWindow, mở LoginWindow.</summary>
    public event Action? LogoutSucceeded;

    public MainWindowViewModel(
        IApiService api,
        ISessionService session,
        ProductListViewModel productListVM,
        ProductDetailViewModel productDetailVM,
        ProductFormViewModel productFormVM,
        CustomerListViewModel customerListVM,
        CustomerDetailViewModel customerDetailVM,
        CustomerFormViewModel customerFormVM)
    {
        _api = api;
        _session = session;
        UserName = session.User?.Name ?? session.User?.Email ?? Localizer.Get("common.notAvailable");
        UserEmail = session.User?.Email ?? "";

        // Product navigation
        _productListVM = productListVM;
        _productDetailVM = productDetailVM;
        _productFormVM = productFormVM;
        _productListVM.ProductSelected += OnProductSelected;
        _productListVM.CreateClicked += OnCreateClicked;
        _productDetailVM.EditClicked += OnEditClicked;
        _productDetailVM.BackClicked += GoToProductList;
        _productFormVM.Saved += GoToProductList;
        _productFormVM.Cancelled += GoToProductList;

        // Customer navigation
        _customerListVM = customerListVM;
        _customerDetailVM = customerDetailVM;
        _customerFormVM = customerFormVM;
        _customerListVM.CustomerSelected += OnCustomerSelected;
        _customerListVM.CreateClicked += OnCustomerCreateClicked;
        _customerDetailVM.EditClicked += OnCustomerEditClicked;
        _customerDetailVM.BackClicked += GoToCustomerList;
        _customerFormVM.Saved += GoToCustomerList;
        _customerFormVM.Cancelled += GoToCustomerList;

        // Start at product list + trigger load
        _productListVM.LoadProductsCommand.Execute(null);
        CurrentPage = _productListVM;
    }

    [ObservableProperty]
    private ObservableObject _currentPage = null!;

    [ObservableProperty]
    private string _userName = "";

    [ObservableProperty]
    private string _userEmail = "";

    // ── Product navigation ──

    [RelayCommand]
    private void GoToProducts()
    {
        _productListVM.LoadProductsCommand.Execute(null);
        CurrentPage = _productListVM;
    }

    private void GoToProductList()
    {
        _productListVM.RefreshCommand.Execute(null);
        CurrentPage = _productListVM;
    }

    private async void OnProductSelected(string productId)
    {
        await _productDetailVM.LoadAsync(productId);
        CurrentPage = _productDetailVM;
    }

    private void OnCreateClicked()
    {
        _productFormVM.LoadForCreate();
        CurrentPage = _productFormVM;
    }

    private void OnEditClicked(string productId)
    {
        _productFormVM.LoadForEdit(productId);
        CurrentPage = _productFormVM;
    }

    // ── Customer navigation ──

    [RelayCommand]
    private void GoToCustomers()
    {
        _customerListVM.LoadCustomersCommand.Execute(null);
        CurrentPage = _customerListVM;
    }

    private void GoToCustomerList()
    {
        _customerListVM.RefreshCommand.Execute(null);
        CurrentPage = _customerListVM;
    }

    private async void OnCustomerSelected(string customerId)
    {
        await _customerDetailVM.LoadAsync(customerId);
        CurrentPage = _customerDetailVM;
    }

    private void OnCustomerCreateClicked()
    {
        _customerFormVM.LoadForCreate();
        CurrentPage = _customerFormVM;
    }

    private void OnCustomerEditClicked(string customerId)
    {
        _customerFormVM.LoadForEdit(customerId);
        CurrentPage = _customerFormVM;
    }

    [ObservableProperty]
    private string _currentLanguageDisplay = Localizer.Language == "vi" ? "Tiếng Việt" : "English";

    [RelayCommand]
    private void ToggleLanguage()
    {
        if (Localizer.Language == "vi")
        {
            Localizer.Language = "en";
            CurrentLanguageDisplay = "English";
        }
        else
        {
            Localizer.Language = "vi";
            CurrentLanguageDisplay = "Tiếng Việt";
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _api.LogoutAsync();
        _session.Clear();
        LogoutSucceeded?.Invoke();
    }
}
