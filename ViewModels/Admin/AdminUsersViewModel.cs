using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Helpers;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

public partial class AdminUsersViewModel(ViewModelContext context, IAdminService admin) : BaseViewModel(context)
{
    private List<UserAccount> _all = [];
    private UserRole? _role;

    public ObservableCollection<UserAccount> Items { get; } = [];
    public ObservableCollection<FilterChip> RoleChips { get; } = new(FilterChip.Create(
        ("Tous", null), ("Visiteurs", UserRole.Visiteur), ("Pharmacies", UserRole.Pharmacie), ("Administrateurs", UserRole.Administrateur)));

    [ObservableProperty] private string filterText = string.Empty;

    protected override async Task LoadAsync()
    {
        if (RoleChips.All(c => !c.IsSelected)) RoleChips[0].IsSelected = true;
        await RunLoadAsync(async () =>
        {
            _all = (await admin.GetUsersAsync()).ToList();
            Apply();
        });
    }

    partial void OnFilterTextChanged(string value) => Apply();

    private void Apply()
    {
        var list = _all
            .Where(u => _role is null || u.Role == _role)
            .Where(u => TextSearch.Matches(FilterText, u.Name, u.Email))
            .ToList();
        Items.Clear();
        foreach (var u in list) Items.Add(u);
        State = list.Count == 0 ? ViewState.Empty : ViewState.Content;
    }

    [RelayCommand]
    private void SelectRole(FilterChip chip)
    {
        foreach (var c in RoleChips) c.IsSelected = c == chip;
        _role = chip.Value as UserRole?;
        Apply();
    }

    [RelayCommand]
    private async Task Manage(UserAccount user)
    {
        if (user.Id == Auth.CurrentUser?.Id)
        {
            await Dialogs.AlertAsync("Votre compte", "Vous ne pouvez pas suspendre votre propre compte.");
            return;
        }
        var action = user.Status == AccountStatus.Actif ? "Suspendre le compte" : "Réactiver le compte";
        var isActive = user.Status == AccountStatus.Actif;
        string[] buttons = isActive ? Array.Empty<string>() : new[] { action };
        var choice = await Dialogs.ActionSheetAsync($"{user.Name} · {user.Email}", "Annuler",
            isActive ? action : null, buttons);
        if (choice != action) return;

        var newStatus = user.Status == AccountStatus.Actif ? AccountStatus.Suspendu : AccountStatus.Actif;
        if (await RunActionAsync(() => admin.SetUserStatusAsync(user.Id, newStatus)))
            await LoadAsync();
    }
}
