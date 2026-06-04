using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Services;

namespace bizflow_desktop_app.ViewModels;

/// <summary>
/// MainWindowViewModel — điều khiển navigation sidebar + ContentControl.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IProductService _productService;
    private readonly ProductListViewModel _productListVM;
    private readonly ProductDetailViewModel _productDetailVM;
    private readonly ProductFormViewModel _productFormVM;

    public MainWindowViewModel(
        IProductService productService,
        ISessionService session,
        ProductListViewModel productListVM,
        ProductDetailViewModel productDetailVM,
        ProductFormViewModel productFormVM)
    {
        _productService = productService;
        _productDetailVM = productDetailVM;
        _productFormVM = productFormVM;

        UserName = session.User?.Name ?? session.User?.Email ?? "Admin";
        UserEmail = session.User?.Email ?? "";

        // Wire up navigation callbacks
        _productListVM = productListVM;
        _productListVM.ProductSelected += OnProductSelected;
        _productListVM.CreateClicked += OnCreateClicked;

        _productDetailVM.EditClicked += OnEditClicked;
        _productDetailVM.BackClicked += GoToProductList;

        _productFormVM.Saved += GoToProductList;
        _productFormVM.Cancelled += GoToProductList;

        // Start at product list
        CurrentPage = _productListVM;
    }

    [ObservableProperty]
    private ObservableObject _currentPage = null!;

    [ObservableProperty]
    private string _userName = "";

    [ObservableProperty]
    private string _userEmail = "";

    // --- Navigation commands ---

    [RelayCommand]
    private void GoToProducts()
    {
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

    [RelayCommand]
    private void Logout()
    {
        // TODO: trigger logout flow
    }
}
