using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Helpers;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Référentiel central des médicaments.</summary>
public partial class AdminMedicinesViewModel(ViewModelContext context, IMedicineService medicines) : BaseViewModel(context)
{
    private List<Medicine> _all = [];

    public ObservableCollection<Medicine> Items { get; } = [];

    [ObservableProperty] private string filterText = string.Empty;
    [ObservableProperty] private bool showInactive;
    [ObservableProperty] private string countText = string.Empty;

    protected override bool ReloadOnAppearing => true;

    protected override async Task LoadAsync()
    {
        await RunLoadAsync(async () =>
        {
            _all = (await medicines.GetAllAsync(includeInactive: true)).ToList();
            Apply();
        });
    }

    partial void OnFilterTextChanged(string value) => Apply();
    partial void OnShowInactiveChanged(bool value) => Apply();

    private void Apply()
    {
        var list = _all
            .Where(m => ShowInactive || m.IsActive)
            .Where(m => TextSearch.Matches(FilterText, m.BrandName, m.Inn, m.Category))
            .ToList();
        Items.Clear();
        foreach (var m in list) Items.Add(m);
        CountText = $"{_all.Count(m => m.IsActive)} actifs · {_all.Count(m => !m.IsActive)} désactivés";
        State = list.Count == 0 ? ViewState.Empty : ViewState.Content;
    }

    [RelayCommand]
    private Task Add() => Nav.GoToAsync(Routes.AdminMedicineEdit, new Dictionary<string, object> { [NavParams.MedicineId] = 0 });

    [RelayCommand]
    private Task Edit(Medicine m)
        => Nav.GoToAsync(Routes.AdminMedicineEdit, new Dictionary<string, object> { [NavParams.MedicineId] = m.Id });
}
