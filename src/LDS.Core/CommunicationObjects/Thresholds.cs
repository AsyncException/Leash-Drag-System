namespace LDS.Core.CommunicationObjects;

public sealed class Thresholds {
    public bool CounterEnabled { get; set; }
    public bool LeashEnabled { get; set; }

    public float CounterThreshold { get; set; }
    public float RunningUpperThreshold { get; set; }
    public float RunningLowerThreshold { get; set; }
    public float StretchThreshold { get; set; }
    public float TurningGoal { get; set; }
    public float TurningMultiplier { get; set; }
    public float TurningThreshold { get; set; }
}



