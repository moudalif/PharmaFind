using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaFind.Api.Data;
using PharmaFind.Api.Models;
using PharmaFind.Api.Services;

namespace PharmaFind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly PharmaFindDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(PharmaFindDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        {
            return BadRequest(new { message = "Le courriel et le mot de passe sont obligatoires." });
        }

        var email = req.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        if (user == null || !PasswordHasher.Verify(req.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Courriel ou mot de passe incorrect." });
        }

        if (user.Status == AccountStatus.Suspendu)
        {
            return StatusCode(403, new { message = "Ce compte est suspendu. Contactez l'administration PharmaFind." });
        }

        // Automatic transparent migration of legacy hashes to PBKDF2
        if (!user.PasswordHash.StartsWith("$pbkdf2$", StringComparison.Ordinal))
        {
            user.PasswordHash = PasswordHasher.Hash(req.Password);
        }

        var token = _tokenService.CreateToken(user);

        // Audit log
        _db.AuditEntries.Add(new AuditEntry
        {
            Actor = user.Email,
            Action = "Connexion",
            Resource = user.Role.ToString(),
            Timestamp = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return Ok(new AuthResponse(token, user));
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var exists = await _db.Users.AnyAsync(u => u.Email.ToLower() == email);
        if (exists)
        {
            return BadRequest(new { message = "Un compte existe déjà avec ce courriel." });
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        var user = new User
        {
            Name = req.Name.Trim(),
            Email = email,
            PasswordHash = PasswordHasher.Hash(req.Password),
            Role = UserRole.Visiteur,
            Status = AccountStatus.Actif,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        _db.AuditEntries.Add(new AuditEntry
        {
            Actor = email,
            Action = "Inscription",
            Resource = "Visiteur",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        var token = _tokenService.CreateToken(user);
        return Ok(new AuthResponse(token, user));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<User>> GetCurrentUser()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound(new { message = "Utilisateur introuvable." });

        return Ok(user);
    }
}
