using Microsoft.Extensions.DependencyInjection;
using PharmaFind.Services;
using PharmaFind.Services.Maps;
using PharmaFind.Services.Mock;
using PharmaFind.ViewModels;
using PharmaFind.Views;

namespace PharmaFind;

public static class ServiceRegistration
{
    /// <summary>
    /// Services. Pour brancher le futur backend, remplacer uniquement les lignes « Mock… »
    /// par des implémentations HttpClient (ex. ApiPharmacyService) : les ViewModels ne changent pas.
    /// </summary>
    public static IServiceCollection AddPharmaFindServices(this IServiceCollection services)
    {
        services.AddSingleton<DemoSettings>();
        services.AddSingleton<SearchContext>();
        services.AddSingleton<INavigationService, ShellNavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<INetworkService, NetworkService>();
        services.AddSingleton<ILocationService, DeviceLocationService>();
        services.AddSingleton<IMapProvider, LeafletMapProvider>();
        services.AddSingleton<IHistoryService, LocalHistoryService>();
        services.AddSingleton<ViewModelContext>();

        // --- Données fictives (à remplacer par l'API) ---
        services.AddSingleton<MockDataStore>();
        services.AddSingleton<IAuthService, MockAuthService>();
        services.AddSingleton<IMedicineService, MockMedicineService>();
        services.AddSingleton<IPharmacyService, MockPharmacyService>();
        services.AddSingleton<IStockService, MockStockService>();
        services.AddSingleton<IFavoritesService, MockFavoritesService>();
        services.AddSingleton<IReportService, MockReportService>();
        services.AddSingleton<IAdminService, MockAdminService>();
        return services;
    }

    public static IServiceCollection AddPharmaFindPages(this IServiceCollection services)
    {
        services.AddSingleton<AppShell>();
        services.AddTransient<LaunchPage>();
        services.AddTransient<PrivacyPage>();

        // Visiteur
        services.AddPage<HomePage, HomeViewModel>();
        services.AddPage<SearchPage, SearchViewModel>();
        services.AddPage<ResultsPage, ResultsViewModel>();
        services.AddPage<MapPage, MapViewModel>();
        services.AddPage<PharmacyDetailPage, PharmacyDetailViewModel>();
        services.AddPage<MedicineDetailPage, MedicineDetailViewModel>();
        services.AddPage<FavoritesPage, FavoritesViewModel>();
        services.AddPage<HistoryPage, HistoryViewModel>();
        services.AddPage<ProfilePage, ProfileViewModel>();
        services.AddPage<LoginPage, LoginViewModel>();
        services.AddPage<RegisterPage, RegisterViewModel>();
        services.AddPage<ReportPage, ReportViewModel>();
        services.AddPage<LocationPickerPage, LocationPickerViewModel>();

        // Pharmacie
        services.AddPage<AccountRequestPage, AccountRequestViewModel>();
        services.AddPage<PharmacyDashboardPage, PharmacyDashboardViewModel>();
        services.AddPage<StockPage, StockViewModel>();
        services.AddPage<StockEditPage, StockEditViewModel>();
        services.AddPage<CatalogPage, CatalogViewModel>();
        services.AddPage<NotificationsPage, NotificationsViewModel>();
        services.AddPage<PharmacyInfoPage, PharmacyInfoViewModel>();
        services.AddPage<PharmacyHistoryPage, PharmacyHistoryViewModel>();

        // Administration
        services.AddPage<AdminDashboardPage, AdminDashboardViewModel>();
        services.AddPage<AdminPharmaciesPage, AdminPharmaciesViewModel>();
        services.AddPage<AdminPharmacyReviewPage, AdminPharmacyReviewViewModel>();
        services.AddPage<AdminMedicinesPage, AdminMedicinesViewModel>();
        services.AddPage<AdminMedicineEditPage, AdminMedicineEditViewModel>();
        services.AddPage<AdminUsersPage, AdminUsersViewModel>();
        services.AddPage<AdminReportsPage, AdminReportsViewModel>();
        services.AddPage<AdminStatsPage, AdminStatsViewModel>();
        services.AddPage<AdminAuditPage, AdminAuditViewModel>();
        services.AddPage<AdminMorePage, AdminMoreViewModel>();
        services.AddPage<AdminSettingsPage, AdminSettingsViewModel>();
        return services;
    }

    private static void AddPage<TPage, TViewModel>(this IServiceCollection services)
        where TPage : Page where TViewModel : class
    {
        services.AddTransient<TPage>();
        services.AddTransient<TViewModel>();
    }
}
