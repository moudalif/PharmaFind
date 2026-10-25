# PharmaFind — fonctionnement et guide de reprise

## 1. Présentation

PharmaFind est une application mobile destinée à aider le public à trouver des médicaments déclarés dans les pharmacies proches, et à donner aux pharmacies partenaires des outils de gestion de stock. Un espace d’administration sert à gérer les pharmacies, le catalogue et les signalements.

Le projet présent est une **application cliente .NET MAUI avec des données de démonstration**. Il ne contient ni API serveur, ni base de données distante. Les parcours et les opérations sont simulés en mémoire afin de présenter l’expérience utilisateur et de préparer une future connexion à un backend.

L’application ne remplace pas un professionnel de santé : la disponibilité affichée correspond à une déclaration de stock, pas à une garantie de présence, et les écrans ne fournissent pas de diagnostic ni de conseil médical.

## 2. Objectifs

- Permettre au public de rechercher un médicament et de repérer les pharmacies qui déclarent en avoir.
- Présenter les résultats par distance, disponibilité, prix et horaires, sous forme de liste ou de carte.
- Permettre aux pharmacies de tenir à jour leur établissement et leur inventaire.
- Permettre à l’administration de valider les pharmacies, gérer le catalogue et traiter les signalements.
- Rendre explicites la fraîcheur et les limites des informations de stock.
- Fournir une base d’interface organisée pour remplacer progressivement les services simulés par une API.

Les données de démonstration sont principalement situées à Douala. Les pharmacies, coordonnées, numéros, prix, utilisateurs et historiques fournis par le jeu de données sont fictifs.

## 3. Fonctionnalités

### 3.1 Visiteur

L’espace visiteur est accessible sans compte obligatoire et comprend les onglets **Accueil**, **Carte**, **Favoris** et **Profil**.

- **Accueil et recherche** : affichage de médicaments populaires et de recherches récentes ; recherche par nom commercial ou dénomination commune internationale (DCI) ; suggestions différées pendant la saisie ; filtre par forme ; suggestion de correction lorsque la recherche ne trouve rien.
- **Résultats** : recherche par médicament et position ; rayons proposés de 1, 3, 5, 10 et 20 km ; tri par distance ou prix ; filtres de disponibilité et d’ouverture ; possibilité d’élargir le rayon ; avertissement sur la fraîcheur des stocks.
- **Carte** : affichage des pharmacies autour du lieu choisi, d’un rayon et de marqueurs colorés selon l’état du stock. La sélection d’un marqueur donne accès à un résumé de la pharmacie. Le rendu utilise une WebView et Leaflet.
- **Fiche pharmacie** : adresse, quartier, repère, téléphone, horaires, informations de garde et stocks déclarés ; actions pour appeler ou ouvrir un itinéraire ; ajout ou retrait des favoris ; signalement d’une information incorrecte.
- **Fiche médicament** : nom, DCI, dosage, forme, description, condition de délivrance indiquée et autres présentations associées à la même DCI. Ces informations sont de démonstration et ne constituent pas un avis médical.
- **Favoris et historique** : consultation des pharmacies favorites ; historique local de recherches activable et effaçable. Dans l’implémentation actuelle, cet historique est en mémoire et ne survit pas à la fermeture de l’application.
- **Profil et accès** : profil visiteur, connexion, inscription, choix de position par GPS ou lieu connu et accès à l’espace professionnel.
- **Confidentialité et signalements** : écran de confidentialité et formulaire de signalement d’un stock, prix, coordonnées, horaires ou état de fermeture incorrect.

### 3.2 Pharmacie partenaire

L’espace pharmacie comprend les onglets **Tableau**, **Stocks**, **Catalogue**, **Alertes** et **Établissement**.

- Demande de création de compte pharmacie avec les informations de l’établissement et les justificatifs attendus par l’interface.
- Tableau de bord adapté au statut du compte ; un compte en attente de validation ne dispose pas encore des mêmes fonctions qu’un compte validé.
- Consultation et filtrage de l’inventaire ; ajout ou modification d’une ligne de stock, quantité, prix en FCFA, seuil de stock faible et déclaration de rupture ; retrait d’une ligne.
- Consultation du catalogue central pour sélectionner des médicaments.
- Consultation des alertes et signalements liés à la pharmacie, ainsi que marquage des notifications comme lues.
- Modification des informations de l’établissement, dont les coordonnées, horaires et statut de garde.
- Consultation de l’historique des modifications de stock.

Dans le prototype, ces règles de rôle sont simulées par les services clients. Elles ne constituent pas une autorisation sécurisée.

### 3.3 Administration

L’espace administrateur comprend les onglets **Tableau**, **Pharmacies**, **Médicaments**, **Signalements** et **Plus**.

- Tableau de bord avec indicateurs de démonstration.
- Consultation des pharmacies par statut ; examen d’une demande, validation, refus, suspension ou réactivation, avec motif lorsque prévu.
- Gestion du catalogue de médicaments : consultation, création et modification des fiches.
- Consultation et changement de statut des signalements.
- Consultation des utilisateurs et modification de leur statut de compte.
- Consultation de statistiques agrégées, de graphiques de démonstration et du journal d’audit.
- Réglage des paramètres de fiabilité des stocks.

Les statistiques et les événements d’audit proviennent du jeu de données simulé ; ils ne décrivent pas une activité réelle.

### 3.4 États et gestion d’erreurs

Les ViewModels partagent les états d’interface `Content`, `Loading`, `Empty`, `Error` et `Offline`. Selon l’écran, l’application prévoit également les cas de permission GPS refusée, localisation désactivée ou indisponible, coordonnées manquantes, absence de résultat, données de stock anciennes et carte indisponible.

Le mode démonstration peut simuler l’absence de réseau, une panne de service et une connexion lente. Les opérations simulées présentent une latence avant de renvoyer les données ou une erreur.

## 4. Règles métier importantes

- Les statuts de disponibilité sont calculés dans `Helpers/StockRules.cs` à partir de la quantité, du seuil de stock faible, d’une déclaration de rupture et de la date de mise à jour.
- La fraîcheur utilise des seuils configurables : récente jusqu’à 24 h, à vérifier jusqu’à 72 h, puis ancienne. Au-delà de 7 jours, la disponibilité devient non confirmée. Les valeurs par défaut sont dans `StockRulesSettings`.
- Les rayons proposés sont 1, 3, 5, 10 et 20 km ; le rayon initial vaut 5 km.
- Le prix est affiché en FCFA. Les formats de devise, de dates, de téléphone et les règles géographiques se trouvent dans `Helpers/`.
- Une disponibilité est une déclaration de stock : l’interface doit continuer à afficher des réserves adaptées et ne jamais la présenter comme une garantie.
- Les conditions de délivrance et descriptions des médicaments de démonstration doivent être validées par une source réglementaire et pharmaceutique compétente avant toute utilisation réelle.

## 5. Technologies et architecture

### Technologies

- C# et .NET 10.
- .NET MAUI / `Microsoft.Maui.Controls` pour les interfaces et le déploiement multiplateforme.
- XAML pour les pages et styles.
- CommunityToolkit.Mvvm 8.4.0 pour les propriétés observables et commandes (`ObservableProperty`, `RelayCommand`).
- Injection de dépendances Microsoft.Extensions.DependencyInjection intégrée à MAUI.
- Shell MAUI pour les onglets, espaces et routes.
- WebView, Leaflet et tuiles OpenStreetMap pour la carte ; Leaflet est chargé depuis `unpkg.com`, donc la carte nécessite un accès Internet.

Le fichier `PharmaFind.csproj` déclare la cible Android sur Linux, Android et iOS hors Linux, et Windows lorsque le projet est ciblé depuis Windows. Les versions minimales déclarées sont Android 23, iOS 15 et Windows 10.0.17763.

### Flux d’une fonctionnalité

```text
Views (XAML / code-behind)
        ↓ liaison de données et commandes
ViewModels (CommunityToolkit.Mvvm)
        ↓ interfaces injectées
Services (contrats dans Services/Interfaces.cs)
        ↓ implémentations inscrites dans ServiceRegistration.cs
Services.Mock aujourd’hui — future API REST demain
```

- Les pages sont déclarées dans `AppShell.xaml` et inscrites avec leur ViewModel dans `ServiceRegistration.cs`.
- Les ViewModels héritent généralement de `BaseViewModel`, qui centralise les états de chargement et de panne et l’affichage des erreurs.
- La navigation passe par `INavigationService`, les dialogues par `IDialogService`, la connectivité par `INetworkService`.
- Les pages reçoivent leurs dépendances par injection ; elles ne devraient pas instancier directement leurs services.
- Les paramètres de navigation sont définis dans `Routes.cs` et transmis aux pages qui implémentent `IQueryAttributable`.
- La carte est isolée derrière `IMapProvider`.

### Interfaces de services

Les contrats existants dans `Services/Interfaces.cs` couvrent :

| Interface | Responsabilité |
|---|---|
| `IMedicineService` | Suggestions, catalogue, médicaments populaires, équivalents, détails et sauvegarde du référentiel |
| `IPharmacyService` | Recherche de pharmacies, détails, stocks consultables, liste par statut et demandes de compte |
| `IStockService` | Établissement et stocks de la pharmacie connectée, alertes, confirmation des stocks et historique |
| `IAuthService` | Connexion, inscription, déconnexion et utilisateur courant |
| `IFavoritesService` | Lecture, vérification et bascule des favoris |
| `IHistoryService` | Historique local des recherches et consentement d’activation |
| `IReportService` | Création et traitement des signalements |
| `IAdminService` | Indicateurs, statuts pharmacie/utilisateur, journal et séries statistiques |
| `ILocationService` | Position GPS et lieux connus |
| `IMapProvider` | Construction du contenu de carte |
| `INavigationService`, `IDialogService`, `INetworkService` | Services transverses de l’application |

## 6. Structure du dépôt

```text
App.xaml / App.xaml.cs             Ressources globales et création de la fenêtre
AppShell.xaml / AppShell.xaml.cs   Espaces, onglets et navigation
MauiProgram.cs                     Démarrage MAUI, composition des services
ServiceRegistration.cs             Enregistrement des services, pages et ViewModels
Routes.cs                          Routes et noms des paramètres de navigation
Models/                             Entités, énumérations et modèles d’affichage
Helpers/                            Règles de stock, géographie, formats et statuts
Services/Interfaces.cs              Contrats des services
Services/AppServices.cs              Navigation, dialogues, réseau et état transversal
Services/DeviceLocationService.cs     Géolocalisation et permissions
Services/ContactActions.cs            Appel et itinéraires
Services/Maps/                       Fournisseur de carte Leaflet
Services/Mock/                       Jeu de données et implémentations de démonstration
ViewModels/Visitor/                  Logique des parcours visiteur
ViewModels/Pharmacy/                 Logique des parcours pharmacie
ViewModels/Admin/                    Logique des parcours administrateur
Views/Visitor/                       Pages visiteur
Views/Pharmacy/                      Pages pharmacie
Views/Admin/                          Pages administration
Controls/ et Converters/              Contrôles et conversions réutilisés
Resources/                            Styles, couleurs, icônes et images
Platforms/                            Configurations et points d’entrée plateforme
```

## 7. Mode de démonstration

Les comptes de démonstration documentés dans le README sont :

| Rôle | Courriel | Mot de passe de démonstration |
|---|---|---|
| Visiteur | `visiteur@demo.cm` | `Demo1234` |
| Pharmacie validée | `pharmacie@demo.cm` | `Demo1234` |
| Pharmacie en attente | `attente@demo.cm` | `Demo1234` |
| Administrateur | `admin@demo.cm` | `Demo1234` |

Accès aux espaces professionnels : **Profil → Professionnels**. Le mode démo est accessible depuis le profil et permet de choisir la position simulée d’Akwa et d’activer des scénarios de panne ou de latence.

Ces identifiants et données ne sont pas des secrets de production. L’authentification de démonstration vérifie un mot de passe fixe ; les modifications du magasin simulé ne sont pas persistées et les données sont reconstruites au démarrage.

## 8. Compiler et valider

Prérequis : SDK .NET 10 avec les workloads MAUI nécessaires et dépendances NuGet restaurées. Depuis la racine du dépôt :

```bash
dotnet restore PharmaFind.csproj
dotnet build PharmaFind.csproj -f net10.0-android
```

Pour lancer sur un appareil ou émulateur Android configuré :

```bash
dotnet build -t:Run -f net10.0-android
```

Une compilation Android a été vérifiée dans l’environnement de préparation du présent document : réussite, 0 erreur et deux avertissements `CS0618` concernant l’usage de `ScaleTo` dans `Views/Visitor/LaunchPage.xaml.cs`. Cette vérification n’est pas un test d’exécution sur appareil et ne certifie pas les cibles iOS ou Windows.

Aucun projet de tests automatisés n’est actuellement fourni. Pour tout changement de règle métier, ajouter ou exécuter des tests ciblés, notamment sur `StockRules`, `GeoHelper` et les formats.

## 9. Limites connues et travaux de continuation

### Limites actuelles

- Aucun backend HTTP, base de données, synchronisation ni persistance des comptes et données.
- Authentification, rôles et autorisations uniquement simulés côté client ; aucune sécurité de production.
- L’historique, les favoris et les changements de données ne constituent pas une persistance durable.
- Statistiques, stocks, prix, pharmacie et coordonnées sont fictifs.
- La carte dépend d’Internet et du service de tuiles OpenStreetMap. Vérifier les politiques d’usage des fournisseurs avant une mise en production ; ne pas considérer l’usage gratuit comme une garantie de service.
- L’interface est en français et le thème clair est imposé.
- Les justificatifs ne sont pas téléversés : seule l’interface de demande est présente et le nom des documents peut être simulé.
- Certaines fonctions mentionnées par l’interface (par exemple la réinitialisation du mot de passe) ne sont pas implémentées comme un service réel.
- Les obligations de confidentialité, conservation des données et réglementation pharmaceutique doivent être étudiées avant un déploiement.

### Ordre conseillé pour poursuivre

1. Préciser le contrat du backend : endpoints, schémas, pagination, erreurs, authentification, rôles et règles de propriété des ressources.
2. Implémenter les services API derrière les interfaces existantes, enregistrer les implémentations dans `ServiceRegistration.cs` et gérer correctement les délais, annulations et erreurs.
3. Faire appliquer par le serveur les contrôles d’accès et les transitions d’état ; ne jamais faire confiance à un rôle fourni par l’application cliente.
4. Ajouter une persistance adaptée pour les données locales autorisées et définir les règles de synchronisation/confidentialité.
5. Ajouter des tests pour les règles métier et les ViewModels importants, puis des validations sur chaque plateforme ciblée.
6. Valider les textes, données et règles médicales avec les personnes compétentes ; vérifier les exigences de confidentialité et les politiques cartographiques.

Pour se familiariser avec le code, commencer par `PharmaFind.csproj`, `AppShell.xaml`, `Routes.cs`, `ServiceRegistration.cs`, `Services/Interfaces.cs`, puis suivre une fonctionnalité de bout en bout dans son ViewModel, sa page et son service simulé. Garder les ViewModels indépendants des implémentations concrètes et ne pas remplacer le mode démo sans préserver la possibilité de le sélectionner explicitement.

## 10. Références du dépôt

- `README.md` : lancement, comptes de démonstration et résumé des écrans.
- `PharmaFind_Interfaces_Mode_Simulation.pdf` : galerie des captures d’écran de démonstration.
- `PharmaFind_Interfaces_Mode_Simulation.docx` : document source associé à la galerie.
