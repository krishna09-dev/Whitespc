using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using whitespc.Data;
using whitespc.Services;

#if MACCATALYST
using whitespc.Platforms.MacCatalyst;
#elif IOS
using whitespc.Platforms.iOS;
#endif

namespace whitespc
{
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

            // ----------------------------
            // SQLite Database
            // ----------------------------
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "whitespc.db");
            builder.Services.AddDbContext<JournalDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // ----------------------------
            // Core Services
            // ----------------------------
            builder.Services.AddScoped<JournalService>();
            builder.Services.AddScoped<TagService>();
            builder.Services.AddScoped<AnalyticsService>();
            builder.Services.AddScoped<SecurityService>();
            builder.Services.AddScoped<ThemeService>();
            builder.Services.AddScoped<MotivationService>();
            builder.Services.AddSingleton<IFileSaverService, FileSaverService>();

            // ----------------------------
            // PDF Export (Platform-specific)
            // ----------------------------
#if MACCATALYST
            builder.Services.AddSingleton<IPdfExportService, PdfExportService>();
#elif IOS
            builder.Services.AddSingleton<IPdfExportService, PdfExportService>();
#endif

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // ----------------------------
            // Database Initialization
            // ----------------------------
            using (var scope = app.Services.CreateScope())
            {
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

            return app;
        }
    }
}