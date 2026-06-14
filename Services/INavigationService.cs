using bizflow_desktop_app.ViewModels;

namespace bizflow_desktop_app.Services;

/// <summary>
/// Centralized navigation between ViewModels. ViewModels don't subscribe to
/// each other's events anymore — they call <see cref="NavigateTo"/> to swap
/// the page, <see cref="NavigateToFresh{T}"/> to resolve a new instance from
/// DI and optionally initialize it, or <see cref="GoBack"/> to pop the stack.
///
/// <see cref="CurrentPage"/> is bound by MainWindow's ContentControl.
/// Resolving the target ViewModel from DI keeps the navigation contract
/// independent of any specific module — adding a new screen just needs
/// a corresponding DI registration and DataTemplate in App.axaml.
/// </summary>
public interface INavigationService
{
    /// <summary>Currently displayed ViewModel.</summary>
    ViewModelBase? CurrentPage { get; }

    /// <summary>Fired when CurrentPage changes (drives ContentControl re-render).</summary>
    event Action? CurrentPageChanged;

    /// <summary>True if there's a previous page on the stack.</summary>
    bool CanGoBack { get; }

    /// <summary>Switches to the given ViewModel instance, pushing current onto the back-stack.</summary>
    void NavigateTo(ViewModelBase viewModel);

    /// <summary>
    /// Resolves a fresh ViewModel of type T from DI, runs <paramref name="initializer"/>
    /// on it (e.g. <c>vm.LoadAsync(id)</c>), then navigates. Use for "create new"
    /// or "edit existing" flows that need to seed state after resolution.
    /// </summary>
    void NavigateToFresh<TViewModel>(Action<TViewModel>? initializer = null) where TViewModel : ViewModelBase;

    /// <summary>Returns to the previous page, or no-op if stack is empty.</summary>
    void GoBack();
}
