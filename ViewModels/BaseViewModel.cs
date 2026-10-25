using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>
/// Base commune : état d'écran (chargement, vide, erreur, hors connexion), gestion uniforme des erreurs
/// et accès aux services transverses.
/// </summary>
public abstract partial class BaseViewModel(ViewModelContext context) : ObservableObject
{
    protected ViewModelContext Context { get; } = context;
    protected INavigationService Nav => Context.Navigation;
    protected IDialogService Dialogs => Context.Dialogs;
    protected IAuthService Auth => Context.Auth;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusy), nameof(IsContent), nameof(HasError))]
    private ViewState state = ViewState.Content;

    [ObservableProperty]
    private string? stateMessage;

    /// <summary>Opération secondaire en cours (enregistrement…) sans masquer le contenu.</summary>
    [ObservableProperty]
    private bool isWorking;

    public bool IsBusy => State == ViewState.Loading;
    public bool IsContent => State == ViewState.Content;
    public bool HasError => State is ViewState.Error or ViewState.Offline;

    private bool _loadedOnce;

    /// <summary>Appelé à chaque apparition de la page.</summary>
    public virtual async Task OnAppearingAsync()
    {
        if (!_loadedOnce || ReloadOnAppearing)
        {
            _loadedOnce = true;
            await LoadAsync();
        }
    }

    /// <summary>Recharger à chaque retour sur la page (listes modifiables ailleurs).</summary>
    protected virtual bool ReloadOnAppearing => false;

    /// <summary>Chargement principal de l'écran. À surcharger.</summary>
    protected virtual Task LoadAsync() => Task.CompletedTask;

    [RelayCommand]
    private Task RetryAsync() => LoadAsync();

    /// <summary>
    /// Exécute un chargement avec gestion uniforme : réseau absent, service indisponible, erreur inattendue.
    /// L'action peut fixer State = Empty ; sinon l'écran repasse en Content.
    /// </summary>
    protected async Task RunLoadAsync(Func<Task> action, string? loadingMessage = null)
    {
        if (!Context.Network.IsConnected)
        {
            StateMessage = null;
            State = ViewState.Offline;
            return;
        }

        try
        {
            StateMessage = loadingMessage;
            State = ViewState.Loading;
            await action();
            if (State == ViewState.Loading) State = ViewState.Content;
        }
        catch (ServiceUnavailableException ex)
        {
            Console.WriteLine($"[PharmaFind ServiceUnavailable] {ex}");
            StateMessage = ex.Message;
            State = ViewState.Error;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PharmaFind CRITICAL ERROR] {ex}");
            Debug.WriteLine(ex);
            StateMessage = "Une erreur inattendue est survenue.";
            State = ViewState.Error;
        }
    }

    /// <summary>Exécute une action utilisateur (enregistrer, valider…) et affiche les erreurs en boîte de dialogue.</summary>
    protected async Task<bool> RunActionAsync(Func<Task> action)
    {
        if (IsWorking) return false;
        if (!Context.Network.IsConnected)
        {
            await Dialogs.AlertAsync("Pas de connexion", "Cette action nécessite une connexion internet. Réessayez une fois connecté.");
            return false;
        }

        try
        {
            IsWorking = true;
            await action();
            return true;
        }
        catch (ServiceUnavailableException ex)
        {
            await Dialogs.AlertAsync("Service indisponible", ex.Message);
        }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException or ArgumentException)
        {
            await Dialogs.AlertAsync("Action impossible", ex.Message);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Dialogs.AlertAsync("Erreur", "Une erreur inattendue est survenue.");
        }
        finally
        {
            IsWorking = false;
        }
        return false;
    }

    protected static int? ReadInt(IDictionary<string, object> query, string key)
    {
        if (!query.TryGetValue(key, out var value)) return null;
        return value switch
        {
            int i => i,
            string s when int.TryParse(s, out var parsed) => parsed,
            _ => null
        };
    }
}
