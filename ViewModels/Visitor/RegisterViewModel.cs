using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

public partial class RegisterViewModel(ViewModelContext context) : BaseViewModel(context)
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string confirmation = string.Empty;
    [ObservableProperty] private bool acceptsTerms;
    [ObservableProperty] private string? errorText;

    [RelayCommand]
    private async Task Register()
    {
        ErrorText = Validate();
        if (ErrorText is not null) return;

        try
        {
            IsWorking = true;
            await Auth.RegisterAsync(Name, Email, Password);
            await Dialogs.AlertAsync("Compte créé", "Bienvenue sur PharmaFind ! Vous pouvez maintenant enregistrer vos pharmacies favorites.");
            await Nav.GoToAsync(Routes.VisitorProfile);
        }
        catch (InvalidOperationException ex) { ErrorText = ex.Message; }
        catch (ServiceUnavailableException ex) { ErrorText = ex.Message; }
        finally { IsWorking = false; }
    }

    private string? Validate()
    {
        if (Name.Trim().Length < 2) return "Indiquez votre nom.";
        if (!EmailRegex.IsMatch(Email.Trim())) return "Saisissez une adresse courriel valide.";
        if (Password.Length < 8 || !Password.Any(char.IsDigit) || !Password.Any(char.IsLetter))
            return "Le mot de passe doit contenir au moins 8 caractères, dont une lettre et un chiffre.";
        if (Password != Confirmation) return "Les deux mots de passe ne correspondent pas.";
        if (!AcceptsTerms) return "Vous devez accepter la politique de confidentialité.";
        if (!Context.Network.IsConnected) return "Pas de connexion internet.";
        return null;
    }

    [RelayCommand]
    private Task OpenPrivacy() => Nav.GoToAsync(Routes.Privacy);
}
