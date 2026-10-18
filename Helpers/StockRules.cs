using PharmaFind.Models;

namespace PharmaFind.Helpers;

/// <summary>Paramètres de fiabilité modifiables par l'administrateur.</summary>
public class StockRulesSettings
{
    /// <summary>En dessous : mise à jour récente.</summary>
    public int FreshHours { get; set; } = 24;
    /// <summary>Au-delà : donnée ancienne, appel recommandé.</summary>
    public int StaleHours { get; set; } = 72;
    /// <summary>Au-delà : la disponibilité n'est plus confirmée.</summary>
    public int UnconfirmedDays { get; set; } = 7;
    public int DefaultRadiusKm { get; set; } = 5;
}

/// <summary>
/// Règles métier de disponibilité : la disponibilité est un stock déclaré, jamais une garantie de présence.
/// </summary>
public static class StockRules
{
    public static StockRulesSettings Settings { get; } = new();

    public static readonly int[] RadiusOptions = [1, 3, 5, 10, 20];

    public static AvailabilityStatus ComputeStatus(StockItem item, DateTime now)
    {
        if (item.DeclaredOutOfStock || item.Quantity <= 0) return AvailabilityStatus.Indisponible;
        if ((now - item.LastUpdated).TotalDays > Settings.UnconfirmedDays) return AvailabilityStatus.NonConfirmee;
        if (item.Quantity <= item.LowStockThreshold) return AvailabilityStatus.StockFaible;
        return AvailabilityStatus.Disponible;
    }

    public static StockFreshness ComputeFreshness(DateTime lastUpdated, DateTime now)
    {
        var hours = (now - lastUpdated).TotalHours;
        if (hours <= Settings.FreshHours) return StockFreshness.Recente;
        if (hours <= Settings.StaleHours) return StockFreshness.AVerifier;
        return StockFreshness.Ancienne;
    }

    public static string StatusText(AvailabilityStatus status) => status switch
    {
        AvailabilityStatus.Disponible => "Disponible",
        AvailabilityStatus.StockFaible => "Stock faible",
        AvailabilityStatus.Indisponible => "Indisponible",
        _ => "Non confirmée"
    };
}
