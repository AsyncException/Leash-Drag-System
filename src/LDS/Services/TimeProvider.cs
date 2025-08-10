using LiteDB;
using Serilog;
using System;
using System.Linq;
using LDS.Models;
using LDS.Utilities;

namespace LDS.Services;

public interface ITimeProvider
{
    TimeSpan GetTime();
    void SaveTime(TimeSpan span);
}

public class TimeProvider(ILiteDatabase database, IDebugLogger debugLogger) : ITimeProvider
{
    private readonly IDebugLogger f_debugLogger = debugLogger;
    private readonly ILiteCollection<TimerStorage> f_collection = database.GetCollection<TimerStorage>(nameof(TimerStorage));

    public TimeSpan GetTime() {
        TimerStorage? timer = f_collection.Find(e => e.Id == TimerStorage.Target).FirstOrDefault();

        if (timer is null) {
            timer = new TimerStorage();
            f_collection.Insert(timer);
        }

        f_debugLogger.LogApp($"Fetched counter: {timer.Hours}, {timer.Minutes}, {timer.Seconds}");

        return new TimeSpan(timer.Hours, timer.Minutes, timer.Seconds);
    }

    public void SaveTime(TimeSpan span) {
        f_collection.Update(new TimerStorage() { Hours = span.Hours, Minutes = span.Minutes, Seconds = span.Seconds });
        Log.Information("Saved counter: {hours}, {minutes}, {seconds}", span.Hours, span.Minutes, span.Seconds);
        f_debugLogger.LogApp($"Saved counter: {span.Hours}, {span.Minutes}, {span.Seconds}");
    }
}
