using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Helpers;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

public partial class AdminPharmaciesViewModel(ViewModelContext context, IPharmacyService pharmacies) : BaseViewModel(context)
{
    private List<Pharmacy> _all = [];
    private ValidationStatus? _status = ValidationStatus.EnAttente;

    public ObservableCollection<Pharmacy> Items { get; } = [];
    public ObservableCollection<FilterChip> StatusChips { get; } = new(FilterChip.Create(
        ("En attente", ValidationStatus.EnAttente),
        ("Validées", ValidationStatus.Validee),
        ("Suspendues", ValidationStatus.Suspendue),
        ("Refusées", ValidationStatus.Refusee),
        ("Toutes", null)));

    [ObservableProperty] private string filterText = string.Empty;

    protected override bool ReloadOnAppearing => true;

    protected override async Task LoadAsync()
    {
        foreach (var c in StatusChips) c.IsSelected = Equals(c.Value, _status);
        await RunLoadAsync(async () =>
        {
            _all = (await pharmacies.GetAllAsync()).ToList();
            Apply();
        });
    }

    partial void OnFilterTextChanged(string value) => Apply();

    private void Apply()
    {
        var list = _all
            .Where(p => _status is null || p.Status == _status)
            .Where(p => TextSearch.Matches(FilterText, p.Name, p.Neighborhood, p.City, p.LicenseNumber))
            .ToList();
        Items.Clear();
        foreach (var p in list) Items.Add(p);
        State = list.Count == 0 ? ViewState.Empty : ViewState.Content;
    }

    [RelayCommand]
    private void SelectStatus(FilterChip chip)
    {
        foreach (var c in StatusChips) c.IsSelected = c == chip;
        _status = chip.Value as ValidationStatus?;
        Apply();
    }

    [RelayCommand]
    private Task Open(Pharmacy pharmacy)
        => Nav.GoToAsync(Routes.AdminPharmacyReview, new Dictionary<string, object> { [NavParams.PharmacyId] = pharmacy.Id });
}
