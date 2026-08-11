using CommunityToolkit.Mvvm.ComponentModel;
using LDS.Core;

namespace LDS.Models;

/// <summary>
/// This class holds the parameters received from VRChat OSC.
/// </summary>
public partial class OSCParameters : ObservableObject
{
    public const string ENABLED = "Leash_Enabled";
    public const string IS_GRABBED = "Leash_IsGrabbed";
    public const string ANGLE = "Leash_Angle";
    public const string STRETCH = "Leash_Stretch";
    public const string FRONT_COLLIDER = "Leash_Front";
    public const string BACK_COLLIDER = "Leash_Back";
    public const string RIGHT_COLLIDER = "Leash_Right";
    public const string LEFT_COLLIDER = "Leash_Left";

    public const string HOUR = "timer_hour";
    public const string MINUTE = "timer_minute";
    public const string SECOND = "timer_second";

    private IController? _controller;
    public OSCParameters Bind(IController controller) {
        _controller = controller;
        _controller.ParameterChanged += ParameterChanged;
        return this;
    }

    private void ParameterChanged(object? sender, Parameter parameter) {
        switch (parameter.Name) {
            case ENABLED:
                Enabled = parameter.GetValue<bool>();
                break;
            case IS_GRABBED:
                IsGrabbed = parameter.GetValue<bool>();
                break;
            case ANGLE:
                Angle = parameter.GetValue<float>();
                break;
            case STRETCH:
                Stretch = parameter.GetValue<float>();
                break;
            case FRONT_COLLIDER:
                FrontDistance = parameter.GetValue<float>();
                break;
            case BACK_COLLIDER:
                BackDistance = parameter.GetValue<float>();
                break;
            case RIGHT_COLLIDER:
                RightDistance = parameter.GetValue<float>();
                break;
            case LEFT_COLLIDER:
                LeftDistance = parameter.GetValue<float>();
                break;
        }
    }

    [ObservableProperty] public partial bool Enabled { get; set; } = false;
    [ObservableProperty] public partial bool IsGrabbed { get; set; } = false;
    [ObservableProperty] public partial float Angle { get; set; } = 0f;
    [ObservableProperty] public partial float Stretch { get; set; } = 0f;
    [ObservableProperty] public partial float FrontDistance { get; set; } = 0f;
    [ObservableProperty] public partial float BackDistance { get; set; } = 0f;
    [ObservableProperty] public partial float RightDistance { get; set; } = 0f;
    [ObservableProperty] public partial float LeftDistance { get; set; } = 0f;
}
