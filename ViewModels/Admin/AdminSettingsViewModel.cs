using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Helpers;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Paramétrage des règles de fiabilité affichées aux visiteurs.</summary>
public partial class AdminSettingsViewModel(ViewModelContext context, SearchContext search) : BaseViewModel(context)
{
    public List<int> RadiusOptions { get; } = [.. StockRules.RadiusOptions];

    [ObservableProperty] private string freshHours = StockRules.Settings.FreshHours.ToString();
    [ObservableProperty] private string staleHours = StockRules.Settings.StaleHours.ToString();
    [ObservableProperty] private string unconfirmedDays = StockRules.Settings.UnconfirmedDays.ToString();
    [ObservableProperty] private int defaultRadius = StockRules.Settings.DefaultRadiusKm;
    [ObservableProperty] private string? errorText;

    [RelayCommand]
    private async Task Save()
    {
        ErrorText = null;
        if (!int.TryParse(FreshHours, out var fresh) || fresh < 1) { ErrorText = "Délai « à jour » invalide."; return; }
        if (!int.TryParse(StaleHours, out var stale) || stale <= fresh) { ErrorText = "Le délai « ancien » doit être supérieur au délai « à jour »."; return; }
        if (!int.TryParse(UnconfirmedDays, out var days) || days * 24 < stale) { ErrorText = "Le délai « non confirmée » doit être supérieur au délai « ancien »."; return; }

        StockRules.Settings.FreshHours = fresh;
        StockRules.Settings.StaleHours = stale;
        StockRules.Settings.UnconfirmedDays = days;
        StockRules.Settings.DefaultRadiusKm = DefaultRadius;
        search.RadiusKm = DefaultRadius;

        await Dialogs.AlertAsync("Paramètres enregistrés", "Dans la maquette, ils s'appliquent à cet appareil. Dans la version finale, ils seront stockés par l'API.");
        await Nav.GoBackAsync();
    }
}
