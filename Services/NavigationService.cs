using System;
using System.Collections.Generic;
using bizflow_desktop_app.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace bizflow_desktop_app.Services;

/// <summary>
/// Singleton navigation service. Holds a back-stack of ViewModels and exposes
/// the current one. Backed by the DI container so any ViewModel registered as
/// Transient can be resolved fresh on each navigation, which keeps state
/// isolated per navigation entry.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;
    private readonly Stack<ViewModelBase> _backStack = new();

    public NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    public ViewModelBase? CurrentPage { get; private set; }

    public event Action? CurrentPageChanged;

    public bool CanGoBack => _backStack.Count > 0;

    public void NavigateTo(ViewModelBase viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        if (CurrentPage is not null)
            _backStack.Push(CurrentPage);

        CurrentPage = viewModel;
        CurrentPageChanged?.Invoke();
    }

    public void NavigateToFresh<TViewModel>(Action<TViewModel>? initializer = null) where TViewModel : ViewModelBase
    {
        var instance = _services.GetRequiredService<TViewModel>();
        initializer?.Invoke(instance);
        NavigateTo(instance);
    }

    public void GoBack()
    {
        if (_backStack.Count == 0) return;

        CurrentPage = _backStack.Pop();
        CurrentPageChanged?.Invoke();
    }
}
