using CommunityToolkit.Mvvm.ComponentModel;
using LDS.Core;
using LiteDB;
using System;

namespace LDS.Models;

/// <summary>
/// This class is a storage object for a TimeSpan for the database.
/// </summary>
public partial class CounterDataModel : ObservableObject
{
    [BsonIgnore] public static Guid Target { get; } = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
    [BsonId] public Guid Id { get; set; } = Target;

    private IController? _controller;
    public CounterDataModel Bind(IController controller) {
        _controller = controller;
        _controller.ControllerData.Hours = Hours;
        _controller.ControllerData.Minutes = Minutes;
        _controller.ControllerData.Seconds = Seconds;

        _controller.ControllerData.OnHoursChanged += (s, a) => Hours = a;
        _controller.ControllerData.OnMinutesChanged += (s, a) => Minutes = a;
        _controller.ControllerData.OnSecondsChanged += (s, a) => Seconds = a;

        return this;
    }

    [ObservableProperty] public partial int Hours { get; set; }
    [ObservableProperty] public partial int Minutes { get; set; }
    [ObservableProperty] public partial int Seconds { get; set; }
}