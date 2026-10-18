using System.Globalization;
using System.Text;
using System.Text.Json;

namespace PharmaFind.Services.Maps;

/// <summary>
/// Carte Leaflet + OpenStreetMap affichée dans une WebView : gratuite, sans clé API, identique sur Android, iOS et Windows.
/// Les interactions remontent à l'application par des URL « pharmafind:// » interceptées par la page.
/// En production, utiliser un fournisseur de tuiles commercial ou auto-hébergé (politique d'usage OSM),
/// ou remplacer ce fournisseur par Microsoft.Maui.Controls.Maps ou Mapsui via <see cref="IMapProvider"/>.
/// </summary>
public class LeafletMapProvider : IMapProvider
{
    public const string Scheme = "pharmafind";

    public string Name => "Leaflet / OpenStreetMap";

    public string BuildHtml(MapModel model)
    {
        var inv = CultureInfo.InvariantCulture;
        var markers = JsonSerializer.Serialize(model.Markers.Select(m => new
        {
            id = m.Id, lat = m.Latitude, lng = m.Longitude, title = m.Title, sub = m.Subtitle, color = m.ColorHex
        }));

        var sb = new StringBuilder();
        sb.Append("""
<!DOCTYPE html>
<html lang="fr">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1" />
<link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
<style>
  html, body, #map { height: 100%; margin: 0; padding: 0; background: #EEF1F4; }
  .leaflet-tooltip { font: 13px/1.3 sans-serif; border-radius: 8px; }
  .pf-user { width: 16px; height: 16px; border-radius: 50%; background: #2F74B5; border: 3px solid #fff; box-shadow: 0 0 0 6px rgba(47,116,181,.25); }
</style>
</head>
<body>
<div id="map"></div>
<script>function notify(path) { window.location.href = 'pharmafind://' + path; }</script>
<script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
<script>
if (typeof L === 'undefined') { notify('error/load'); }
else {
""");
        sb.Append(CultureInfo.InvariantCulture, $"  var center = [{model.CenterLatitude.ToString(inv)}, {model.CenterLongitude.ToString(inv)}];\n");
        sb.Append(CultureInfo.InvariantCulture, $"  var radiusKm = {model.RadiusKm};\n");
        sb.Append(CultureInfo.InvariantCulture, $"  var showUser = {(model.ShowUser ? "true" : "false")};\n");
        sb.Append("  var markers = ").Append(markers).Append(";\n");
        sb.Append("""
  var map = L.map('map', { zoomControl: false }).setView(center, 14);
  L.control.zoom({ position: 'topright' }).addTo(map);
  var tileErrors = 0;
  var tiles = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19, attribution: '&copy; OpenStreetMap'
  }).addTo(map);
  tiles.on('tileerror', function () { tileErrors++; if (tileErrors === 8) notify('error/tiles'); });

  var bounds = [];
  if (showUser) {
    L.marker(center, { icon: L.divIcon({ className: '', html: '<div class="pf-user"></div>', iconSize: [22, 22] }) }).addTo(map);
    L.circle(center, { radius: radiusKm * 1000, color: '#2F74B5', weight: 1, fillOpacity: 0.05 }).addTo(map);
    bounds.push(center);
  }
  markers.forEach(function (m) {
    var mk = L.circleMarker([m.lat, m.lng], { radius: 11, color: '#ffffff', weight: 3, fillColor: m.color, fillOpacity: 1 }).addTo(map);
    mk.bindTooltip(m.title, { direction: 'top', offset: [0, -10] });
    mk.on('click', function (e) { L.DomEvent.stopPropagation(e); notify('pharmacy/' + m.id); });
    bounds.push([m.lat, m.lng]);
  });
  if (bounds.length > 1) map.fitBounds(bounds, { padding: [40, 40], maxZoom: 15 });
  map.on('click', function () { notify('map/tap'); });
}
</script>
</body>
</html>
""");
        return sb.ToString();
    }
}
