using LocationTrackerApp.Models;
using SQLite;
namespace LocationTrackerApp.Data;
public sealed class LocationRepository
{
    private readonly SQLiteAsyncConnection _connection;
    public LocationRepository()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "locationtracker.db3");
        _connection = new SQLiteAsyncConnection(dbPath);
    }
    public async Task InitializeAsync()
    {
        await _connection.CreateTableAsync<LocationPoint>();
    }
    public Task<int> InsertAsync(LocationPoint point)
    {
        return _connection.InsertAsync(point);
    }
    public Task<List<LocationPoint>> GetAllAsync()
    {
        return _connection.Table<LocationPoint>()
            .OrderBy(p => p.CapturedAtUtc)
            .ToListAsync();
    }
    public Task<int> ClearAllAsync()
    {
        return _connection.DeleteAllAsync<LocationPoint>();
    }
}
