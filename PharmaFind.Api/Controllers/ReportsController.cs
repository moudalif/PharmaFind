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
public class ReportsController : ControllerBase
{
    private readonly PharmaFindDbContext _db;
    private readonly IHubContext<PharmaHub, IPharmaClient> _hub;

    public ReportsController(PharmaFindDbContext db, IHubContext<PharmaHub, IPharmaClient> hub)
    {
        _db = db;
        _hub = hub;
    }

    [HttpPost]
    public async Task<ActionResult<PharmacyReport>> Submit([FromBody] PharmacyReport report)
    {
        if (report.PharmacyId <= 0)
        {
            return BadRequest(new { message = "L'identifiant de la pharmacie est invalide." });
        }

        var pharmacy = await _db.Pharmacies.FindAsync(report.PharmacyId);
        if (pharmacy == null)
        {
            return BadRequest(new { message = "Pharmacie introuvable." });
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        report.Id = 0;
        report.PharmacyName = pharmacy.Name;
        report.CreatedAt = DateTime.UtcNow;
        report.Status = ReportStatus.Nouveau;

        // Automatically create and link notification for the pharmacy
        var notif = new AppNotification
        {
            PharmacyId = report.PharmacyId,
            Kind = NotificationKind.Signalement,
            Title = "Nouveau signalement",
            Message = $"Un usager a signalé une anomalie : {report.Reason}",
            Date = DateTime.UtcNow,
            IsRead = false
        };
        _db.Notifications.Add(notif);

        _db.PharmacyReports.Add(report);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        // Broadcast to pharmacy group and admin group
        await _hub.Clients.Group($"pharmacy_{report.PharmacyId}").NotificationReceived(notif);
        await _hub.Clients.Group("admin").ReportSubmitted(report);

        return Ok(report);
    }

    [HttpGet]
    [Authorize(Roles = "Administrateur")]
    public async Task<ActionResult<IReadOnlyList<PharmacyReport>>> GetAll([FromQuery] ReportStatus? status = null)
    {
        var query = _db.PharmacyReports.AsNoTracking();
        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var list = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        return Ok(list);
    }

    [HttpGet("pharmacy/{pharmacyId:int}")]
    public async Task<ActionResult<IReadOnlyList<PharmacyReport>>> GetForPharmacy(int pharmacyId)
    {
        var list = await _db.PharmacyReports
            .Where(r => r.PharmacyId == pharmacyId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(list);
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Administrateur")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] ReportStatusChangeRequest req)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        var report = await _db.PharmacyReports.FindAsync(id);
        if (report == null) return NotFound(new { message = "Signalement introuvable." });

        report.Status = req.Status;

        var actor = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "administrateur";

        _db.AuditEntries.Add(new AuditEntry
        {
            Actor = actor,
            Action = $"Signalement : {req.Status}",
            Resource = report.PharmacyName,
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(report);
    }
}
