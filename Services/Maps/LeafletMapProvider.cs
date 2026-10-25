using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using PharmaFind.Models;

namespace PharmaFind.Services.Maps;

/// <summary>
/// Carte Leaflet + OpenStreetMap affichée dans une WebView.
/// 100% autonome : Leaflet CSS et JS sont embarqués dans l'application pour un fonctionnement
/// hors-ligne immédiat et sans latence, avec tuiles OSM quand internet est disponible.
/// </summary>
public class LeafletMapProvider : IMapProvider
{
    public const string HostPrefix = "https://local.pharmafind/";

    public string Name => "Leaflet / OpenStreetMap";

    private static readonly string EmbeddedCss;
    private static readonly string EmbeddedJs;

    static LeafletMapProvider()
    {
        try
        {
            var asm = typeof(LeafletMapProvider).Assembly;
            using var cssStream = asm.GetManifestResourceStream("PharmaFind.Embedded.leaflet.css");
            if (cssStream != null)
            {
                using var reader = new StreamReader(cssStream);
                EmbeddedCss = reader.ReadToEnd();
            }
            else
            {
                EmbeddedCss = string.Empty;
            }

            using var jsStream = asm.GetManifestResourceStream("PharmaFind.Embedded.leaflet.js");
            if (jsStream != null)
            {
                using var reader = new StreamReader(jsStream);
                EmbeddedJs = reader.ReadToEnd();
            }
            else
            {
                EmbeddedJs = string.Empty;
            }
        }
        catch
        {
            EmbeddedCss = string.Empty;
            EmbeddedJs = string.Empty;
        }
    }

    public string BuildHtml(MapModel model)
    {
        var inv = CultureInfo.InvariantCulture;
        var markers = JsonSerializer.Serialize(model.Markers.Select(m => new
        {
            id = m.Id,
            lat = m.Latitude,
            lng = m.Longitude,
            title = m.Title,
            sub = m.Subtitle,
            color = m.ColorHex
        }));

        var sb = new StringBuilder();
        sb.Append("""
<!DOCTYPE html>
<html lang="fr">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
""");

        if (!string.IsNullOrEmpty(EmbeddedCss))
        {
            sb.Append("<style>\n").Append(EmbeddedCss).Append("\n</style>\n");
        }
        else
        {
            sb.Append("<link rel=\"stylesheet\" href=\"https://unpkg.com/leaflet@1.9.4/dist/leaflet.css\" />\n");
        }

        sb.Append("""
<style>
  * { box-sizing: border-box; -webkit-tap-highlight-color: transparent; }
  html, body { height: 100%; margin: 0; padding: 0; background: #E2E8F0; font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif; overflow: hidden; }
  #map {
    height: 100%;
    width: 100%;
    background-color: #EBF1F5;
    background-image:
      radial-gradient(#CBD5E1 1.2px, transparent 1.2px),
      linear-gradient(to right, rgba(203, 213, 225, 0.35) 1px, transparent 1px),
      linear-gradient(to bottom, rgba(203, 213, 225, 0.35) 1px, transparent 1px);
    background-size: 24px 24px, 96px 96px, 96px 96px;
  }
  .leaflet-tooltip {
    font: 12px/1.3 sans-serif;
    border-radius: 8px;
    padding: 6px 10px;
    box-shadow: 0 4px 12px rgba(0,0,0,0.18);
    border: none;
    color: #1F2937;
  }
  .pf-user {
    width: 18px;
    height: 18px;
    border-radius: 50%;
    background: #2563EB;
    border: 3px solid #FFFFFF;
    box-shadow: 0 0 0 6px rgba(37, 99, 235, 0.3);
    animation: pf-pulse 2s infinite;
  }
  @keyframes pf-pulse {
    0% { box-shadow: 0 0 0 4px rgba(37, 99, 235, 0.4); }
    70% { box-shadow: 0 0 0 12px rgba(37, 99, 235, 0); }
    100% { box-shadow: 0 0 0 4px rgba(37, 99, 235, 0); }
  }
  .leaflet-container {
    background-color: #EBF1F5 !important;
  }
  .pf-pin {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #FFFFFF;
    font-weight: bold;
    font-size: 16px;
    border: 2.5px solid #FFFFFF;
    box-shadow: 0 4px 10px rgba(0,0,0,0.35);
    cursor: pointer;
    pointer-events: auto;
    touch-action: manipulation;
    transition: transform 0.15s ease;
  }
  .pf-pin:active {
    transform: scale(1.2);
  }
</style>
</head>
<body>
<div id="map"></div>
<script>
function notify(path) {
  try {
    window.location.href = 'https://local.pharmafind/' + path + '?_t=' + new Date().getTime();
  } catch(e) {}
}
</script>
""");

        if (!string.IsNullOrEmpty(EmbeddedJs))
        {
            sb.Append("<script>\n").Append(EmbeddedJs).Append("\n</script>\n");
        }
        else
        {
            sb.Append("<script src=\"https://unpkg.com/leaflet@1.9.4/dist/leaflet.js\"></script>\n");
        }

        sb.Append("""
<script>
var rendered = false;
function renderMap() {
  if (rendered) {
    if (window.mapInstance) window.mapInstance.invalidateSize(true);
    return;
  }
  if (typeof L === 'undefined') {
    setTimeout(renderMap, 150);
    return;
  }
  rendered = true;
""");
        sb.Append(CultureInfo.InvariantCulture, $"  var center = [{model.CenterLatitude.ToString(inv)}, {model.CenterLongitude.ToString(inv)}];\n");
        sb.Append(CultureInfo.InvariantCulture, $"  var radiusKm = {model.RadiusKm};\n");
        sb.Append(CultureInfo.InvariantCulture, $"  var showUser = {(model.ShowUser ? "true" : "false")};\n");
        sb.Append("  var markers = ").Append(markers).Append(";\n");
        sb.Append("""
  var map = L.map('map', {
    zoomControl: false,
    attributionControl: false
  }).setView(center, 13);
  window.mapInstance = map;

  L.control.zoom({ position: 'topright' }).addTo(map);

  // Tuiles OpenStreetMap avec tolérance hors-ligne totale
  try {
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      timeout: 3500
    }).addTo(map);
  } catch (err) {}

  var bounds = [];

  if (showUser) {
    L.marker(center, {
      icon: L.divIcon({ className: '', html: '<div class="pf-user"></div>', iconSize: [24, 24], iconAnchor: [12, 12] })
    }).addTo(map);

    L.circle(center, {
      radius: radiusKm * 1000,
      color: '#2563EB',
      fillColor: '#3B82F6',
      weight: 1.5,
      dashArray: '4, 6',
      fillOpacity: 0.08
    }).addTo(map);
    bounds.push(center);
  }

  markers.forEach(function (m) {
    var iconHtml = '<div class="pf-pin" onclick="notify(\'pharmacy/' + m.id + '\');" style="background:' + m.color + ';">' +
                   '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round">' +
                   '<line x1="12" y1="5" x2="12" y2="19"></line><line x1="5" y1="12" x2="19" y2="12"></line></svg>' +
                   '</div>';

    var icon = L.divIcon({
      className: '',
      html: iconHtml,
      iconSize: [32, 32],
      iconAnchor: [16, 16]
    });

    var marker = L.marker([m.lat, m.lng], { icon: icon }).addTo(map);

    marker.on('click', function (e) {
      if (e && e.originalEvent) {
        L.DomEvent.stopPropagation(e);
      }
      notify('pharmacy/' + m.id);
    });

    marker.bindTooltip('<strong>' + m.title + '</strong><br/><span style="color:#6B7280;">' + m.sub + '</span>', {
      direction: 'top',
      offset: [0, -14]
    });

    bounds.push([m.lat, m.lng]);
  });

  if (bounds.length > 1) {
    map.fitBounds(bounds, { padding: [40, 40], maxZoom: 15 });
  }

  map.on('click', function () {
    notify('map/tap');
  });

  setTimeout(function() { map.invalidateSize(true); }, 150);
  setTimeout(function() { map.invalidateSize(true); }, 500);
  window.addEventListener('resize', function() { map.invalidateSize(true); });
}

if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', renderMap);
} else {
  renderMap();
}
window.addEventListener('load', renderMap);
setTimeout(renderMap, 50);
</script>
</body>
</html>
""");
        return sb.ToString();
    }
}
