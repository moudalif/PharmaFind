using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Traitement des signalements des visiteurs.</summary>
public partial class AdminReportsViewModel(ViewModelContext context, IReportService reports) : BaseViewModel(context)
{
    private List<PharmacyReport> _all = [];

    public ObservableCollection<PharmacyReport> Items { get; } = [];
    public ObservableCollection<FilterChip> StatusChips { get; } = new(FilterChip.Create(
        ("Ouverts", "open"), ("Nouveaux", ReportStatus.Nouveau), ("En cours", ReportStatus.EnCours),
        ("Traités", ReportStatus.Traite), ("Rejetés", ReportStatus.Rejete), ("Tous", null)));

    private object? _filter = "open";

    protected override bool ReloadOnAppearing => true;

    protected override async Task LoadAsync()
    {
        foreach (var c in StatusChips) c.IsSelected = Equals(c.Value, _filter);
        await RunLoadAsync(async () =>
        {
            _all = (await reports.GetAllAsync()).ToList();
            Apply();
        });
    }

    private void Apply()
    {
        var list = _all.Where(r => _filter switch
        {
            "open" => r.Status is ReportStatus.Nouveau or ReportStatus.EnCours,
            ReportStatus s => r.Status == s,
            _ => true
        }).ToList();
        Items.Clear();
        foreach (var r in list) Items.Add(r);
        State = list.Count == 0 ? ViewState.Empty : ViewState.Content;
    }

    [RelayCommand]
    private void SelectStatus(FilterChip chip)
    {
        foreach (var c in StatusChips) c.IsSelected = c == chip;
        _filter = chip.Value;
        Apply();
    }

    [RelayCommand]
    private async Task Manage(PharmacyReport report)
    {
        const string inProgress = "Marquer « en cours »";
        const string done = "Marquer « traité »";
        const string reject = "Rejeter";
        const string open = "Voir la pharmacie";

        var choice = await Dialogs.ActionSheetAsync(
            $"{report.ReasonText} · {report.Subject}\n« {report.Comment} »", "Fermer", reject, inProgress, done, open);

        switch (choice)
        {
            case inProgress: await SetStatus(report, ReportStatus.EnCours); break;
            case done: await SetStatus(report, ReportStatus.Traite); break;
            case reject: await SetStatus(report, ReportStatus.Rejete); break;
            case open:
                await Nav.GoToAsync(Routes.AdminPharmacyReview, new Dictionary<string, object> { [NavParams.PharmacyId] = report.PharmacyId });
                break;
        }
    }

    private async Task SetStatus(PharmacyReport report, ReportStatus status)
    {
        if (await RunActionAsync(() => reports.SetStatusAsync(report.Id, status)))
            await LoadAsync();
    }
}
