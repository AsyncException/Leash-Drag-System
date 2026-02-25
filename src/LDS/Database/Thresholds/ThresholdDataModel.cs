using CommunityToolkit.Mvvm.ComponentModel;
using LDS.Core;
using LiteDB;
using System;

namespace LDS.UI.Models;

public partial class ThresholdsDataModel : ObservableObject
{
    [BsonIgnore] public static Guid Target { get; } = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
    [BsonId] public Guid Id { get; set; } = Target;

    public const int PERCENTAGE_MIN = 0;
    public const int PERCENTAGE_MAX = 100;
    public const int PERCENTAGE_STEP = 1;

    public const float TURNING_MIN = 0f;
    public const float TURNING_MAX = 3f;
    public const float TURNING_STEP = 0.1f;

    public const float ZOOMING_MIN = 0f;
    public const float ZOOMING_MAX = 5f;
    public const float ZOOMING_STEP = 0.1f;

    private IController? _controller;
    public ThresholdsDataModel Bind(IController controller) {
        _controller = controller;

        _controller.Thresholds.CounterEnabled = CounterEnabled;
        _controller.Thresholds.LeashEnabled = LeashEnabled;
        _controller.Thresholds.CounterThreshold = CounterThreshold;
        _controller.Thresholds.RunningUpperThreshold = RunningUpperThreshold;
        _controller.Thresholds.RunningLowerThreshold = RunningLowerThreshold;
        _controller.Thresholds.StretchThreshold = StretchThreshold;
        _controller.Thresholds.TurningGoal = TurningGoal;
        _controller.Thresholds.TurningMultiplier = TurningMultiplier;
        _controller.Thresholds.TurningThreshold = TurningThreshold;

        return this;
    }

    [ObservableProperty] public partial float CounterThreshold { get; set; } = 0.20f;
    partial void OnCounterThresholdChanged(float value) => _controller?.Thresholds.CounterThreshold = value;

    [ObservableProperty] public partial float RunningUpperThreshold { get; set; } = 0.90f;
    partial void OnRunningUpperThresholdChanged(float value) => _controller?.Thresholds.RunningUpperThreshold = value;
    
    [ObservableProperty] public partial float RunningLowerThreshold { get; set; } = 0.75f;
    partial void OnRunningLowerThresholdChanged(float value) => _controller?.Thresholds.RunningLowerThreshold = value;

    [ObservableProperty] public partial float StretchThreshold { get; set; } = 0.30f;
    partial void OnStretchThresholdChanged(float value) => _controller?.Thresholds.StretchThreshold = value;

    [ObservableProperty] public partial float TurningThreshold { get; set; } = 0.35f;
    partial void OnTurningThresholdChanged(float value) => _controller?.Thresholds.TurningThreshold = value;

    [ObservableProperty] public partial float TurningGoal { get; set; } = 0.90f;
    partial void OnTurningGoalChanged(float value) => _controller?.Thresholds.TurningGoal = value;

    [ObservableProperty] public partial float TurningMultiplier { get; set; } = 1.50f;
    partial void OnTurningMultiplierChanged(float value) => _controller?.Thresholds.TurningMultiplier = value;
    
    [ObservableProperty] public partial bool LeashEnabled { get; set; } = true;
    partial void OnLeashEnabledChanged(bool value) => _controller?.Thresholds.LeashEnabled = value;

    [ObservableProperty] public partial bool CounterEnabled { get; set; } = true;
    partial void OnCounterEnabledChanged(bool value) => _controller?.Thresholds.CounterEnabled = value;

    [ObservableProperty] public partial float Zooming { get; set; } = 3f;
    [ObservableProperty] public partial bool ShowPositionLayer { get; set; } = true;
    [ObservableProperty] public partial bool ShowStretchLayer { get; set; } = true;
}
