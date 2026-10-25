namespace PharmaFind.Api.Models;

public enum AvailabilityStatus { Disponible, StockFaible, Indisponible, NonConfirmee }

public enum StockFreshness { Recente, AVerifier, Ancienne }

public enum DeliveryCondition { SansOrdonnance, SurOrdonnance, AVerifier }

public enum ValidationStatus { EnAttente, Validee, Refusee, Suspendue }

public enum UserRole { Visiteur, Pharmacie, Administrateur }

public enum AccountStatus { Actif, Suspendu }

public enum ReportStatus { Nouveau, EnCours, Traite, Rejete }

public enum ReportReason { StockIncorrect, PrixIncorrect, CoordonneesErronees, HorairesErrones, PharmacieFermee, Autre }

public enum NotificationKind { StockFaible, Signalement, Validation, Information }
