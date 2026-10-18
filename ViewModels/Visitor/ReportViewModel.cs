using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Signalement d'un stock incorrect ou d'une information de pharmacie erronée (anonyme possible).</summary>
public partial class ReportViewModel(
    ViewModelContext context,
    IReportService reports,
    IPharmacyService pharmacies,
    IMedicineService medicines) : BaseViewModel(context), IQueryAttributable
{
    public const int MaxCommentLength = 500;

    private int _pharmacyId;
    private int? _medicineId;

    public ObservableCollection<ReasonOption> Reasons { get; } =
        new(Enum.GetValues<ReportReason>().Select(r => new ReasonOption(r)));

    [ObservableProperty] private string pharmacyName = string.Empty;
    [ObservableProperty] private string? medicineName;
    [ObservableProperty] private bool includeMedicine = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CounterText))]
    private string comment = string.Empty;

    [ObservableProperty] private string? errorText;

    public string CounterText => $"{Comment.Length}/{MaxCommentLength}";

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _pharmacyId = ReadInt(query, NavParams.PharmacyId) ?? 0;
        _medicineId = ReadInt(query, NavParams.MedicineId);
    }

    protected override async Task LoadAsync()
    {
        await RunLoadAsync(async () =>
        {
            PharmacyName = (await pharmacies.GetByIdAsync(_pharmacyId))?.Name ?? "Pharmacie";
            MedicineName = _medicineId is { } id ? (await medicines.GetByIdAsync(id))?.DisplayName : null;
            if (MedicineName is not null) Reasons[0].IsSelected = true;
        });
    }

    [RelayCommand]
    private void SelectReason(ReasonOption option)
    {
        foreach (var r in Reasons) r.IsSelected = r == option;
        ErrorText = null;
    }

    [RelayCommand]
    private async Task Submit()
    {
        var reason = Reasons.FirstOrDefault(r => r.IsSelected);
        if (reason is null) { ErrorText = "Choisissez le motif du signalement."; return; }
        if (reason.Reason == ReportReason.Autre && Comment.Trim().Length < 10) { ErrorText = "Décrivez le problème en quelques mots."; return; }

        var sent = await RunActionAsync(() => reports.SubmitAsync(new PharmacyReport
        {
            Author = Auth.CurrentUser?.Email ?? "Visiteur anonyme",
            PharmacyId = _pharmacyId,
            PharmacyName = PharmacyName,
            MedicineName = IncludeMedicine ? MedicineName : null,
            Reason = reason.Reason,
            Comment = Comment.Trim()
        }));

        if (!sent) return;
        await Dialogs.AlertAsync("Merci", "Votre signalement a été transmis. L'équipe PharmaFind et la pharmacie concernée vont le vérifier.");
        await Nav.GoBackAsync();
    }
}
