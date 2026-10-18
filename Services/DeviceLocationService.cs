using PharmaFind.Models;

namespace PharmaFind.Services;

/// <summary>
/// Géolocalisation de l'appareil (API MAUI Essentials). La permission est demandée au moment de la recherche,
/// la position n'est ni conservée ni envoyée ailleurs que dans la requête de recherche.
/// </summary>
public class DeviceLocationService(DemoSettings demo) : ILocationService
{
    private static readonly KnownPlace DemoPlace = new() { Name = "Akwa", District = "Douala Ier", City = "Douala", Latitude = 4.0500, Longitude = 9.6960 };

    public async Task<LocationResult> GetCurrentLocationAsync(CancellationToken ct = default)
    {
        try
        {
            var status = await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var current = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                return current == PermissionStatus.Granted
                    ? current
                    : await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            });

            if (status != PermissionStatus.Granted)
                return new LocationResult(null, LocationError.PermissionDenied);

            if (demo.UseDemoPosition)
                return new LocationResult(new UserLocation
                {
                    Latitude = DemoPlace.Latitude,
                    Longitude = DemoPlace.Longitude,
                    Label = "Ma position (démo : Akwa)",
                    Source = LocationSource.Gps
                }, LocationError.None);

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request, ct)
                           ?? await Geolocation.Default.GetLastKnownLocationAsync();

            return location is null
                ? new LocationResult(null, LocationError.Unavailable)
                : new LocationResult(new UserLocation
                {
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    Label = "Ma position",
                    Source = LocationSource.Gps
                }, LocationError.None);
        }
        catch (FeatureNotEnabledException) { return new LocationResult(null, LocationError.Disabled); }
        catch (PermissionException) { return new LocationResult(null, LocationError.PermissionDenied); }
        catch (FeatureNotSupportedException) { return new LocationResult(null, LocationError.Unavailable); }
        catch (Exception) { return new LocationResult(null, LocationError.Unavailable); }
    }

    /// <summary>Lieux proposés pour la saisie manuelle (pilote Douala, puis autres villes).</summary>
    public IReadOnlyList<KnownPlace> GetKnownPlaces() =>
    [
        new() { Name = "Akwa", District = "Douala Ier", City = "Douala", Latitude = 4.0500, Longitude = 9.6960 },
        new() { Name = "Bonanjo", District = "Douala Ier", City = "Douala", Latitude = 4.0430, Longitude = 9.6890 },
        new() { Name = "Bonapriso", District = "Douala Ier", City = "Douala", Latitude = 4.0330, Longitude = 9.6980 },
        new() { Name = "Deïdo", District = "Douala Ier", City = "Douala", Latitude = 4.0640, Longitude = 9.7080 },
        new() { Name = "New Bell", District = "Douala IIe", City = "Douala", Latitude = 4.0380, Longitude = 9.7090 },
        new() { Name = "Ndokotti", District = "Douala IIIe", City = "Douala", Latitude = 4.0480, Longitude = 9.7400 },
        new() { Name = "Logbaba", District = "Douala IIIe", City = "Douala", Latitude = 4.0420, Longitude = 9.7700 },
        new() { Name = "Bonabéri", District = "Douala IVe", City = "Douala", Latitude = 4.0780, Longitude = 9.6580 },
        new() { Name = "Bépanda", District = "Douala Ve", City = "Douala", Latitude = 4.0620, Longitude = 9.7240 },
        new() { Name = "Makepe", District = "Douala Ve", City = "Douala", Latitude = 4.0760, Longitude = 9.7550 },
        new() { Name = "Kotto", District = "Douala Ve", City = "Douala", Latitude = 4.0700, Longitude = 9.7720 },
        new() { Name = "Logpom", District = "Douala Ve", City = "Douala", Latitude = 4.0870, Longitude = 9.7580 },
        new() { Name = "Bastos", District = "Yaoundé Ier", City = "Yaoundé", Latitude = 3.8930, Longitude = 11.5110 },
        new() { Name = "Centre-ville", District = "Yaoundé IIIe", City = "Yaoundé", Latitude = 3.8667, Longitude = 11.5167 },
        new() { Name = "Centre-ville", District = "Bafoussam Ier", City = "Bafoussam", Latitude = 5.4778, Longitude = 10.4176 },
        new() { Name = "Centre-ville", District = "Garoua Ier", City = "Garoua", Latitude = 9.3017, Longitude = 13.3921 },
    ];
}
