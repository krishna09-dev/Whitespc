using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using whitespc.Data;
using whitespc.Services;

namespace whitespc;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // SQLite
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "whitespc.db");
        builder.Services.AddDbContext<JournalDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}")
        );

        // Core services
        builder.Services.AddScoped<JournalService>();
        builder.Services.AddScoped<TagService>();
        builder.Services.AddScoped<AnalyticsService>();
        builder.Services.AddScoped<SecurityService>();
        builder.Services.AddScoped<ThemeService>();
        builder.Services.AddScoped<MotivationService>();

        // UI services (platform-specific implementations)
#if MACCATALYST
        builder.Services.AddScoped<IFileSaverService, whitespc.Platforms.MacCatalyst.FileSaverService>();
        builder.Services.AddScoped<IPdfExportService, whitespc.Platforms.MacCatalyst.PdfExportService>();
#elif IOS
        builder.Services.AddScoped<IFileSaverService, whitespc.Platforms.iOS.FileSaverService>();
        builder.Services.AddScoped<IPdfExportService, whitespc.Platforms.iOS.PdfExportService>();
#else
        // Optional: fallback if you ever build other platforms
        // builder.Services.AddScoped<IFileSaverService, DummyFileSaverService>();
        // builder.Services.AddScoped<IPdfExportService, DummyPdfExportService>();
#endif

        var app = builder.Build();

        // DB init
        try
        {
            Directory.CreateDirectory(FileSystem.AppDataDirectory);

            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<JournalDbContext>();

#if DEBUG
            try
            {
                _ = context.UserSettings.FirstOrDefault();
            }
            catch (Microsoft.Data.Sqlite.SqliteException)
            {
                context.Database.EnsureDeleted();
            }
#endif

            context.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            var logPath = Path.Combine(FileSystem.AppDataDirectory, "startup_error.txt");
            File.WriteAllText(logPath, ex.ToString());
            throw;
        }

        return app;
    }
}