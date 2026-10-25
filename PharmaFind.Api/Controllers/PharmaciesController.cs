using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PharmaFind.Api.Data;
using PharmaFind.Api.Hubs;
using PharmaFind.Api.Models;
using PharmaFind.Api.Services;

namespace PharmaFind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PharmaciesController : ControllerBase
{
    private readonly PharmaFindDbContext _db;
    private readonly IHubContext<PharmaHub, IPharmaClient> _hub;

    public PharmaciesController(PharmaFindDbContext db, IHubContext<PharmaHub, IPharmaClient> hub)
    {
        _db = db;
        _hub = hub;
    }

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<PharmacyResultDto>>> Search(
        [FromQuery] int? medicineId,
        [FromQuery] double? lat,
        [FromQuery] double? lon,
        [FromQuery] int radiusKm = 5)
    {
        bool hasUserCoords = lat.HasValue && lon.HasValue &&
                             (Math.Abs(lat.Value) > 0.0001 || Math.Abs(lon.Value) > 0.0001);

        if (medicineId.HasValue && medicineId.Value > 0)
        {
            _db.SearchStats.Add(new SearchStat
            {
                MedicineId = medicineId.Value,
                Timestamp = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        var pharmacies = await _db.Pharmacies
            .AsNoTracking()
            .Where(p => p.Status == ValidationStatus.Validee)
            .ToListAsync();

        var stocks = (medicineId.HasValue && medicineId.Value > 0)
            ? await _db.StockItems
                .AsNoTracking()
                .Include(s => s.Medicine)
                .Where(s => s.MedicineId == medicineId.Value)
                .ToListAsync()
            : [];

        var results = new List<PharmacyResultDto>();

        foreach (var p in pharmacies)
        {
            double? distance = null;
            if (hasUserCoords && p.HasCoordinates)
            {
                distance = GeoService.DistanceKm(lat!.Value, lon!.Value, p.Latitude!.Value, p.Longitude!.Value);
                if (distance > radiusKm) continue; // Out of requested radius
            }
            else if (hasUserCoords && !p.HasCoordinates)
            {
                // Pharmacy without coordinates: only keep if radius is large or city matches
                if (radiusKm < 20) continue;
            }

            StockItem? stock = null;
            if (medicineId.HasValue && medicineId.Value > 0)
            {
                stock = stocks.FirstOrDefault(s => s.PharmacyId == p.Id);
                if (stock == null) continue; // Not carried by this pharmacy
            }

            results.Add(new PharmacyResultDto
            {
                Pharmacy = p,
                Stock = stock,
                DistanceKm = distance
            });
        }

        var ordered = hasUserCoords
            ? results.OrderBy(r => r.DistanceKm ?? double.MaxValue).ThenBy(r => r.Pharmacy.Name).ToList()
            : results.OrderBy(r => r.Pharmacy.Name).ToList();

        return Ok(ordered);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Pharmacy>> GetById(int id)
    {
        var pharmacy = await _db.Pharmacies.FindAsync(id);
        if (pharmacy == null) return NotFound(new { message = "Pharmacie introuvable." });
        return Ok(pharmacy);
    }

    [HttpGet("{id:int}/stock")]
    public async Task<ActionResult<IReadOnlyList<StockItem>>> GetStock(int id)
    {
        var exists = await _db.Pharmacies.AnyAsync(p => p.Id == id);
        if (!exists) return NotFound(new { message = "Pharmacie introuvable." });

        var items = await _db.StockItems
            .AsNoTracking()
            .Include(s => s.Medicine)
            .Where(s => s.PharmacyId == id)
            .OrderBy(s => s.Medicine.BrandName)
            .ThenBy(s => s.Medicine.Dosage)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Pharmacy>>> GetAll([FromQuery] ValidationStatus? status = null)
    {
        var query = _db.Pharmacies.AsNoTracking();
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        var list = await query
            .OrderBy(p => p.Status == ValidationStatus.EnAttente ? 0 : 1)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost("request-account")]
    public async Task<ActionResult<Pharmacy>> RequestAccount([FromBody] Pharmacy request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Phone))
        {
            return BadRequest(new { message = "Le nom de l'établissement, le courriel et le téléphone sont obligatoires." });
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        request.Id = 0;
        request.Status = ValidationStatus.EnAttente;
        request.RequestedAt = DateTime.UtcNow;

        _db.Pharmacies.Add(request);
        _db.AuditEntries.Add(new AuditEntry
        {
            Actor = request.Email,
            Action = "Demande de compte",
            Resource = request.Name,
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        // Broadcast to admin group about new account request
        await _hub.Clients.Group("admin").PharmacyStatusChanged(request.Id, ValidationStatus.EnAttente, null);

        return Ok(request);
    }
}
