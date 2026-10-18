using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Helpers;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Inventaire de la pharmacie connectée, filtrable par statut.</summary>
public partial class StockViewModel(ViewModelContext context, IStockService stock) : BaseViewModel(context)
{
    private List<StockItem> _all = [];

    public ObservableCollection<StockItem> Items { get; } = [];
    public ObservableCollection<FilterChip> StatusChips { get; } = new(FilterChip.Create(
        ("Tous", null),
        ("Disponible", AvailabilityStatus.Disponible),
        ("Stock faible", AvailabilityStatus.StockFaible),
        ("Rupture", AvailabilityStatus.Indisponible),
        ("À mettre à jour", "stale")));

    [ObservableProperty] private string filterText = string.Empty;
    [ObservableProperty] private string countText = string.Empty;

    private object? _statusFilter;

    protected override bool ReloadOnAppearing => true;

    protected override async Task LoadAsync()
    {
        if (StatusChips.All(c => !c.IsSelected)) StatusChips[0].IsSelected = true;
        await RunLoadAsync(async () =>
        {
            _all = (await stock.GetMyStockAsync()).ToList();
            Apply();
        });
    }

    partial void OnFilterTextChanged(string value) => Apply();

    private void Apply()
    {
        var list = _all
            .Where(i => TextSearch.Matches(FilterText, i.Medicine.BrandName, i.Medicine.Inn))
            .Where(i => _statusFilter switch
            {
                AvailabilityStatus s => i.Status == s,
                "stale" => i.NeedsUpdate,
                _ => true
            })
            .ToList();
        Items.Clear();
        foreach (var i in list) Items.Add(i);
        CountText = $"{list.Count} ligne{(list.Count > 1 ? "s" : "")} sur {_all.Count}";
        State = _all.Count == 0 || list.Count == 0 ? ViewState.Empty : ViewState.Content;
    }

    [RelayCommand]
    private void SelectStatus(FilterChip chip)
    {
        foreach (var c in StatusChips) c.IsSelected = c == chip;
        _statusFilter = chip.Value;
        Apply();
    }

    [RelayCommand]
    private Task Edit(StockItem item)
        => Nav.GoToAsync(Routes.StockEdit, new Dictionary<string, object> { [NavParams.StockId] = item.Id });

    [RelayCommand]
    private Task AddFromCatalog() => Nav.GoToAsync(Routes.PharmacyCatalog);

    [RelayCommand]
    private async Task ConfirmAll()
    {
        if (!await Dialogs.ConfirmAsync("Confirmer les stocks", "Marquer toutes les lignes comme vérifiées maintenant, sans modification ?", "Confirmer")) return;
        if (await RunActionAsync(stock.ConfirmAllUnchangedAsync)) await LoadAsync();
    }
}
