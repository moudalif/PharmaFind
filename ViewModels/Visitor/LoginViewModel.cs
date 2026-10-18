using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>
/// Connexion commune aux trois espaces. Le rôle réel est contrôlé par le serveur ;
/// l'application se contente d'ouvrir l'espace correspondant au rôle renvoyé.
/// </summary>
public partial class LoginViewModel(ViewModelContext context) : BaseViewModel(context), IQueryAttributable
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    [ObservableProperty] private string space = "visitor";
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private bool isPasswordHidden = true;
    [ObservableProperty] private string? errorText;
    [ObservableProperty] private string heading = "Connexion";
    [ObservableProperty] private string subheading = string.Empty;
    [ObservableProperty] private string demoHint = string.Empty;

    public bool IsVisitorSpace => Space == "visitor";
    public bool IsPharmacySpace => Space == "pharmacy";

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(NavParams.Space, out var s) && s is string value) Space = value;
    }

    partial void OnSpaceChanged(string value)
    {
        (Heading, Subheading, DemoHint, Title) = value switch
        {
            "pharmacy" => ("Espace pharmacie", "Accès réservé aux établissements validés par PharmaFind.", "Démo : pharmacie@demo.cm (ou attente@demo.cm) · Demo1234", "Espace pharmacie"),
            "admin" => ("Administration", "Accès réservé à l'équipe PharmaFind.", "Démo : admin@demo.cm · Demo1234", "Administration"),
            _ => ("Connexion", "Retrouvez vos pharmacies favorites sur tous vos appareils.", "Démo : visiteur@demo.cm · Demo1234", "Connexion")
        };
        OnPropertyChanged(nameof(IsVisitorSpace));
        OnPropertyChanged(nameof(IsPharmacySpace));
    }

    protected override Task LoadAsync()
    {
        OnSpaceChanged(Space);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void TogglePassword() => IsPasswordHidden = !IsPasswordHidden;

    [RelayCommand]
    private async Task Login()
    {
        ErrorText = null;
        if (!EmailRegex.IsMatch(Email.Trim())) { ErrorText = "Saisissez une adresse courriel valide."; return; }
        if (string.IsNullOrEmpty(Password)) { ErrorText = "Saisissez votre mot de passe."; return; }
        if (!Context.Network.IsConnected) { ErrorText = "Pas de connexion internet."; return; }

        try
        {
            IsWorking = true;
            var user = await Auth.LoginAsync(Email, Password);
            var expected = Space switch { "pharmacy" => UserRole.Pharmacie, "admin" => UserRole.Administrateur, _ => UserRole.Visiteur };
            if (user.Role != expected && Space != "visitor")
            {
                await Auth.LogoutAsync();
                ErrorText = "Ce compte n'a pas accès à cet espace.";
                return;
            }
            Password = string.Empty;
            await Nav.GoToAsync(user.Role switch
            {
                UserRole.Pharmacie => Routes.PharmacyDashboard,
                UserRole.Administrateur => Routes.AdminDashboard,
                _ => ".."
            });
        }
        catch (UnauthorizedAccessException ex) { ErrorText = ex.Message; }
        catch (ServiceUnavailableException ex) { ErrorText = ex.Message; }
        finally { IsWorking = false; }
    }

    [RelayCommand]
    private Task ForgotPassword()
        => Dialogs.AlertAsync("Mot de passe oublié", "La réinitialisation par courriel sera gérée par l'API. Elle n'est pas disponible dans la maquette.");

    [RelayCommand]
    private Task Register() => Nav.GoToAsync(Space == "pharmacy" ? Routes.AccountRequest : Routes.Register);
}
