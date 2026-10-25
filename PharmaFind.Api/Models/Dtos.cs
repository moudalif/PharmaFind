using System.ComponentModel.DataAnnotations;

namespace PharmaFind.Api.Models;

public record LoginRequest(
    [Required(ErrorMessage = "Le courriel est requis.")]
    [EmailAddress(ErrorMessage = "Format de courriel invalide.")]
    string Email,

    [Required(ErrorMessage = "Le mot de passe est requis.")]
    string Password);

public record RegisterRequest(
    [Required(ErrorMessage = "Le nom est requis.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Le nom doit comporter entre 2 et 100 caractères.")]
    string Name,

    [Required(ErrorMessage = "Le courriel est requis.")]
    [EmailAddress(ErrorMessage = "Format de courriel invalide.")]
    string Email,

    [Required(ErrorMessage = "Le mot de passe est requis.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Le mot de passe doit comporter au moins 6 caractères.")]
    string Password);

public record AuthResponse(string Token, User User);

public class PharmacyResultDto
{
    public required Pharmacy Pharmacy { get; set; }
    public StockItem? Stock { get; set; }
    public double? DistanceKm { get; set; }
}

public class DashboardStatsDto
{
    public int ValidatedPharmacies { get; init; }
    public int PendingPharmacies { get; init; }
    public int SuspendedPharmacies { get; init; }
    public int Medicines { get; init; }
    public int Users { get; init; }
    public int OpenReports { get; init; }
    public int SearchesLast7Days { get; init; }
    public double StaleStockRatio { get; init; }
}

public class ChartBarDto
{
    public string Label { get; init; } = string.Empty;
    public double Value { get; init; }
    public double Ratio { get; init; }
    public string ValueText { get; init; } = string.Empty;
}

public record StatusChangeRequest(
    [Required] ValidationStatus Status,
    [StringLength(500)] string? Reason = null);

public record UserStatusChangeRequest(
    [Required] AccountStatus Status);

public record ReportStatusChangeRequest(
    [Required] ReportStatus Status);

