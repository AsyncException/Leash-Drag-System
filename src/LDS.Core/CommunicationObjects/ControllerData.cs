namespace LDS.Core.CommunicationObjects;

public interface IControllerData
{
    public float HorizontalLook { get; }
    public event EventHandler<float> OnHorizontalLookChanged;
    public float HorizontalOffset { get; }
    public event EventHandler<float> OnHorizontalOffsetChanged;
    public float VerticalOffset { get; }
    public event EventHandler<float> OnVerticalOffsetChanged;
    public bool ShouldRun { get; }
    public event EventHandler<bool> OnShouldRunChanged;

    public int Hours { get; set; }
    public event EventHandler<int> OnHoursChanged;
    public int Minutes { get; set; }
    public event EventHandler<int> OnMinutesChanged;
    public int Seconds { get; set; }
    public event EventHandler<int> OnSecondsChanged;
}

public sealed class ControllerData : IControllerData {

    public event EventHandler<float> OnHorizontalLookChanged = delegate { };
    public float HorizontalLook {
        get;
        set {
            field = value;
            OnHorizontalLookChanged?.Invoke(this, value);
        }
    }

    public event EventHandler<float> OnHorizontalOffsetChanged = delegate { };
    public float HorizontalOffset {
        get;
        set {
            field = value;
            OnHorizontalOffsetChanged?.Invoke(this, value);
        }
    }



    public event EventHandler<float> OnVerticalOffsetChanged = delegate { };
    public float VerticalOffset {
        get;
        set {
            field = value;
            OnVerticalOffsetChanged?.Invoke(this, value);
        }
    }



    public event EventHandler<bool> OnShouldRunChanged = delegate { };
    public bool ShouldRun {
        get;
        set {
            field = value;
            OnShouldRunChanged?.Invoke(this, value);
        }
    }



    public event EventHandler<int> OnHoursChanged = delegate { };
    public int Hours {
        get;
        set {
            field = value;
            OnHoursChanged?.Invoke(this, value);
        }
    }

    public event EventHandler<int> OnMinutesChanged = delegate { };
    public int Minutes {
        get;
        set {
            field = value;
            OnMinutesChanged?.Invoke(this, value);
        }
    }

    public event EventHandler<int> OnSecondsChanged = delegate { };
    public int Seconds {
        get;
        set {
            field = value;
            OnSecondsChanged?.Invoke(this, value);
        }
    }
}

