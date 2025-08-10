using LiteDB;
using Serilog;
using LDS.Models;
using LDS.Utilities;

namespace LDS.Services;

public interface IApplicationSettingsProvider
{
    /// <summary>
    /// Fetch application settings from the database.
    /// </summary>
    /// <returns></returns>
    ApplicationSettings GetSettings();
}

public class ApplicationSettingsProvider(ILiteDatabase database, IDebugLogger debugLogger) : IApplicationSettingsProvider
{
    private readonly IDebugLogger f_debugLogger = debugLogger;
    private readonly ILiteCollection<ApplicationSettings> f_collection = database.GetCollection<ApplicationSettings>(nameof(ApplicationSettings));

    public ApplicationSettings GetSettings() {
        if (!f_collection.Find(e => e.Id == ThresholdSettings.Target).TryGetFirst(out ApplicationSettings? settings)) {
            f_collection.Insert(settings ??= new());
        }

        f_debugLogger.LogApp($"Fetched: {System.Text.Json.JsonSerializer.Serialize(settings)}");
        
        settings.PropertyChanged += (sender, args) => SaveChanges(settings);

        return settings;
    }

    private void SaveChanges(ApplicationSettings settings) {
        f_collection?.Update(settings);
        Log.Information("Saved changes to ApplicationSettings");
        f_debugLogger.LogApp($"Saved: {System.Text.Json.JsonSerializer.Serialize(settings)}");
    }
}
