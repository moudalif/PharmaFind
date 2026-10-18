using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Tableau de bord pharmacie. Accessible uniquement après validation administrative.</summary>
public partial class PharmacyDashboardViewModel(
    ViewModelContext context,
    IStockService stock,
    IReportService reports) : BaseViewModel(context)
{
    public ObservableCollection<StockItem> Attention { get; } = [];
    public ObservableCollection<AppNotification> RecentNotifications { get; } = [];

    [ObservableProperty] private Pharmacy? pharmacy;
    [ObservableProperty] private bool isPending;
    [ObservableProperty] private bool isBlocked;
    [ObservableProperty] private int totalCount;
    [ObservableProperty] private int availableCount;
    [ObservableProperty] private int lowCount;
    [ObservableProperty] private int outCount;
    [ObservableProperty] private int staleCount;
    [ObservableProperty] private int openReports;
    [ObservableProperty] private double freshRatio;
    [ObservableProperty] private string freshText = string.Empty;

    protected override bool ReloadOnAppearing => true;

    protected override async Task LoadAsync()
    {
        await RunLoadAsync(async () =>
        {
            Pharmacy = await stock.GetMyPharmacyAsync();
            Title = Pharmacy.Name;
            IsPending = Pharmacy.Status == ValidationStatus.EnAttente;
            IsBlocked = Pharmacy.Status is ValidationStatus.Suspendue or ValidationStatus.Refusee;
            if (IsPending || IsBlocked) return;

            var items = await stock.GetMyStockAsync();
            TotalCount = items.Count;
            AvailableCount = items.Count(i => i.Status == AvailabilityStatus.Disponible);
            LowCount = items.Count(i => i.Status == AvailabilityStatus.StockFaible);
            OutCount = items.Count(i => i.Status == AvailabilityStatus.Indisponible);
            StaleCount = items.Count(i => i.NeedsUpdate);
            FreshRatio = TotalCount == 0 ? 0 : (TotalCount - StaleCount) / (double)TotalCount;
            FreshText = $"{TotalCount - StaleCount} sur {TotalCount} lignes mises à jour depuis moins de 24 h";

            Attention.Clear();
            foreach (var i in items
                         .Where(i => i.Status is AvailabilityStatus.StockFaible or AvailabilityStatus.Indisponible || i.NeedsUpdate)
                         .OrderBy(i => i.Status == AvailabilityStatus.StockFaible ? 0 : 1)
                         .Take(5))
                Attention.Add(i);

            OpenReports = (await reports.GetForPharmacyAsync(Pharmacy.Id)).Count(r => r.Status is ReportStatus.Nouveau or ReportStatus.EnCours);
            RecentNotifications.Clear();
            foreach (var n in (await stock.GetNotificationsAsync()).Where(n => !n.IsRead).Take(3)) RecentNotifications.Add(n);
        });
    }

    [RelayCommand]
    private Task OpenStock() => Nav.GoToAsync(Routes.PharmacyStock);

    [RelayCommand]
    private Task OpenCatalog() => Nav.GoToAsync(Routes.PharmacyCatalog);

    [RelayCommand]
    private Task OpenNotifications() => Nav.GoToAsync(Routes.PharmacyNotifications);

    [RelayCommand]
    private Task EditItem(StockItem item)
        => Nav.GoToAsync(Routes.StockEdit, new Dictionary<string, object> { [NavParams.StockId] = item.Id });

    [RelayCommand]
    private async Task ConfirmAll()
    {
        if (!await Dialogs.ConfirmAsync("Confirmer les stocks",
                "Toutes vos lignes de stock seront marquées comme vérifiées maintenant, sans changement de quantité ni de prix.", "Confirmer"))
            return;
        if (await RunActionAsync(stock.ConfirmAllUnchangedAsync)) await LoadAsync();
    }

    [RelayCommand]
    private async Task Logout()
    {
        await Auth.LogoutAsync();
        await Nav.GoToAsync(Routes.VisitorHome);
    }
}
