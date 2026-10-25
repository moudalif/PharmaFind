using System.Net.Http.Json;
using PharmaFind.Models;

namespace PharmaFind.Services.Api;

public class ApiAuthService : IAuthService
{
    private readonly ApiClient _api;
    public UserAccount? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;
    public event EventHandler? CurrentUserChanged;

    private record AuthApiResult(string Token, UserAccount User);

    public ApiAuthService(ApiClient api)
    {
        _api = api;
    }

    public async Task<UserAccount> LoginAsync(string email, string password)
    {
        var response = await _api.PostAsync<object, AuthApiResult>("api/auth/login", new { Email = email, Password = password });
        _api.Token = response.Token;
        CurrentUser = response.User;
        CurrentUserChanged?.Invoke(this, EventArgs.Empty);
        return CurrentUser;
    }

    public async Task<UserAccount> RegisterAsync(string name, string email, string password)
    {
        var response = await _api.PostAsync<object, AuthApiResult>("api/auth/register", new { Name = name, Email = email, Password = password });
        _api.Token = response.Token;
        CurrentUser = response.User;
        CurrentUserChanged?.Invoke(this, EventArgs.Empty);
        return CurrentUser;
    }

    public Task LogoutAsync()
    {
        _api.Token = null;
        CurrentUser = null;
        CurrentUserChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}

public class ApiMedicineService : IMedicineService
{
    private readonly ApiClient _api;

    public ApiMedicineService(ApiClient api)
    {
        _api = api;
    }

    public async Task<IReadOnlyList<Medicine>> SuggestAsync(string query, string? form = null, CancellationToken ct = default)
    {
        var q = Uri.EscapeDataString(query);
        var url = string.IsNullOrEmpty(form) ? $"api/medicines/suggest?query={q}" : $"api/medicines/suggest?query={q}&form={Uri.EscapeDataString(form)}";
        return await _api.GetAsync<List<Medicine>>(url, ct);
    }

    public async Task<Medicine?> DidYouMeanAsync(string query, CancellationToken ct = default)
    {
        var q = Uri.EscapeDataString(query);
        return await _api.GetAsync<Medicine?>($"api/medicines/did-you-mean?query={q}", ct);
    }

    public async Task<IReadOnlyList<Medicine>> GetAllAsync(bool includeInactive = false)
    {
        return await _api.GetAsync<List<Medicine>>($"api/medicines?includeInactive={includeInactive}");
    }

    public async Task<IReadOnlyList<Medicine>> GetPopularAsync()
    {
        return await _api.GetAsync<List<Medicine>>("api/medicines/popular");
    }

    public async Task<IReadOnlyList<Medicine>> GetEquivalentsAsync(Medicine medicine)
    {
        return await _api.GetAsync<List<Medicine>>($"api/medicines/{medicine.Id}/equivalents");
    }

    public async Task<Medicine?> GetByIdAsync(int id)
    {
        return await _api.GetAsync<Medicine?>($"api/medicines/{id}");
    }

    public async Task<IReadOnlyList<string>> GetFormsAsync()
    {
        return await _api.GetAsync<List<string>>("api/medicines/forms");
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync()
    {
        return await _api.GetAsync<List<string>>("api/medicines/categories");
    }

    public async Task<Medicine> SaveAsync(Medicine medicine)
    {
        return await _api.PostAsync<Medicine, Medicine>("api/medicines", medicine);
    }
}

public class ApiPharmacyService : IPharmacyService
{
    private readonly ApiClient _api;

    public ApiPharmacyService(ApiClient api)
    {
        _api = api;
    }

    public async Task<IReadOnlyList<PharmacyResult>> SearchAsync(int? medicineId, UserLocation location, int radiusKm)
    {
        var lat = location.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var lon = location.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var url = $"api/pharmacies/search?lat={lat}&lon={lon}&radiusKm={radiusKm}";
        if (medicineId.HasValue)
        {
            url += $"&medicineId={medicineId.Value}";
        }

        var results = await _api.GetAsync<List<PharmacyResult>>(url);
        return results;
    }

    public async Task<Pharmacy?> GetByIdAsync(int id)
    {
        return await _api.GetAsync<Pharmacy?>($"api/pharmacies/{id}");
    }

    public async Task<IReadOnlyList<StockItem>> GetStockAsync(int pharmacyId)
    {
        return await _api.GetAsync<List<StockItem>>($"api/pharmacies/{pharmacyId}/stock");
    }

    public async Task<IReadOnlyList<Pharmacy>> GetAllAsync(ValidationStatus? status = null)
    {
        var url = status.HasValue ? $"api/pharmacies?status={status.Value}" : "api/pharmacies";
        return await _api.GetAsync<List<Pharmacy>>(url);
    }

    public async Task RequestAccountAsync(Pharmacy request)
    {
        await _api.PostAsync("api/pharmacies/request-account", request);
    }
}

public class ApiStockService : IStockService
{
    private readonly ApiClient _api;

    public ApiStockService(ApiClient api)
    {
        _api = api;
    }

    public async Task<Pharmacy> GetMyPharmacyAsync()
    {
        return await _api.GetAsync<Pharmacy>("api/stock/my-pharmacy");
    }

    public async Task UpdateMyPharmacyAsync(Pharmacy pharmacy)
    {
        await _api.PutAsync<Pharmacy, Pharmacy>("api/stock/my-pharmacy", pharmacy);
    }

    public async Task<IReadOnlyList<StockItem>> GetMyStockAsync()
    {
        return await _api.GetAsync<List<StockItem>>("api/stock/my-stock");
    }

    public async Task<StockItem?> GetStockItemAsync(int stockId)
    {
        return await _api.GetAsync<StockItem?>($"api/stock/{stockId}");
    }

    public async Task<StockItem> SaveStockAsync(StockItem item)
    {
        return await _api.PostAsync<StockItem, StockItem>("api/stock", item);
    }

    public async Task RemoveStockAsync(int stockId)
    {
        await _api.DeleteAsync($"api/stock/{stockId}");
    }

    public async Task ConfirmAllUnchangedAsync()
    {
        await _api.PostAsync("api/stock/confirm-all", new { });
    }

    public async Task<IReadOnlyList<StockChange>> GetHistoryAsync()
    {
        return await _api.GetAsync<List<StockChange>>("api/stock/history");
    }

    public async Task<IReadOnlyList<AppNotification>> GetNotificationsAsync()
    {
        return await _api.GetAsync<List<AppNotification>>("api/stock/notifications");
    }

    public async Task MarkNotificationsReadAsync()
    {
        await _api.PostAsync("api/stock/notifications/read", new { });
    }
}

public class ApiFavoritesService : IFavoritesService
{
    private readonly ApiClient _api;

    public ApiFavoritesService(ApiClient api)
    {
        _api = api;
    }

    public async Task<IReadOnlyList<Pharmacy>> GetFavoritesAsync()
    {
        return await _api.GetAsync<List<Pharmacy>>("api/favorites");
    }

    public async Task<bool> IsFavoriteAsync(int pharmacyId)
    {
        return await _api.GetAsync<bool>($"api/favorites/{pharmacyId}/check");
    }

    public async Task<bool> ToggleAsync(int pharmacyId)
    {
        return await _api.PostAsync<object, bool>($"api/favorites/{pharmacyId}/toggle", new { });
    }
}

public class ApiReportService : IReportService
{
    private readonly ApiClient _api;

    public ApiReportService(ApiClient api)
    {
        _api = api;
    }

    public async Task SubmitAsync(PharmacyReport report)
    {
        await _api.PostAsync("api/reports", report);
    }

    public async Task<IReadOnlyList<PharmacyReport>> GetAllAsync(ReportStatus? status = null)
    {
        var url = status.HasValue ? $"api/reports?status={status.Value}" : "api/reports";
        return await _api.GetAsync<List<PharmacyReport>>(url);
    }

    public async Task<IReadOnlyList<PharmacyReport>> GetForPharmacyAsync(int pharmacyId)
    {
        return await _api.GetAsync<List<PharmacyReport>>($"api/reports/pharmacy/{pharmacyId}");
    }

    public async Task SetStatusAsync(int reportId, ReportStatus status)
    {
        await _api.PutAsync<object, object>($"api/reports/{reportId}/status", new { Status = status });
    }
}

public class ApiAdminService : IAdminService
{
    private readonly ApiClient _api;

    public ApiAdminService(ApiClient api)
    {
        _api = api;
    }

    public async Task<DashboardStats> GetStatsAsync()
    {
        return await _api.GetAsync<DashboardStats>("api/admin/stats");
    }

    public async Task SetPharmacyStatusAsync(int pharmacyId, ValidationStatus status, string? reason)
    {
        await _api.PutAsync<object, object>($"api/admin/pharmacies/{pharmacyId}/status", new { Status = status, Reason = reason });
    }

    public async Task<IReadOnlyList<UserAccount>> GetUsersAsync()
    {
        return await _api.GetAsync<List<UserAccount>>("api/admin/users");
    }

    public async Task SetUserStatusAsync(int userId, AccountStatus status)
    {
        await _api.PutAsync<object, object>($"api/admin/users/{userId}/status", new { Status = status });
    }

    public async Task<IReadOnlyList<AuditEntry>> GetAuditAsync()
    {
        return await _api.GetAsync<List<AuditEntry>>("api/admin/audit");
    }

    public async Task<IReadOnlyList<ChartBar>> GetSearchesPerDayAsync()
    {
        return await _api.GetAsync<List<ChartBar>>("api/admin/charts/searches-per-day");
    }

    public async Task<IReadOnlyList<ChartBar>> GetTopSearchedAsync()
    {
        return await _api.GetAsync<List<ChartBar>>("api/admin/charts/top-searched");
    }

    public async Task<IReadOnlyList<ChartBar>> GetPharmaciesByDistrictAsync()
    {
        return await _api.GetAsync<List<ChartBar>>("api/admin/charts/by-district");
    }

    public async Task<IReadOnlyList<ChartBar>> GetFreshnessDistributionAsync()
    {
        return await _api.GetAsync<List<ChartBar>>("api/admin/charts/freshness");
    }
}
