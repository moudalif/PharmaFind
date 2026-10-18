using PharmaFind.Helpers;

namespace PharmaFind.Models;

/// <summary>Ligne de stock déclarée par une pharmacie pour un médicament.</summary>
public class StockItem
{
    public int Id { get; set; }
    public int PharmacyId { get; set; }
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = new();
    public int Quantity { get; set; }
    public int PriceXaf { get; set; }
    public int LowStockThreshold { get; set; } = 10;
    public DateTime LastUpdated { get; set; }
    public bool DeclaredOutOfStock { get; set; }

    public AvailabilityStatus Status => StockRules.ComputeStatus(this, DateTime.Now);
    public StockFreshness Freshness => StockRules.ComputeFreshness(LastUpdated, DateTime.Now);
    public bool NeedsUpdate => Freshness != StockFreshness.Recente;

    public string PriceText => PriceFormatter.Format(PriceXaf);
    public string QuantityText => DeclaredOutOfStock ? "Rupture déclarée" : $"{Quantity} en stock";
    public string UpdatedText => "Mis à jour " + DateFormatter.Relative(LastUpdated);
    public string UpdatedFullText => DateFormatter.Full(LastUpdated);

    public StockItem Clone() => (StockItem)MemberwiseClone();
}
