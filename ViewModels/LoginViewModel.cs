using System;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Models;
using bizflow_desktop_app.Services;
using Microsoft.Extensions.Logging;
using Jeek.Avalonia.Localization;

namespace bizflow_desktop_app.ViewModels;

/// <summary>
/// LoginViewModel — xử lý đăng nhập: form validation, gọi API, lưu session.
/// </summary>
public partial class LoginViewModel : ViewModelBase
{
    private readonly IApiService _api;
    private readonly ISessionService _session;
    private readonly ILogger<LoginViewModel> _logger;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isLoading;

    /// <summary>Event fired khi login thành công → View đóng window, mở dashboard</summary>
    public event Action? LoginSucceeded;

    public LoginViewModel(IApiService api, ISessionService session, ILogger<LoginViewModel> logger)
    {
        _api = api;
        _session = session;
        _logger = logger;
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        IsLoading = true;
        HasError = false;

        try
        {
            _logger.LogInformation("Login attempt for {Email}", Email);

            var request = new LoginRequest(Email.Trim(), Password);
            var result = await _api.LoginAsync(request);

            if (result.Success && result.Data is not null)
            {
                _session.Token = result.Data.Token;
                _session.User = result.Data;

                _logger.LogInformation("Login succeeded for {Email} (role: {Role})",
                    result.Data.Email, result.Data.Role);

                LoginSucceeded?.Invoke();
            }
            else
            {
                _logger.LogWarning("Login failed for {Email}: {Message}",
                    Email, result.Message);

                ErrorMessage = result.Message;
                HasError = true;
            }
        }
        catch (HttpRequestException)
        {
            ErrorMessage = Localizer.Get("login.errorConnection");
            HasError = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for {Email}", Email);
            ErrorMessage = $"{Localizer.Get("common.error")}: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanLogin()
    {
        return !string.IsNullOrWhiteSpace(Email)
            && !string.IsNullOrWhiteSpace(Password)
            && !IsLoading;
    }

    partial void OnEmailChanged(string value) => LoginCommand.NotifyCanExecuteChanged();
    partial void OnPasswordChanged(string value) => LoginCommand.NotifyCanExecuteChanged();
}
