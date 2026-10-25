using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PharmaFind.Api.Models;

namespace PharmaFind.Api.Data;

public class PharmaFindDbContext : DbContext
{
    public PharmaFindDbContext(DbContextOptions<PharmaFindDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<PharmacyReport> PharmacyReports => Set<PharmacyReport>();
    public DbSet<StockChange> StockChanges => Set<StockChange>();
    public DbSet<AppNotification> Notifications => Set<AppNotification>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<UserFavorite> Favorites => Set<UserFavorite>();
    public DbSet<SearchStat> SearchStats => Set<SearchStat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Store List<string> as JSON in SQLite
        var stringListComparer = new ValueComparer<List<string>>(
            (c1, c2) => c1 != null && c2 != null ? c1.SequenceEqual(c2) : c1 == c2,
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        modelBuilder.Entity<Pharmacy>(entity =>
        {
            entity.Property(p => p.Documents)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
                .Metadata.SetValueComparer(stringListComparer);

            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => new { p.City, p.District });
        });

        modelBuilder.Entity<StockItem>(entity =>
        {
            entity.HasOne(s => s.Medicine)
                  .WithMany()
                  .HasForeignKey(s => s.MedicineId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Pharmacy>()
                  .WithMany()
                  .HasForeignKey(s => s.PharmacyId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Guarantee 1 stock entry per (Pharmacy, Medicine)
            entity.HasIndex(s => new { s.PharmacyId, s.MedicineId })
                  .IsUnique();

            // Optimistic concurrency token
            entity.Property(s => s.LastUpdated)
                  .IsConcurrencyToken();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<UserFavorite>(entity =>
        {
            entity.HasIndex(f => new { f.UserId, f.PharmacyId }).IsUnique();
        });

        modelBuilder.Entity<PharmacyReport>(entity =>
        {
            entity.HasIndex(r => new { r.PharmacyId, r.Status });
        });

        modelBuilder.Entity<SearchStat>(entity =>
        {
            entity.HasIndex(s => s.Timestamp);
            entity.HasIndex(s => s.MedicineId);
        });
    }
}
