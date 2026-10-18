using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PharmaFind.Helpers;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Journal d'audit : qui a fait quoi, sur quelle ressource, quand.</summary>
public partial class AdminAuditViewModel(ViewModelContext context, IAdminService admin) : BaseViewModel(context)
{
    private List<AuditEntry> _all = [];

    public ObservableCollection<AuditEntry> Entries { get; } = [];

    [ObservableProperty] private string filterText = string.Empty;

    protected override async Task LoadAsync()
    {
        await RunLoadAsync(async () =>
        {
            _all = (await admin.GetAuditAsync()).ToList();
            Apply();
        });
    }

    partial void OnFilterTextChanged(string value) => Apply();

    private void Apply()
    {
        Entries.Clear();
        foreach (var e in _all.Where(e => TextSearch.Matches(FilterText, e.Actor, e.Action, e.Resource))) Entries.Add(e);
        State = Entries.Count == 0 ? ViewState.Empty : ViewState.Content;
    }
}
