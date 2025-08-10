using CommunityToolkit.Mvvm.ComponentModel;
using LiteDB;
using System;

namespace LDS.Models;

public class TimerStorage
{
    [BsonIgnore] public static Guid Target { get; } = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
    [BsonId] public Guid Id { get; set; } = Target;

    public int Hours { get; set; }
    public int Minutes { get; set; }
    public int Seconds { get; set; }
}

public partial class TimerData : ObservableObject
{
    [ObservableProperty] public partial TimeSpan TimeSpan { get; set; } = new();
}