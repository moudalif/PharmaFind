using PharmaFind.Models;

namespace PharmaFind.Services.Mock;

/// <summary>
/// Jeu de données de démonstration, en mémoire. Les noms de pharmacies, numéros, prix et comptes sont FICTIFS ;
/// les quartiers et arrondissements de Douala sont réels pour rendre la démonstration crédible.
/// Les données sont réinitialisées à chaque lancement.
/// </summary>
public class MockDataStore
{
    public List<Medicine> Medicines { get; } = [];
    public List<Pharmacy> Pharmacies { get; } = [];
    public List<StockItem> Stocks { get; } = [];
    public List<UserAccount> Users { get; } = [];
    public List<PharmacyReport> Reports { get; } = [];
    public List<AuditEntry> Audit { get; } = [];
    public List<StockChange> StockChanges { get; } = [];
    public List<AppNotification> Notifications { get; } = [];
    public HashSet<int> FavoritePharmacyIds { get; } = [2, 5];
    public Dictionary<int, int> SearchCounts { get; } = [];

    private int _nextStockId = 1;
    public int NextStockId() => _nextStockId++;

    public MockDataStore()
    {
        SeedMedicines();
        SeedPharmacies();
        SeedStocks();
        SeedUsers();
        SeedReports();
        SeedAuditAndNotifications();
    }

    public void AddAudit(string actor, string action, string resource)
        => Audit.Insert(0, new AuditEntry { Id = Audit.Count + 1, Actor = actor, Action = action, Resource = resource, Timestamp = DateTime.Now });

    private void SeedMedicines()
    {
        var id = 1;
        void Add(string brand, string inn, string dosage, string form, string category, DeliveryCondition delivery, string description)
            => Medicines.Add(new Medicine
            {
                Id = id++, BrandName = brand, Inn = inn, Dosage = dosage, Form = form,
                Category = category, Delivery = delivery, Description = description
            });

        Add("Doliprane", "Paracétamol", "500 mg", "Comprimé", "Antalgique / antipyrétique", DeliveryCondition.SansOrdonnance,
            "Médicament utilisé contre la douleur et la fièvre.");
        Add("Doliprane", "Paracétamol", "1000 mg", "Comprimé", "Antalgique / antipyrétique", DeliveryCondition.SansOrdonnance,
            "Médicament utilisé contre la douleur et la fièvre, dosage adulte.");
        Add("Efferalgan", "Paracétamol", "500 mg", "Comprimé effervescent", "Antalgique / antipyrétique", DeliveryCondition.SansOrdonnance,
            "Forme effervescente à dissoudre dans un verre d'eau.");
        Add("Paracétamol pédiatrique", "Paracétamol", "2,4 %", "Sirop", "Antalgique / antipyrétique", DeliveryCondition.SansOrdonnance,
            "Forme buvable destinée aux enfants. Demandez conseil au pharmacien.");
        Add("Amoxicilline", "Amoxicilline", "500 mg", "Gélule", "Antibiotique", DeliveryCondition.SurOrdonnance,
            "Antibiotique de la famille des pénicillines.");
        Add("Coartem", "Artéméther / Luméfantrine", "20/120 mg", "Comprimé", "Antipaludique", DeliveryCondition.AVerifier,
            "Traitement du paludisme. Un test de diagnostic est recommandé avant tout traitement.");
        Add("Ibuprofène", "Ibuprofène", "400 mg", "Comprimé", "Anti-inflammatoire", DeliveryCondition.AVerifier,
            "Anti-inflammatoire non stéroïdien. Contre-indications à vérifier avec un professionnel de santé.");
        Add("Spasfon", "Phloroglucinol", "80 mg", "Comprimé", "Antispasmodique", DeliveryCondition.SansOrdonnance,
            "Médicament utilisé contre les douleurs spasmodiques.");
        Add("Smecta", "Diosmectite", "3 g", "Poudre pour suspension buvable", "Gastro-entérologie", DeliveryCondition.SansOrdonnance,
            "Pansement digestif en sachets.");
        Add("Ventoline", "Salbutamol", "100 µg/dose", "Suspension pour inhalation", "Pneumologie", DeliveryCondition.SurOrdonnance,
            "Bronchodilatateur en aérosol doseur.");
        Add("Metformine", "Metformine", "500 mg", "Comprimé", "Antidiabétique", DeliveryCondition.SurOrdonnance,
            "Traitement du diabète de type 2, suivi médical nécessaire.");
        Add("Amlodipine", "Amlodipine", "5 mg", "Comprimé", "Cardiologie", DeliveryCondition.SurOrdonnance,
            "Traitement de l'hypertension artérielle, suivi médical nécessaire.");
        Add("SRO", "Sels de réhydratation orale", "20,5 g", "Poudre en sachet", "Réhydratation", DeliveryCondition.SansOrdonnance,
            "Sachets à dissoudre pour compenser les pertes en eau et en sels.");
        Add("Vitamine C", "Acide ascorbique", "500 mg", "Comprimé à croquer", "Vitamines", DeliveryCondition.SansOrdonnance,
            "Complément en vitamine C.");
    }

    private void SeedPharmacies()
    {
        var id = 1;
        void Add(string name, string district, string quarter, string landmark, double? lat, double? lng, string phone,
                 ValidationStatus status, int opens = 8, int closes = 21, bool h24 = false, bool sunday = false, bool duty = false,
                 string city = "Douala")
        {
            var current = id++;
            Pharmacies.Add(new Pharmacy
            {
                Id = current, Name = name, City = city, District = district, Neighborhood = quarter, Landmark = landmark,
                Latitude = lat, Longitude = lng, Phone = phone, Email = $"contact{current}@pharmacie-demo.cm",
                LicenseNumber = $"DEMO-PH-{2020 + current % 5}-{current:000}", Pharmacist = $"Dr Pharmacien démo {current}",
                OpensAt = TimeSpan.FromHours(opens), ClosesAt = TimeSpan.FromHours(closes),
                Is24h = h24, OpenOnSunday = sunday, IsOnDuty = duty, Status = status,
                RequestedAt = status == ValidationStatus.EnAttente
                    ? DateTime.Today.AddDays(-(current % 3) - 1)
                    : DateTime.Today.AddDays(-60 - current * 7),
                Documents = ["Autorisation d'exercice.pdf", "Registre de commerce.pdf", "Photo de la devanture.jpg"]
            });
        }

        Add("Pharmacie des Cocotiers", "Douala Ier", "Akwa", "Près du carrefour principal, face à la banque", 4.0512, 9.6978, "600000101", ValidationStatus.Validee, sunday: true);
        Add("Pharmacie Bonanjo Santé", "Douala Ier", "Bonanjo", "À côté de l'immeuble administratif", 4.0438, 9.6884, "600000102", ValidationStatus.Validee, 7, 20);
        Add("Pharmacie La Rosée", "Douala Ier", "Bonapriso", "Rue des écoles, derrière la boulangerie", 4.0329, 9.6992, "600000103", ValidationStatus.Validee, h24: true);
        Add("Pharmacie Étoile de Deïdo", "Douala Ier", "Deïdo", "Face au marché", 4.0645, 9.7071, "600000104", ValidationStatus.Validee, duty: true);
        Add("Pharmacie du Carrefour Ndokotti", "Douala IIIe", "Ndokotti", "Au rond-point, côté station-service", 4.0476, 9.7412, "600000105", ValidationStatus.Validee, 7, 22, sunday: true);
        Add("Pharmacie Les Lauriers", "Douala IIe", "New Bell", "Près de l'école publique", 4.0386, 9.7097, "600000106", ValidationStatus.Validee);
        Add("Pharmacie Makepe Plus", "Douala Ve", "Makepe", "Entrée du quartier, après le pont", 4.0768, 9.7546, "600000107", ValidationStatus.Validee);
        Add("Pharmacie Kotto Espérance", "Douala Ve", "Kotto", "Bloc C, à côté de la chapelle", 4.0703, 9.7731, "600000108", ValidationStatus.Validee, 8, 19);
        Add("Pharmacie Mboa Bépanda", "Douala Ve", "Bépanda", "Carrefour Omnisports", 4.0618, 9.7251, "600000109", ValidationStatus.Validee);
        Add("Pharmacie Rive Droite", "Douala IVe", "Bonabéri", "Après le pont, première à droite", 4.0779, 9.6589, "600000110", ValidationStatus.Validee, sunday: true);
        Add("Pharmacie du Rail", "Douala IIIe", "Logbaba", "Près de la voie ferrée", null, null, "600000111", ValidationStatus.Validee);
        Add("Pharmacie Bastos Centre", "Yaoundé Ier", "Bastos", "Près du rond-point", 3.8935, 11.5108, "600000112", ValidationStatus.Validee, city: "Yaoundé");
        Add("Pharmacie Logpom Nouvelle", "Douala Ve", "Logpom", "Face à l'entrée de la cité", 4.0872, 9.7583, "600000113", ValidationStatus.EnAttente);
        Add("Pharmacie Bonne Santé PK14", "Douala IIIe", "PK14", "Bord de route, côté marché", 4.0231, 9.7981, "600000114", ValidationStatus.EnAttente);
        Add("Pharmacie Ancienne Route", "Douala IIIe", "Ndokotti", "Ancienne route de Bassa", 4.0459, 9.7380, "600000115", ValidationStatus.Suspendue);

        Pharmacies.First(p => p.Id == 15).StatusReason = "Signalements répétés de stocks erronés (démo).";
    }

    private void SeedStocks()
    {
        int[] basePrices = [1000, 1500, 1600, 1800, 2500, 4500, 1200, 3200, 3800, 5500, 2000, 3000, 300, 1500];
        var rng = new Random(42);
        foreach (var pharmacy in Pharmacies.Where(p => p.Status != ValidationStatus.EnAttente))
        {
            foreach (var med in Medicines)
            {
                if (rng.NextDouble() > 0.72) continue;
                var hoursAgo = rng.Next(0, 10) switch { < 5 => rng.NextDouble() * 20, < 8 => 26 + rng.NextDouble() * 40, _ => 80 + rng.NextDouble() * 200 };
                var qty = rng.Next(0, 10) switch { 0 => 0, < 3 => rng.Next(1, 10), _ => rng.Next(12, 150) };
                var price = (int)(Math.Round(basePrices[med.Id - 1] * (0.9 + rng.NextDouble() * 0.25) / 25.0) * 25);
                Stocks.Add(new StockItem
                {
                    Id = NextStockId(), PharmacyId = pharmacy.Id, MedicineId = med.Id, Medicine = med,
                    Quantity = qty, PriceXaf = price, LowStockThreshold = 10,
                    LastUpdated = DateTime.Now.AddHours(-hoursAgo), DeclaredOutOfStock = qty == 0 && rng.NextDouble() > 0.5
                });
            }
        }

        // Garantit des cas de démonstration lisibles pour la pharmacie 1 (compte pharmacie démo).
        var demo = Stocks.Where(s => s.PharmacyId == 1).ToList();
        if (demo.Count > 2) { demo[0].Quantity = 4; demo[0].DeclaredOutOfStock = false; demo[0].LastUpdated = DateTime.Now.AddHours(-2); }
        if (demo.Count > 3) { demo[1].LastUpdated = DateTime.Now.AddDays(-5); }

        foreach (var m in Medicines) SearchCounts[m.Id] = rng.Next(20, 400);
        SearchCounts[1] = 640; SearchCounts[6] = 590; SearchCounts[2] = 455;
    }

    private void SeedUsers()
    {
        Users.AddRange(
        [
            new() { Id = 1, Name = "Visiteur Démo", Email = "visiteur@demo.cm", Role = UserRole.Visiteur, CreatedAt = DateTime.Today.AddDays(-40) },
            new() { Id = 2, Name = "Pharmacie des Cocotiers", Email = "pharmacie@demo.cm", Role = UserRole.Pharmacie, PharmacyId = 1, CreatedAt = DateTime.Today.AddDays(-120) },
            new() { Id = 3, Name = "Administrateur Démo", Email = "admin@demo.cm", Role = UserRole.Administrateur, CreatedAt = DateTime.Today.AddDays(-200) },
            new() { Id = 4, Name = "Pharmacie Logpom Nouvelle", Email = "attente@demo.cm", Role = UserRole.Pharmacie, PharmacyId = 13, CreatedAt = DateTime.Today.AddDays(-2) },
            new() { Id = 5, Name = "Utilisateur Exemple A", Email = "exemple.a@demo.cm", Role = UserRole.Visiteur, CreatedAt = DateTime.Today.AddDays(-12) },
            new() { Id = 6, Name = "Utilisateur Exemple B", Email = "exemple.b@demo.cm", Role = UserRole.Visiteur, Status = AccountStatus.Suspendu, CreatedAt = DateTime.Today.AddDays(-60) },
            new() { Id = 7, Name = "Pharmacie Bonanjo Santé", Email = "bonanjo@demo.cm", Role = UserRole.Pharmacie, PharmacyId = 2, CreatedAt = DateTime.Today.AddDays(-95) },
        ]);
    }

    private void SeedReports()
    {
        Reports.AddRange(
        [
            new() { Id = 1, PharmacyId = 1, PharmacyName = "Pharmacie des Cocotiers", MedicineName = "Coartem 20/120 mg", Reason = ReportReason.StockIncorrect,
                    Comment = "Indiqué disponible mais le produit était en rupture ce matin.", CreatedAt = DateTime.Now.AddHours(-5) },
            new() { Id = 2, PharmacyId = 4, PharmacyName = "Pharmacie Étoile de Deïdo", Reason = ReportReason.HorairesErrones,
                    Comment = "Fermée à 19 h alors que l'application indique 21 h.", CreatedAt = DateTime.Now.AddDays(-1), Status = ReportStatus.EnCours },
            new() { Id = 3, PharmacyId = 1, PharmacyName = "Pharmacie des Cocotiers", MedicineName = "Smecta 3 g", Reason = ReportReason.PrixIncorrect,
                    Comment = "Le prix au comptoir était plus élevé.", CreatedAt = DateTime.Now.AddDays(-3), Status = ReportStatus.Traite },
            new() { Id = 4, PharmacyId = 11, PharmacyName = "Pharmacie du Rail", Reason = ReportReason.CoordonneesErronees,
                    Comment = "La pharmacie n'apparaît pas sur la carte.", CreatedAt = DateTime.Now.AddDays(-2) },
        ]);
    }

    private void SeedAuditAndNotifications()
    {
        var now = DateTime.Now;
        Audit.AddRange(
        [
            new() { Id = 8, Actor = "admin@demo.cm", Action = "Connexion", Resource = "Administration", Timestamp = now.AddMinutes(-20) },
            new() { Id = 7, Actor = "pharmacie@demo.cm", Action = "Mise à jour du stock", Resource = "Doliprane 500 mg", Timestamp = now.AddHours(-2) },
            new() { Id = 6, Actor = "admin@demo.cm", Action = "Suspension", Resource = "Pharmacie Ancienne Route", Timestamp = now.AddDays(-1) },
            new() { Id = 5, Actor = "admin@demo.cm", Action = "Ajout au référentiel", Resource = "Vitamine C 500 mg", Timestamp = now.AddDays(-2) },
            new() { Id = 4, Actor = "attente@demo.cm", Action = "Demande de compte", Resource = "Pharmacie Logpom Nouvelle", Timestamp = now.AddDays(-2) },
            new() { Id = 3, Actor = "admin@demo.cm", Action = "Validation", Resource = "Pharmacie Rive Droite", Timestamp = now.AddDays(-6) },
            new() { Id = 2, Actor = "bonanjo@demo.cm", Action = "Modification des horaires", Resource = "Pharmacie Bonanjo Santé", Timestamp = now.AddDays(-8) },
            new() { Id = 1, Actor = "admin@demo.cm", Action = "Suspension de compte", Resource = "exemple.b@demo.cm", Timestamp = now.AddDays(-15) },
        ]);

        var myStocks = Stocks.Where(s => s.PharmacyId == 1).Take(4).ToList();
        if (myStocks.Count > 0)
            Notifications.Add(new AppNotification { Id = 1, Kind = NotificationKind.StockFaible, Title = "Stock faible",
                Message = $"{myStocks[0].Medicine.DisplayName} : {myStocks[0].Quantity} unités restantes.", Date = now.AddHours(-2), StockId = myStocks[0].Id });
        Notifications.Add(new AppNotification { Id = 2, Kind = NotificationKind.Signalement, Title = "Nouveau signalement",
            Message = "Un visiteur signale un stock incorrect pour Coartem 20/120 mg.", Date = now.AddHours(-5), ReportId = 1 });
        if (myStocks.Count > 1)
            Notifications.Add(new AppNotification { Id = 3, Kind = NotificationKind.Information, Title = "Stock à mettre à jour",
                Message = $"{myStocks[1].Medicine.DisplayName} n'a pas été mis à jour depuis 5 jours.", Date = now.AddDays(-1), StockId = myStocks[1].Id });
        Notifications.Add(new AppNotification { Id = 4, Kind = NotificationKind.Validation, Title = "Compte validé",
            Message = "Votre établissement est visible par les visiteurs.", Date = now.AddDays(-120), IsRead = true });

        foreach (var s in myStocks)
            StockChanges.Add(new StockChange { Date = s.LastUpdated, MedicineName = s.Medicine.DisplayName,
                Description = $"Quantité : {s.Quantity} · Prix : {s.PriceText}", Author = "pharmacie@demo.cm" });
        StockChanges.Add(new StockChange { Date = now.AddDays(-7), MedicineName = "Établissement",
            Description = "Horaires modifiés : 08:00 – 21:00, 7 j/7", Author = "pharmacie@demo.cm" });
    }
}
