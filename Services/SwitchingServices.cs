using PharmaFind.Models;
using PharmaFind.Services.Api;
using PharmaFind.Services.Mock;

namespace PharmaFind.Services;

public class SwitchingAuthService : IAuthService
{
    private readonly MockAuthService _mock;
    private readonly ApiAuthService _api;
    private readonly DemoSettings _demo;

    public SwitchingAuthService(MockAuthService mock, ApiAuthService api, DemoSettings demo)
    {
        _mock = mock;
        _api = api;
        _demo = demo;

        _mock.CurrentUserChanged += (_, e) => { if (!_demo.UseRemoteApi) CurrentUserChanged?.Invoke(this, e); };
        _api.CurrentUserChanged += (_, e) => { if (_demo.UseRemoteApi) CurrentUserChanged?.Invoke(this, e); };
    }

    private IAuthService Current => _demo.UseRemoteApi ? _api : _mock;

    public UserAccount? CurrentUser => Current.CurrentUser ?? _mock.CurrentUser;
    public bool IsLoggedIn => Current.IsLoggedIn || _mock.IsLoggedIn;
    public event EventHandler? CurrentUserChanged;

    public async Task<UserAccount> LoginAsync(string email, string password)
    {
        if (_demo.UseRemoteApi)
        {
            try
            {
                return await _api.LoginAsync(email, password);
            }
            catch (Exception)
            {
                // Fallback to mock auth if API server is offline
                return await _mock.LoginAsync(email, password);
            }
        }
        return await _mock.LoginAsync(email, password);
    }

    public async Task<UserAccount> RegisterAsync(string name, string email, string password)
    {
        if (_demo.UseRemoteApi)
        {
            try
            {
                return await _api.RegisterAsync(name, email, password);
            }
            catch (Exception)
            {
                // Fallback to mock register if API server is offline
                return await _mock.RegisterAsync(name, email, password);
            }
        }
        return await _mock.RegisterAsync(name, email, password);
    }

    public async Task LogoutAsync()
    {
        if (_demo.UseRemoteApi)
        {
            try { await _api.LogoutAsync(); } catch { }
        }
        await _mock.LogoutAsync();
    }
}

public class SwitchingMedicineService(MockMedicineService mock, ApiMedicineService api, DemoSettings demo) : IMedicineService
{
    public async Task<IReadOnlyList<Medicine>> SuggestAsync(string query, string? form = null, CancellationToken ct = default)
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.SuggestAsync(query, form, ct); }
            catch (Exception) { }
        }
        return await mock.SuggestAsync(query, form, ct);
    }

    public async Task<Medicine?> DidYouMeanAsync(string query, CancellationToken ct = default)
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.DidYouMeanAsync(query, ct); }
            catch (Exception) { }
        }
        return await mock.DidYouMeanAsync(query, ct);
    }

    public async Task<IReadOnlyList<Medicine>> GetAllAsync(bool includeInactive = false)
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetAllAsync(includeInactive); }
            catch (Exception) { }
        }
        return await mock.GetAllAsync(includeInactive);
    }

    public async Task<IReadOnlyList<Medicine>> GetPopularAsync()
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetPopularAsync(); }
            catch (Exception) { }
        }
        return await mock.GetPopularAsync();
    }

    public async Task<IReadOnlyList<Medicine>> GetEquivalentsAsync(Medicine medicine)
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetEquivalentsAsync(medicine); }
            catch (Exception) { }
        }
        return await mock.GetEquivalentsAsync(medicine);
    }

    public async Task<Medicine?> GetByIdAsync(int id)
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetByIdAsync(id); }
            catch (Exception) { }
        }
        return await mock.GetByIdAsync(id);
    }

    public async Task<IReadOnlyList<string>> GetFormsAsync()
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetFormsAsync(); }
            catch (Exception) { }
        }
        return await mock.GetFormsAsync();
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync()
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetCategoriesAsync(); }
            catch (Exception) { }
        }
        return await mock.GetCategoriesAsync();
    }

    public async Task<Medicine> SaveAsync(Medicine medicine)
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.SaveAsync(medicine); }
            catch (Exception) { }
        }
        return await mock.SaveAsync(medicine);
    }
}

public class SwitchingPharmacyService(MockPharmacyService mock, ApiPharmacyService api, DemoSettings demo) : IPharmacyService
{
    public async Task<IReadOnlyList<PharmacyResult>> SearchAsync(int? medicineId, UserLocation location, int radiusKm)
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.SearchAsync(medicineId, location, radiusKm); }
            catch (Exception) { }
        }
        return await mock.SearchAsync(medicineId, location, radiusKm);
    }

    public async Task<Pharmacy?> GetByIdAsync(int id)
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetByIdAsync(id); }
            catch (Exception) { }
        }
        return await mock.GetByIdAsync(id);
    }

    public async Task<IReadOnlyList<StockItem>> GetStockAsync(int pharmacyId)
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetStockAsync(pharmacyId); }
            catch (Exception) { }
        }
        return await mock.GetStockAsync(pharmacyId);
    }

    public async Task<IReadOnlyList<Pharmacy>> GetAllAsync(ValidationStatus? status = null)
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetAllAsync(status); }
            catch (Exception) { }
        }
        return await mock.GetAllAsync(status);
    }

    public async Task RequestAccountAsync(Pharmacy request)
    {
        if (demo.UseRemoteApi)
        {
            try { await api.RequestAccountAsync(request); return; }
            catch (Exception) { }
        }
        await mock.RequestAccountAsync(request);
    }
}

public class SwitchingStockService(MockStockService mock, ApiStockService api, DemoSettings demo) : IStockService
{
    public async Task<Pharmacy> GetMyPharmacyAsync()
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetMyPharmacyAsync(); }
            catch (Exception) { }
        }
        return await mock.GetMyPharmacyAsync();
    }

    public async Task UpdateMyPharmacyAsync(Pharmacy pharmacy)
    {
        if (demo.UseRemoteApi)
        {
            try { await api.UpdateMyPharmacyAsync(pharmacy); return; }
            catch (Exception) { }
        }
        await mock.UpdateMyPharmacyAsync(pharmacy);
    }

    public async Task<IReadOnlyList<StockItem>> GetMyStockAsync()
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetMyStockAsync(); }
            catch (Exception) { }
        }
        return await mock.GetMyStockAsync();
    }

    public async Task<StockItem?> GetStockItemAsync(int stockId)
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetStockItemAsync(stockId); }
            catch (Exception) { }
        }
        return await mock.GetStockItemAsync(stockId);
    }

    public async Task<StockItem> SaveStockAsync(StockItem item)
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.SaveStockAsync(item); }
            catch (Exception) { }
        }
        return await mock.SaveStockAsync(item);
    }

    public async Task RemoveStockAsync(int stockId)
    {
        if (demo.UseRemoteApi)
        {
            try { await api.RemoveStockAsync(stockId); return; }
            catch (Exception) { }
        }
        await mock.RemoveStockAsync(stockId);
    }

    public async Task ConfirmAllUnchangedAsync()
    {
        if (demo.UseRemoteApi)
        {
            try { await api.ConfirmAllUnchangedAsync(); return; }
            catch (Exception) { }
        }
        await mock.ConfirmAllUnchangedAsync();
    }

    public async Task<IReadOnlyList<StockChange>> GetHistoryAsync()
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetHistoryAsync(); }
            catch (Exception) { }
        }
        return await mock.GetHistoryAsync();
    }

    public async Task<IReadOnlyList<AppNotification>> GetNotificationsAsync()
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetNotificationsAsync(); }
            catch (Exception) { }
        }
        return await mock.GetNotificationsAsync();
    }

    public async Task MarkNotificationsReadAsync()
    {
        if (demo.UseRemoteApi)
        {
            try { await api.MarkNotificationsReadAsync(); return; }
            catch (Exception) { }
        }
        await mock.MarkNotificationsReadAsync();
    }
}

public class SwitchingFavoritesService(MockFavoritesService mock, ApiFavoritesService api, DemoSettings demo) : IFavoritesService
{
    public async Task<IReadOnlyList<Pharmacy>> GetFavoritesAsync()
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetFavoritesAsync(); }
            catch (Exception) { }
        }
        return await mock.GetFavoritesAsync();
    }

    public async Task<bool> IsFavoriteAsync(int pharmacyId)
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.IsFavoriteAsync(pharmacyId); }
            catch (Exception) { }
        }
        return await mock.IsFavoriteAsync(pharmacyId);
    }

    public async Task<bool> ToggleAsync(int pharmacyId)
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.ToggleAsync(pharmacyId); }
            catch (Exception) { }
        }
        return await mock.ToggleAsync(pharmacyId);
    }
}

public class SwitchingReportService(MockReportService mock, ApiReportService api, DemoSettings demo) : IReportService
{
    public async Task SubmitAsync(PharmacyReport report)
    {
        if (demo.UseRemoteApi)
        {
            try { await api.SubmitAsync(report); return; }
            catch (Exception) { }
        }
        await mock.SubmitAsync(report);
    }

    public async Task<IReadOnlyList<PharmacyReport>> GetAllAsync(ReportStatus? status = null)
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetAllAsync(status); }
            catch (Exception) { }
        }
        return await mock.GetAllAsync(status);
    }

    public async Task<IReadOnlyList<PharmacyReport>> GetForPharmacyAsync(int pharmacyId)
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetForPharmacyAsync(pharmacyId); }
            catch (Exception) { }
        }
        return await mock.GetForPharmacyAsync(pharmacyId);
    }

    public async Task SetStatusAsync(int reportId, ReportStatus status)
    {
        if (demo.UseRemoteApi)
        {
            try { await api.SetStatusAsync(reportId, status); return; }
            catch (Exception) { }
        }
        await mock.SetStatusAsync(reportId, status);
    }
}

public class SwitchingAdminService(MockAdminService mock, ApiAdminService api, DemoSettings demo) : IAdminService
{
    public async Task<DashboardStats> GetStatsAsync()
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetStatsAsync(); }
            catch (Exception) { }
        }
        return await mock.GetStatsAsync();
    }

    public async Task SetPharmacyStatusAsync(int pharmacyId, ValidationStatus status, string? reason)
    {
        if (demo.UseRemoteApi)
        {
            try { await api.SetPharmacyStatusAsync(pharmacyId, status, reason); return; }
            catch (Exception) { }
        }
        await mock.SetPharmacyStatusAsync(pharmacyId, status, reason);
    }

    public async Task<IReadOnlyList<UserAccount>> GetUsersAsync()
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetUsersAsync(); }
            catch (Exception) { }
        }
        return await mock.GetUsersAsync();
    }

    public async Task SetUserStatusAsync(int userId, AccountStatus status)
    {
        if (demo.UseRemoteApi)
        {
            try { await api.SetUserStatusAsync(userId, status); return; }
            catch (Exception) { }
        }
        await mock.SetUserStatusAsync(userId, status);
    }

    public async Task<IReadOnlyList<AuditEntry>> GetAuditAsync()
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetAuditAsync(); }
            catch (Exception) { }
        }
        return await mock.GetAuditAsync();
    }

    public async Task<IReadOnlyList<ChartBar>> GetSearchesPerDayAsync()
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetSearchesPerDayAsync(); }
            catch (Exception) { }
        }
        return await mock.GetSearchesPerDayAsync();
    }

    public async Task<IReadOnlyList<ChartBar>> GetTopSearchedAsync()
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetTopSearchedAsync(); }
            catch (Exception) { }
        }
        return await mock.GetTopSearchedAsync();
    }

    public async Task<IReadOnlyList<ChartBar>> GetPharmaciesByDistrictAsync()
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetPharmaciesByDistrictAsync(); }
            catch (Exception) { }
        }
        return await mock.GetPharmaciesByDistrictAsync();
    }

    public async Task<IReadOnlyList<ChartBar>> GetFreshnessDistributionAsync()
    {
        if (demo.UseRemoteApi)
        {
            try { return await api.GetFreshnessDistributionAsync(); }
            catch (Exception) { }
        }
        return await mock.GetFreshnessDistributionAsync();
    }
}
