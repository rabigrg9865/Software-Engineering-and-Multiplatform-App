using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocationTrackerApp.Data;
using LocationTrackerApp.Models;
using LocationTrackerApp.Services;
using Microsoft.Maui.Controls.Maps;
namespace LocationTrackerApp.ViewModels;
public partial class MainViewModel : ObservableObject
{
    private readonly LocationRepository _repository;
    private readonly LocationTrackingService _trackingService;
    private readonly HeatmapService _heatmapService;
    [ObservableProperty]
    private string _statusText = "Ready";
    [ObservableProperty]
    private int _pointCount;
    [ObservableProperty]
    private bool _isTracking;
    [ObservableProperty]
    private int _captureIntervalSeconds = 15;
    [ObservableProperty]
    private double _totalDistanceKm;
    public event EventHandler<IReadOnlyList<Circle>>? HeatmapChanged;
    public event EventHandler<IReadOnlyList<Location>>? RoutePointsChanged;
    public event EventHandler<Location>? CameraRequested;
    public MainViewModel(
        LocationRepository repository,
        LocationTrackingService trackingService,
        HeatmapService heatmapService)
    {
        _repository = repository;
        _trackingService = trackingService;
        _heatmapService = heatmapService;
        _trackingService.LocationCaptured += OnLocationCaptured;
    }
    public async Task InitializeAsync()
    {
        await _trackingService.InitializeAsync();
        await RefreshHeatmapAsync();
    }
    [RelayCommand]
    private async Task StartAsync()
    {
        try
        {
            var hasPermission = await _trackingService.RequestPermissionsAsync();
            if (!hasPermission)
            {
                StatusText = "Location permission denied.";
                return;
            }
            var interval = TimeSpan.FromSeconds(Math.Clamp(CaptureIntervalSeconds, 5, 120));
            _trackingService.StartTracking(interval);
            IsTracking = true;
            StatusText = "Tracking started.";
        }
        catch (Exception ex)
        {
            StatusText = $"Start failed: {ex.Message}";
        }
    }
    [RelayCommand]
    private Task StopAsync()
    {
        _trackingService.StopTracking();
        IsTracking = false;
        StatusText = "Tracking stopped.";
        return Task.CompletedTask;
    }
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await RefreshHeatmapAsync();
    }
    [RelayCommand]
    private async Task ClearAsync()
    {
        await _repository.ClearAllAsync();
        await RefreshHeatmapAsync();
        StatusText = "All points cleared.";
    }

    [RelayCommand]
    private async Task LoadDemoRouteAsync()
    {
        _trackingService.StopTracking();
        IsTracking = false;

        await _repository.ClearAllAsync();

        var now = DateTimeOffset.UtcNow;
        var route = BuildDemoRoute(now);
        foreach (var point in route)
        {
            await _repository.InsertAsync(point);
        }

        await RefreshHeatmapAsync();
        StatusText = "Loaded demo route points.";
    }
    private async void OnLocationCaptured(object? sender, LocationPoint point)
    {
        await RefreshHeatmapAsync();
        CameraRequested?.Invoke(this, new Location(point.Latitude, point.Longitude));
    }
    private async Task RefreshHeatmapAsync()
    {
        var points = await _repository.GetAllAsync();
        PointCount = points.Count;
        HeatmapChanged?.Invoke(this, _heatmapService.BuildHeatmap(points));
        RoutePointsChanged?.Invoke(this, points.Select(p => new Location(p.Latitude, p.Longitude)).ToList());
        TotalDistanceKm = CalculateDistanceKm(points);
        if (points.Count == 0)
        {
            StatusText = "No saved locations yet.";
        }
        else
        {
            var latest = points[^1];
            StatusText = $"Last sample: {latest.CapturedAtUtc:yyyy-MM-dd HH:mm:ss} UTC";
            CameraRequested?.Invoke(this, new Location(latest.Latitude, latest.Longitude));
        }
    }

    private static List<LocationPoint> BuildDemoRoute(DateTimeOffset start)
    {
        // Demo route around Mountain View/Cupertino for screenshot-friendly output.
        var coords = new[]
        {
            (37.33480, -122.00900),
            (37.33495, -122.00770),
            (37.33510, -122.00630),
            (37.33525, -122.00490),
            (37.33535, -122.00340),
            (37.33545, -122.00190),
            (37.33555, -122.00050),
            (37.33565, -121.99910),
            (37.33575, -121.99780),
            (37.33585, -121.99650),
            (37.33595, -121.99520),
            (37.33605, -121.99380),
            (37.33615, -121.99240),
            (37.33625, -121.99100),
            (37.33635, -121.98960),
            (37.33645, -121.98820)
        };

        var points = new List<LocationPoint>(coords.Length);
        for (var i = 0; i < coords.Length; i++)
        {
            points.Add(new LocationPoint
            {
                Latitude = coords[i].Item1,
                Longitude = coords[i].Item2,
                CapturedAtUtc = start.AddSeconds(i * 10)
            });
        }

        return points;
    }

    private static double CalculateDistanceKm(IReadOnlyList<LocationPoint> points)
    {
        if (points.Count < 2)
        {
            return 0;
        }

        double totalKm = 0;
        for (var i = 1; i < points.Count; i++)
        {
            totalKm += HaversineKm(
                points[i - 1].Latitude,
                points[i - 1].Longitude,
                points[i].Latitude,
                points[i].Longitude);
        }

        return Math.Round(totalKm, 2);
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;

        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);
        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180.0);
}
