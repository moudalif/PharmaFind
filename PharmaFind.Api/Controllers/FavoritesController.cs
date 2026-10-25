using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaFind.Api.Data;
using PharmaFind.Api.Models;

namespace PharmaFind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FavoritesController : ControllerBase
{
    private readonly PharmaFindDbContext _db;

    public FavoritesController(PharmaFindDbContext db)
    {
        _db = db;
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(claim) && int.TryParse(claim, out var uid))
        {
            return uid;
        }
        return 1; // Default to visitor 1 for unauthenticated demo
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Pharmacy>>> GetFavorites()
    {
        var uid = GetUserId();
        var favIds = await _db.Favorites.Where(f => f.UserId == uid).Select(f => f.PharmacyId).ToListAsync();
        var list = await _db.Pharmacies.Where(p => favIds.Contains(p.Id)).OrderBy(p => p.Name).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{pharmacyId:int}/check")]
    public async Task<ActionResult<bool>> IsFavorite(int pharmacyId)
    {
        var uid = GetUserId();
        var exists = await _db.Favorites.AnyAsync(f => f.UserId == uid && f.PharmacyId == pharmacyId);
        return Ok(exists);
    }

    [HttpPost("{pharmacyId:int}/toggle")]
    public async Task<ActionResult<bool>> Toggle(int pharmacyId)
    {
        var uid = GetUserId();
        var pharmacyExists = await _db.Pharmacies.AnyAsync(p => p.Id == pharmacyId);
        if (!pharmacyExists) return NotFound(new { message = "Pharmacie introuvable." });

        var fav = await _db.Favorites.FirstOrDefaultAsync(f => f.UserId == uid && f.PharmacyId == pharmacyId);

        if (fav != null)
        {
            _db.Favorites.Remove(fav);
            await _db.SaveChangesAsync();
            return Ok(false);
        }

        _db.Favorites.Add(new UserFavorite { UserId = uid, PharmacyId = pharmacyId });
        try
        {
            await _db.SaveChangesAsync();
            return Ok(true);
        }
        catch (DbUpdateException)
        {
            // Already created concurrently by another request
            return Ok(true);
        }
    }
}
