using LocationTrackerApp.Models;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace LocationTrackerApp.Services;

public sealed class HeatmapService
{
    private const double CellSizeDegrees = 0.0015;

    public IReadOnlyList<Circle> BuildHeatmap(IEnumerable<LocationPoint> points)
    {
        var buckets = points
            .GroupBy(p => new
            {
                LatBucket = Math.Round(p.Latitude / CellSizeDegrees),
                LonBucket = Math.Round(p.Longitude / CellSizeDegrees)
            })
            .Select(g => new
            {
                Latitude = g.Average(x => x.Latitude),
                Longitude = g.Average(x => x.Longitude),
                Count = g.Count()
            })
            .ToList();

        if (buckets.Count == 0)
        {
            return Array.Empty<Circle>();
        }

        var maxCount = buckets.Max(b => b.Count);

        return buckets.Select(bucket =>
        {
            var normalized = (double)bucket.Count / maxCount;
            var fillColor = Color.FromRgba(
                red: (byte)(255 * normalized),
                green: (byte)(80 * (1 - normalized)),
                blue: (byte)(35 * (1 - normalized)),
                alpha: (byte)(70 + (130 * normalized)));

            return new Circle
            {
                Center = new Location(bucket.Latitude, bucket.Longitude),
                Radius = Distance.FromMeters(40 + (normalized * 180)),
                FillColor = fillColor,
                StrokeColor = Colors.Transparent,
                StrokeWidth = 0
            };
        }).ToList();
    }
}
