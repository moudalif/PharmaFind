using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PharmaFind.Api.Data;
using PharmaFind.Api.Hubs;
using PharmaFind.Api.Models;

namespace PharmaFind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Pharmacie,Administrateur")]
public class StockController : ControllerBase
{
    private readonly PharmaFindDbContext _db;
    private readonly IHubContext<PharmaHub, IPharmaClient> _hub;

    public StockController(PharmaFindDbContext db, IHubContext<PharmaHub, IPharmaClient> hub)
    {
        _db = db;
        _hub = hub;
    }

    private async Task<int> GetMyPharmacyIdAsync()
    {
        var claim = User.FindFirst("PharmacyId")?.Value;
        if (!string.IsNullOrEmpty(claim) && int.TryParse(claim, out var pid))
        {
            return pid;
        }

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdStr, out var uid))
        {
            var user = await _db.Users.FindAsync(uid);
            if (user?.PharmacyId != null) return user.PharmacyId.Value;
        }

        throw new UnauthorizedAccessException("Aucune pharmacie associée à ce compte.");
    }

    private string GetActor() => User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "utilisateur";

    [HttpGet("my-pharmacy")]
    public async Task<ActionResult<Pharmacy>> GetMyPharmacy()
    {
        var pid = await GetMyPharmacyIdAsync();
        var pharmacy = await _db.Pharmacies.FindAsync(pid);
        if (pharmacy == null) return NotFound(new { message = "Pharmacie introuvable." });
        return Ok(pharmacy);
    }

    [HttpPut("my-pharmacy")]
    public async Task<ActionResult<Pharmacy>> UpdateMyPharmacy([FromBody] Pharmacy pharmacy)
    {
        var pid = await GetMyPharmacyIdAsync();
        if (pharmacy.Id != 0 && pharmacy.Id != pid) return Forbid();

        await using var tx = await _db.Database.BeginTransactionAsync();

        var existing = await _db.Pharmacies.FindAsync(pid);
        if (existing == null) return NotFound(new { message = "Pharmacie introuvable." });

        existing.Name = pharmacy.Name;
        existing.Phone = pharmacy.Phone;
        existing.Email = pharmacy.Email;
        existing.Neighborhood = pharmacy.Neighborhood;
        existing.District = pharmacy.District;
        existing.Landmark = pharmacy.Landmark;
        existing.Latitude = pharmacy.Latitude;
        existing.Longitude = pharmacy.Longitude;
        existing.OpensAt = pharmacy.OpensAt;
        existing.ClosesAt = pharmacy.ClosesAt;
        existing.Is24h = pharmacy.Is24h;
        existing.OpenOnSunday = pharmacy.OpenOnSunday;
        existing.IsOnDuty = pharmacy.IsOnDuty;
        existing.Pharmacist = pharmacy.Pharmacist;

        _db.StockChanges.Add(new StockChange
        {
            PharmacyId = pid,
            Date = DateTime.UtcNow,
            MedicineName = "Établissement",
            Description = "Informations de l'établissement modifiées",
            Author = GetActor()
        });

        _db.AuditEntries.Add(new AuditEntry
        {
            Actor = GetActor(),
            Action = "Modification de l'établissement",
            Resource = pharmacy.Name,
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        await _hub.Clients.All.PharmacyStatusChanged(pid, existing.Status, existing.StatusReason);

        return Ok(existing);
    }

    [HttpGet("my-stock")]
    public async Task<ActionResult<IReadOnlyList<StockItem>>> GetMyStock()
    {
        var pid = await GetMyPharmacyIdAsync();
        var items = await _db.StockItems
            .Include(s => s.Medicine)
            .Where(s => s.PharmacyId == pid)
            .OrderBy(s => s.Medicine.BrandName)
            .ThenBy(s => s.Medicine.Dosage)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StockItem>> GetStockItem(int id)
    {
        var pid = await GetMyPharmacyIdAsync();
        var item = await _db.StockItems
            .Include(s => s.Medicine)
            .FirstOrDefaultAsync(s => s.Id == id && s.PharmacyId == pid);

        if (item == null) return NotFound(new { message = "Ligne de stock introuvable." });
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<StockItem>> SaveStock([FromBody] StockItem item)
    {
        var pid = await GetMyPharmacyIdAsync();
        if (item.Quantity < 0 || item.PriceXaf <= 0 || item.PriceXaf > 100_000_000 || item.LowStockThreshold < 0)
        {
            return BadRequest(new { message = "Quantité, prix et seuil doivent être valides et positifs." });
        }

        var medicine = await _db.Medicines.FindAsync(item.MedicineId);
        if (medicine == null) return BadRequest(new { message = "Médicament introuvable dans le référentiel." });

        await using var tx = await _db.Database.BeginTransactionAsync();

        item.PharmacyId = pid;
        item.LastUpdated = DateTime.UtcNow;

        if (item.Id == 0)
        {
            var exists = await _db.StockItems.AnyAsync(s => s.PharmacyId == pid && s.MedicineId == item.MedicineId);
            if (exists)
            {
                return BadRequest(new { message = "Ce médicament est déjà présent dans votre inventaire. Veuillez modifier la ligne existante." });
            }

            _db.StockItems.Add(item);
        }
        else
        {
            var existing = await _db.StockItems.FirstOrDefaultAsync(s => s.Id == item.Id && s.PharmacyId == pid);
            if (existing == null) return Forbid();

            existing.Quantity = item.Quantity;
            existing.PriceXaf = item.PriceXaf;
            existing.LowStockThreshold = item.LowStockThreshold;
            existing.DeclaredOutOfStock = item.DeclaredOutOfStock;
            existing.LastUpdated = item.LastUpdated;
        }

        _db.StockChanges.Add(new StockChange
        {
            PharmacyId = pid,
            Date = item.LastUpdated,
            MedicineName = medicine.DisplayName,
            Author = GetActor(),
            Description = item.DeclaredOutOfStock ? "Rupture déclarée" : $"Quantité : {item.Quantity} · Prix : {item.PriceXaf:N0} FCFA"
        });

        _db.AuditEntries.Add(new AuditEntry
        {
            Actor = GetActor(),
            Action = "Mise à jour du stock",
            Resource = medicine.DisplayName,
            Timestamp = DateTime.UtcNow
        });

        // Trigger notification if stock is below threshold
        AppNotification? alertNotif = null;
        if (item.Quantity <= item.LowStockThreshold && !item.DeclaredOutOfStock)
        {
            alertNotif = new AppNotification
            {
                PharmacyId = pid,
                Kind = NotificationKind.StockFaible,
                Title = "Stock faible",
                Message = $"Attention, le stock de {medicine.DisplayName} est passé à {item.Quantity} unité(s).",
                Date = DateTime.UtcNow,
                IsRead = false
            };
            _db.Notifications.Add(alertNotif);
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        // Reload with medicine navigation property
        var saved = await _db.StockItems.Include(s => s.Medicine).FirstAsync(s => s.Id == item.Id);

        // Real-time SignalR notifications
        await _hub.Clients.All.StockUpdated(saved);
        if (alertNotif != null)
        {
            await _hub.Clients.Group($"pharmacy_{pid}").NotificationReceived(alertNotif);
        }

        return Ok(saved);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> RemoveStock(int id)
    {
        var pid = await GetMyPharmacyIdAsync();

        await using var tx = await _db.Database.BeginTransactionAsync();

        var item = await _db.StockItems.Include(s => s.Medicine).FirstOrDefaultAsync(s => s.Id == id && s.PharmacyId == pid);
        if (item == null) return NotFound(new { message = "Ligne de stock introuvable." });

        _db.StockItems.Remove(item);
        _db.StockChanges.Add(new StockChange
        {
            PharmacyId = pid,
            Date = DateTime.UtcNow,
            MedicineName = item.Medicine.DisplayName,
            Description = "Retiré de l'inventaire",
            Author = GetActor()
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        await _hub.Clients.All.StockRemoved(id, pid);

        return Ok();
    }

    [HttpPost("confirm-all")]
    public async Task<IActionResult> ConfirmAllUnchanged()
    {
        var pid = await GetMyPharmacyIdAsync();

        await using var tx = await _db.Database.BeginTransactionAsync();

        var items = await _db.StockItems.Where(s => s.PharmacyId == pid).ToListAsync();
        var now = DateTime.UtcNow;
        foreach (var s in items)
        {
            s.LastUpdated = now;
        }

        _db.StockChanges.Add(new StockChange
        {
            PharmacyId = pid,
            Date = now,
            MedicineName = "Inventaire complet",
            Description = "Stocks confirmés sans changement",
            Author = GetActor()
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok();
    }

    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<StockChange>>> GetHistory()
    {
        var pid = await GetMyPharmacyIdAsync();
        var changes = await _db.StockChanges
            .Where(c => c.PharmacyId == pid)
            .OrderByDescending(c => c.Date)
            .ToListAsync();

        return Ok(changes);
    }

    [HttpGet("notifications")]
    public async Task<ActionResult<IReadOnlyList<AppNotification>>> GetNotifications()
    {
        var pid = await GetMyPharmacyIdAsync();
        var notifications = await _db.Notifications
            .Where(n => n.PharmacyId == pid)
            .OrderByDescending(n => n.Date)
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpPost("notifications/read")]
    public async Task<IActionResult> MarkNotificationsRead()
    {
        var pid = await GetMyPharmacyIdAsync();
        var notifications = await _db.Notifications.Where(n => n.PharmacyId == pid && !n.IsRead).ToListAsync();
        foreach (var n in notifications)
        {
            n.IsRead = true;
        }

        await _db.SaveChangesAsync();
        return Ok();
    }
}
