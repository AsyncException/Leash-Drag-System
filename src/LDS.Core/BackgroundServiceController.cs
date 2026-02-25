using LDS.Core.CommunicationObjects;

namespace LDS.Core;

public interface IController {
    public event EventHandler<string> ParameterChanged; 
    public IReadOnlyDictionary<string, Parameter> Parameters { get; }
    public IConnectionStatus ConnectionStatus { get; }
    public IControllerData ControllerData { get; }
    public Thresholds Thresholds { get; }
    public Settings Settings { get; }

    public void InvokeStop();
    public void ToggleUnity();
}

public sealed class BackgroundServiceController : IController {
    public event EventHandler<string> ParameterChanged = delegate { };
    IReadOnlyDictionary<string, Parameter> IController.Parameters => Parameters;
    internal Dictionary<string, Parameter> Parameters { get; } = [];
    internal void InvokeParameterChanged(string parameter) => ParameterChanged.Invoke(this, parameter);

    IConnectionStatus IController.ConnectionStatus => ConnectionStatus;
    public ConnectionStatus ConnectionStatus { get; } = new();

    IControllerData IController.ControllerData => ControllerData;
    public ControllerData ControllerData { get; } = new();

    public Thresholds Thresholds { get; } = new();
    public Settings Settings { get; } = new();


    public event EventHandler StopInvoked = delegate { };
    public void InvokeStop() => StopInvoked.Invoke(this, EventArgs.Empty);

    public event EventHandler ToggleUnityInvoked = delegate { };
    public void ToggleUnity() => ToggleUnityInvoked.Invoke(this, EventArgs.Empty);

    public event EventHandler EmergencyStopInvoked = delegate { };
    public void EmergencyStop() => EmergencyStopInvoked.Invoke(this, EventArgs.Empty);
}

public sealed class Settings {

    public event EventHandler<bool> OnGlobalEnableCounterChanged = delegate { };
    public bool GlobalEnableCounter {
        get;
        set {
            field = value;
            OnGlobalEnableCounterChanged?.Invoke(this, value);
        }
    }

    public event EventHandler<bool> OnGlobalEnableLeashChanged = delegate { };
    public bool GlobalEnableLeash {
        get;
        set {
            field = value;
            OnGlobalEnableLeashChanged?.Invoke(this, value);
        }
    }

    public event EventHandler<bool> OnEnableToggleOnNullInputChanged = delegate { };
    public bool EnableToggleOnNullInput {
        get;
        set {
            field = value;
            OnEnableToggleOnNullInputChanged?.Invoke(this, value);
        }
    }

    public event EventHandler<MovementCalculatorType> OnCalculatorTypeChanged = delegate { };
    public MovementCalculatorType CalculatorType {
        get;
        set {
            field = value;
            OnCalculatorTypeChanged?.Invoke(this, value);
        }
    }
}

public enum MovementCalculatorType : byte
{
    Location = 0,
    Stretch = 1,
    Combined = 2,
}