using LiteDB;
using Serilog;
using LDS.Models;
using LDS.Utilities;

namespace LDS.Services;

public interface IThresholdSettingsProvider
{
    ThresholdSettings GetSettings();
}

public class ThresholdSettingsProvider(ILiteDatabase database, IDebugLogger debugLogger) : IThresholdSettingsProvider
{
    private readonly IDebugLogger f_debugLogger = debugLogger;
    private readonly ILiteCollection<ThresholdSettings> f_collection = database.GetCollection<ThresholdSettings>(nameof(ThresholdSettings));

    public ThresholdSettings GetSettings() {
        if (!f_collection.Find(e => e.Id == ThresholdSettings.Target).TryGetFirst(out ThresholdSettings? settings)) {
            f_collection.Insert(settings ??= new());
        }

        f_debugLogger.LogApp($"Fetched: {System.Text.Json.JsonSerializer.Serialize(settings)}");
        
        settings.PropertyChanged += (sender, args) => SaveChanges(settings);

        return settings;
    }

    private void SaveChanges(ThresholdSettings settings) {
        f_collection?.Update(settings);
        Log.Information("Saved changes to ThresholdSettings");
        f_debugLogger.LogApp($"Saved: {System.Text.Json.JsonSerializer.Serialize(settings)}");
    }
}
