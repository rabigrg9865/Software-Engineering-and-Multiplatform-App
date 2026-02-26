using System.Text.Json;
using LocationTrackerApp.ViewModels;
using Microsoft.Maui.Maps;

namespace LocationTrackerApp;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        RouteMapWeb.Source = new HtmlWebViewSource
        {
            Html = BuildMapHtml()
        };

        _viewModel.RoutePointsChanged += async (_, routePoints) =>
        {
            var jsPoints = routePoints
                .Select(p => new { lat = p.Latitude, lng = p.Longitude })
                .ToList();
            var payload = JsonSerializer.Serialize(jsPoints);
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await RouteMapWeb.EvaluateJavaScriptAsync($"renderRoute({payload});");
            });
        };

        _viewModel.CameraRequested += async (_, location) =>
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await RouteMapWeb.EvaluateJavaScriptAsync($"focusPoint({location.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},{location.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)});");
            });
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    private static string BuildMapHtml()
    {
        return """
<!doctype html>
<html>
<head>
  <meta charset="utf-8"/>
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"/>
  <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
  <style>
    html, body, #map { height: 100%; width: 100%; margin: 0; }
  </style>
</head>
<body>
  <div id="map"></div>
  <script>
    const map = L.map('map').setView([37.3348, -122.0090], 13);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);

    let markersLayer = L.layerGroup().addTo(map);
    let routeLine = null;

    function renderRoute(points) {
      markersLayer.clearLayers();
      if (routeLine) {
        map.removeLayer(routeLine);
        routeLine = null;
      }
      if (!points || points.length === 0) return;

      const latlngs = points.map(p => [p.lat, p.lng]);
      for (let i = 0; i < latlngs.length; i++) {
        L.circleMarker(latlngs[i], {
          radius: 6,
          color: '#0d6efd',
          fillColor: '#0d6efd',
          fillOpacity: 0.9
        }).bindTooltip(`P${i + 1}`).addTo(markersLayer);
      }

      routeLine = L.polyline(latlngs, {
        color: '#ff3b30',
        weight: 4,
        opacity: 0.8
      }).addTo(map);

      map.fitBounds(routeLine.getBounds(), { padding: [20, 20] });
    }

    function focusPoint(lat, lng) {
      map.setView([lat, lng], 15);
    }
  </script>
</body>
</html>
""";
    }
}
