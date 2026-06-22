using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.Services;
using Jeek.Avalonia.Localization;

namespace bizflow_desktop_app.ViewModels;

public partial class OrderDetailViewModel : ViewModelBase
{
    private readonly IOrderService _orderService;
    private readonly INavigationService _nav;
    private readonly IFileSaveService _fileSaveService;

    [ObservableProperty]
    private OrderResponse? _order;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _isDownloading;

    [ObservableProperty]
    private string? _downloadMessage;

    public string TotalDisplay => Order is null ? "" : $"{Order.TotalAmount:N0} đ";
    public string PaidDisplay => Order is null ? "" : $"{Order.PaidAmount:N0} đ";
    public string DebtDisplay => Order is null ? "" : $"{Order.DebtAmount:N0} đ";
    public string StatusDisplay => Order?.Status ?? "";
    public string CreatedDate => Order?.CreatedAt.ToString("dd/MM/yyyy HH:mm") ?? "";
    public string UpdatedDate => Order?.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "";
    public bool HasItems => Order?.Items?.Count > 0;

    public OrderDetailViewModel(IOrderService orderService, INavigationService nav, IFileSaveService fileSaveService)
    {
        _orderService = orderService;
        _nav = nav;
        _fileSaveService = fileSaveService;
    }

    public async Task LoadAsync(string id)
    {
        IsLoading = true;
        HasError = false;

        try
        {
            var result = await _orderService.GetOrderAsync(id);
            Order = result.Data;

            if (Order is null)
            {
                HasError = true;
                ErrorMessage = Localizer.Get("order.detail.error");
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
            OnPropertyChanged(nameof(TotalDisplay));
            OnPropertyChanged(nameof(PaidDisplay));
            OnPropertyChanged(nameof(DebtDisplay));
            OnPropertyChanged(nameof(StatusDisplay));
            OnPropertyChanged(nameof(CreatedDate));
            OnPropertyChanged(nameof(UpdatedDate));
            OnPropertyChanged(nameof(HasItems));
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _nav.GoBack();
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private async Task CancelOrderAsync()
    {
        if (Order is null) return;

        try
        {
            await _orderService.CancelOrderAsync(Order.Id, new CancelOrderRequest(Notes: null));
            _nav.GoBack();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"{Localizer.Get("common.error")}: {ex.Message}";
        }
    }

    private bool CanCancel()
    {
        return Order is not null && Order.Status == "DRAFT";
    }

    [RelayCommand]
    private async Task DownloadReceipt()
    {
        if (Order is null) return;

        IsDownloading = true;
        DownloadMessage = null;

        try
        {
            Stream stream = await _orderService.DownloadReceiptAsync(Order.Id);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            byte[] bytes = ms.ToArray();

            string fileName = $"receipt-{Order.ReferenceNumber}.pdf";
            bool saved = await _fileSaveService.SaveFileAsync(fileName, "pdf", bytes);
            DownloadMessage = saved
                ? $"Đã lưu file: {fileName}"
                : "Đã hủy lưu file";
        }
        catch (Exception ex)
        {
            DownloadMessage = $"Lỗi tải PDF: {ex.Message}";
        }
        finally
        {
            IsDownloading = false;
        }
    }
}
