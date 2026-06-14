using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jeek.Avalonia.Localization;

namespace bizflow_desktop_app.ViewModels.Base;

/// <summary>
/// Base class for create/edit form screens. Owns the common submission state
/// (IsSubmitting, HasError, ErrorMessage) and a Title/SubmitText driven by
/// the IsEditMode flag. Subclasses implement <see cref="OnSave"/> to perform
/// the actual create/update API calls.
/// </summary>
public abstract partial class FormViewModelBase : ViewModelBase
{
    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isSubmitting;

    [ObservableProperty]
    private bool _isEditMode;

    /// <summary>Localization key prefix used to derive the form's Title/SubmitText (e.g. "product.form").</summary>
    protected abstract string FormKeyPrefix { get; }

    /// <summary>Subclass: reset all input fields to defaults for "create new" mode.</summary>
    protected abstract void ClearInputs();

    /// <summary>Subclass: perform the actual save (create or update) — return true on success.</summary>
    protected abstract Task<bool> OnSave();

    /// <summary>Override in subclass to expose custom validation (default: always allow).</summary>
    protected virtual bool IsValid() => true;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        IsSubmitting = true;
        HasError = false;

        try
        {
            var success = await OnSave();
            if (success)
                OnSaveSucceeded();
            else
            {
                HasError = true;
                ErrorMessage = Localizer.Get($"{FormKeyPrefix}.errorUpdate");
            }
        }
        catch (System.Exception ex)
        {
            HasError = true;
            ErrorMessage = $"{Localizer.Get("common.error")}: {ex.Message}";
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    /// <summary>Called after a successful save. Default: caller is expected to call navigation externally.
    /// Subclasses can override to add cleanup (e.g. close modal).</summary>
    protected virtual void OnSaveSucceeded() { }

    private bool CanSave() => !IsSubmitting && IsValid();
}
