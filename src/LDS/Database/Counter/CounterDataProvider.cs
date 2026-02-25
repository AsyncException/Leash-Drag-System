using LDS.Models;
using LiteDB;
using Microsoft.Extensions.Logging;
using System;

namespace LDS.UI.Database.Counter;

public interface ICounterDataProvider
{
    CounterDataModel GetTime();
}

public class CounterDataProvider(ILiteDatabase database, ILogger<CounterDataProvider> logger) : ICounterDataProvider
{
    private readonly ILogger<CounterDataProvider> _logger = logger;
    private readonly ILiteCollection<CounterDataModel> _collection = database.GetCollection<CounterDataModel>(nameof(CounterDataModel));

    public CounterDataModel GetTime() {
        CounterDataModel? counter = _collection.FindById(CounterDataModel.Target);

        if (counter is null) {
            counter = new CounterDataModel();
            _collection.Insert(counter);
        }

        _logger.LogDebug("Fetched TimerStorage as: {@timerStorage}", counter);

        counter.PropertyChanged += (sender, args) => SaveTime(counter);

        return counter;
    }

    public void SaveTime(CounterDataModel storage) {
        if (storage is CounterDataModel timerStorage) {
            _collection.Update(timerStorage);
            _logger.LogInformation("Saved TimerStorage");
        }
    }
}
