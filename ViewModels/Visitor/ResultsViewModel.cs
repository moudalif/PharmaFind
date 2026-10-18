using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Helpers;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Pharmacies déclarant le médicament, triées par distance (ou prix), avec filtres.</summary>
public partial class ResultsViewModel(
    ViewModelContext context,
    IMedicineService medicines,
    IPharmacyService pharmacies,
    ILocationService location,
    SearchContext search) : BaseViewModel(context), IQueryAttributable
{
    private int _medicineId;
    private int _loadedVersion = -1;
    private List<PharmacyResult> _all = [];

    public SearchContext Search { get; } = search;
    public ObservableCollection<PharmacyResult> Results { get; } = [];
    public ObservableCollection<FilterChip> RadiusChips { get; } =
        new(StockRules.RadiusOptions.Select(r => new FilterChip($"{r} km", r)));

    [ObservableProperty] private Medicine? medicine;
    [ObservableProperty] private bool needsLocation;
    [ObservableProperty] private bool onlyAvailable;
    [ObservableProperty] private bool onlyOpen;
    [ObservableProperty] private ResultSort sort = ResultSort.Distance;
    [ObservableProperty] private string summary = string.Empty;
    [ObservableProperty] private string? expandText;
    [ObservableProperty] private bool hasStaleResults;

    public string SortText => Sort == ResultSort.Distance ? "Trier : distance" : "Trier : prix";

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _medicineId = ReadInt(query, NavParams.MedicineId) ?? 0;

    protected override bool ReloadOnAppearing => true;

    protected override async Task LoadAsync()
    {
        foreach (var chip in RadiusChips) chip.IsSelected = (int)chip.Value! == Search.RadiusKm;

        if (!Search.HasLocation)
        {
            NeedsLocation = true;
            State = ViewState.Content;
            if (Medicine is null) Medicine = await medicines.GetByIdAsync(_medicineId);
            return;
        }
        NeedsLocation = false;
        if (_loadedVersion == Search.Version && Medicine is not null && State == ViewState.Content) return;

        await RunLoadAsync(async () =>
        {
            Medicine ??= await medicines.GetByIdAsync(_medicineId);
            Title = Medicine?.DisplayName ?? "Résultats";
            _all = (await pharmacies.SearchAsync(_medicineId, Search.CurrentLocation!, Search.RadiusKm)).ToList();
            _loadedVersion = Search.Version;
            ApplyFilters();
        }, "Recherche des pharmacies…");
    }

    private void ApplyFilters()
    {
        IEnumerable<PharmacyResult> filtered = _all;
        if (OnlyAvailable) filtered = filtered.Where(r => r.Status is AvailabilityStatus.Disponible or AvailabilityStatus.StockFaible);
        if (OnlyOpen) filtered = filtered.Where(r => r.IsOpen);
        filtered = Sort == ResultSort.Prix
            ? filtered.OrderBy(r => r.Stock?.PriceXaf ?? int.MaxValue)
            : filtered.OrderBy(r => r.DistanceKm ?? double.MaxValue);

        var list = filtered.ToList();
        Results.Clear();
        foreach (var r in list) Results.Add(r);

        HasStaleResults = list.Any(r => r.IsStale);
        Summary = list.Count == 0
            ? string.Empty
            : $"{list.Count} pharmacie{(list.Count > 1 ? "s" : "")} · rayon {Search.RadiusKm} km · distances à vol d'oiseau";

        var next = StockRules.RadiusOptions.FirstOrDefault(r => r > Search.RadiusKm);
        ExpandText = next > 0 ? $"Élargir à {next} km" : null;
        State = list.Count == 0 ? ViewState.Empty : ViewState.Content;
    }

    partial void OnOnlyAvailableChanged(bool value) => ApplyFilters();
    partial void OnOnlyOpenChanged(bool value) => ApplyFilters();
    partial void OnSortChanged(ResultSort value)
    {
        OnPropertyChanged(nameof(SortText));
        ApplyFilters();
    }

    [RelayCommand]
    private void ToggleOnlyAvailable() => OnlyAvailable = !OnlyAvailable;

    [RelayCommand]
    private void ToggleOnlyOpen() => OnlyOpen = !OnlyOpen;

    [RelayCommand]
    private void ToggleSort() => Sort = Sort == ResultSort.Distance ? ResultSort.Prix : ResultSort.Distance;

    [RelayCommand]
    private async Task SelectRadius(FilterChip chip)
    {
        Search.RadiusKm = (int)chip.Value!;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ExpandRadius()
    {
        var next = StockRules.RadiusOptions.FirstOrDefault(r => r > Search.RadiusKm);
        if (next == 0) return;
        Search.RadiusKm = next;
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
            return;
        }

        var message = result.Error switch
        {
            LocationError.PermissionDenied => "Vous avez refusé l'accès à la localisation. Vous pouvez saisir un quartier ou une ville à la place.",
            LocationError.Disabled => "La localisation est désactivée sur votre téléphone. Activez-la ou saisissez un lieu.",
            _ => "Votre position est introuvable pour le moment. Saisissez un quartier ou une ville."
        };
        if (await Dialogs.ConfirmAsync("Position indisponible", message, "Saisir un lieu", "Fermer"))
            await Nav.GoToAsync(Routes.LocationPicker);
    }

    [RelayCommand]
    private Task ChangeLocation() => Nav.GoToAsync(Routes.LocationPicker);

    [RelayCommand]
    private Task ShowMap()
        => Nav.GoToAsync(Routes.ResultsMap, new Dictionary<string, object> { [NavParams.MedicineId] = _medicineId });

    [RelayCommand]
    private Task OpenMedicine()
        => Nav.GoToAsync(Routes.MedicineDetail, new Dictionary<string, object> { [NavParams.MedicineId] = _medicineId });

    [RelayCommand]
    private Task OpenPharmacy(PharmacyResult result)
        => Nav.GoToAsync(Routes.PharmacyDetail, new Dictionary<string, object>
        {
            [NavParams.PharmacyId] = result.Pharmacy.Id,
            [NavParams.MedicineId] = _medicineId
        });
}
