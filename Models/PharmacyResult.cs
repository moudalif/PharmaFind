using PharmaFind.Helpers;

namespace PharmaFind.Models;

/// <summary>Résultat de recherche : une pharmacie, sa ligne de stock (si un médicament est recherché) et sa distance.</summary>
public class PharmacyResult
{
    public required Pharmacy Pharmacy { get; init; }
    public StockItem? Stock { get; init; }
    public double? DistanceKm { get; init; }

    public AvailabilityStatus Status => Stock?.Status ?? AvailabilityStatus.NonConfirmee;
    public bool HasStock => Stock is not null;
    public string DistanceText => DistanceKm is { } d ? GeoHelper.FormatDistance(d) : "Distance inconnue";
    public string PriceText => Stock?.PriceText ?? "—";
    public string UpdatedText => Stock?.UpdatedText ?? string.Empty;
    public StockFreshness Freshness => Stock?.Freshness ?? StockFreshness.Ancienne;
    public bool IsStale => Stock is not null && Stock.Freshness == StockFreshness.Ancienne;
    public bool IsOpen => Pharmacy.IsOpenNow;
}
