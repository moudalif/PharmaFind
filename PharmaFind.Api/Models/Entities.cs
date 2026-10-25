using System.Text.Json.Serialization;

namespace PharmaFind.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public AccountStatus Status { get; set; } = AccountStatus.Actif;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? PharmacyId { get; set; }
}

public class Medicine
{
    public int Id { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string Inn { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Form { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DeliveryCondition Delivery { get; set; } = DeliveryCondition.AVerifier;
    public bool IsActive { get; set; } = true;

    public string DisplayName => $"{BrandName} {Dosage}";
    public string Details => $"{Inn} · {Form}";
}

public class Pharmacy
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string Landmark { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string Pharmacist { get; set; } = string.Empty;

    public TimeSpan OpensAt { get; set; } = new(8, 0, 0);
    public TimeSpan ClosesAt { get; set; } = new(20, 0, 0);
    public bool Is24h { get; set; }
    public bool OpenOnSunday { get; set; }
    public bool IsOnDuty { get; set; }

    public ValidationStatus Status { get; set; } = ValidationStatus.EnAttente;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public string? StatusReason { get; set; }
    public List<string> Documents { get; set; } = [];

    public bool HasCoordinates => Latitude.HasValue && Longitude.HasValue;
    public string LocationText => $"{Neighborhood}, {District}";
    public string FullLocationText => $"{Neighborhood}, {District}, {City}";
}

public class StockItem
{
    public int Id { get; set; }
    public int PharmacyId { get; set; }
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;
    public int Quantity { get; set; }
    public int PriceXaf { get; set; }
    public int LowStockThreshold { get; set; } = 10;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public bool DeclaredOutOfStock { get; set; }
}

public class PharmacyReport
{
    public int Id { get; set; }
    public string Author { get; set; } = "Visiteur anonyme";
    public int PharmacyId { get; set; }
    public string PharmacyName { get; set; } = string.Empty;
    public string? MedicineName { get; set; }
    public ReportReason Reason { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ReportStatus Status { get; set; } = ReportStatus.Nouveau;
}

public class AuditEntry
{
    public int Id { get; set; }
    public string Actor { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class StockChange
{
    public int Id { get; set; }
    public int PharmacyId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string MedicineName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
}

public class AppNotification
{
    public int Id { get; set; }
    public int PharmacyId { get; set; }
    public NotificationKind Kind { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int? ReportId { get; set; }
    public int? StockId { get; set; }
    public bool IsRead { get; set; }
}

public class UserFavorite
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PharmacyId { get; set; }
}

public class SearchStat
{
    public int Id { get; set; }
    public int? MedicineId { get; set; }
    public string Query { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
