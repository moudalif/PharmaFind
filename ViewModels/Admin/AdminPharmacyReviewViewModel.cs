using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Examen d'une pharmacie : justificatifs, données, puis validation, refus, suspension ou réactivation.</summary>
public partial class AdminPharmacyReviewViewModel(
    ViewModelContext context,
    IPharmacyService pharmacies,
    IAdminService admin,
    IReportService reports) : BaseViewModel(context), IQueryAttributable
{
    private int _pharmacyId;

    public ObservableCollection<string> Documents { get; } = [];

    [ObservableProperty] private Pharmacy? pharmacy;
    [ObservableProperty] private int stockCount;
    [ObservableProperty] private int staleCount;
    [ObservableProperty] private int reportCount;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanValidateOnly))]
    private bool canValidate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanValidateOnly))]
    private bool canRefuse;
    [ObservableProperty] private bool canSuspend;
    [ObservableProperty] private bool canReactivate;

    /// <summary>Validation seule (ex. demande précédemment refusée) : un bouton unique.</summary>
    public bool CanValidateOnly => CanValidate && !CanRefuse;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _pharmacyId = ReadInt(query, NavParams.PharmacyId) ?? 0;

    protected override async Task LoadAsync()
    {
        await RunLoadAsync(async () =>
        {
            Pharmacy = await pharmacies.GetByIdAsync(_pharmacyId);
            if (Pharmacy is null)
            {
                StateMessage = "Pharmacie introuvable.";
                State = ViewState.Error;
                return;
            }
            Title = Pharmacy.Name;
            Documents.Clear();
            foreach (var d in Pharmacy.Documents) Documents.Add(d);

            var stock = await pharmacies.GetStockAsync(_pharmacyId);
            StockCount = stock.Count;
            StaleCount = stock.Count(s => s.Freshness == StockFreshness.Ancienne);
            ReportCount = (await reports.GetForPharmacyAsync(_pharmacyId)).Count;

            CanValidate = Pharmacy.Status is ValidationStatus.EnAttente or ValidationStatus.Refusee;
            CanRefuse = Pharmacy.Status == ValidationStatus.EnAttente;
            CanSuspend = Pharmacy.Status == ValidationStatus.Validee;
            CanReactivate = Pharmacy.Status == ValidationStatus.Suspendue;
        });
    }

    [RelayCommand]
    private Task Validate() => ChangeStatusAsync(ValidationStatus.Validee, "Valider la pharmacie",
        "L'établissement deviendra visible des visiteurs et pourra gérer son stock.", askReason: false);

    [RelayCommand]
    private Task Refuse() => ChangeStatusAsync(ValidationStatus.Refusee, "Refuser la demande",
        "Indiquez le motif du refus (communiqué à la pharmacie).", askReason: true);

    [RelayCommand]
    private Task Suspend() => ChangeStatusAsync(ValidationStatus.Suspendue, "Suspendre la pharmacie",
        "Elle disparaîtra des résultats. Indiquez le motif de la suspension.", askReason: true);

    [RelayCommand]
    private Task Reactivate() => ChangeStatusAsync(ValidationStatus.Validee, "Réactiver la pharmacie",
        "L'établissement sera de nouveau visible des visiteurs.", askReason: false);

    private async Task ChangeStatusAsync(ValidationStatus status, string title, string message, bool askReason)
    {
        if (Pharmacy is null) return;
        string? reason = null;
        if (askReason)
        {
            reason = await Dialogs.PromptAsync(title, message, "Motif");
            if (string.IsNullOrWhiteSpace(reason)) return;
        }
        else if (!await Dialogs.ConfirmAsync(title, message))
        {
            return;
        }

        if (await RunActionAsync(() => admin.SetPharmacyStatusAsync(Pharmacy.Id, status, reason)))
            await LoadAsync();
    }

    [RelayCommand]
    private Task OpenDocument(string name)
        => Dialogs.AlertAsync(name, "Aperçu du justificatif : le fichier sera servi par l'API de manière sécurisée. Non disponible dans la maquette.");

    [RelayCommand]
    private Task Call() => Pharmacy is null ? Task.CompletedTask : ContactActions.CallAsync(Pharmacy, Dialogs);
}
