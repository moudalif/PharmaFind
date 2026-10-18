using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Helpers;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Fiche pharmacie : coordonnées, horaires, stock déclaré, appel, itinéraire, signalement.</summary>
public partial class PharmacyDetailViewModel(
    ViewModelContext context,
    IPharmacyService pharmacies,
    IFavoritesService favorites,
    SearchContext search) : BaseViewModel(context), IQueryAttributable
{
    private int _pharmacyId;
    private int? _medicineId;
    private List<StockItem> _allStock = [];

    public ObservableCollection<StockItem> Stock { get; } = [];

    [ObservableProperty] private Pharmacy? pharmacy;
    [ObservableProperty] private StockItem? highlighted;
    [ObservableProperty] private string distanceText = string.Empty;
    [ObservableProperty] private bool isFavorite;
    [ObservableProperty] private string stockFilter = string.Empty;
    [ObservableProperty] private string stockCountText = string.Empty;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _pharmacyId = ReadInt(query, NavParams.PharmacyId) ?? 0;
        _medicineId = ReadInt(query, NavParams.MedicineId);
        if (_medicineId == 0) _medicineId = null;
    }

    protected override async Task LoadAsync()
    {
        await RunLoadAsync(async () =>
        {
            Pharmacy = await pharmacies.GetByIdAsync(_pharmacyId);
            if (Pharmacy is null)
            {
                StateMessage = "Cette pharmacie n'existe plus ou n'est plus visible.";
                State = ViewState.Error;
                return;
            }
            Title = Pharmacy.Name;

            DistanceText = search.CurrentLocation is { } loc && Pharmacy.HasCoordinates
                ? GeoHelper.FormatDistance(GeoHelper.DistanceKm(loc.Latitude, loc.Longitude, Pharmacy.Latitude!.Value, Pharmacy.Longitude!.Value)) + " à vol d'oiseau"
                : string.Empty;

            _allStock = (await pharmacies.GetStockAsync(_pharmacyId)).ToList();
            Highlighted = _medicineId is { } mid ? _allStock.FirstOrDefault(s => s.MedicineId == mid) : null;
            IsFavorite = Auth.IsLoggedIn && await favorites.IsFavoriteAsync(_pharmacyId);
            ApplyStockFilter();
        });
    }

    partial void OnStockFilterChanged(string value) => ApplyStockFilter();

    private void ApplyStockFilter()
    {
        Stock.Clear();
        foreach (var s in _allStock.Where(s => s.Id != Highlighted?.Id &&
                                               TextSearch.Matches(StockFilter, s.Medicine.BrandName, s.Medicine.Inn)))
            Stock.Add(s);
        StockCountText = $"{_allStock.Count} médicament{(_allStock.Count > 1 ? "s" : "")} déclaré{(_allStock.Count > 1 ? "s" : "")}";
    }

    [RelayCommand]
    private Task Call() => Pharmacy is null ? Task.CompletedTask : ContactActions.CallAsync(Pharmacy, Dialogs);

    [RelayCommand]
    private Task Route() => Pharmacy is null ? Task.CompletedTask : ContactActions.OpenRouteAsync(Pharmacy, Dialogs);

    [RelayCommand]
    private async Task ToggleFavorite()
    {
        if (!Auth.IsLoggedIn)
        {
            if (await Dialogs.ConfirmAsync("Compte facultatif",
                    "Les favoris sont enregistrés dans un compte gratuit. La recherche reste accessible sans compte.",
                    "Se connecter", "Plus tard"))
                await Nav.GoToAsync(Routes.Login);
            return;
        }
        await RunActionAsync(async () => IsFavorite = await favorites.ToggleAsync(_pharmacyId));
    }

    [RelayCommand]
    private Task Report()
    {
        var parameters = new Dictionary<string, object> { [NavParams.PharmacyId] = _pharmacyId };
        if (_medicineId is { } mid) parameters[NavParams.MedicineId] = mid;
        return Nav.GoToAsync(Routes.Report, parameters);
    }

    [RelayCommand]
    private Task OpenMedicine(StockItem item)
        => Nav.GoToAsync(Routes.MedicineDetail, new Dictionary<string, object> { [NavParams.MedicineId] = item.MedicineId });
}
