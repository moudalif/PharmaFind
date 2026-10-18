using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Helpers;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Une ligne du catalogue central, avec l'indication « déjà dans mon inventaire ».</summary>
public class CatalogEntry(Medicine medicine, StockItem? stock)
{
    public Medicine Medicine { get; } = medicine;
    public StockItem? Stock { get; } = stock;
    public bool InInventory => Stock is not null;
    public string ActionText => InInventory ? "Modifier" : "Ajouter";
}

/// <summary>Catalogue central géré par l'administrateur ; la pharmacie y associe ses propres stocks.</summary>
public partial class CatalogViewModel(
    ViewModelContext context,
    IMedicineService medicines,
    IStockService stock) : BaseViewModel(context)
{
    private List<CatalogEntry> _all = [];

    public ObservableCollection<CatalogEntry> Entries { get; } = [];

    [ObservableProperty] private string filterText = string.Empty;
    [ObservableProperty] private bool hideInInventory;
    [ObservableProperty] private string countText = string.Empty;

    protected override bool ReloadOnAppearing => true;

    protected override async Task LoadAsync()
    {
        await RunLoadAsync(async () =>
        {
            var mine = (await stock.GetMyStockAsync()).ToDictionary(s => s.MedicineId);
            _all = (await medicines.GetAllAsync())
                .Select(m => new CatalogEntry(m, mine.GetValueOrDefault(m.Id)))
                .ToList();
            Apply();
        });
    }

    partial void OnFilterTextChanged(string value) => Apply();
    partial void OnHideInInventoryChanged(bool value) => Apply();

    private void Apply()
    {
        var list = _all
            .Where(e => !HideInInventory || !e.InInventory)
            .Where(e => TextSearch.Matches(FilterText, e.Medicine.BrandName, e.Medicine.Inn, e.Medicine.Category))
            .ToList();
        Entries.Clear();
        foreach (var e in list) Entries.Add(e);
        CountText = $"{_all.Count(e => e.InInventory)} sur {_all.Count} médicaments du catalogue dans votre inventaire";
        State = list.Count == 0 ? ViewState.Empty : ViewState.Content;
    }

    [RelayCommand]
    private Task Select(CatalogEntry entry)
        => entry.Stock is { } s
            ? Nav.GoToAsync(Routes.StockEdit, new Dictionary<string, object> { [NavParams.StockId] = s.Id })
            : Nav.GoToAsync(Routes.StockEdit, new Dictionary<string, object> { [NavParams.MedicineId] = entry.Medicine.Id });

    [RelayCommand]
    private Task RequestMissing()
        => Dialogs.AlertAsync("Médicament absent du catalogue",
            "Le référentiel est géré par l'administration PharmaFind. Signalez le médicament manquant (nom, DCI, dosage, forme) à l'administration pour qu'il soit vérifié puis ajouté.");
}
