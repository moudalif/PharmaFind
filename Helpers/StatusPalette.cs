using PharmaFind.Models;

namespace PharmaFind.Helpers;

/// <summary>Correspondance statut → texte et couleurs. Centralisée pour garder des badges cohérents.</summary>
public static class StatusPalette
{
    public static readonly Color Muted = Color.FromArgb("#6B7280");

    private static readonly (Color Fg, Color Bg) Green = (Color.FromArgb("#15803D"), Color.FromArgb("#DCFCE7"));
    private static readonly (Color Fg, Color Bg) Amber = (Color.FromArgb("#B45309"), Color.FromArgb("#FEF3C7"));
    private static readonly (Color Fg, Color Bg) Red = (Color.FromArgb("#B91C1C"), Color.FromArgb("#FEE2E2"));
    private static readonly (Color Fg, Color Bg) Gray = (Color.FromArgb("#4B5563"), Color.FromArgb("#F1F3F5"));
    private static readonly (Color Fg, Color Bg) Blue = (Color.FromArgb("#2F74B5"), Color.FromArgb("#E8F1FA"));

    public static (string Text, Color Foreground, Color Background) For(AvailabilityStatus s) => s switch
    {
        AvailabilityStatus.Disponible => ("Disponible", Green.Fg, Green.Bg),
        AvailabilityStatus.StockFaible => ("Stock faible", Amber.Fg, Amber.Bg),
        AvailabilityStatus.Indisponible => ("Indisponible", Red.Fg, Red.Bg),
        _ => ("Non confirmée", Gray.Fg, Gray.Bg)
    };

    public static (string Text, Color Foreground, Color Background) For(object? value) => value switch
    {
        AvailabilityStatus a => For(a),
        ValidationStatus v => v switch
        {
            ValidationStatus.Validee => ("Validée", Green.Fg, Green.Bg),
            ValidationStatus.EnAttente => ("En attente", Amber.Fg, Amber.Bg),
            ValidationStatus.Suspendue => ("Suspendue", Red.Fg, Red.Bg),
            _ => ("Refusée", Gray.Fg, Gray.Bg)
        },
        ReportStatus r => r switch
        {
            ReportStatus.Nouveau => ("Nouveau", Blue.Fg, Blue.Bg),
            ReportStatus.EnCours => ("En cours", Amber.Fg, Amber.Bg),
            ReportStatus.Traite => ("Traité", Green.Fg, Green.Bg),
            _ => ("Rejeté", Gray.Fg, Gray.Bg)
        },
        AccountStatus u => u == AccountStatus.Actif ? ("Actif", Green.Fg, Green.Bg) : ("Suspendu", Red.Fg, Red.Bg),
        UserRole role => role switch
        {
            UserRole.Administrateur => ("Admin", Blue.Fg, Blue.Bg),
            UserRole.Pharmacie => ("Pharmacie", Green.Fg, Green.Bg),
            _ => ("Visiteur", Gray.Fg, Gray.Bg)
        },
        DeliveryCondition d => d switch
        {
            DeliveryCondition.SansOrdonnance => ("Sans ordonnance", Green.Fg, Green.Bg),
            DeliveryCondition.SurOrdonnance => ("Sur ordonnance", Blue.Fg, Blue.Bg),
            _ => ("À vérifier", Gray.Fg, Gray.Bg)
        },
        StockFreshness f => f switch
        {
            StockFreshness.Recente => ("À jour", Green.Fg, Green.Bg),
            StockFreshness.AVerifier => ("À vérifier", Amber.Fg, Amber.Bg),
            _ => ("Ancien", Red.Fg, Red.Bg)
        },
        bool open => open ? ("Ouverte", Green.Fg, Green.Bg) : ("Fermée", Red.Fg, Red.Bg),
        _ => (value?.ToString() ?? string.Empty, Gray.Fg, Gray.Bg)
    };

    public static Color ForFreshness(StockFreshness f) => f switch
    {
        StockFreshness.Recente => Color.FromArgb("#4B5563"),
        StockFreshness.AVerifier => Amber.Fg,
        _ => Red.Fg
    };

    /// <summary>Couleur hexadécimale pour les marqueurs de la carte (HTML).</summary>
    public static string HexFor(AvailabilityStatus s) => s switch
    {
        AvailabilityStatus.Disponible => "#15803D",
        AvailabilityStatus.StockFaible => "#D97706",
        AvailabilityStatus.Indisponible => "#B91C1C",
        _ => "#6B7280"
    };
}
