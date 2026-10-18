using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Helpers;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>
/// Carte interactive : position du visiteur, cercle de recherche, marqueurs colorés par disponibilité
/// et fiche synthétique au toucher d'un marqueur. Sans médicament, affiche toutes les pharmacies proches.
/// </summary>
public partial class MapViewModel(
    ViewModelContext context,
    IMedicineService medicines,
    IPharmacyService pharmacies,
    ILocationService location,
    IMapProvider mapProvider,
    SearchContext search) : BaseViewModel(context), IQueryAttributable
{
    // Centre par défaut : Douala (ville pilote).
    private const double DoualaLat = 4.0511, DoualaLng = 9.7085;

    private int? _medicineId;
    private int _loadedVersion = -1;
    private List<PharmacyResult> _results = [];

    public SearchContext Search { get; } = search;
    public ObservableCollection<FilterChip> RadiusChips { get; } =
        new(StockRules.RadiusOptions.Select(r => new FilterChip($"{r} km", r)));

    [ObservableProperty] private string mapHtml = string.Empty;
    [ObservableProperty] private Medicine? medicine;
    [ObservableProperty] private PharmacyResult? selected;
    [ObservableProperty] private bool isMapUnavailable;
    [ObservableProperty] private string resultText = string.Empty;

    public bool HasMedicine => _medicineId is not null;
    public string ProviderName => mapProvider.Name;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _medicineId = ReadInt(query, NavParams.MedicineId);
        OnPropertyChanged(nameof(HasMedicine));
    }

    protected override bool ReloadOnAppearing => true;

    protected override async Task LoadAsync()
    {
        foreach (var chip in RadiusChips) chip.IsSelected = (int)chip.Value! == Search.RadiusKm;
        if (_loadedVersion == Search.Version && !string.IsNullOrEmpty(MapHtml) && State == ViewState.Content) return;

        await RunLoadAsync(async () =>
        {
            if (_medicineId is { } id && Medicine is null)
            {
                Medicine = await medicines.GetByIdAsync(id);
                Title = Medicine?.DisplayName ?? "Carte";
            }
            else if (_medicineId is null) Title = "Pharmacies proches";

            var center = Search.CurrentLocation ?? new UserLocation { Latitude = DoualaLat, Longitude = DoualaLng, Label = "Douala" };
            _results = (await pharmacies.SearchAsync(_medicineId, center, Search.RadiusKm)).ToList();
            _loadedVersion = Search.Version;
            Selected = null;
            IsMapUnavailable = false;

            var markers = _results
                .Where(r => r.Pharmacy.HasCoordinates)
                .Select(r => new MapMarker(r.Pharmacy.Id, r.Pharmacy.Latitude!.Value, r.Pharmacy.Longitude!.Value,
                    r.Pharmacy.Name, r.DistanceText,
                    StatusPalette.HexFor(HasMedicine ? r.Status : AvailabilityStatus.Disponible)))
                .ToList();

            var missing = _results.Count - markers.Count;
            ResultText = $"{markers.Count} pharmacie{(markers.Count > 1 ? "s" : "")} sur la carte"
                         + (missing > 0 ? $" · {missing} sans coordonnées GPS" : "");

            MapHtml = mapProvider.BuildHtml(new MapModel(center.Latitude, center.Longitude, Search.RadiusKm,
                Search.HasLocation, markers));
        }, "Chargement de la carte…");
    }

    /// <summary>Appelé par la page quand un marqueur est touché.</summary>
    public void OnMarkerSelected(int pharmacyId) => Selected = _results.FirstOrDefault(r => r.Pharmacy.Id == pharmacyId);

    public void OnMapTapped() => Selected = null;

    /// <summary>Appelé par la page si la bibliothèque ou les tuiles ne se chargent pas.</summary>
    public void OnMapFailed() => IsMapUnavailable = true;

    [RelayCommand]
    private async Task SelectRadius(FilterChip chip)
    {
        Search.RadiusKm = (int)chip.Value!;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task UseMyLocation()
    {
        IsWorking = true;
        var result = await location.GetCurrentLocationAsync();
        IsWorking = false;
        if (result.Location is { } loc)
        {
            Search.CurrentLocation = loc;
            await LoadAsync();
        }
        else if (await Dialogs.ConfirmAsync("Position indisponible",
                     "Impossible d'obtenir votre position. Voulez-vous saisir un quartier ou une ville ?", "Saisir un lieu", "Fermer"))
        {
            await Nav.GoToAsync(Routes.LocationPicker);
        }
    }

    [RelayCommand]
    private Task ChangeLocation() => Nav.GoToAsync(Routes.LocationPicker);

    [RelayCommand]
    private void CloseCard() => Selected = null;

    [RelayCommand]
    private Task OpenSelected()
    {
        if (Selected is null) return Task.CompletedTask;
        var parameters = new Dictionary<string, object> { [NavParams.PharmacyId] = Selected.Pharmacy.Id };
        if (_medicineId is { } id) parameters[NavParams.MedicineId] = id;
        return Nav.GoToAsync(Routes.PharmacyDetail, parameters);
    }

    [RelayCommand]
    private Task Call() => Selected is null ? Task.CompletedTask : ContactActions.CallAsync(Selected.Pharmacy, Dialogs);

    [RelayCommand]
    private Task Route() => Selected is null ? Task.CompletedTask : ContactActions.OpenRouteAsync(Selected.Pharmacy, Dialogs);

    [RelayCommand]
    private Task ShowList()
        => HasMedicine ? Nav.GoBackAsync() : Nav.GoToAsync(Routes.Search);

    [RelayCommand]
    private async Task ReloadMap()
    {
        _loadedVersion = -1;
        await LoadAsync();
    }
}
