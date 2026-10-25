using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
public class MedicinesController : ControllerBase
{
    private readonly PharmaFindDbContext _db;
    private readonly IHubContext<PharmaHub, IPharmaClient> _hub;

    public MedicinesController(PharmaFindDbContext db, IHubContext<PharmaHub, IPharmaClient> hub)
    {
        _db = db;
        _hub = hub;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Medicine>>> GetAll([FromQuery] bool includeInactive = false)
    {
        var query = _db.Medicines.AsNoTracking();
        if (!includeInactive) query = query.Where(m => m.IsActive);
        var list = await query.OrderBy(m => m.BrandName).ThenBy(m => m.Dosage).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Medicine>> GetById(int id)
    {
        var med = await _db.Medicines.FindAsync(id);
        if (med == null) return NotFound(new { message = "Médicament introuvable." });
        return Ok(med);
    }

    [HttpGet("popular")]
    public async Task<ActionResult<IReadOnlyList<Medicine>>> GetPopular()
    {
        var popularIds = await _db.SearchStats
            .Where(s => s.MedicineId.HasValue)
            .GroupBy(s => s.MedicineId!.Value)
            .OrderByDescending(g => g.Count())
            .Take(6)
            .Select(g => g.Key)
            .ToListAsync();

        var medicines = await _db.Medicines
            .Where(m => m.IsActive)
            .ToListAsync();

        var result = medicines
            .OrderBy(m => {
                var idx = popularIds.IndexOf(m.Id);
                return idx >= 0 ? idx : 999;
            })
            .Take(6)
            .ToList();

        return Ok(result);
    }

    [HttpGet("suggest")]
    public async Task<ActionResult<IReadOnlyList<Medicine>>> Suggest([FromQuery] string query, [FromQuery] string? form = null)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Ok(Array.Empty<Medicine>());
        }

        var meds = await _db.Medicines.Where(m => m.IsActive).ToListAsync();

        var filtered = meds
            .Where(m => TextHelper.Matches(query, m.BrandName, m.Inn, m.DisplayName))
            .Where(m => form == null || m.Form == form)
            .OrderBy(m => TextHelper.Normalize(m.BrandName).StartsWith(TextHelper.Normalize(query)) ? 0 : 1)
            .ThenBy(m => m.BrandName)
            .ThenBy(m => m.Dosage)
            .Take(20)
            .ToList();

        return Ok(filtered);
    }

    [HttpGet("did-you-mean")]
    public async Task<ActionResult<Medicine?>> DidYouMean([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return Ok(null);
        var q = TextHelper.Normalize(query);
        if (q.Length < 3) return Ok(null);

        var meds = await _db.Medicines.Where(m => m.IsActive).ToListAsync();
        var best = meds
            .Select(m => (m, score: Math.Min(TextHelper.Closeness(q, m.BrandName), TextHelper.Closeness(q, m.Inn))))
            .OrderBy(x => x.score)
            .FirstOrDefault();

        if (best.m != null && best.score <= Math.Max(1, q.Length / 3))
        {
            return Ok(best.m);
        }

        return Ok(null);
    }

    [HttpGet("{id:int}/equivalents")]
    public async Task<ActionResult<IReadOnlyList<Medicine>>> GetEquivalents(int id)
    {
        var current = await _db.Medicines.FindAsync(id);
        if (current == null) return NotFound(new { message = "Médicament introuvable." });

        var normalizedInn = TextHelper.Normalize(current.Inn);
        var meds = await _db.Medicines.Where(m => m.IsActive && m.Id != id).ToListAsync();

        var equivalents = meds
            .Where(m => TextHelper.Normalize(m.Inn) == normalizedInn)
            .ToList();

        return Ok(equivalents);
    }

    [HttpGet("forms")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetForms()
    {
        var forms = await _db.Medicines
            .Select(m => m.Form)
            .Distinct()
            .OrderBy(f => f)
            .ToListAsync();
        return Ok(forms);
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetCategories()
    {
        var categories = await _db.Medicines
            .Select(m => m.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
        return Ok(categories);
    }

    [HttpPost]
    [Authorize(Roles = "Administrateur")]
    public async Task<ActionResult<Medicine>> Save([FromBody] Medicine medicine)
    {
        if (string.IsNullOrWhiteSpace(medicine.BrandName) ||
            string.IsNullOrWhiteSpace(medicine.Inn) ||
            string.IsNullOrWhiteSpace(medicine.Dosage) ||
            string.IsNullOrWhiteSpace(medicine.Form))
        {
            return BadRequest(new { message = "Le nom commercial, la DCI, le dosage et la forme sont obligatoires." });
        }

        var allMeds = await _db.Medicines.ToListAsync();
        var duplicate = allMeds.Any(m => m.Id != medicine.Id &&
            TextHelper.Normalize(m.BrandName) == TextHelper.Normalize(medicine.BrandName) &&
            TextHelper.Normalize(m.Dosage) == TextHelper.Normalize(medicine.Dosage) &&
            m.Form == medicine.Form);

        if (duplicate)
        {
            return BadRequest(new { message = "Ce médicament existe déjà avec ce dosage et cette forme." });
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        var actor = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "administrateur";

        if (medicine.Id == 0)
        {
            _db.Medicines.Add(medicine);
            _db.AuditEntries.Add(new AuditEntry
            {
                Actor = actor,
                Action = "Ajout au référentiel",
                Resource = medicine.DisplayName,
                Timestamp = DateTime.UtcNow
            });
        }
        else
        {
            var existing = await _db.Medicines.FindAsync(medicine.Id);
            if (existing == null) return NotFound(new { message = "Médicament introuvable." });

            existing.BrandName = medicine.BrandName;
            existing.Inn = medicine.Inn;
            existing.Dosage = medicine.Dosage;
            existing.Form = medicine.Form;
            existing.Category = medicine.Category;
            existing.Description = medicine.Description;
            existing.Delivery = medicine.Delivery;
            existing.IsActive = medicine.IsActive;

            _db.AuditEntries.Add(new AuditEntry
            {
                Actor = actor,
                Action = "Modification du référentiel",
                Resource = medicine.DisplayName,
                Timestamp = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        // Broadcast real-time update to all connected clients
        await _hub.Clients.All.MedicineUpdated(medicine);

        return Ok(medicine);
    }
}
