using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Services;
using bizflow_desktop_app.ViewModels;
using Jeek.Avalonia.Localization;
using Microsoft.Extensions.Logging;

namespace bizflow_desktop_app.ViewModels;

/// <summary>
/// MainWindowViewModel — shell coordinator. Owns the top nav buttons and the
/// logout flow, and re-exposes <see cref="INavigationService.CurrentPage"/>
/// so MainWindow.axaml can bind it to a ContentControl.
///
/// All in-app page navigation (List → Detail → Form → Back) is now handled
/// by <see cref="INavigationService"/> — no event subscriptions between VMs.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _nav;
    private readonly ISessionService _session;
    private readonly IApiService _api;
    private readonly ILogger<MainWindowViewModel> _logger;

    /// <summary>Emitted when logout completes — View closes MainWindow, opens LoginWindow.</summary>
    public event Action? LogoutSucceeded;

    public MainWindowViewModel(
        INavigationService nav,
        ISessionService session,
        IApiService api,
        ILogger<MainWindowViewModel> logger)
    {
        _nav = nav;
        _session = session;
        _api = api;
        _logger = logger;

        // Mirror nav service's CurrentPage into our property so XAML can bind it.
        _nav.CurrentPageChanged += () => OnPropertyChanged(nameof(CurrentPage));

        UserName = session.User?.Name ?? session.User?.Email ?? Localizer.Get("common.notAvailable");
        UserEmail = session.User?.Email ?? "";
        _currentLanguageDisplay = Localizer.Language == "vi"
            ? Localizer.Get("nav.languageVi")
            : Localizer.Get("nav.languageEn");

        // Start at product list. Each nav entry resolves a fresh VM from DI,
        // so subsequent visits get a clean state.
        _nav.NavigateToFresh<ProductListViewModel>(vm => vm.LoadCommand.Execute(null));
    }

    public ViewModelBase? CurrentPage => _nav.CurrentPage;

    [ObservableProperty]
    private string _userName = "";

    [ObservableProperty]
    private string _userEmail = "";

    [ObservableProperty]
    private string _currentLanguageDisplay = "";

    [RelayCommand]
    private void GoToProducts()
        => _nav.NavigateToFresh<ProductListViewModel>(vm => vm.LoadCommand.Execute(null));

    [RelayCommand]
    private void GoToCustomers()
        => _nav.NavigateToFresh<CustomerListViewModel>(vm => vm.LoadCommand.Execute(null));

    [RelayCommand]
    private void ToggleLanguage()
    {
        if (Localizer.Language == "vi")
        {
            Localizer.Language = "en";
            CurrentLanguageDisplay = Localizer.Get("nav.languageEn");
        }
        else
        {
            Localizer.Language = "vi";
            CurrentLanguageDisplay = Localizer.Get("nav.languageVi");
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _api.LogoutAsync();
        _session.Clear();
        _logger.LogInformation("User logged out");
        LogoutSucceeded?.Invoke();
    }
}
