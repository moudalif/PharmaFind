# PharmaFind — Application Mobile Fullstack (.NET MAUI & ASP.NET Core)

PharmaFind est une application fullstack **100 % C# / .NET 10** comprenant :
1. **Frontend Mobile (.NET MAUI)** : tous les écrans du cahier des charges en C# / XAML / MVVM, avec recherche géolocalisée, carte Leaflet, gestion de stock et panneau d'administration.
2. **Backend API (ASP.NET Core Web API & Entity Framework Core)** : dans le sous-dossier `PharmaFind.Api/`, avec base de données SQLite (`pharmafind.db`), authentification JWT, contrôleurs REST complets et chargement automatique des données (Seed).
3. **Bascule transparente** : l'application peut fonctionner au choix en **Mode Connecté (API réelle C#)** ou en **Mode Démo local** (sélectionnable dans le Profil).

Pour une description détaillée des objectifs, fonctionnalités, architecture et règles métier, consulter [Fonctionnement.md](Fonctionnement.md).

> **État de compilation vérifié :** Le frontend Android et le backend Web API compilent avec succès (0 avertissement, 0 erreur) sur le SDK .NET 10.

---

## 1. Lancer le projet

**Prérequis**
- Visual Studio 2022 17.14 ou plus récent (ou Visual Studio 2026), charge de travail **Développement d'interface utilisateur d'application multiplateforme .NET**.
- SDK **.NET 10**, avec la charge de travail MAUI : `dotnet workload install maui`.
- Un émulateur Android (API 23 ou plus) ou un téléphone Android en mode développeur.

**Étapes**
1. Ouvrir `PharmaFind.csproj` dans Visual Studio (la solution `.sln` est créée automatiquement à l'enregistrement).
2. Laisser la restauration NuGet se faire (`Microsoft.Maui.Controls`, `CommunityToolkit.Mvvm 8.4.0`).
3. Choisir la cible **Android Emulator** (ou **Windows Machine** pour un test rapide sur PC), puis lancer avec F5.

**Lancer le backend API (C# ASP.NET Core) :**
```bash
cd PharmaFind.Api
dotnet run --urls "http://0.0.0.0:5133"
```
*La base SQLite locale `pharmafind.db` et toutes les données de départ (Seed) sont initialisées automatiquement au démarrage.*

**Lancer l'application mobile MAUI :**
```bash
dotnet build -t:Run -f net10.0-android
```

**Utiliser .NET 9 à la place de .NET 10**
- Dans `PharmaFind.csproj`, remplacer `net10.0` par `net9.0` (3 occurrences) et `Microsoft.Extensions.Logging.Debug` 10.0.0 par 9.0.0.
- Dans `Services/AppServices.cs` (classe `DialogService`, seul fichier concerné), remplacer `DisplayAlertAsync` par `DisplayAlert` et `DisplayActionSheetAsync` par `DisplayActionSheet`.

---

## 2. Comptes et mode démonstration

| Espace | Courriel | Mot de passe |
|---|---|---|
| Visiteur (compte facultatif) | `visiteur@demo.cm` | `Demo1234` |
| Pharmacie validée | `pharmacie@demo.cm` | `Demo1234` |
| Pharmacie en attente de validation | `attente@demo.cm` | `Demo1234` |
| Administrateur | `admin@demo.cm` | `Demo1234` |

Pour ouvrir les espaces pharmacie et administration : **Profil → Professionnels**.

**Profil → Mode démonstration** sert à la soutenance. On peut y :
- remplacer la position GPS par Akwa (activé par défaut, car l'émulateur est placé aux États-Unis) ;
- simuler l'absence de connexion, une panne serveur ou un réseau lent, pour montrer les états d'erreur de chaque écran.

Toutes les données (pharmacies, numéros, prix, comptes) sont **fictives**. Seuls les quartiers et arrondissements de Douala sont réels. Les données sont réinitialisées à chaque lancement.

---

## 3. Écrans réalisés (34)

**Visiteur** (sans inscription obligatoire), onglets *Accueil · Carte · Favoris · Profil*
- Lancement, accueil (recherches récentes, médicaments populaires, rappel de fiabilité).
- Recherche : nom commercial ou DCI, suggestions temporisées, filtre par forme, proposition « Vouliez-vous dire… ? ».
- Résultats : rayon 1/3/5/10/20 km, filtres *disponible* et *ouverte maintenant*, tri par distance ou par prix, avertissement sur les données anciennes, bouton « Élargir le rayon ».
- Carte interactive : position du visiteur, cercle de recherche, marqueurs colorés selon la disponibilité, fiche synthétique au toucher d'un marqueur.
- Fiche pharmacie : appel, itinéraire, horaires, repère local, stock déclaré, favori, signalement.
- Fiche médicament : DCI, dosage, forme, conditions de délivrance, autres présentations de la même DCI. Aucun conseil médical.
- Favoris, historique (avec consentement), profil, connexion, inscription, signalement, choix du lieu (GPS ou quartier), confidentialité.

**Pharmacie partenaire**, onglets *Tableau · Stocks · Catalogue · Alertes · Établissement*
- Demande de compte avec justificatifs.
- Tableau de bord (écran d'attente tant que le compte n'est pas validé).
- Inventaire filtrable et édition d'une ligne : quantité, prix en FCFA, seuil d'alerte, rupture.
- Catalogue central, alertes et signalements, informations de l'établissement (GPS, horaires, garde), historique des modifications.

**Administrateur**, onglets *Tableau · Pharmacies · Médicaments · Signalements · Plus*
- Tableau de bord, liste des pharmacies par statut.
- Examen d'une pharmacie : valider, refuser, suspendre ou réactiver, avec motif.
- Référentiel des médicaments et édition.
- Utilisateurs, signalements, statistiques, journal d'audit, paramètres de fiabilité.

**États gérés sur chaque écran** : chargement, liste vide, erreur serveur, absence de connexion, géolocalisation refusée ou désactivée, coordonnées GPS manquantes, noms similaires, stocks anciens, pharmacie fermée, carte indisponible.

---

## 4. Architecture

```
Présentation XAML (Views) → ViewModels (MVVM) → Interfaces de services → [Mock aujourd'hui | API REST demain]
```

```
PharmaFind/
├── App.xaml, AppShell.xaml        Shell : 3 TabBar (visiteur, pharmacie, admin) + routes
├── MauiProgram.cs                 point d'entrée
├── ServiceRegistration.cs         injection de dépendances : ⇦ le seul endroit à modifier pour brancher l'API
├── Routes.cs                      noms des routes et des paramètres de navigation
├── Models/                        Medicine, Pharmacy, StockItem, PharmacyReport, UserAccount…
├── Helpers/                       règles métier (StockRules), FCFA, +237, distances, palette des statuts
├── Services/
│   ├── Interfaces.cs              contrats IMedicineService, IPharmacyService, IStockService…
│   ├── AppServices.cs             navigation, dialogues, réseau, contexte de recherche
│   ├── DeviceLocationService.cs   géolocalisation réelle (permission demandée au bon moment)
│   ├── ContactActions.cs          appel et itinéraire (applications du téléphone)
│   ├── Maps/LeafletMapProvider.cs carte Leaflet + OpenStreetMap, derrière IMapProvider
│   └── Mock/                      données et services simulés (latence, erreurs)
├── ViewModels/{Visitor,Pharmacy,Admin}/
├── Views/{Visitor,Pharmacy,Admin}/
├── Controls/                      Badge (statuts), StateView (états vides / erreur / hors ligne)
├── Converters/
└── Resources/                     charte (Colors.xaml, Styles.xaml), 52 icônes SVG, icône, splash
```

**Choix techniques**
- **CommunityToolkit.Mvvm** : `[ObservableProperty]` et `[RelayCommand]` réduisent le code répétitif. Chaque écran hérite de `BaseViewModel`, qui gère les états et les erreurs de la même façon partout.
- **Pages résolues par l'injection de dépendances.** Les paramètres de navigation passent par `IQueryAttributable`, donc aucune page ne crée elle-même ses services.
- **Carte en WebView (Leaflet + OpenStreetMap)** : gratuite, sans clé API, identique sur Android, iOS et Windows. Le fournisseur est isolé derrière `IMapProvider`, ce qui permet de passer à `Microsoft.Maui.Controls.Maps` (clé Google Maps requise sur Android) ou à Mapsui sans toucher aux ViewModels.
- **Règles de fiabilité centralisées** dans `Helpers/StockRules.cs` : disponible, stock faible, indisponible, non confirmée. Les seuils (24 h, 72 h, 7 jours) sont modifiables par l'administrateur.
- **Contexte camerounais** : prix en FCFA sans décimales, numéros au format `+237 6 99 12 34 56`, adresses par ville, arrondissement, quartier et repère. Les coordonnées GPS sont validées dans les limites du Cameroun.

---

## 5. Brancher le backend plus tard

1. Créer, par exemple, `Services/Api/ApiPharmacyService.cs`, qui implémente `IPharmacyService` avec `HttpClient`.
2. Dans `ServiceRegistration.cs`, remplacer :
   ```csharp
   services.AddSingleton<IPharmacyService, MockPharmacyService>();
   ```
   par (paquet NuGet `Microsoft.Extensions.Http`) :
   ```csharp
   services.AddHttpClient<IPharmacyService, ApiPharmacyService>(c => c.BaseAddress = new Uri("https://…"));
   ```
3. Les services doivent lever `ServiceUnavailableException` en cas de panne ou de délai dépassé, pour que les écrans affichent l'état d'erreur prévu.
4. Supprimer la section *Mode démonstration* du profil et la classe `DemoSettings`.

Les contrôles de sécurité (rôles, propriété des données, hachage des mots de passe) **doivent être faits par l'API**. Les vérifications de l'interface ne servent qu'au confort de l'utilisateur.

---

## 6. Limites connues et hypothèses

- **Non compilé et non testé** dans cet environnement (voir l'avertissement en haut).
- La carte charge Leaflet depuis `unpkg.com` et les tuiles depuis OpenStreetMap : elle **nécessite internet**, et ces tuiles ne conviennent pas à une mise en production (politique d'usage OSM). Si la carte ne répond pas, l'écran « Carte indisponible » renvoie vers la liste.
- Les textes sont écrits directement en français dans le XAML. Pour la version anglaise, il faudra les déplacer dans des fichiers `.resx`.
- Le mode sombre n'est pas pris en charge : le thème clair est imposé.
- Les justificatifs : seul le nom du fichier est conservé. La réinitialisation du mot de passe et l'aperçu des documents sont annoncés, mais pas réalisés.
- Les statistiques de l'administration sont des valeurs fictives.
- Aucun test unitaire n'est fourni avec la maquette. Les règles de `StockRules`, `GeoHelper`, `PhoneFormatter` et `TextSearch` sont isolées et faciles à tester.
- Les conditions de délivrance des médicaments de démonstration sont indicatives et doivent être vérifiées selon la réglementation camerounaise.
