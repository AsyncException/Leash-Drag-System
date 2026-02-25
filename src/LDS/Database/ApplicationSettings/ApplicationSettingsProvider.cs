using LiteDB;
using LDS.Models;
using Microsoft.Extensions.Logging;

namespace LDS.UI.Database.ApplicationSettings;

public interface IApplicationSettingsProvider
{
    /// <summary>
    /// Fetch application settings from the database.
    /// </summary>
    /// <returns></returns>
    ApplicationSettingsDataModel GetSettings();
}

public class ApplicationSettingsProvider(ILiteDatabase database, ILogger<ApplicationSettingsProvider> logger) : IApplicationSettingsProvider
{
    private readonly ILogger<ApplicationSettingsProvider> _logger = logger;
    private readonly ILiteCollection<ApplicationSettingsDataModel> _collection = database.GetCollection<ApplicationSettingsDataModel>(nameof(ApplicationSettingsDataModel));

    public ApplicationSettingsDataModel GetSettings() {
        ApplicationSettingsDataModel? settings = _collection.FindById(ApplicationSettingsDataModel.Target);

        if (settings is null) {
            settings = new();
            _collection.Insert(settings);
        }

        _logger.LogDebug("Fetched ApplicationSettings as: {@settings}", settings);
        
        settings.PropertyChanged += (sender, args) => SaveChanges(settings);

        return settings;
    }

    private void SaveChanges(ApplicationSettingsDataModel settings) {
        _collection?.Update(settings);
        _logger.LogDebug("Saved changes to ApplicationSettings");
    }
}
