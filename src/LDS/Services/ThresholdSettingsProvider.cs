using LDS.Models;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace LDS.Services;

public interface IThresholdSettingsProvider
{
    ThresholdSettings GetSettings();
}

public class ThresholdSettingsProvider(ILiteDatabase database, ILogger<ThresholdSettingsProvider> logger) : IThresholdSettingsProvider
{
    private readonly ILogger<ThresholdSettingsProvider> _logger = logger;
    private readonly ILiteCollection<ThresholdSettings> _collection = database.GetCollection<ThresholdSettings>(nameof(ThresholdSettings));

    public ThresholdSettings GetSettings() {
        ThresholdSettings settings = _collection.FindById(ThresholdSettings.Target);

        if (settings is null) {
            settings = new ThresholdSettings();
            _collection.Insert(settings);
        }

        _logger.LogDebug("Fetched ThresholdSettings as: {@settings}", settings);
        
        settings.PropertyChanged += (sender, args) => SaveChanges(settings);

        return settings;
    }

    private void SaveChanges(ThresholdSettings settings) {
        _collection?.Update(settings);
        _logger.LogInformation("Saved changes to ThresholdSettings");
    }
}
