using CommunityToolkit.Mvvm.ComponentModel;
using LiteDB;
using System;

namespace LDS.Models;

/// <summary>
/// This class is a storage object for a TimeSpan for the database.
/// </summary>
public partial class TimerStorage : ObservableObject
{
    [BsonIgnore] public static Guid Target { get; } = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
    [BsonId] public Guid Id { get; set; } = Target;

    [ObservableProperty] public partial int Hours { get; set; }
    [ObservableProperty] public partial int Minutes { get; set; }
    [ObservableProperty] public partial int Seconds { get; set; }

    public TimeSpan GetTimeSpan() => new(Hours, Minutes, Seconds);
    public void FromTimeSpan(TimeSpan span) {
        if(Hours != span.Hours) {
            Hours = span.Hours;
        }

        if(Minutes != span.Minutes) {
            Minutes = span.Minutes;
        }

        if(Seconds != span.Seconds) {
            Seconds = span.Seconds;
        }
    }
}