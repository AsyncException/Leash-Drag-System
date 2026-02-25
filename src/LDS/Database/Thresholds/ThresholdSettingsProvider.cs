using LDS.Core.CommunicationObjects;
using LDS.UI.Models;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace LDS.UI.Database.Thresholds;

public interface IThresholdSettingsProvider
{
    ThresholdsDataModel GetThresholds();
}

public class ThresholdSettingsProvider(ILiteDatabase database, ILogger<ThresholdSettingsProvider> logger) : IThresholdSettingsProvider
{
    private readonly ILogger<ThresholdSettingsProvider> _logger = logger;
    private readonly ILiteCollection<ThresholdsDataModel> _collection = database.GetCollection<ThresholdsDataModel>(nameof(Thresholds));

    public ThresholdsDataModel GetThresholds() {
        ThresholdsDataModel settings = _collection.FindById(ThresholdsDataModel.Target);

        if (settings is null) {
            settings = new ThresholdsDataModel();
            _collection.Insert(settings);
        }

        _logger.LogDebug("Fetched ThresholdSettings as: {@settings}", settings);
        
        settings.PropertyChanged += (sender, args) => SaveChanges(settings);

        return settings;
    }

    private void SaveChanges(ThresholdsDataModel settings) {
        _collection?.Update(settings);
        _logger.LogInformation("Saved changes to ThresholdSettings");
    }
}
