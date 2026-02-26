using SQLite;
namespace LocationTrackerApp.Models;
public sealed class LocationPoint
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTimeOffset CapturedAtUtc { get; set; }
}
