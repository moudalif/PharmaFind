using CommunityToolkit.Mvvm.Input;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

public partial class AdminMoreViewModel(ViewModelContext context) : BaseViewModel(context)
{
    public string AdminName => Auth.CurrentUser?.Name ?? "Administrateur";
    public string AdminEmail => Auth.CurrentUser?.Email ?? string.Empty;

    protected override bool ReloadOnAppearing => true;

    protected override Task LoadAsync()
    {
        OnPropertyChanged(nameof(AdminName));
        OnPropertyChanged(nameof(AdminEmail));
        return Task.CompletedTask;
    }

    [RelayCommand] private Task OpenUsers() => Nav.GoToAsync(Routes.AdminUsers);
    [RelayCommand] private Task OpenStats() => Nav.GoToAsync(Routes.AdminStats);
    [RelayCommand] private Task OpenAudit() => Nav.GoToAsync(Routes.AdminAudit);
    [RelayCommand] private Task OpenSettings() => Nav.GoToAsync(Routes.AdminSettings);

    [RelayCommand]
    private async Task Logout()
    {
        if (!await Dialogs.ConfirmAsync("Déconnexion", "Quitter l'administration ?", "Se déconnecter")) return;
        await Auth.LogoutAsync();
        await Nav.GoToAsync(Routes.VisitorHome);
    }
}
