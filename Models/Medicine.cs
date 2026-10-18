namespace PharmaFind.Models;

/// <summary>Médicament du référentiel central (géré par l'administrateur).</summary>
public class Medicine
{
    public int Id { get; set; }
    public string BrandName { get; set; } = string.Empty;   // nom commercial
    public string Inn { get; set; } = string.Empty;         // DCI
    public string Dosage { get; set; } = string.Empty;
    public string Form { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DeliveryCondition Delivery { get; set; } = DeliveryCondition.AVerifier;
    public bool IsActive { get; set; } = true;

    public string DisplayName => $"{BrandName} {Dosage}";
    public string Details => $"{Inn} · {Form}";

    public string DeliveryText => Delivery switch
    {
        DeliveryCondition.SansOrdonnance => "Sans ordonnance",
        DeliveryCondition.SurOrdonnance => "Sur ordonnance",
        _ => "Conditions de délivrance à vérifier"
    };

    public bool RequiresPrescription => Delivery == DeliveryCondition.SurOrdonnance;

    public Medicine Clone() => (Medicine)MemberwiseClone();
}
