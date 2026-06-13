using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.Services;
using Jeek.Avalonia.Localization;

namespace bizflow_desktop_app.ViewModels;

/// <summary>
/// CustomerFormViewModel — form create/edit khách hàng (dùng chung 1 ViewModel).
/// </summary>
public partial class CustomerFormViewModel : ViewModelBase
{
    private readonly ICustomerService _customerService;
    private string? _editId;

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _phone = "";

    [ObservableProperty]
    private string _email = "";

    [ObservableProperty]
    private string _address = "";

    [ObservableProperty]
    private string _notes = "";

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isSubmitting;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private string _title = Localizer.Get("customer.form.createTitle");

    [ObservableProperty]
    private string _submitText = Localizer.Get("customer.form.save");

    public event Action? Saved;
    public event Action? Cancelled;

    public CustomerFormViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public void LoadForCreate()
    {
        IsEditMode = false;
        Title = Localizer.Get("customer.form.createTitle");
        SubmitText = Localizer.Get("customer.form.save");
        ClearForm();
    }

    public async void LoadForEdit(string id)
    {
        IsEditMode = true;
        Title = Localizer.Get("customer.form.editTitle");
        SubmitText = Localizer.Get("customer.form.saveChanges");
        _editId = id;

        try
        {
            var customer = await _customerService.GetCustomerAsync(id);
            if (customer is not null)
            {
                Name = customer.Name;
                Phone = customer.Phone ?? "";
                Email = customer.Email ?? "";
                Address = customer.Address ?? "";
                Notes = customer.Notes ?? "";
            }
        }
        catch
        {
            HasError = true;
            ErrorMessage = Localizer.Get("customer.form.errorLoad");
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        IsSubmitting = true;
        HasError = false;

        try
        {
            if (IsEditMode && _editId is not null)
            {
                var request = new UpdateCustomerRequest(
                    Name: string.IsNullOrWhiteSpace(Name) ? null : Name,
                    Phone: string.IsNullOrWhiteSpace(Phone) ? null : Phone,
                    Email: string.IsNullOrWhiteSpace(Email) ? null : Email,
                    Address: string.IsNullOrWhiteSpace(Address) ? null : Address,
                    Notes: string.IsNullOrWhiteSpace(Notes) ? null : Notes
                );

                var result = await _customerService.UpdateCustomerAsync(_editId, request);
                if (result is not null)
                    Saved?.Invoke();
                else
                {
                    HasError = true;
                    ErrorMessage = Localizer.Get("customer.form.errorUpdate");
                }
            }
            else
            {
                var request = new CreateCustomerRequest(
                    Name: Name,
                    Phone: string.IsNullOrWhiteSpace(Phone) ? null : Phone,
                    Email: string.IsNullOrWhiteSpace(Email) ? null : Email,
                    Address: string.IsNullOrWhiteSpace(Address) ? null : Address,
                    Notes: string.IsNullOrWhiteSpace(Notes) ? null : Notes
                );

                var result = await _customerService.CreateCustomerAsync(request);
                if (result is not null)
                    Saved?.Invoke();
                else
                {
                    HasError = true;
                    ErrorMessage = Localizer.Get("customer.form.errorCreate");
                }
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"{Localizer.Get("common.error")}: {ex.Message}";
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Cancelled?.Invoke();
    }

    private bool CanSave()
    {
        if (IsSubmitting) return false;
        return !string.IsNullOrWhiteSpace(Name);
    }

    private void ClearForm()
    {
        Name = "";
        Phone = "";
        Email = "";
        Address = "";
        Notes = "";
        ErrorMessage = "";
        HasError = false;
        _editId = null;
    }

    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
}
