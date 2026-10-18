namespace PharmaFind.Helpers;

/// <summary>
/// Distance géographique (à vol d'oiseau, formule de Haversine).
/// Elle se distingue de la distance routière et du temps de trajet, fournis par l'application de navigation.
/// </summary>
public static class GeoHelper
{
    private const double EarthRadiusKm = 6371.0;

    public static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return EarthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    public static string FormatDistance(double km)
    {
        if (km < 1) return $"{Math.Round(km * 1000 / 10) * 10:0} m";
        return km.ToString(km < 10 ? "0.0" : "0", AppCulture.Fr) + " km";
    }

    private static double ToRad(double deg) => deg * Math.PI / 180.0;
}
