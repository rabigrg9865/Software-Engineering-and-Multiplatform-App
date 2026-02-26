using LocationTrackerApp.Data;
using LocationTrackerApp.Services;
using LocationTrackerApp.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Hosting;

namespace LocationTrackerApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps();

        builder.Services.AddSingleton<LocationRepository>();
        builder.Services.AddSingleton<LocationTrackingService>();
        builder.Services.AddSingleton<HeatmapService>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

        return builder.Build();
    }
}
