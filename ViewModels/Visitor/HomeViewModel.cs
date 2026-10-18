using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

public partial class HomeViewModel(
    ViewModelContext context,
    SearchContext search,
    IMedicineService medicines,
    IHistoryService history) : BaseViewModel(context)
{
    public SearchContext Search { get; } = search;

    public ObservableCollection<Medicine> PopularMedicines { get; } = [];
    public ObservableCollection<SearchHistoryEntry> RecentSearches { get; } = [];

    [ObservableProperty] private string greeting = "Bonjour";

    protected override bool ReloadOnAppearing => true;

    protected override async Task LoadAsync()
    {
        Greeting = Auth.CurrentUser is { Role: UserRole.Visiteur } user
            ? $"Bonjour {user.Name.Split(' ')[0]}"
            : "Bonjour";

        RecentSearches.Clear();
        foreach (var entry in history.GetEntries().Take(3)) RecentSearches.Add(entry);

        if (PopularMedicines.Count > 0) return;
        await RunLoadAsync(async () =>
        {
            foreach (var m in await medicines.GetPopularAsync()) PopularMedicines.Add(m);
        });
    }

    [RelayCommand]
    private Task OpenSearch() => Nav.GoToAsync(Routes.Search);

    [RelayCommand]
    private Task ChooseLocation() => Nav.GoToAsync(Routes.LocationPicker);

    [RelayCommand]
    private Task OpenMap() => Nav.GoToAsync(Routes.VisitorMap);

    [RelayCommand]
    private Task OpenHistory() => Nav.GoToAsync(Routes.History);

    [RelayCommand]
    private Task SelectMedicine(Medicine medicine)
    {
        history.Add(medicine.DisplayName, medicine.Id);
        return Nav.GoToAsync(Routes.Results, new Dictionary<string, object> { [NavParams.MedicineId] = medicine.Id });
    }

    [RelayCommand]
    private Task SelectRecent(SearchHistoryEntry entry)
        => entry.MedicineId is { } id
            ? Nav.GoToAsync(Routes.Results, new Dictionary<string, object> { [NavParams.MedicineId] = id })
            : Nav.GoToAsync(Routes.Search, new Dictionary<string, object> { [NavParams.Query] = entry.Query });
}
