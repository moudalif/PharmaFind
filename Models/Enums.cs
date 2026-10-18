global using ViewState = PharmaFind.Models.ViewState;

namespace PharmaFind.Models;

/// <summary>Statut de disponibilité affiché au visiteur (règle métier du cahier des charges).</summary>
public enum AvailabilityStatus { Disponible, StockFaible, Indisponible, NonConfirmee }

/// <summary>Fraîcheur de la dernière mise à jour du stock.</summary>
public enum StockFreshness { Recente, AVerifier, Ancienne }

public enum DeliveryCondition { SansOrdonnance, SurOrdonnance, AVerifier }

public enum ValidationStatus { EnAttente, Validee, Refusee, Suspendue }

public enum UserRole { Visiteur, Pharmacie, Administrateur }

public enum AccountStatus { Actif, Suspendu }

public enum ReportStatus { Nouveau, EnCours, Traite, Rejete }

public enum ReportReason { StockIncorrect, PrixIncorrect, CoordonneesErronees, HorairesErrones, PharmacieFermee, Autre }

public enum LocationSource { Gps, Manuelle }

public enum NotificationKind { StockFaible, Signalement, Validation, Information }

/// <summary>État d'affichage commun à tous les écrans.</summary>
public enum ViewState { Content, Loading, Empty, Error, Offline }

public enum ResultSort { Distance, Prix }
