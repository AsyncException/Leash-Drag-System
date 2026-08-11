namespace LDS.Core.CommunicationObjects;

public sealed class Thresholds {

    public event EventHandler<bool> OnCounterEnabledChanged = delegate { };
    public bool CounterEnabled {
        get;
        set {
            field = value;
            OnCounterEnabledChanged?.Invoke(this, value);
        }
    }


    public event EventHandler<bool> OnLeashEnabledChanged = delegate { };
    public bool LeashEnabled {
        get;
        set {
            field = value;
            OnLeashEnabledChanged?.Invoke(this, value);
        }
    }

    public float CounterThreshold { get; set; }
    public float RunningUpperThreshold { get; set; }
    public float RunningLowerThreshold { get; set; }
    public float StretchThreshold { get; set; }
    public float TurningGoal { get; set; }
    public float TurningMultiplier { get; set; }
    public float TurningThreshold { get; set; }
}



