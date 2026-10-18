using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

public partial class AdminDashboardViewModel(
    ViewModelContext context,
    IAdminService admin,
    IPharmacyService pharmacies,
    IReportService reports) : BaseViewModel(context)
{
    public ObservableCollection<Pharmacy> Pending { get; } = [];
    public ObservableCollection<PharmacyReport> RecentReports { get; } = [];

    [ObservableProperty] private DashboardStats? stats;

    protected override bool ReloadOnAppearing => true;

    protected override async Task LoadAsync()
    {
        await RunLoadAsync(async () =>
        {
            Stats = await admin.GetStatsAsync();
            Pending.Clear();
            foreach (var p in (await pharmacies.GetAllAsync(ValidationStatus.EnAttente)).Take(3)) Pending.Add(p);
            RecentReports.Clear();
            foreach (var r in (await reports.GetAllAsync()).Where(r => r.Status is ReportStatus.Nouveau or ReportStatus.EnCours).Take(3))
                RecentReports.Add(r);
        });
    }

    [RelayCommand]
    private Task Review(Pharmacy pharmacy)
        => Nav.GoToAsync(Routes.AdminPharmacyReview, new Dictionary<string, object> { [NavParams.PharmacyId] = pharmacy.Id });

    [RelayCommand] private Task OpenPharmacies() => Nav.GoToAsync(Routes.AdminPharmacies);
    [RelayCommand] private Task OpenReports() => Nav.GoToAsync(Routes.AdminReports);
    [RelayCommand] private Task OpenMedicines() => Nav.GoToAsync(Routes.AdminMedicines);
    [RelayCommand] private Task OpenUsers() => Nav.GoToAsync(Routes.AdminUsers);
    [RelayCommand] private Task OpenStats() => Nav.GoToAsync(Routes.AdminStats);
}
