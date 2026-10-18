using PharmaFind.Views;

namespace PharmaFind;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Visiteur
        Routing.RegisterRoute(Routes.Search, typeof(SearchPage));
        Routing.RegisterRoute(Routes.Results, typeof(ResultsPage));
        Routing.RegisterRoute(Routes.ResultsMap, typeof(MapPage));
        Routing.RegisterRoute(Routes.PharmacyDetail, typeof(PharmacyDetailPage));
        Routing.RegisterRoute(Routes.MedicineDetail, typeof(MedicineDetailPage));
        Routing.RegisterRoute(Routes.History, typeof(HistoryPage));
        Routing.RegisterRoute(Routes.Login, typeof(LoginPage));
        Routing.RegisterRoute(Routes.Register, typeof(RegisterPage));
        Routing.RegisterRoute(Routes.Report, typeof(ReportPage));
        Routing.RegisterRoute(Routes.LocationPicker, typeof(LocationPickerPage));
        Routing.RegisterRoute(Routes.Privacy, typeof(PrivacyPage));

        // Pharmacie
        Routing.RegisterRoute(Routes.AccountRequest, typeof(AccountRequestPage));
        Routing.RegisterRoute(Routes.StockEdit, typeof(StockEditPage));
        Routing.RegisterRoute(Routes.PharmacyHistory, typeof(PharmacyHistoryPage));

        // Administration
        Routing.RegisterRoute(Routes.AdminPharmacyReview, typeof(AdminPharmacyReviewPage));
        Routing.RegisterRoute(Routes.AdminMedicineEdit, typeof(AdminMedicineEditPage));
        Routing.RegisterRoute(Routes.AdminUsers, typeof(AdminUsersPage));
        Routing.RegisterRoute(Routes.AdminStats, typeof(AdminStatsPage));
        Routing.RegisterRoute(Routes.AdminAudit, typeof(AdminAuditPage));
        Routing.RegisterRoute(Routes.AdminSettings, typeof(AdminSettingsPage));
    }
}
