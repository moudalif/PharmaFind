using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Helpers;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Informations de l'établissement : coordonnées, position GPS, horaires, garde.</summary>
public partial class PharmacyInfoViewModel(
    ViewModelContext context,
    IStockService stock,
    ILocationService location) : BaseViewModel(context)
{
    private Pharmacy? _pharmacy;

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string licenseNumber = string.Empty;
    [ObservableProperty] private string phone = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string city = string.Empty;
    [ObservableProperty] private string district = string.Empty;
    [ObservableProperty] private string neighborhood = string.Empty;
    [ObservableProperty] private string landmark = string.Empty;
    [ObservableProperty] private string latitudeText = string.Empty;
    [ObservableProperty] private string longitudeText = string.Empty;
    [ObservableProperty] private TimeSpan opensAt;
    [ObservableProperty] private TimeSpan closesAt;
    [ObservableProperty] private bool is24h;
    [ObservableProperty] private bool openOnSunday;
    [ObservableProperty] private bool isOnDuty;
    [ObservableProperty] private string? errorText;

    protected override async Task LoadAsync()
    {
        await RunLoadAsync(async () =>
        {
            _pharmacy = await stock.GetMyPharmacyAsync();
            Name = _pharmacy.Name;
            LicenseNumber = _pharmacy.LicenseNumber;
            Phone = PhoneFormatter.Digits(_pharmacy.Phone);
            Email = _pharmacy.Email;
            City = _pharmacy.City;
            District = _pharmacy.District;
            Neighborhood = _pharmacy.Neighborhood;
            Landmark = _pharmacy.Landmark;
            LatitudeText = _pharmacy.Latitude?.ToString("0.000000", CultureInfo.InvariantCulture) ?? string.Empty;
            LongitudeText = _pharmacy.Longitude?.ToString("0.000000", CultureInfo.InvariantCulture) ?? string.Empty;
            OpensAt = _pharmacy.OpensAt;
            ClosesAt = _pharmacy.ClosesAt;
            Is24h = _pharmacy.Is24h;
            OpenOnSunday = _pharmacy.OpenOnSunday;
            IsOnDuty = _pharmacy.IsOnDuty;
        });
    }

    [RelayCommand]
    private async Task UseCurrentPosition()
    {
        if (!await Dialogs.ConfirmAsync("Position de l'établissement",
                "Utilisez cette fonction uniquement si vous êtes à l'intérieur de la pharmacie.", "Je suis sur place")) return;
        IsWorking = true;
        var result = await location.GetCurrentLocationAsync();
        IsWorking = false;
        if (result.Location is { } loc)
        {
            LatitudeText = loc.Latitude.ToString("0.000000", CultureInfo.InvariantCulture);
            LongitudeText = loc.Longitude.ToString("0.000000", CultureInfo.InvariantCulture);
        }
        else
        {
            await Dialogs.AlertAsync("Position indisponible", "Impossible d'obtenir la position. Saisissez les coordonnées manuellement.");
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        ErrorText = Validate(out var lat, out var lng);
        if (ErrorText is not null || _pharmacy is null) return;

        var updated = _pharmacy.Clone();
        updated.Phone = PhoneFormatter.Digits(Phone);
        updated.Email = Email.Trim();
        updated.District = District.Trim();
        updated.Neighborhood = Neighborhood.Trim();
        updated.Landmark = Landmark.Trim();
        updated.Latitude = lat;
        updated.Longitude = lng;
        updated.OpensAt = OpensAt;
        updated.ClosesAt = ClosesAt;
        updated.Is24h = Is24h;
        updated.OpenOnSunday = OpenOnSunday;
        updated.IsOnDuty = IsOnDuty;

        if (await RunActionAsync(() => stock.UpdateMyPharmacyAsync(updated)))
        {
            _pharmacy = updated;
            await Dialogs.AlertAsync("Enregistré", "Les informations de l'établissement ont été mises à jour.");
        }
    }

    private string? Validate(out double? lat, out double? lng)
    {
        lat = lng = null;
        if (!PhoneFormatter.IsValid(Phone)) return "Numéro camerounais attendu : 9 chiffres (ex. 6 99 12 34 56).";
        if (Neighborhood.Trim().Length < 2) return "Le quartier est obligatoire.";
        if (!Is24h && ClosesAt <= OpensAt) return "L'heure de fermeture doit être après l'heure d'ouverture.";

        var hasLat = !string.IsNullOrWhiteSpace(LatitudeText);
        var hasLng = !string.IsNullOrWhiteSpace(LongitudeText);
        if (hasLat != hasLng) return "Renseignez la latitude et la longitude, ou aucune des deux.";
        if (!hasLat) return null;

        if (!double.TryParse(LatitudeText.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var la) ||
            !double.TryParse(LongitudeText.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var lo))
            return "Coordonnées GPS invalides.";
        // Cameroun : latitude ≈ 1,6 à 13,1 ; longitude ≈ 8,4 à 16,2.
        if (la is < 1.5 or > 13.2 || lo is < 8.3 or > 16.3)
            return "Ces coordonnées sont hors du Cameroun. Vérifiez-les.";
        lat = la;
        lng = lo;
        return null;
    }

    [RelayCommand]
    private Task OpenHistory() => Nav.GoToAsync(Routes.PharmacyHistory);

    [RelayCommand]
    private async Task Logout()
    {
        if (!await Dialogs.ConfirmAsync("Déconnexion", "Quitter l'espace pharmacie ?", "Se déconnecter")) return;
        await Auth.LogoutAsync();
        await Nav.GoToAsync(Routes.VisitorHome);
    }
}
