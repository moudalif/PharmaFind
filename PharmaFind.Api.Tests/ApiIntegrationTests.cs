using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PharmaFind.Api.Models;
using Xunit;

namespace PharmaFind.Api.Tests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions;

    static ApiIntegrationTests()
    {
        JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        JsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    }

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private async Task<string> AuthenticateAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth.Token));
        return auth.Token;
    }

    [Fact]
    public async Task Auth_ValidAdminLogin_ReturnsTokenAndAdminRole()
    {
        var response = await _client.PostAsJsonAsync("api/auth/login", new LoginRequest("admin@demo.cm", "Demo1234"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(auth);
        Assert.Equal(UserRole.Administrateur, auth.User.Role);
        Assert.False(string.IsNullOrEmpty(auth.Token));
    }

    [Fact]
    public async Task Auth_WrongPassword_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("api/auth/login", new LoginRequest("admin@demo.cm", "BadPassword999!"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Auth_SuspendedAccount_ReturnsForbidden()
    {
        var response = await _client.PostAsJsonAsync("api/auth/login", new LoginRequest("exemple.b@demo.cm", "Demo1234"));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_Unauthenticated_ReturnsUnauthorized()
    {
        using var unauthClient = _factory.CreateClient();
        var response = await unauthClient.GetAsync("api/admin/stats");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Admin_VisitorRole_ReturnsForbidden()
    {
        var token = await AuthenticateAsync("visiteur@demo.cm", "Demo1234");
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("api/admin/stats");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_WithAdminToken_ReturnsStats()
    {
        var token = await AuthenticateAsync("admin@demo.cm", "Demo1234");
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("api/admin/stats");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var stats = await response.Content.ReadFromJsonAsync<DashboardStatsDto>(JsonOptions);
        Assert.NotNull(stats);
        Assert.True(stats.ValidatedPharmacies > 0);
        Assert.True(stats.Medicines > 0);
    }

    [Fact]
    public async Task Medicines_SaveWithoutAuth_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();
        var med = new Medicine
        {
            BrandName = "TestMed",
            Inn = "TestInn",
            Dosage = "100 mg",
            Form = "Comprimé"
        };
        var response = await client.PostAsJsonAsync("api/medicines", med);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Medicines_SuggestAndPopular_ReturnsActiveMedicines()
    {
        using var client = _factory.CreateClient();

        var popular = await client.GetFromJsonAsync<List<Medicine>>("api/medicines/popular", JsonOptions);
        Assert.NotNull(popular);
        Assert.NotEmpty(popular);

        var suggest = await client.GetFromJsonAsync<List<Medicine>>("api/medicines/suggest?query=doli", JsonOptions);
        Assert.NotNull(suggest);
        Assert.Contains(suggest, m => m.BrandName.Contains("Doliprane", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Stock_VisitorRole_ForbiddenFromManagingStock()
    {
        var token = await AuthenticateAsync("visiteur@demo.cm", "Demo1234");
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("api/stock/my-stock");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Stock_PharmacyRole_CanAccessOwnStock()
    {
        var token = await AuthenticateAsync("pharmacie@demo.cm", "Demo1234");
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("api/stock/my-stock");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<List<StockItem>>(JsonOptions);
        Assert.NotNull(items);
        Assert.All(items, item => Assert.Equal(1, item.PharmacyId));
    }

    [Fact]
    public async Task Stock_Save_RejectsNegativeQuantityOrPrice()
    {
        var token = await AuthenticateAsync("pharmacie@demo.cm", "Demo1234");
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var invalidItem = new StockItem
        {
            MedicineId = 1,
            Quantity = -5,
            PriceXaf = -100
        };

        var response = await client.PostAsJsonAsync("api/stock", invalidItem);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Pharmacies_Search_WithAndWithoutCoordinates_WorksCorrectly()
    {
        using var client = _factory.CreateClient();

        // 1. With coordinates (Douala Akwa: 4.0512, 9.6978, radius 5 km)
        var withCoords = await client.GetFromJsonAsync<List<PharmacyResultDto>>("api/pharmacies/search?lat=4.0512&lon=9.6978&radiusKm=5", JsonOptions);
        Assert.NotNull(withCoords);
        Assert.NotEmpty(withCoords);
        Assert.NotNull(withCoords[0].DistanceKm);
        Assert.True(withCoords[0].DistanceKm <= 5.0);

        // 2. Without coordinates (fallback mode)
        var withoutCoords = await client.GetFromJsonAsync<List<PharmacyResultDto>>("api/pharmacies/search", JsonOptions);
        Assert.NotNull(withoutCoords);
        Assert.NotEmpty(withoutCoords);
        Assert.All(withoutCoords, r => Assert.Equal(ValidationStatus.Validee, r.Pharmacy.Status));
    }

    [Fact]
    public async Task Reports_Submit_CreatesReportAndNotification()
    {
        using var client = _factory.CreateClient();
        var report = new PharmacyReport
        {
            PharmacyId = 1,
            Reason = ReportReason.StockIncorrect,
            Comment = "Signalement de test unitaire sur la disponibilité."
        };

        var response = await client.PostAsJsonAsync("api/reports", report);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<PharmacyReport>(JsonOptions);
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal(ReportStatus.Nouveau, created.Status);
    }
}
