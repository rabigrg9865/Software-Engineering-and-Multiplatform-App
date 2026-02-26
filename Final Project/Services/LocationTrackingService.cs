using LocationTrackerApp.Data;
using LocationTrackerApp.Models;
using Microsoft.Maui.Devices.Sensors;
namespace LocationTrackerApp.Services;
public sealed class LocationTrackingService
{
    private readonly LocationRepository _repository;
    private IDispatcherTimer? _timer;
    public bool IsTracking { get; private set; }
    public event EventHandler<LocationPoint>? LocationCaptured;
    public LocationTrackingService(LocationRepository repository)
    {
        _repository = repository;
    }
    public async Task InitializeAsync()
    {
        await _repository.InitializeAsync();
    }
    public async Task<bool> RequestPermissionsAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }
        return status == PermissionStatus.Granted;
    }
    public void StartTracking(TimeSpan interval)
    {
        if (IsTracking)
        {
            return;
        }
        _timer = Application.Current?.Dispatcher.CreateTimer();
        if (_timer is null)
        {
            throw new InvalidOperationException("Unable to create dispatcher timer.");
        }
        _timer.Interval = interval;
        _timer.Tick += OnTimerTick;
        _timer.Start();
        IsTracking = true;
    }
    public void StopTracking()
    {
        if (!IsTracking || _timer is null)
        {
            return;
        }
        _timer.Tick -= OnTimerTick;
        _timer.Stop();
        _timer = null;
        IsTracking = false;
    }
    private async void OnTimerTick(object? sender, EventArgs e)
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);
            if (location is null)
            {
                return;
            }

            var point = new LocationPoint
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                CapturedAtUtc = DateTimeOffset.UtcNow
            };

            await _repository.InsertAsync(point);
            LocationCaptured?.Invoke(this, point);
        }
        catch
        {
            // Swallow timer-callback exceptions to keep periodic tracking alive.
        }
    }
}
