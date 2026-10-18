using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Historique des recherches, conservé uniquement si l'utilisateur l'autorise.</summary>
public partial class HistoryViewModel(ViewModelContext context, IHistoryService history) : BaseViewModel(context)
{
    public ObservableCollection<SearchHistoryEntry> Entries { get; } = [];

    [ObservableProperty] private bool keepHistory = history.IsEnabled;

    protected override bool ReloadOnAppearing => true;

    protected override Task LoadAsync()
    {
        Entries.Clear();
        foreach (var e in history.GetEntries()) Entries.Add(e);
        State = Entries.Count == 0 ? ViewState.Empty : ViewState.Content;
        return Task.CompletedTask;
    }

    partial void OnKeepHistoryChanged(bool value)
    {
        history.IsEnabled = value;
        if (!value)
        {
            history.Clear();
            _ = LoadAsync();
        }
    }

    [RelayCommand]
    private async Task Clear()
    {
        if (Entries.Count == 0) return;
        if (!await Dialogs.ConfirmAsync("Effacer l'historique", "Toutes vos recherches récentes seront supprimées de cet appareil.", "Effacer")) return;
        history.Clear();
        await LoadAsync();
    }

    [RelayCommand]
    private Task Open(SearchHistoryEntry entry)
        => entry.MedicineId is { } id
            ? Nav.GoToAsync(Routes.Results, new Dictionary<string, object> { [NavParams.MedicineId] = id })
            : Nav.GoToAsync(Routes.Search, new Dictionary<string, object> { [NavParams.Query] = entry.Query });
}
