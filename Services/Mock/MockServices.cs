using PharmaFind.Helpers;
using PharmaFind.Models;

namespace PharmaFind.Services.Mock;

/// <summary>Simule le comportement d'une API distante : latence, panne serveur, absence de réseau.</summary>
public abstract class MockServiceBase(MockDataStore store, DemoSettings demo)
{
    protected MockDataStore Store { get; } = store;
    protected DemoSettings Demo { get; } = demo;

    protected async Task SimulateAsync()
    {
        await Task.Delay(Demo.SimulateSlowNetwork ? 2500 : 350);
        if (Demo.SimulateOffline)
            throw new ServiceUnavailableException("Connexion impossible. Vérifiez votre réseau.");
        if (Demo.SimulateServerError)
            throw new ServiceUnavailableException("Le serveur PharmaFind ne répond pas (erreur simulée).");
    }
}

public class MockMedicineService(MockDataStore store, DemoSettings demo) : MockServiceBase(store, demo), IMedicineService
{
    private IEnumerable<Medicine> Active => Store.Medicines.Where(m => m.IsActive);

    public async Task<IReadOnlyList<Medicine>> SuggestAsync(string query, string? form = null, CancellationToken ct = default)
    {
        await SimulateAsync();
        ct.ThrowIfCancellationRequested();
        return Active
            .Where(m => TextSearch.Matches(query, m.BrandName, m.Inn, m.DisplayName))
            .Where(m => form is null || m.Form == form)
            .OrderBy(m => TextSearch.Normalize(m.BrandName).StartsWith(TextSearch.Normalize(query)) ? 0 : 1)
            .ThenBy(m => m.BrandName).ThenBy(m => m.Dosage)
            .Take(20).ToList();
    }

    public Task<Medicine?> DidYouMeanAsync(string query, CancellationToken ct = default)
    {
        var q = TextSearch.Normalize(query);
        if (q.Length < 3) return Task.FromResult<Medicine?>(null);
        var best = Active
            .Select(m => (m, score: Math.Min(TextSearch.Closeness(q, m.BrandName), TextSearch.Closeness(q, m.Inn))))
            .OrderBy(x => x.score).FirstOrDefault();
        return Task.FromResult(best.m is not null && best.score <= Math.Max(1, q.Length / 3) ? best.m : null);
    }

    public async Task<IReadOnlyList<Medicine>> GetAllAsync(bool includeInactive = false)
    {
        await SimulateAsync();
        return (includeInactive ? Store.Medicines : Active).OrderBy(m => m.BrandName).ThenBy(m => m.Dosage).ToList();
    }

    public async Task<IReadOnlyList<Medicine>> GetPopularAsync()
    {
        await SimulateAsync();
        return Active.OrderByDescending(m => Store.SearchCounts.GetValueOrDefault(m.Id)).Take(6).ToList();
    }

    public Task<IReadOnlyList<Medicine>> GetEquivalentsAsync(Medicine medicine)
        => Task.FromResult<IReadOnlyList<Medicine>>(Active
            .Where(m => m.Id != medicine.Id && TextSearch.Normalize(m.Inn) == TextSearch.Normalize(medicine.Inn))
            .ToList());

    public async Task<Medicine?> GetByIdAsync(int id)
    {
        await SimulateAsync();
        return Store.Medicines.FirstOrDefault(m => m.Id == id);
    }

    public Task<IReadOnlyList<string>> GetFormsAsync()
        => Task.FromResult<IReadOnlyList<string>>(Store.Medicines.Select(m => m.Form).Distinct().Order().ToList());

    public Task<IReadOnlyList<string>> GetCategoriesAsync()
        => Task.FromResult<IReadOnlyList<string>>(Store.Medicines.Select(m => m.Category).Distinct().Order().ToList());

    public async Task<Medicine> SaveAsync(Medicine medicine)
    {
        await SimulateAsync();
        var duplicate = Store.Medicines.Any(m => m.Id != medicine.Id &&
            TextSearch.Normalize(m.BrandName) == TextSearch.Normalize(medicine.BrandName) &&
            TextSearch.Normalize(m.Dosage) == TextSearch.Normalize(medicine.Dosage) &&
            m.Form == medicine.Form);
        if (duplicate)
            throw new InvalidOperationException("Ce médicament existe déjà avec ce dosage et cette forme.");

        if (medicine.Id == 0)
        {
            medicine.Id = Store.Medicines.Max(m => m.Id) + 1;
            Store.Medicines.Add(medicine);
            Store.AddAudit("admin@demo.cm", "Ajout au référentiel", medicine.DisplayName);
        }
        else
        {
            var existing = Store.Medicines.First(m => m.Id == medicine.Id);
            existing.BrandName = medicine.BrandName; existing.Inn = medicine.Inn; existing.Dosage = medicine.Dosage;
            existing.Form = medicine.Form; existing.Category = medicine.Category; existing.Description = medicine.Description;
            existing.Delivery = medicine.Delivery; existing.IsActive = medicine.IsActive;
            Store.AddAudit("admin@demo.cm", "Modification du référentiel", medicine.DisplayName);
        }
        return medicine;
    }
}

public class MockPharmacyService(MockDataStore store, DemoSettings demo) : MockServiceBase(store, demo), IPharmacyService
{
    public async Task<IReadOnlyList<PharmacyResult>> SearchAsync(int? medicineId, UserLocation location, int radiusKm)
    {
        await SimulateAsync();
        if (medicineId is { } mid) Store.SearchCounts[mid] = Store.SearchCounts.GetValueOrDefault(mid) + 1;

        var results = new List<PharmacyResult>();
        foreach (var p in Store.Pharmacies.Where(p => p.Status == ValidationStatus.Validee))
        {
            double? distance = p.HasCoordinates
                ? GeoHelper.DistanceKm(location.Latitude, location.Longitude, p.Latitude!.Value, p.Longitude!.Value)
                : null;

            // Coordonnées manquantes : affichée seulement si elle est dans la même ville, en fin de liste.
            if (distance is null && p.City != "Douala") continue;
            if (distance > radiusKm) continue;

            StockItem? stock = null;
            if (medicineId is not null)
            {
                stock = Store.Stocks.FirstOrDefault(s => s.PharmacyId == p.Id && s.MedicineId == medicineId);
                if (stock is null) continue; // ne déclare pas ce médicament
            }
            results.Add(new PharmacyResult { Pharmacy = p, Stock = stock, DistanceKm = distance });
        }
        return results.OrderBy(r => r.DistanceKm ?? double.MaxValue).ToList();
    }

    public async Task<Pharmacy?> GetByIdAsync(int id)
    {
        await SimulateAsync();
        // Copie : comme une réponse d'API, l'objet reçu ne change pas tout seul.
        return Store.Pharmacies.FirstOrDefault(p => p.Id == id)?.Clone();
    }

    public async Task<IReadOnlyList<StockItem>> GetStockAsync(int pharmacyId)
    {
        await SimulateAsync();
        return Store.Stocks.Where(s => s.PharmacyId == pharmacyId)
            .OrderBy(s => s.Medicine.BrandName).ThenBy(s => s.Medicine.Dosage).ToList();
    }

    public async Task<IReadOnlyList<Pharmacy>> GetAllAsync(ValidationStatus? status = null)
    {
        await SimulateAsync();
        return Store.Pharmacies.Where(p => status is null || p.Status == status)
            .OrderBy(p => p.Status == ValidationStatus.EnAttente ? 0 : 1).ThenBy(p => p.Name).ToList();
    }

    public async Task RequestAccountAsync(Pharmacy request)
    {
        await SimulateAsync();
        request.Id = Store.Pharmacies.Max(p => p.Id) + 1;
        request.Status = ValidationStatus.EnAttente;
        request.RequestedAt = DateTime.Now;
        Store.Pharmacies.Add(request);
        Store.AddAudit(request.Email, "Demande de compte", request.Name);
    }
}

public class MockAuthService(MockDataStore store, DemoSettings demo) : MockServiceBase(store, demo), IAuthService
{
    // Maquette uniquement : la vérification réelle (mot de passe haché, jetons) sera faite par l'API.
    private const string DemoPassword = "Demo1234";

    public UserAccount? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser is not null;
    public event EventHandler? CurrentUserChanged;

    public async Task<UserAccount> LoginAsync(string email, string password)
    {
        await SimulateAsync();
        var user = Store.Users.FirstOrDefault(u => string.Equals(u.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));
        if (user is null || password != DemoPassword)
            throw new UnauthorizedAccessException("Courriel ou mot de passe incorrect.");
        if (user.Status == AccountStatus.Suspendu)
            throw new UnauthorizedAccessException("Ce compte est suspendu. Contactez l'administration PharmaFind.");
        CurrentUser = user;
        Store.AddAudit(user.Email, "Connexion", user.RoleText);
        CurrentUserChanged?.Invoke(this, EventArgs.Empty);
        return user;
    }

    public async Task<UserAccount> RegisterAsync(string name, string email, string password)
    {
        await SimulateAsync();
        if (Store.Users.Any(u => string.Equals(u.Email, email.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Un compte existe déjà avec ce courriel.");
        var user = new UserAccount { Id = Store.Users.Max(u => u.Id) + 1, Name = name.Trim(), Email = email.Trim(), Role = UserRole.Visiteur, CreatedAt = DateTime.Now };
        Store.Users.Add(user);
        CurrentUser = user;
        CurrentUserChanged?.Invoke(this, EventArgs.Empty);
        return user;
    }

    public Task LogoutAsync()
    {
        CurrentUser = null;
        CurrentUserChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}

public class MockFavoritesService(MockDataStore store, DemoSettings demo) : MockServiceBase(store, demo), IFavoritesService
{
    public async Task<IReadOnlyList<Pharmacy>> GetFavoritesAsync()
    {
        await SimulateAsync();
        return Store.Pharmacies.Where(p => Store.FavoritePharmacyIds.Contains(p.Id)).OrderBy(p => p.Name).ToList();
    }

    public Task<bool> IsFavoriteAsync(int pharmacyId) => Task.FromResult(Store.FavoritePharmacyIds.Contains(pharmacyId));

    public async Task<bool> ToggleAsync(int pharmacyId)
    {
        await SimulateAsync();
        if (!Store.FavoritePharmacyIds.Remove(pharmacyId)) Store.FavoritePharmacyIds.Add(pharmacyId);
        return Store.FavoritePharmacyIds.Contains(pharmacyId);
    }
}

public class MockReportService(MockDataStore store, DemoSettings demo) : MockServiceBase(store, demo), IReportService
{
    public async Task SubmitAsync(PharmacyReport report)
    {
        await SimulateAsync();
        report.Id = Store.Reports.Count == 0 ? 1 : Store.Reports.Max(r => r.Id) + 1;
        report.CreatedAt = DateTime.Now;
        Store.Reports.Insert(0, report);
    }

    public async Task<IReadOnlyList<PharmacyReport>> GetAllAsync(ReportStatus? status = null)
    {
        await SimulateAsync();
        return Store.Reports.Where(r => status is null || r.Status == status).OrderByDescending(r => r.CreatedAt).ToList();
    }

    public async Task<IReadOnlyList<PharmacyReport>> GetForPharmacyAsync(int pharmacyId)
    {
        await SimulateAsync();
        return Store.Reports.Where(r => r.PharmacyId == pharmacyId).OrderByDescending(r => r.CreatedAt).ToList();
    }

    public async Task SetStatusAsync(int reportId, ReportStatus status)
    {
        await SimulateAsync();
        var report = Store.Reports.First(r => r.Id == reportId);
        report.Status = status;
        Store.AddAudit("admin@demo.cm", $"Signalement : {ReportTexts.Status(status)}", report.Subject);
    }
}

/// <summary>Espace pharmacie : toutes les opérations portent sur la pharmacie du compte connecté (contrôle simulé).</summary>
public class MockStockService(MockDataStore store, DemoSettings demo, IAuthService auth) : MockServiceBase(store, demo), IStockService
{
    private int MyPharmacyId => auth.CurrentUser?.PharmacyId
        ?? throw new UnauthorizedAccessException("Aucune pharmacie associée à ce compte.");

    private string Actor => auth.CurrentUser?.Email ?? "inconnu";

    public async Task<Pharmacy> GetMyPharmacyAsync()
    {
        await SimulateAsync();
        return Store.Pharmacies.First(p => p.Id == MyPharmacyId).Clone();
    }

    public async Task UpdateMyPharmacyAsync(Pharmacy pharmacy)
    {
        await SimulateAsync();
        if (pharmacy.Id != MyPharmacyId) throw new UnauthorizedAccessException("Modification non autorisée.");
        var index = Store.Pharmacies.FindIndex(p => p.Id == pharmacy.Id);
        Store.Pharmacies[index] = pharmacy.Clone();
        Store.StockChanges.Insert(0, new StockChange { Date = DateTime.Now, MedicineName = "Établissement", Description = "Informations de l'établissement modifiées", Author = Actor });
        Store.AddAudit(Actor, "Modification de l'établissement", pharmacy.Name);
    }

    public async Task<IReadOnlyList<StockItem>> GetMyStockAsync()
    {
        await SimulateAsync();
        return Store.Stocks.Where(s => s.PharmacyId == MyPharmacyId)
            .OrderBy(s => s.Medicine.BrandName).ThenBy(s => s.Medicine.Dosage).ToList();
    }

    public async Task<StockItem?> GetStockItemAsync(int stockId)
    {
        await SimulateAsync();
        return Store.Stocks.FirstOrDefault(s => s.Id == stockId && s.PharmacyId == MyPharmacyId)?.Clone();
    }

    public async Task<StockItem> SaveStockAsync(StockItem item)
    {
        await SimulateAsync();
        if (item.Quantity < 0 || item.PriceXaf <= 0 || item.LowStockThreshold < 0)
            throw new ArgumentException("Quantité, prix et seuil doivent être positifs.");

        item.PharmacyId = MyPharmacyId;
        item.LastUpdated = DateTime.Now;
        if (item.Id == 0)
        {
            if (Store.Stocks.Any(s => s.PharmacyId == item.PharmacyId && s.MedicineId == item.MedicineId))
                throw new InvalidOperationException("Ce médicament est déjà dans votre inventaire.");
            item.Id = Store.NextStockId();
            Store.Stocks.Add(item);
        }
        else
        {
            var index = Store.Stocks.FindIndex(s => s.Id == item.Id && s.PharmacyId == MyPharmacyId);
            if (index < 0) throw new UnauthorizedAccessException("Modification non autorisée.");
            Store.Stocks[index] = item;
        }
        Store.StockChanges.Insert(0, new StockChange
        {
            Date = item.LastUpdated, MedicineName = item.Medicine.DisplayName, Author = Actor,
            Description = item.DeclaredOutOfStock ? "Rupture déclarée" : $"Quantité : {item.Quantity} · Prix : {item.PriceText}"
        });
        Store.AddAudit(Actor, "Mise à jour du stock", item.Medicine.DisplayName);
        return item;
    }

    public async Task RemoveStockAsync(int stockId)
    {
        await SimulateAsync();
        var item = Store.Stocks.FirstOrDefault(s => s.Id == stockId && s.PharmacyId == MyPharmacyId)
                   ?? throw new UnauthorizedAccessException("Suppression non autorisée.");
        Store.Stocks.Remove(item);
        Store.StockChanges.Insert(0, new StockChange { Date = DateTime.Now, MedicineName = item.Medicine.DisplayName, Description = "Retiré de l'inventaire", Author = Actor });
    }

    public async Task ConfirmAllUnchangedAsync()
    {
        await SimulateAsync();
        foreach (var s in Store.Stocks.Where(s => s.PharmacyId == MyPharmacyId)) s.LastUpdated = DateTime.Now;
        Store.StockChanges.Insert(0, new StockChange { Date = DateTime.Now, MedicineName = "Inventaire complet", Description = "Stocks confirmés sans changement", Author = Actor });
    }

    public async Task<IReadOnlyList<StockChange>> GetHistoryAsync()
    {
        await SimulateAsync();
        return Store.StockChanges.OrderByDescending(c => c.Date).ToList();
    }

    public async Task<IReadOnlyList<AppNotification>> GetNotificationsAsync()
    {
        await SimulateAsync();
        return Store.Notifications.OrderByDescending(n => n.Date).ToList();
    }

    public Task MarkNotificationsReadAsync()
    {
        foreach (var n in Store.Notifications) n.IsRead = true;
        return Task.CompletedTask;
    }
}

public class MockAdminService(MockDataStore store, DemoSettings demo) : MockServiceBase(store, demo), IAdminService
{
    public async Task<DashboardStats> GetStatsAsync()
    {
        await SimulateAsync();
        var active = Store.Stocks.Where(s => Store.Pharmacies.Any(p => p.Id == s.PharmacyId && p.Status == ValidationStatus.Validee)).ToList();
        return new DashboardStats
        {
            ValidatedPharmacies = Store.Pharmacies.Count(p => p.Status == ValidationStatus.Validee),
            PendingPharmacies = Store.Pharmacies.Count(p => p.Status == ValidationStatus.EnAttente),
            SuspendedPharmacies = Store.Pharmacies.Count(p => p.Status == ValidationStatus.Suspendue),
            Medicines = Store.Medicines.Count(m => m.IsActive),
            Users = Store.Users.Count,
            OpenReports = Store.Reports.Count(r => r.Status is ReportStatus.Nouveau or ReportStatus.EnCours),
            SearchesLast7Days = 1284,
            StaleStockRatio = active.Count == 0 ? 0 : active.Count(s => s.Freshness == StockFreshness.Ancienne) / (double)active.Count
        };
    }

    public async Task SetPharmacyStatusAsync(int pharmacyId, ValidationStatus status, string? reason)
    {
        await SimulateAsync();
        var pharmacy = Store.Pharmacies.First(p => p.Id == pharmacyId);
        pharmacy.Status = status;
        pharmacy.StatusReason = reason;
        var action = status switch
        {
            ValidationStatus.Validee => "Validation",
            ValidationStatus.Refusee => "Refus",
            ValidationStatus.Suspendue => "Suspension",
            _ => "Remise en attente"
        };
        Store.AddAudit("admin@demo.cm", action, pharmacy.Name);
    }

    public async Task<IReadOnlyList<UserAccount>> GetUsersAsync()
    {
        await SimulateAsync();
        return Store.Users.OrderBy(u => u.Role).ThenBy(u => u.Name).ToList();
    }

    public async Task SetUserStatusAsync(int userId, AccountStatus status)
    {
        await SimulateAsync();
        var user = Store.Users.First(u => u.Id == userId);
        user.Status = status;
        Store.AddAudit("admin@demo.cm", status == AccountStatus.Actif ? "Réactivation de compte" : "Suspension de compte", user.Email);
    }

    public async Task<IReadOnlyList<AuditEntry>> GetAuditAsync()
    {
        await SimulateAsync();
        return Store.Audit.OrderByDescending(a => a.Timestamp).ToList();
    }

    public Task<IReadOnlyList<ChartBar>> GetSearchesPerDayAsync()
    {
        int[] values = [142, 168, 155, 201, 189, 236, 193];
        var max = values.Max();
        var bars = values.Select((v, i) =>
        {
            var day = DateTime.Today.AddDays(i - 6);
            return new ChartBar { Label = day.ToString("ddd dd", AppCulture.Fr), Value = v, Ratio = v / (double)max, ValueText = v.ToString() };
        }).ToList();
        return Task.FromResult<IReadOnlyList<ChartBar>>(bars);
    }

    public Task<IReadOnlyList<ChartBar>> GetTopSearchedAsync()
    {
        var top = Store.SearchCounts.OrderByDescending(kv => kv.Value).Take(5).ToList();
        var max = top.Max(kv => kv.Value);
        return Task.FromResult<IReadOnlyList<ChartBar>>(top.Select(kv => new ChartBar
        {
            Label = Store.Medicines.First(m => m.Id == kv.Key).DisplayName,
            Value = kv.Value, Ratio = kv.Value / (double)max, ValueText = kv.Value.ToString()
        }).ToList());
    }

    public Task<IReadOnlyList<ChartBar>> GetPharmaciesByDistrictAsync()
    {
        var groups = Store.Pharmacies.Where(p => p.Status == ValidationStatus.Validee)
            .GroupBy(p => p.District).OrderByDescending(g => g.Count()).ToList();
        var max = groups.Max(g => g.Count());
        return Task.FromResult<IReadOnlyList<ChartBar>>(groups.Select(g => new ChartBar
        {
            Label = g.Key, Value = g.Count(), Ratio = g.Count() / (double)max, ValueText = g.Count().ToString()
        }).ToList());
    }

    public Task<IReadOnlyList<ChartBar>> GetFreshnessDistributionAsync()
    {
        var all = Store.Stocks;
        var total = Math.Max(1, all.Count);
        (string Label, int Count)[] parts =
        [
            ("À jour (≤ 24 h)", all.Count(s => s.Freshness == StockFreshness.Recente)),
            ("À vérifier (24–72 h)", all.Count(s => s.Freshness == StockFreshness.AVerifier)),
            ("Ancien (> 72 h)", all.Count(s => s.Freshness == StockFreshness.Ancienne)),
        ];
        return Task.FromResult<IReadOnlyList<ChartBar>>(parts.Select(p => new ChartBar
        {
            Label = p.Label, Value = p.Count, Ratio = p.Count / (double)total,
            ValueText = (p.Count / (double)total).ToString("P0", AppCulture.Fr)
        }).ToList());
    }
}
