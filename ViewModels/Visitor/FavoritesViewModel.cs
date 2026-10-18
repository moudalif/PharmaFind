using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

public partial class FavoritesViewModel(ViewModelContext context, IFavoritesService favorites) : BaseViewModel(context)
{
    public ObservableCollection<Pharmacy> Favorites { get; } = [];

    [ObservableProperty] private bool requiresLogin;

    protected override bool ReloadOnAppearing => true;

    protected override async Task LoadAsync()
    {
        RequiresLogin = !Auth.IsLoggedIn;
        if (RequiresLogin)
        {
            State = ViewState.Content;
            return;
        }
        await RunLoadAsync(async () =>
        {
            Favorites.Clear();
            foreach (var p in await favorites.GetFavoritesAsync()) Favorites.Add(p);
            if (Favorites.Count == 0) State = ViewState.Empty;
        });
    }

    [RelayCommand]
    private Task Login() => Nav.GoToAsync(Routes.Login);

    [RelayCommand]
    private Task Register() => Nav.GoToAsync(Routes.Register);

    [RelayCommand]
    private Task Open(Pharmacy pharmacy)
        => Nav.GoToAsync(Routes.PharmacyDetail, new Dictionary<string, object> { [NavParams.PharmacyId] = pharmacy.Id });

    [RelayCommand]
    private async Task Remove(Pharmacy pharmacy)
    {
        if (!await Dialogs.ConfirmAsync("Retirer des favoris", $"Retirer « {pharmacy.Name} » de vos favoris ?", "Retirer")) return;
        if (await RunActionAsync(() => favorites.ToggleAsync(pharmacy.Id)))
        {
            Favorites.Remove(pharmacy);
            if (Favorites.Count == 0) State = ViewState.Empty;
        }
    }

    [RelayCommand]
    private Task Call(Pharmacy pharmacy) => ContactActions.CallAsync(pharmacy, Dialogs);
}
