namespace PharmaFind;

/// <summary>Routes Shell. Les routes absolues (//…) changent d'espace ; les autres empilent une page.</summary>
public static class Routes
{
    // Espaces (onglets)
    public const string VisitorHome = "//visitor/home";
    public const string VisitorMap = "//visitor/map";
    public const string VisitorFavorites = "//visitor/favorites";
    public const string VisitorProfile = "//visitor/profile";
    public const string PharmacyDashboard = "//pharmacy/phdashboard";
    public const string PharmacyStock = "//pharmacy/phstock";
    public const string PharmacyCatalog = "//pharmacy/phcatalog";
    public const string PharmacyNotifications = "//pharmacy/phnotifications";
    public const string AdminDashboard = "//admin/addashboard";
    public const string AdminPharmacies = "//admin/adpharmacies";
    public const string AdminMedicines = "//admin/admedicines";
    public const string AdminReports = "//admin/adreports";

    // Pages empilées
    public const string Search = "search";
    public const string Results = "results";
    public const string ResultsMap = "resultsmap";
    public const string PharmacyDetail = "pharmacydetail";
    public const string MedicineDetail = "medicinedetail";
    public const string History = "history";
    public const string Login = "login";
    public const string Register = "register";
    public const string Report = "report";
    public const string LocationPicker = "locationpicker";
    public const string Privacy = "privacy";

    public const string AccountRequest = "accountrequest";
    public const string StockEdit = "stockedit";
    public const string PharmacyHistory = "pharmacyhistory";

    public const string AdminPharmacyReview = "adminpharmacyreview";
    public const string AdminMedicineEdit = "adminmedicineedit";
    public const string AdminUsers = "adminusers";
    public const string AdminStats = "adminstats";
    public const string AdminAudit = "adminaudit";
    public const string AdminSettings = "adminsettings";
}

/// <summary>Noms des paramètres de navigation (IQueryAttributable).</summary>
public static class NavParams
{
    public const string MedicineId = "MedicineId";
    public const string PharmacyId = "PharmacyId";
    public const string StockId = "StockId";
    public const string Query = "Query";
    public const string Space = "Space";
}
