using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using bizflow_desktop_app.Models;
using Jeek.Avalonia.Localization;
using Microsoft.Extensions.Logging;

namespace bizflow_desktop_app.ViewModels.Base;

/// <summary>
/// Base class for list-style screens (Product list, Customer list, future Order list).
/// Owns the common pagination + loading + error state, plus the standard
/// Refresh/Search/GoToPage commands. Subclasses implement
/// <see cref="FetchAsync(int, int, string?)"/> to do the actual API call.
///
/// Subclass MUST set <see cref="ErrorLoadKey"/> to a localization key like
/// "product.list.errorLoad" — the base uses it to format error messages.
/// </summary>
public abstract partial class PagedListViewModelBase<T> : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<T> _items = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private bool _hasPrevious;

    [ObservableProperty]
    private bool _hasNext;

    /// <summary>Localization key for "Failed to load {items}" — e.g. "product.list.errorLoad".</summary>
    protected abstract string ErrorLoadKey { get; }

    /// <summary>Subclass: perform the actual API call. Return paginated data + pagination meta.</summary>
    protected abstract Task<PaginatedResponse<T>> FetchAsync(int page, int size, string? search);

    /// <summary>Subclass: extract the search term from any extra state. Default: SearchText.</summary>
    protected virtual string? GetSearchTerm() => string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();

    /// <summary>Subclass: process the items after fetch (e.g. extract unique categories). Default: no-op.</summary>
    protected virtual void OnItemsLoaded(ObservableCollection<T> items) { }

    /// <summary>How many items per page. Override in subclass if needed.</summary>
    protected virtual int PageSize => 20;

    [RelayCommand]
    public virtual async Task LoadAsync()
    {
        IsLoading = true;
        HasError = false;

        try
        {
            var result = await FetchAsync(CurrentPage, PageSize, GetSearchTerm());

            Items = new ObservableCollection<T>(result.Data);
            TotalCount = (int)result.Pagination.TotalElements;
            TotalPages = result.Pagination.TotalPages;
            HasPrevious = CurrentPage > 1;
            HasNext = CurrentPage < TotalPages;
            IsEmpty = Items.Count == 0;

            OnItemsLoaded(Items);
        }
        catch (System.Exception ex)
        {
            HasError = true;
            ErrorMessage = $"{Localizer.Get(ErrorLoadKey)}: {ex.Message}";
            Items = [];
            IsEmpty = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        CurrentPage = 1;
        await LoadAsync();
    }

    [RelayCommand]
    public void DoSearch()
    {
        CurrentPage = 1;
        LoadCommand.Execute(null);
    }

    [RelayCommand]
    public void GoToPage(int page)
    {
        if (page < 1 || page > TotalPages) return;
        CurrentPage = page;
        LoadCommand.Execute(null);
    }
}
