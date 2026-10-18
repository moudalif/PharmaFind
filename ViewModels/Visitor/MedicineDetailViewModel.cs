using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Fiche informative d'un médicament. Aucun diagnostic ni conseil de prescription.</summary>
public partial class MedicineDetailViewModel(
    ViewModelContext context,
    IMedicineService medicines,
    IHistoryService history) : BaseViewModel(context), IQueryAttributable
{
    private int _medicineId;

    public ObservableCollection<Medicine> Equivalents { get; } = [];

    [ObservableProperty] private Medicine? medicine;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _medicineId = ReadInt(query, NavParams.MedicineId) ?? 0;

    protected override async Task LoadAsync()
    {
        await RunLoadAsync(async () =>
        {
            Medicine = await medicines.GetByIdAsync(_medicineId);
            if (Medicine is null)
            {
                StateMessage = "Ce médicament n'est plus disponible au catalogue.";
                State = ViewState.Error;
                return;
            }
            Title = Medicine.BrandName;
            Equivalents.Clear();
            foreach (var m in await medicines.GetEquivalentsAsync(Medicine)) Equivalents.Add(m);
        });
    }

    [RelayCommand]
    private Task FindPharmacies()
    {
        if (Medicine is null) return Task.CompletedTask;
        history.Add(Medicine.DisplayName, Medicine.Id);
        return Nav.GoToAsync(Routes.Results, new Dictionary<string, object> { [NavParams.MedicineId] = Medicine.Id });
    }

    [RelayCommand]
    private Task OpenEquivalent(Medicine m)
        => Nav.GoToAsync(Routes.MedicineDetail, new Dictionary<string, object> { [NavParams.MedicineId] = m.Id });
}
