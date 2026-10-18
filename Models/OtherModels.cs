using CommunityToolkit.Mvvm.ComponentModel;
using PharmaFind.Helpers;

namespace PharmaFind.Models;

public class UserAccount
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public AccountStatus Status { get; set; } = AccountStatus.Actif;
    public DateTime CreatedAt { get; set; }
    public int? PharmacyId { get; set; }   // ComptePharmacie : lien vers l'établissement

    public string RoleText => Role switch
    {
        UserRole.Pharmacie => "Pharmacie",
        UserRole.Administrateur => "Administrateur",
        _ => "Visiteur"
    };
    public string CreatedText => "Inscrit le " + CreatedAt.ToString("dd/MM/yyyy", AppCulture.Fr);
    public string Initials => string.Concat(Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => char.ToUpper(p[0])));
}

/// <summary>Signalement d'une information erronée (stock, prix, coordonnées…).</summary>
public class PharmacyReport
{
    public int Id { get; set; }
    public string Author { get; set; } = "Visiteur anonyme";
    public int PharmacyId { get; set; }
    public string PharmacyName { get; set; } = string.Empty;
    public string? MedicineName { get; set; }
    public ReportReason Reason { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Nouveau;

    public string ReasonText => ReportTexts.Reason(Reason);
    public string CreatedText => DateFormatter.Relative(CreatedAt);
    public string Subject => MedicineName is null ? PharmacyName : $"{PharmacyName} · {MedicineName}";
}

public class AuditEntry
{
    public int Id { get; set; }
    public string Actor { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string TimestampText => DateFormatter.Full(Timestamp);
}

public class SearchHistoryEntry
{
    public string Query { get; set; } = string.Empty;
    public int? MedicineId { get; set; }
    public DateTime Date { get; set; }
    public string DateText => DateFormatter.Relative(Date);
}

/// <summary>Historique des modifications de stock d'une pharmacie.</summary>
public class StockChange
{
    public DateTime Date { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string DateText => DateFormatter.Full(Date);
}

public partial class AppNotification : ObservableObject
{
    public int Id { get; set; }
    public NotificationKind Kind { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int? ReportId { get; set; }
    public int? StockId { get; set; }

    [ObservableProperty]
    private bool isRead;

    public string DateText => DateFormatter.Relative(Date);
    public string Icon => Kind switch
    {
        NotificationKind.StockFaible => "ic_alert_amber.png",
        NotificationKind.Signalement => "ic_flag.png",
        NotificationKind.Validation => "ic_check.png",
        _ => "ic_info_blue.png"
    };
}

public class UserLocation
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string Label { get; init; } = string.Empty;
    public LocationSource Source { get; init; }
}

/// <summary>Lieu connu pour la saisie manuelle (ville, arrondissement, quartier).</summary>
public class KnownPlace
{
    public string Name { get; init; } = string.Empty;
    public string District { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string Subtitle => string.IsNullOrEmpty(District) ? City : $"{District} · {City}";
}

public class DashboardStats
{
    public int ValidatedPharmacies { get; init; }
    public int PendingPharmacies { get; init; }
    public int SuspendedPharmacies { get; init; }
    public int Medicines { get; init; }
    public int Users { get; init; }
    public int OpenReports { get; init; }
    public int SearchesLast7Days { get; init; }
    public double StaleStockRatio { get; init; }
    public string StaleStockText => StaleStockRatio.ToString("P0", AppCulture.Fr);
}

/// <summary>Barre d'un petit graphique (statistiques administrateur).</summary>
public class ChartBar
{
    public string Label { get; init; } = string.Empty;
    public double Value { get; init; }
    public double Ratio { get; init; }
    public string ValueText { get; init; } = string.Empty;
}

/// <summary>Raison de signalement sélectionnable.</summary>
public partial class ReasonOption(ReportReason reason) : ObservableObject
{
    public ReportReason Reason { get; } = reason;
    public string Text => ReportTexts.Reason(Reason);

    [ObservableProperty]
    private bool isSelected;
}

public static class ReportTexts
{
    public static string Reason(ReportReason reason) => reason switch
    {
        ReportReason.StockIncorrect => "Stock incorrect",
        ReportReason.PrixIncorrect => "Prix incorrect",
        ReportReason.CoordonneesErronees => "Adresse ou téléphone erroné",
        ReportReason.HorairesErrones => "Horaires erronés",
        ReportReason.PharmacieFermee => "Pharmacie fermée définitivement",
        _ => "Autre"
    };

    public static string Status(ReportStatus status) => status switch
    {
        ReportStatus.Nouveau => "Nouveau",
        ReportStatus.EnCours => "En cours",
        ReportStatus.Traite => "Traité",
        _ => "Rejeté"
    };
}
