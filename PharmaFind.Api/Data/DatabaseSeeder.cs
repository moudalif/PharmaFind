using Microsoft.EntityFrameworkCore;
using PharmaFind.Api.Models;
using PharmaFind.Api.Services;

namespace PharmaFind.Api.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(PharmaFindDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Medicines.AnyAsync()) return;

        // 1. Medicines
        var medicines = new List<Medicine>
        {
            new() { Id = 1, BrandName = "Doliprane", Inn = "Paracétamol", Dosage = "500 mg", Form = "Comprimé", Category = "Antalgique / antipyrétique", Delivery = DeliveryCondition.SansOrdonnance, Description = "Médicament utilisé contre la douleur et la fièvre." },
            new() { Id = 2, BrandName = "Doliprane", Inn = "Paracétamol", Dosage = "1000 mg", Form = "Comprimé", Category = "Antalgique / antipyrétique", Delivery = DeliveryCondition.SansOrdonnance, Description = "Médicament utilisé contre la douleur et la fièvre, dosage adulte." },
            new() { Id = 3, BrandName = "Efferalgan", Inn = "Paracétamol", Dosage = "500 mg", Form = "Comprimé effervescent", Category = "Antalgique / antipyrétique", Delivery = DeliveryCondition.SansOrdonnance, Description = "Forme effervescente à dissoudre dans un verre d'eau." },
            new() { Id = 4, BrandName = "Paracétamol pédiatrique", Inn = "Paracétamol", Dosage = "2,4 %", Form = "Sirop", Category = "Antalgique / antipyrétique", Delivery = DeliveryCondition.SansOrdonnance, Description = "Forme buvable destinée aux enfants. Demandez conseil au pharmacien." },
            new() { Id = 5, BrandName = "Amoxicilline", Inn = "Amoxicilline", Dosage = "500 mg", Form = "Gélule", Category = "Antibiotique", Delivery = DeliveryCondition.SurOrdonnance, Description = "Antibiotique de la famille des pénicillines." },
            new() { Id = 6, BrandName = "Coartem", Inn = "Artéméther / Luméfantrine", Dosage = "20/120 mg", Form = "Comprimé", Category = "Antipaludique", Delivery = DeliveryCondition.AVerifier, Description = "Traitement du paludisme. Un test de diagnostic est recommandé avant tout traitement." },
            new() { Id = 7, BrandName = "Ibuprofène", Inn = "Ibuprofène", Dosage = "400 mg", Form = "Comprimé", Category = "Anti-inflammatoire", Delivery = DeliveryCondition.AVerifier, Description = "Anti-inflammatoire non stéroïdien. Contre-indications à vérifier avec un professionnel de santé." },
            new() { Id = 8, BrandName = "Spasfon", Inn = "Phloroglucinol", Dosage = "80 mg", Form = "Comprimé", Category = "Antispasmodique", Delivery = DeliveryCondition.SansOrdonnance, Description = "Médicament utilisé contre les douleurs spasmodiques." },
            new() { Id = 9, BrandName = "Smecta", Inn = "Diosmectite", Dosage = "3 g", Form = "Poudre pour suspension buvable", Category = "Gastro-entérologie", Delivery = DeliveryCondition.SansOrdonnance, Description = "Pansement digestif en sachets." },
            new() { Id = 10, BrandName = "Ventoline", Inn = "Salbutamol", Dosage = "100 µg/dose", Form = "Suspension pour inhalation", Category = "Pneumologie", Delivery = DeliveryCondition.SurOrdonnance, Description = "Bronchodilatateur en aérosol doseur." },
            new() { Id = 11, BrandName = "Metformine", Inn = "Metformine", Dosage = "500 mg", Form = "Comprimé", Category = "Antidiabétique", Delivery = DeliveryCondition.SurOrdonnance, Description = "Traitement du diabète de type 2, suivi médical nécessaire." },
            new() { Id = 12, BrandName = "Amlodipine", Inn = "Amlodipine", Dosage = "5 mg", Form = "Comprimé", Category = "Cardiologie", Delivery = DeliveryCondition.SurOrdonnance, Description = "Traitement de l'hypertension artérielle, suivi médical nécessaire." },
            new() { Id = 13, BrandName = "SRO", Inn = "Sels de réhydratation orale", Dosage = "20,5 g", Form = "Poudre en sachet", Category = "Réhydratation", Delivery = DeliveryCondition.SansOrdonnance, Description = "Sachets à dissoudre pour compenser les pertes en eau et en sels." },
            new() { Id = 14, BrandName = "Vitamine C", Inn = "Acide ascorbique", Dosage = "500 mg", Form = "Comprimé à croquer", Category = "Vitamines", Delivery = DeliveryCondition.SansOrdonnance, Description = "Complément en vitamine C." }
        };
        await db.Medicines.AddRangeAsync(medicines);

        // 2. Pharmacies
        var pharmacies = new List<Pharmacy>
        {
            new() { Id = 1, Name = "Pharmacie des Cocotiers", City = "Douala", District = "Douala Ier", Neighborhood = "Akwa", Landmark = "Près du carrefour principal, face à la banque", Latitude = 4.0512, Longitude = 9.6978, Phone = "600000101", Email = "contact1@pharmacie-demo.cm", LicenseNumber = "DEMO-PH-2021-001", Pharmacist = "Dr Pharmacien démo 1", OpensAt = TimeSpan.FromHours(8), ClosesAt = TimeSpan.FromHours(21), OpenOnSunday = true, Status = ValidationStatus.Validee, RequestedAt = DateTime.UtcNow.AddDays(-67), Documents = ["Autorisation.pdf", "Registre.pdf"] },
            new() { Id = 2, Name = "Pharmacie Bonanjo Santé", City = "Douala", District = "Douala Ier", Neighborhood = "Bonanjo", Landmark = "À côté de l'immeuble administratif", Latitude = 4.0438, Longitude = 9.6884, Phone = "600000102", Email = "contact2@pharmacie-demo.cm", LicenseNumber = "DEMO-PH-2022-002", Pharmacist = "Dr Pharmacien démo 2", OpensAt = TimeSpan.FromHours(7), ClosesAt = TimeSpan.FromHours(20), Status = ValidationStatus.Validee, RequestedAt = DateTime.UtcNow.AddDays(-74), Documents = ["Autorisation.pdf"] },
            new() { Id = 3, Name = "Pharmacie La Rosée", City = "Douala", District = "Douala Ier", Neighborhood = "Bonapriso", Landmark = "Rue des écoles, derrière la boulangerie", Latitude = 4.0329, Longitude = 9.6992, Phone = "600000103", Email = "contact3@pharmacie-demo.cm", LicenseNumber = "DEMO-PH-2023-003", Pharmacist = "Dr Pharmacien démo 3", OpensAt = TimeSpan.FromHours(8), ClosesAt = TimeSpan.FromHours(20), Is24h = true, Status = ValidationStatus.Validee, RequestedAt = DateTime.UtcNow.AddDays(-81), Documents = ["Autorisation.pdf"] },
            new() { Id = 4, Name = "Pharmacie Étoile de Deïdo", City = "Douala", District = "Douala Ier", Neighborhood = "Deïdo", Landmark = "Face au marché", Latitude = 4.0645, Longitude = 9.7071, Phone = "600000104", Email = "contact4@pharmacie-demo.cm", LicenseNumber = "DEMO-PH-2024-004", Pharmacist = "Dr Pharmacien démo 4", OpensAt = TimeSpan.FromHours(8), ClosesAt = TimeSpan.FromHours(21), IsOnDuty = true, Status = ValidationStatus.Validee, RequestedAt = DateTime.UtcNow.AddDays(-88), Documents = ["Autorisation.pdf"] },
            new() { Id = 5, Name = "Pharmacie du Carrefour Ndokotti", City = "Douala", District = "Douala IIIe", Neighborhood = "Ndokotti", Landmark = "Au rond-point, côté station-service", Latitude = 4.0476, Longitude = 9.7412, Phone = "600000105", Email = "contact5@pharmacie-demo.cm", LicenseNumber = "DEMO-PH-2020-005", Pharmacist = "Dr Pharmacien démo 5", OpensAt = TimeSpan.FromHours(7), ClosesAt = TimeSpan.FromHours(22), OpenOnSunday = true, Status = ValidationStatus.Validee, RequestedAt = DateTime.UtcNow.AddDays(-95), Documents = ["Autorisation.pdf"] },
            new() { Id = 6, Name = "Pharmacie Les Lauriers", City = "Douala", District = "Douala IIe", Neighborhood = "New Bell", Landmark = "Près de l'école publique", Latitude = 4.0386, Longitude = 9.7097, Phone = "600000106", Email = "contact6@pharmacie-demo.cm", LicenseNumber = "DEMO-PH-2021-006", Pharmacist = "Dr Pharmacien démo 6", OpensAt = TimeSpan.FromHours(8), ClosesAt = TimeSpan.FromHours(21), Status = ValidationStatus.Validee, RequestedAt = DateTime.UtcNow.AddDays(-102), Documents = ["Autorisation.pdf"] },
            new() { Id = 7, Name = "Pharmacie Makepe Plus", City = "Douala", District = "Douala Ve", Neighborhood = "Makepe", Landmark = "Entrée du quartier, après le pont", Latitude = 4.0768, Longitude = 9.7546, Phone = "600000107", Email = "contact7@pharmacie-demo.cm", LicenseNumber = "DEMO-PH-2022-007", Pharmacist = "Dr Pharmacien démo 7", OpensAt = TimeSpan.FromHours(8), ClosesAt = TimeSpan.FromHours(21), Status = ValidationStatus.Validee, RequestedAt = DateTime.UtcNow.AddDays(-109), Documents = ["Autorisation.pdf"] },
            new() { Id = 8, Name = "Pharmacie Kotto Espérance", City = "Douala", District = "Douala Ve", Neighborhood = "Kotto", Landmark = "Bloc C, à côté de la chapelle", Latitude = 4.0703, Longitude = 9.7731, Phone = "600000108", Email = "contact8@pharmacie-demo.cm", LicenseNumber = "DEMO-PH-2023-008", Pharmacist = "Dr Pharmacien démo 8", OpensAt = TimeSpan.FromHours(8), ClosesAt = TimeSpan.FromHours(19), Status = ValidationStatus.Validee, RequestedAt = DateTime.UtcNow.AddDays(-116), Documents = ["Autorisation.pdf"] },
            new() { Id = 9, Name = "Pharmacie Mboa Bépanda", City = "Douala", District = "Douala Ve", Neighborhood = "Bépanda", Landmark = "Carrefour Omnisports", Latitude = 4.0618, Longitude = 9.7251, Phone = "600000109", Email = "contact9@pharmacie-demo.cm", LicenseNumber = "DEMO-PH-2024-009", Pharmacist = "Dr Pharmacien démo 9", OpensAt = TimeSpan.FromHours(8), ClosesAt = TimeSpan.FromHours(21), Status = ValidationStatus.Validee, RequestedAt = DateTime.UtcNow.AddDays(-123), Documents = ["Autorisation.pdf"] },
            new() { Id = 10, Name = "Pharmacie Rive Droite", City = "Douala", District = "Douala IVe", Neighborhood = "Bonabéri", Landmark = "Après le pont, première à droite", Latitude = 4.0779, Longitude = 9.6589, Phone = "600000110", Email = "contact10@pharmacie-demo.cm", LicenseNumber = "DEMO-PH-2020-010", Pharmacist = "Dr Pharmacien démo 10", OpensAt = TimeSpan.FromHours(8), ClosesAt = TimeSpan.FromHours(21), OpenOnSunday = true, Status = ValidationStatus.Validee, RequestedAt = DateTime.UtcNow.AddDays(-130), Documents = ["Autorisation.pdf"] },
            new() { Id = 11, Name = "Pharmacie du Rail", City = "Douala", District = "Douala IIIe", Neighborhood = "Logbaba", Landmark = "Près de la voie ferrée", Latitude = null, Longitude = null, Phone = "600000111", Email = "contact11@pharmacie-demo.cm", LicenseNumber = "DEMO-PH-2021-011", Pharmacist = "Dr Pharmacien démo 11", OpensAt = TimeSpan.FromHours(8), ClosesAt = TimeSpan.FromHours(21), Status = ValidationStatus.Validee, RequestedAt = DateTime.UtcNow.AddDays(-137), Documents = ["Autorisation.pdf"] },
            new() { Id = 12, Name = "Pharmacie Bastos Centre", City = "Yaoundé", District = "Yaoundé Ier", Neighborhood = "Bastos", Landmark = "Près du rond-point", Latitude = 3.8935, Longitude = 11.5108, Phone = "600000112", Email = "contact12@pharmacie-demo.cm", LicenseNumber = "DEMO-PH-2022-012", Pharmacist = "Dr Pharmacien démo 12", OpensAt = TimeSpan.FromHours(8), ClosesAt = TimeSpan.FromHours(21), Status = ValidationStatus.Validee, RequestedAt = DateTime.UtcNow.AddDays(-144), Documents = ["Autorisation.pdf"] },
            new() { Id = 13, Name = "Pharmacie Logpom Nouvelle", City = "Douala", District = "Douala Ve", Neighborhood = "Logpom", Landmark = "Face à l'entrée de la cité", Latitude = 4.0872, Longitude = 9.7583, Phone = "600000113", Email = "attente@demo.cm", LicenseNumber = "DEMO-PH-2023-013", Pharmacist = "Dr Pharmacien démo 13", OpensAt = TimeSpan.FromHours(8), ClosesAt = TimeSpan.FromHours(21), Status = ValidationStatus.EnAttente, RequestedAt = DateTime.UtcNow.AddDays(-2), Documents = ["Autorisation.pdf", "Registre.pdf"] },
            new() { Id = 14, Name = "Pharmacie Bonne Santé PK14", City = "Douala", District = "Douala IIIe", Neighborhood = "PK14", Landmark = "Bord de route, côté marché", Latitude = 4.0231, Longitude = 9.7981, Phone = "600000114", Email = "contact14@pharmacie-demo.cm", LicenseNumber = "DEMO-PH-2024-014", Pharmacist = "Dr Pharmacien démo 14", OpensAt = TimeSpan.FromHours(8), ClosesAt = TimeSpan.FromHours(21), Status = ValidationStatus.EnAttente, RequestedAt = DateTime.UtcNow.AddDays(-3), Documents = ["Autorisation.pdf"] },
            new() { Id = 15, Name = "Pharmacie Ancienne Route", City = "Douala", District = "Douala IIIe", Neighborhood = "Ndokotti", Landmark = "Ancienne route de Bassa", Latitude = 4.0459, Longitude = 9.7380, Phone = "600000115", Email = "contact15@pharmacie-demo.cm", LicenseNumber = "DEMO-PH-2020-015", Pharmacist = "Dr Pharmacien démo 15", OpensAt = TimeSpan.FromHours(8), ClosesAt = TimeSpan.FromHours(21), Status = ValidationStatus.Suspendue, StatusReason = "Signalements répétés de stocks erronés (démo).", RequestedAt = DateTime.UtcNow.AddDays(-165), Documents = ["Autorisation.pdf"] }
        };
        await db.Pharmacies.AddRangeAsync(pharmacies);

        // 3. Stock Items
        int[] basePrices = [1000, 1500, 1600, 1800, 2500, 4500, 1200, 3200, 3800, 5500, 2000, 3000, 300, 1500];
        var rng = new Random(42);
        var stocks = new List<StockItem>();
        int stockId = 1;

        foreach (var p in pharmacies.Where(x => x.Status != ValidationStatus.EnAttente))
        {
            foreach (var m in medicines)
            {
                if (rng.NextDouble() > 0.72) continue;
                var hoursAgo = rng.Next(0, 10) switch { < 5 => rng.NextDouble() * 20, < 8 => 26 + rng.NextDouble() * 40, _ => 80 + rng.NextDouble() * 200 };
                var qty = rng.Next(0, 10) switch { 0 => 0, < 3 => rng.Next(1, 10), _ => rng.Next(12, 150) };
                var price = (int)(Math.Round(basePrices[m.Id - 1] * (0.9 + rng.NextDouble() * 0.25) / 25.0) * 25);
                stocks.Add(new StockItem
                {
                    Id = stockId++,
                    PharmacyId = p.Id,
                    MedicineId = m.Id,
                    Quantity = qty,
                    PriceXaf = price,
                    LowStockThreshold = 10,
                    LastUpdated = DateTime.UtcNow.AddHours(-hoursAgo),
                    DeclaredOutOfStock = qty == 0 && rng.NextDouble() > 0.5
                });
            }
        }
        await db.StockItems.AddRangeAsync(stocks);

        // 4. Users (Universal demo password: Demo1234)
        var defaultHash = PasswordHasher.Hash("Demo1234");
        var users = new List<User>
        {
            new() { Id = 1, Name = "Visiteur Démo", Email = "visiteur@demo.cm", PasswordHash = defaultHash, Role = UserRole.Visiteur, CreatedAt = DateTime.UtcNow.AddDays(-40) },
            new() { Id = 2, Name = "Pharmacie des Cocotiers", Email = "pharmacie@demo.cm", PasswordHash = defaultHash, Role = UserRole.Pharmacie, PharmacyId = 1, CreatedAt = DateTime.UtcNow.AddDays(-120) },
            new() { Id = 3, Name = "Administrateur Démo", Email = "admin@demo.cm", PasswordHash = defaultHash, Role = UserRole.Administrateur, CreatedAt = DateTime.UtcNow.AddDays(-200) },
            new() { Id = 4, Name = "Pharmacie Logpom Nouvelle", Email = "attente@demo.cm", PasswordHash = defaultHash, Role = UserRole.Pharmacie, PharmacyId = 13, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new() { Id = 5, Name = "Utilisateur Exemple A", Email = "exemple.a@demo.cm", PasswordHash = defaultHash, Role = UserRole.Visiteur, CreatedAt = DateTime.UtcNow.AddDays(-12) },
            new() { Id = 6, Name = "Utilisateur Exemple B", Email = "exemple.b@demo.cm", PasswordHash = defaultHash, Role = UserRole.Visiteur, Status = AccountStatus.Suspendu, CreatedAt = DateTime.UtcNow.AddDays(-60) },
            new() { Id = 7, Name = "Pharmacie Bonanjo Santé", Email = "bonanjo@demo.cm", PasswordHash = defaultHash, Role = UserRole.Pharmacie, PharmacyId = 2, CreatedAt = DateTime.UtcNow.AddDays(-95) },
        };
        await db.Users.AddRangeAsync(users);

        // 5. Reports
        var reports = new List<PharmacyReport>
        {
            new() { Id = 1, PharmacyId = 1, PharmacyName = "Pharmacie des Cocotiers", MedicineName = "Coartem 20/120 mg", Reason = ReportReason.StockIncorrect, Comment = "Indiqué disponible mais le produit était en rupture ce matin.", CreatedAt = DateTime.UtcNow.AddHours(-5) },
            new() { Id = 2, PharmacyId = 4, PharmacyName = "Pharmacie Étoile de Deïdo", Reason = ReportReason.HorairesErrones, Comment = "Fermée à 19 h alors que l'application indique 21 h.", CreatedAt = DateTime.UtcNow.AddDays(-1), Status = ReportStatus.EnCours },
            new() { Id = 3, PharmacyId = 1, PharmacyName = "Pharmacie des Cocotiers", MedicineName = "Smecta 3 g", Reason = ReportReason.PrixIncorrect, Comment = "Le prix au comptoir était plus élevé.", CreatedAt = DateTime.UtcNow.AddDays(-3), Status = ReportStatus.Traite },
            new() { Id = 4, PharmacyId = 11, PharmacyName = "Pharmacie du Rail", Reason = ReportReason.CoordonneesErronees, Comment = "La pharmacie n'apparaît pas sur la carte.", CreatedAt = DateTime.UtcNow.AddDays(-2) }
        };
        await db.PharmacyReports.AddRangeAsync(reports);

        // 6. Audit & Notifications
        var audit = new List<AuditEntry>
        {
            new() { Id = 1, Actor = "admin@demo.cm", Action = "Suspension de compte", Resource = "exemple.b@demo.cm", Timestamp = DateTime.UtcNow.AddDays(-15) },
            new() { Id = 2, Actor = "bonanjo@demo.cm", Action = "Modification des horaires", Resource = "Pharmacie Bonanjo Santé", Timestamp = DateTime.UtcNow.AddDays(-8) },
            new() { Id = 3, Actor = "admin@demo.cm", Action = "Validation", Resource = "Pharmacie Rive Droite", Timestamp = DateTime.UtcNow.AddDays(-6) },
            new() { Id = 4, Actor = "attente@demo.cm", Action = "Demande de compte", Resource = "Pharmacie Logpom Nouvelle", Timestamp = DateTime.UtcNow.AddDays(-2) },
            new() { Id = 5, Actor = "admin@demo.cm", Action = "Ajout au référentiel", Resource = "Vitamine C 500 mg", Timestamp = DateTime.UtcNow.AddDays(-2) },
            new() { Id = 6, Actor = "admin@demo.cm", Action = "Suspension", Resource = "Pharmacie Ancienne Route", Timestamp = DateTime.UtcNow.AddDays(-1) },
            new() { Id = 7, Actor = "pharmacie@demo.cm", Action = "Mise à jour du stock", Resource = "Doliprane 500 mg", Timestamp = DateTime.UtcNow.AddHours(-2) },
            new() { Id = 8, Actor = "admin@demo.cm", Action = "Connexion", Resource = "Administration", Timestamp = DateTime.UtcNow.AddMinutes(-20) }
        };
        await db.AuditEntries.AddRangeAsync(audit);

        var notifications = new List<AppNotification>
        {
            new() { Id = 1, PharmacyId = 1, Kind = NotificationKind.StockFaible, Title = "Stock faible", Message = "Doliprane 500 mg : 4 unités restantes.", Date = DateTime.UtcNow.AddHours(-2), StockId = 1 },
            new() { Id = 2, PharmacyId = 1, Kind = NotificationKind.Signalement, Title = "Nouveau signalement", Message = "Un visiteur signale un stock incorrect pour Coartem 20/120 mg.", Date = DateTime.UtcNow.AddHours(-5), ReportId = 1 },
            new() { Id = 3, PharmacyId = 1, Kind = NotificationKind.Information, Title = "Stock à mettre à jour", Message = "Doliprane 1000 mg n'a pas été mis à jour depuis 5 jours.", Date = DateTime.UtcNow.AddDays(-1), StockId = 2 },
            new() { Id = 4, PharmacyId = 1, Kind = NotificationKind.Validation, Title = "Compte validé", Message = "Votre établissement est visible par les visiteurs.", Date = DateTime.UtcNow.AddDays(-120), IsRead = true }
        };
        await db.Notifications.AddRangeAsync(notifications);

        var favorites = new List<UserFavorite>
        {
            new() { Id = 1, UserId = 1, PharmacyId = 2 },
            new() { Id = 2, UserId = 1, PharmacyId = 5 }
        };
        await db.Favorites.AddRangeAsync(favorites);

        var changes = new List<StockChange>
        {
            new() { Id = 1, PharmacyId = 1, Date = DateTime.UtcNow.AddDays(-7), MedicineName = "Établissement", Description = "Horaires modifiés : 08:00 – 21:00, 7 j/7", Author = "pharmacie@demo.cm" },
            new() { Id = 2, PharmacyId = 1, Date = DateTime.UtcNow.AddHours(-2), MedicineName = "Doliprane 500 mg", Description = "Quantité : 4 · Prix : 1 000 FCFA", Author = "pharmacie@demo.cm" }
        };
        await db.StockChanges.AddRangeAsync(changes);

        // 7. Search counts
        foreach (var m in medicines)
        {
            db.SearchStats.Add(new SearchStat { MedicineId = m.Id, Query = m.BrandName, Timestamp = DateTime.UtcNow.AddDays(-rng.Next(1, 7)) });
        }

        await db.SaveChangesAsync();
    }
}
