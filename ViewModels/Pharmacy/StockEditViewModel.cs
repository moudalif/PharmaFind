using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Helpers;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>
/// Saisie d'une ligne de stock : quantité, prix (FCFA), seuil d'alerte, rupture.
/// Valeurs négatives refusées ici et, dans la version finale, par l'API.
/// </summary>
public partial class StockEditViewModel(
    ViewModelContext context,
    IStockService stock,
    IMedicineService medicines) : BaseViewModel(context), IQueryAttributable
{
    public const int MaxPrice = 1_000_000;

    private int _stockId;
    private int _medicineId;
    private StockItem? _item;

    [ObservableProperty] private Medicine? medicine;
    [ObservableProperty] private bool isNew;
    [ObservableProperty] private string quantityText = string.Empty;
    [ObservableProperty] private string priceText = string.Empty;
    [ObservableProperty] private string thresholdText = "10";
    [ObservableProperty] private bool declaredOutOfStock;
    [ObservableProperty] private string lastUpdatedText = string.Empty;
    [ObservableProperty] private string? errorText;
    [ObservableProperty] private string pricePreview = string.Empty;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _stockId = ReadInt(query, NavParams.StockId) ?? 0;
        _medicineId = ReadInt(query, NavParams.MedicineId) ?? 0;
    }

    protected override async Task LoadAsync()
    {
        await RunLoadAsync(async () =>
        {
            if (_stockId > 0)
            {
                _item = await stock.GetStockItemAsync(_stockId);
                if (_item is null)
                {
                    StateMessage = "Cette ligne de stock n'existe pas ou n'appartient pas à votre pharmacie.";
                    State = ViewState.Error;
                    return;
                }
                IsNew = false;
                Medicine = _item.Medicine;
                QuantityText = _item.Quantity.ToString();
                PriceText = _item.PriceXaf.ToString();
                ThresholdText = _item.LowStockThreshold.ToString();
                DeclaredOutOfStock = _item.DeclaredOutOfStock;
                LastUpdatedText = "Dernière mise à jour : " + _item.UpdatedFullText;
                Title = "Modifier le stock";
            }
            else
            {
                IsNew = true;
                Medicine = await medicines.GetByIdAsync(_medicineId);
                LastUpdatedText = "Nouvelle ligne d'inventaire";
                Title = "Ajouter à mon stock";
            }
        });
    }

    partial void OnPriceTextChanged(string value)
        => PricePreview = int.TryParse(value, out var p) && p > 0 ? PriceFormatter.Format(p) : string.Empty;

    partial void OnDeclaredOutOfStockChanged(bool value)
    {
        if (value) QuantityText = "0";
    }

    [RelayCommand]
    private void Adjust(string delta)
    {
        if (!int.TryParse(delta, out var d)) return;
        var current = int.TryParse(QuantityText, out var q) ? q : 0;
        QuantityText = Math.Max(0, current + d).ToString();
        if (current + d > 0) DeclaredOutOfStock = false;
    }

    [RelayCommand]
    private async Task Save()
    {
        ErrorText = null;
        if (Medicine is null) return;
        if (!int.TryParse(QuantityText, out var qty) || qty < 0) { ErrorText = "La quantité doit être un nombre entier positif ou nul."; return; }
        if (!int.TryParse(PriceText, out var price) || price <= 0 || price > MaxPrice) { ErrorText = $"Le prix doit être compris entre 1 et {PriceFormatter.Format(MaxPrice)}."; return; }
        if (!int.TryParse(ThresholdText, out var threshold) || threshold < 0) { ErrorText = "Le seuil d'alerte doit être un nombre positif ou nul."; return; }

        var item = _item ?? new StockItem { MedicineId = Medicine.Id };
        item.Medicine = Medicine;
        item.Quantity = qty;
        item.PriceXaf = price;
        item.LowStockThreshold = threshold;
        item.DeclaredOutOfStock = DeclaredOutOfStock || qty == 0;

        if (await RunActionAsync(() => stock.SaveStockAsync(item)))
            await Nav.GoBackAsync();
    }

    [RelayCommand]
    private async Task Remove()
    {
        if (_item is null) return;
        if (!await Dialogs.ConfirmAsync("Retirer de l'inventaire",
                $"« {_item.Medicine.DisplayName} » ne sera plus proposé aux visiteurs pour votre pharmacie.", "Retirer"))
            return;
        if (await RunActionAsync(() => stock.RemoveStockAsync(_item.Id)))
            await Nav.GoBackAsync();
    }
}
