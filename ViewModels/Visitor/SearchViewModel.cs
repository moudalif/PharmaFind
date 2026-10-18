using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>
/// Recherche avec suggestions : nom commercial ou DCI, filtre par forme, tolérance aux fautes de frappe.
/// Les appels sont temporisés (300 ms) pour limiter les échanges réseau.
/// </summary>
public partial class SearchViewModel(
    ViewModelContext context,
    IMedicineService medicines,
    IHistoryService history) : BaseViewModel(context), IQueryAttributable
{
    private CancellationTokenSource? _debounce;
    private string? _selectedForm;

    public ObservableCollection<Medicine> Suggestions { get; } = [];
    public ObservableCollection<FilterChip> FormChips { get; } = [];
    public ObservableCollection<SearchHistoryEntry> Recent { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsQueryEmpty))]
    private string query = string.Empty;

    [ObservableProperty] private Medicine? didYouMean;
    [ObservableProperty] private string resultCountText = string.Empty;

    public bool IsQueryEmpty => string.IsNullOrWhiteSpace(Query);

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(NavParams.Query, out var q) && q is string text) Query = text;
    }

    protected override async Task LoadAsync()
    {
        Recent.Clear();
        foreach (var e in history.GetEntries().Take(6)) Recent.Add(e);

        if (FormChips.Count == 0)
        {
            FormChips.Add(new FilterChip("Toutes les formes", null) { IsSelected = true });
            foreach (var form in await medicines.GetFormsAsync()) FormChips.Add(new FilterChip(form, form));
        }
        if (!IsQueryEmpty) await SearchNowAsync();
    }

    partial void OnQueryChanged(string value)
    {
        _debounce?.Cancel();
        if (string.IsNullOrWhiteSpace(value))
        {
            Suggestions.Clear();
            DidYouMean = null;
            State = ViewState.Content;
            return;
        }
        _debounce = new CancellationTokenSource();
        var token = _debounce.Token;
        _ = Task.Delay(300, token).ContinueWith(t =>
        {
            if (!t.IsCanceled) MainThread.BeginInvokeOnMainThread(async () => await SearchNowAsync());
        }, TaskScheduler.Default);
    }

    private async Task SearchNowAsync()
    {
        var current = Query;
        if (string.IsNullOrWhiteSpace(current)) return;

        await RunLoadAsync(async () =>
        {
            var items = await medicines.SuggestAsync(current, _selectedForm);
            if (current != Query) return; // une saisie plus récente a pris le relais

            Suggestions.Clear();
            foreach (var m in items) Suggestions.Add(m);
            ResultCountText = items.Count switch
            {
                0 => string.Empty,
                1 => "1 médicament",
                _ => $"{items.Count} médicaments"
            };

            DidYouMean = items.Count == 0 ? await medicines.DidYouMeanAsync(current) : null;
            if (items.Count == 0) State = ViewState.Empty;
        }, "Recherche…");
    }

    [RelayCommand]
    private async Task SelectForm(FilterChip chip)
    {
        foreach (var c in FormChips) c.IsSelected = c == chip;
        _selectedForm = chip.Value as string;
        await SearchNowAsync();
    }

    [RelayCommand]
    private Task SelectMedicine(Medicine medicine)
    {
        history.Add(medicine.DisplayName, medicine.Id);
        return Nav.GoToAsync(Routes.Results, new Dictionary<string, object> { [NavParams.MedicineId] = medicine.Id });
    }

    [RelayCommand]
    private Task OpenMedicine(Medicine medicine)
        => Nav.GoToAsync(Routes.MedicineDetail, new Dictionary<string, object> { [NavParams.MedicineId] = medicine.Id });

    [RelayCommand]
    private void AcceptSuggestion()
    {
        if (DidYouMean is { } m) Query = m.BrandName;
    }

    [RelayCommand]
    private void UseRecent(SearchHistoryEntry entry) => Query = entry.Query;

    [RelayCommand]
    private void ClearQuery() => Query = string.Empty;
}
