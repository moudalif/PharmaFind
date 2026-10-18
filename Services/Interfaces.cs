using PharmaFind.Models;

namespace PharmaFind.Services;

// Contrats des services clients. Aujourd'hui implémentés par des services fictifs (Services/Mock).
// Demain : implémentations HttpClient vers l'API ASP.NET Core, sans modifier les ViewModels ni les pages.

public interface IMedicineService
{
    Task<IReadOnlyList<Medicine>> SuggestAsync(string query, string? form = null, CancellationToken ct = default);
    /// <summary>Proposition « Vouliez-vous dire… » quand aucune correspondance exacte n'existe.</summary>
    Task<Medicine?> DidYouMeanAsync(string query, CancellationToken ct = default);
    Task<IReadOnlyList<Medicine>> GetAllAsync(bool includeInactive = false);
    Task<IReadOnlyList<Medicine>> GetPopularAsync();
    Task<IReadOnlyList<Medicine>> GetEquivalentsAsync(Medicine medicine);
    Task<Medicine?> GetByIdAsync(int id);
    Task<IReadOnlyList<string>> GetFormsAsync();
    Task<IReadOnlyList<string>> GetCategoriesAsync();
    Task<Medicine> SaveAsync(Medicine medicine);
}

public interface IPharmacyService
{
    Task<IReadOnlyList<PharmacyResult>> SearchAsync(int? medicineId, UserLocation location, int radiusKm);
    Task<Pharmacy?> GetByIdAsync(int id);
    Task<IReadOnlyList<StockItem>> GetStockAsync(int pharmacyId);
    Task<IReadOnlyList<Pharmacy>> GetAllAsync(ValidationStatus? status = null);
    Task RequestAccountAsync(Pharmacy request);
}

/// <summary>Espace pharmacie : n'agit que sur la pharmacie de l'utilisateur connecté.</summary>
public interface IStockService
{
    Task<Pharmacy> GetMyPharmacyAsync();
    Task UpdateMyPharmacyAsync(Pharmacy pharmacy);
    Task<IReadOnlyList<StockItem>> GetMyStockAsync();
    Task<StockItem?> GetStockItemAsync(int stockId);
    Task<StockItem> SaveStockAsync(StockItem item);
    Task RemoveStockAsync(int stockId);
    Task ConfirmAllUnchangedAsync();
    Task<IReadOnlyList<StockChange>> GetHistoryAsync();
    Task<IReadOnlyList<AppNotification>> GetNotificationsAsync();
    Task MarkNotificationsReadAsync();
}

public interface IAuthService
{
    UserAccount? CurrentUser { get; }
    bool IsLoggedIn { get; }
    event EventHandler? CurrentUserChanged;
    Task<UserAccount> LoginAsync(string email, string password);
    Task<UserAccount> RegisterAsync(string name, string email, string password);
    Task LogoutAsync();
}

public interface IFavoritesService
{
    Task<IReadOnlyList<Pharmacy>> GetFavoritesAsync();
    Task<bool> IsFavoriteAsync(int pharmacyId);
    Task<bool> ToggleAsync(int pharmacyId);
}

/// <summary>Historique local des recherches, conservé uniquement si l'utilisateur l'autorise.</summary>
public interface IHistoryService
{
    bool IsEnabled { get; set; }
    IReadOnlyList<SearchHistoryEntry> GetEntries();
    void Add(string query, int? medicineId);
    void Clear();
}

public interface IReportService
{
    Task SubmitAsync(PharmacyReport report);
    Task<IReadOnlyList<PharmacyReport>> GetAllAsync(ReportStatus? status = null);
    Task<IReadOnlyList<PharmacyReport>> GetForPharmacyAsync(int pharmacyId);
    Task SetStatusAsync(int reportId, ReportStatus status);
}

public interface IAdminService
{
    Task<DashboardStats> GetStatsAsync();
    Task SetPharmacyStatusAsync(int pharmacyId, ValidationStatus status, string? reason);
    Task<IReadOnlyList<UserAccount>> GetUsersAsync();
    Task SetUserStatusAsync(int userId, AccountStatus status);
    Task<IReadOnlyList<AuditEntry>> GetAuditAsync();
    Task<IReadOnlyList<ChartBar>> GetSearchesPerDayAsync();
    Task<IReadOnlyList<ChartBar>> GetTopSearchedAsync();
    Task<IReadOnlyList<ChartBar>> GetPharmaciesByDistrictAsync();
    Task<IReadOnlyList<ChartBar>> GetFreshnessDistributionAsync();
}

public enum LocationError { None, PermissionDenied, Disabled, Unavailable }

public record LocationResult(UserLocation? Location, LocationError Error);

public interface ILocationService
{
    /// <summary>Demande l'autorisation au moment où elle devient utile, puis lit la position. Rien n'est conservé.</summary>
    Task<LocationResult> GetCurrentLocationAsync(CancellationToken ct = default);
    IReadOnlyList<KnownPlace> GetKnownPlaces();
}

public interface INavigationService
{
    Task GoToAsync(string route, IDictionary<string, object>? parameters = null);
    Task GoBackAsync();
}

public interface IDialogService
{
    Task AlertAsync(string title, string message, string ok = "OK");
    Task<bool> ConfirmAsync(string title, string message, string accept = "Confirmer", string cancel = "Annuler");
    Task<string?> ActionSheetAsync(string title, string cancel, string? destructive, params string[] buttons);
    Task<string?> PromptAsync(string title, string message, string placeholder = "", string accept = "Valider");
}

public interface INetworkService
{
    bool IsConnected { get; }
}

/// <summary>Contrat du fournisseur de carte : la page ne dépend pas d'un fournisseur précis.</summary>
public interface IMapProvider
{
    string Name { get; }
    string BuildHtml(MapModel model);
}

public record MapMarker(int Id, double Latitude, double Longitude, string Title, string Subtitle, string ColorHex);

public record MapModel(double CenterLatitude, double CenterLongitude, int RadiusKm, bool ShowUser, IReadOnlyList<MapMarker> Markers);

/// <summary>Erreur « service indisponible » levée par les services (réseau, serveur, délai dépassé).</summary>
public class ServiceUnavailableException(string message) : Exception(message);
