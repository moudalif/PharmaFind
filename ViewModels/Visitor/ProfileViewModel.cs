using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

public partial class ProfileViewModel(ViewModelContext context, IMapProvider mapProvider) : BaseViewModel(context)
{
    public DemoSettings Demo => Context.Demo;

    [ObservableProperty] private bool isLoggedIn;
    [ObservableProperty] private string userName = string.Empty;
    [ObservableProperty] private string userEmail = string.Empty;
    [ObservableProperty] private string initials = string.Empty;

    public string VersionText => $"PharmaFind {AppInfo.Current.VersionString} · maquette d'interface · carte : {mapProvider.Name}";

    protected override bool ReloadOnAppearing => true;

    protected override Task LoadAsync()
    {
        var user = Auth.CurrentUser;
        IsLoggedIn = user is { Role: UserRole.Visiteur };
        UserName = user?.Name ?? string.Empty;
        UserEmail = user?.Email ?? string.Empty;
        Initials = user?.Initials ?? string.Empty;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task Login() => Nav.GoToAsync(Routes.Login);

    [RelayCommand]
    private Task Register() => Nav.GoToAsync(Routes.Register);

    [RelayCommand]
    private async Task Logout()
    {
        if (!await Dialogs.ConfirmAsync("Déconnexion", "Voulez-vous vous déconnecter ?", "Se déconnecter")) return;
        await Auth.LogoutAsync();
        await LoadAsync();
    }

    [RelayCommand]
    private Task OpenHistory() => Nav.GoToAsync(Routes.History);

    [RelayCommand]
    private Task OpenFavorites() => Nav.GoToAsync(Routes.VisitorFavorites);

    [RelayCommand]
    private Task OpenLocation() => Nav.GoToAsync(Routes.LocationPicker);

    [RelayCommand]
    private Task OpenPrivacy() => Nav.GoToAsync(Routes.Privacy);

    [RelayCommand]
    private Task OpenPharmacySpace()
        => Nav.GoToAsync(Routes.Login, new Dictionary<string, object> { [NavParams.Space] = "pharmacy" });

    [RelayCommand]
    private Task OpenAdminSpace()
        => Nav.GoToAsync(Routes.Login, new Dictionary<string, object> { [NavParams.Space] = "admin" });

    [RelayCommand]
    private Task RequestPharmacyAccount() => Nav.GoToAsync(Routes.AccountRequest);

    [RelayCommand]
    private Task ChangeLanguage()
        => Dialogs.AlertAsync("Langue", "L'interface est en français. La version anglaise est prévue : les textes seront regroupés dans des fichiers de ressources.");
}
