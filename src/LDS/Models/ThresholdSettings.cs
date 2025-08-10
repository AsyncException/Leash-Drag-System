using CommunityToolkit.Mvvm.ComponentModel;
using LiteDB;
using System;

namespace LDS.Models;

public partial class ThresholdSettings : ObservableObject
{
    [BsonIgnore] public static Guid Target { get; } = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);

    [BsonId] public Guid Id { get; set; } = Target;

    [ObservableProperty] public partial float TimerThreshold { get; set; } = 0.20f;
    [ObservableProperty] public partial float RunningMaxThreshold { get; set; } = 0.90f;
    [ObservableProperty] public partial float RunningMinThreshold { get; set; } = 0.75f;
    [ObservableProperty] public partial float StretchThreshold { get; set; } = 0.30f;
    [ObservableProperty] public partial float TurningThreshold { get; set; } = 0.35f;
    [ObservableProperty] public partial float TurningGoal { get; set; } = 0.90f;
    [ObservableProperty] public partial float TurningMultiplier { get; set; } = 1.50f;
    [ObservableProperty] public partial float Zooming { get; set; } = 3f;
    [ObservableProperty] public partial bool LeashEnabled { get; set; } = true;
    [ObservableProperty] public partial bool TimerEnabled { get; set; } = true;

    [ObservableProperty] public partial bool ShowPositionLayer { get; set; } = true;
    [ObservableProperty] public partial bool ShowStretchLayer { get; set; } = true;

    public const int PERCENTAGE_MIN = 0;
    public const int PERCENTAGE_MAX = 100;
    public const int PERCENTAGE_STEP = 1;

    public const float TURNING_MIN = 0f;
    public const float TURNING_MAX = 3f;
    public const float TURNING_STEP = 0.1f;

    public const float ZOOMING_MIN = 0f;
    public const float ZOOMING_MAX = 5f;
    public const float ZOOMING_STEP = 0.1f;
}
