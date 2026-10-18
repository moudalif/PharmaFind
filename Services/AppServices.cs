using CommunityToolkit.Mvvm.ComponentModel;
using PharmaFind.Helpers;
using PharmaFind.Models;

namespace PharmaFind.Services;

public class ShellNavigationService : INavigationService
{
    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
        => parameters is null ? Shell.Current.GoToAsync(route) : Shell.Current.GoToAsync(route, parameters);

    public Task GoBackAsync() => Shell.Current.GoToAsync("..");
}

/// <summary>Seul endroit qui appelle les boîtes de dialogue MAUI (API « …Async » de .NET 10).</summary>
public class DialogService : IDialogService
{
    private static Page Page => Shell.Current;

    public Task AlertAsync(string title, string message, string ok = "OK")
        => MainThread.InvokeOnMainThreadAsync(() => Page.DisplayAlertAsync(title, message, ok));

    public Task<bool> ConfirmAsync(string title, string message, string accept = "Confirmer", string cancel = "Annuler")
        => MainThread.InvokeOnMainThreadAsync(() => Page.DisplayAlertAsync(title, message, accept, cancel));

    public async Task<string?> ActionSheetAsync(string title, string cancel, string? destructive, params string[] buttons)
    {
        var choice = await MainThread.InvokeOnMainThreadAsync(() => Page.DisplayActionSheetAsync(title, cancel, destructive, buttons));
        return choice == cancel ? null : choice;
    }

    public Task<string?> PromptAsync(string title, string message, string placeholder = "", string accept = "Valider")
        => MainThread.InvokeOnMainThreadAsync(() => Page.DisplayPromptAsync(title, message, accept, "Annuler", placeholder, maxLength: 300));
}

public class NetworkService(DemoSettings demo) : INetworkService
{
    public bool IsConnected =>
        !demo.SimulateOffline && Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
}

/// <summary>Options de démonstration (soutenance) : permettent de montrer chaque état d'écran.</summary>
public partial class DemoSettings : ObservableObject
{
    /// <summary>Remplace la position GPS par Akwa (Douala) : l'émulateur est par défaut aux États-Unis.</summary>
    [ObservableProperty] private bool useDemoPosition = true;
    [ObservableProperty] private bool simulateOffline;
    [ObservableProperty] private bool simulateServerError;
    [ObservableProperty] private bool simulateSlowNetwork;
}

/// <summary>Contexte de recherche partagé : position courante et rayon choisis par le visiteur.</summary>
public partial class SearchContext : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LocationLabel), nameof(HasLocation))]
    private UserLocation? currentLocation;

    [ObservableProperty] private int radiusKm = StockRules.Settings.DefaultRadiusKm;

    public bool HasLocation => CurrentLocation is not null;
    public string LocationLabel => CurrentLocation?.Label ?? "Position non définie";

    /// <summary>Incrémenté à chaque changement : les écrans savent s'ils doivent recharger.</summary>
    public int Version { get; private set; }

    partial void OnCurrentLocationChanged(UserLocation? value) => Version++;
    partial void OnRadiusKmChanged(int value) => Version++;
}

public class LocalHistoryService : IHistoryService
{
    private readonly List<SearchHistoryEntry> _entries =
    [
        new() { Query = "Doliprane 1000 mg", MedicineId = 2, Date = DateTime.Now.AddHours(-3) },
        new() { Query = "Coartem 20/120 mg", MedicineId = 6, Date = DateTime.Now.AddDays(-1) },
        new() { Query = "Smecta", Date = DateTime.Now.AddDays(-4) },
    ];

    public bool IsEnabled { get; set; } = true;

    public IReadOnlyList<SearchHistoryEntry> GetEntries() => _entries.OrderByDescending(e => e.Date).ToList();

    public void Add(string query, int? medicineId)
    {
        if (!IsEnabled || string.IsNullOrWhiteSpace(query)) return;
        _entries.RemoveAll(e => string.Equals(e.Query, query, StringComparison.OrdinalIgnoreCase));
        _entries.Add(new SearchHistoryEntry { Query = query.Trim(), MedicineId = medicineId, Date = DateTime.Now });
        if (_entries.Count > 30) _entries.RemoveAt(0);
    }

    public void Clear() => _entries.Clear();
}

/// <summary>Regroupe les services transverses injectés dans chaque ViewModel.</summary>
public class ViewModelContext(INavigationService navigation, IDialogService dialogs, INetworkService network, IAuthService auth, DemoSettings demo)
{
    public INavigationService Navigation { get; } = navigation;
    public IDialogService Dialogs { get; } = dialogs;
    public INetworkService Network { get; } = network;
    public IAuthService Auth { get; } = auth;
    public DemoSettings Demo { get; } = demo;
}
