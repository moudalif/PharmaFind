using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Helpers;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Demande de compte professionnel avec justificatifs, validée ensuite par l'administrateur.</summary>
public partial class AccountRequestViewModel(ViewModelContext context, IPharmacyService pharmacies) : BaseViewModel(context)
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public List<string> Cities { get; } = ["Douala", "Yaoundé", "Bafoussam", "Garoua", "Autre ville"];
    public ObservableCollection<string> Documents { get; } = [];

    [ObservableProperty] private string pharmacyName = string.Empty;
    [ObservableProperty] private string licenseNumber = string.Empty;
    [ObservableProperty] private string pharmacist = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string phone = string.Empty;
    [ObservableProperty] private string? city = "Douala";
    [ObservableProperty] private string district = string.Empty;
    [ObservableProperty] private string neighborhood = string.Empty;
    [ObservableProperty] private string landmark = string.Empty;
    [ObservableProperty] private bool certifies;
    [ObservableProperty] private string? errorText;

    [RelayCommand]
    private async Task AddDocument()
    {
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Choisir un justificatif (PDF ou photo)" });
            if (file is not null && !Documents.Contains(file.FileName)) Documents.Add(file.FileName);
        }
        catch (Exception)
        {
            await Dialogs.AlertAsync("Fichier", "Impossible d'ouvrir le sélecteur de fichiers sur cet appareil.");
        }
    }

    [RelayCommand]
    private void RemoveDocument(string name) => Documents.Remove(name);

    [RelayCommand]
    private async Task Submit()
    {
        ErrorText = Validate();
        if (ErrorText is not null) return;

        var ok = await RunActionAsync(() => pharmacies.RequestAccountAsync(new Pharmacy
        {
            Name = PharmacyName.Trim(),
            LicenseNumber = LicenseNumber.Trim(),
            Pharmacist = Pharmacist.Trim(),
            Email = Email.Trim(),
            Phone = PhoneFormatter.Digits(Phone),
            City = City ?? string.Empty,
            District = District.Trim(),
            Neighborhood = Neighborhood.Trim(),
            Landmark = Landmark.Trim(),
            Documents = [.. Documents]
        }));
        if (!ok) return;

        await Dialogs.AlertAsync("Demande envoyée",
            "Votre demande est en cours de vérification par l'équipe PharmaFind. Vous recevrez un courriel une fois l'établissement validé. Il ne sera pas visible avant cette validation.");
        await Nav.GoBackAsync();
    }

    private string? Validate()
    {
        if (PharmacyName.Trim().Length < 3) return "Indiquez le nom de la pharmacie.";
        if (LicenseNumber.Trim().Length < 4) return "Indiquez le numéro d'autorisation d'exploitation.";
        if (Pharmacist.Trim().Length < 3) return "Indiquez le nom du pharmacien titulaire.";
        if (!EmailRegex.IsMatch(Email.Trim())) return "Saisissez une adresse courriel valide.";
        if (!PhoneFormatter.IsValid(Phone)) return "Numéro camerounais attendu : 9 chiffres (ex. 6 99 12 34 56).";
        if (string.IsNullOrEmpty(City) || Neighborhood.Trim().Length < 2) return "Indiquez au moins la ville et le quartier.";
        if (Documents.Count == 0) return "Ajoutez au moins un justificatif (autorisation d'exercice, registre…).";
        if (!Certifies) return "Vous devez certifier l'exactitude des informations.";
        return null;
    }
}
