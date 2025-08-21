using LDS.Models;
using LiteDB;
using Microsoft.Extensions.Logging;
using System;

namespace LDS.TimerSystem;

public interface ITimeDataProvider
{
    TimerStorage GetTime();
    void SaveTime(TimerStorage storage);
}

public class TimeDataProvider(ILiteDatabase database, ILogger<TimeDataProvider> logger) : ITimeDataProvider
{
    private readonly ILogger<TimeDataProvider> _logger = logger;
    private readonly ILiteCollection<TimerStorage> _collection = database.GetCollection<TimerStorage>(nameof(TimerStorage));

    public TimerStorage GetTime() {
        TimerStorage? timer = _collection.FindById(TimerStorage.Target);

        if (timer is null) {
            timer = new TimerStorage();
            _collection.Insert(timer);
        }

        _logger.LogDebug("Fetched TimerStorage as: {@timerStorage}", timer);

        return timer;
    }

    public void SaveTime(TimerStorage storage) {
        _collection.Update(storage);
        _logger.LogInformation("Saved TimerStorage");
    }
}
