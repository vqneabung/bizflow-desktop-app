using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Services;
using Jeek.Avalonia.Localization;
using Microsoft.Extensions.Logging;

namespace bizflow_desktop_app.ViewModels.Base;

/// <summary>
/// Base class for detail screens (Product detail, Customer detail).
/// Owns the Load-by-id flow with loading/error state and a common computed
/// display pattern. Subclasses implement <see cref="LoadEntityAsync(string)"/>
/// to fetch the actual entity.
/// </summary>
public abstract partial class DetailViewModelBase<TEntity> : ViewModelBase
{
    [ObservableProperty]
    private TEntity? _entity;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = "";

    /// <summary>Localization key for "{Entity} not found" — e.g. "product.detail.notFound".</summary>
    protected abstract string NotFoundKey { get; }

    /// <summary>Subclass: fetch the entity by id, or null if missing.</summary>
    protected abstract Task<TEntity?> LoadEntityAsync(string id);

    /// <summary>Subclass hook: refresh computed display properties after Entity changes.</summary>
    protected virtual void OnEntityLoaded() { }

    public DetailViewModelBase()
    {
    }

    public virtual async Task LoadAsync(string id)
    {
        IsLoading = true;
        HasError = false;

        try
        {
            Entity = await LoadEntityAsync(id);

            if (Entity is null)
            {
                HasError = true;
                ErrorMessage = Localizer.Get(NotFoundKey);
            }
            OnEntityLoaded();
        }
        catch (System.Exception ex)
        {
            HasError = true;
            ErrorMessage = $"{Localizer.Get("common.error")}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
