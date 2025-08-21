using LiteDB;
using LDS.Models;
using Microsoft.Extensions.Logging;

namespace LDS.Services;

public interface IApplicationSettingsProvider
{
    /// <summary>
    /// Fetch application settings from the database.
    /// </summary>
    /// <returns></returns>
    ApplicationSettings GetSettings();
}

public class ApplicationSettingsProvider(ILiteDatabase database, ILogger<ApplicationSettingsProvider> logger) : IApplicationSettingsProvider
{
    private readonly ILogger<ApplicationSettingsProvider> _logger = logger;
    private readonly ILiteCollection<ApplicationSettings> _collection = database.GetCollection<ApplicationSettings>(nameof(ApplicationSettings));

    public ApplicationSettings GetSettings() {
        ApplicationSettings? settings = _collection.FindById(ThresholdSettings.Target);

        if (settings is null) {
            settings = new();
            _collection.Insert(settings);
        }

        _logger.LogDebug("Fetched ApplicationSettings as: {@settings}", settings);
        
        settings.PropertyChanged += (sender, args) => SaveChanges(settings);

        return settings;
    }

    private void SaveChanges(ApplicationSettings settings) {
        _collection?.Update(settings);
        _logger.LogDebug("Saved changes to ApplicationSettings");
    }
}
