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

    private void ParameterChanged(object? sender, string e) {
        switch (e) {
            case ENABLED:
                Enabled = _controller?.Parameters[ENABLED]?.GetValue<bool>() ?? false;
                break;
            case IS_GRABBED:
                IsGrabbed = _controller?.Parameters[IS_GRABBED]?.GetValue<bool>() ?? false;
                break;
            case ANGLE:
                Angle = _controller?.Parameters[ANGLE]?.GetValue<float>() ?? 0f;
                break;
            case STRETCH:
                Stretch = _controller?.Parameters[STRETCH]?.GetValue<float>() ?? 0f;
                break;
            case FRONT_COLLIDER:
                FrontDistance = _controller?.Parameters[FRONT_COLLIDER]?.GetValue<float>() ?? 0f;
                break;
            case BACK_COLLIDER:
                BackDistance = _controller?.Parameters[BACK_COLLIDER]?.GetValue<float>() ?? 0f;
                break;
            case RIGHT_COLLIDER:
                RightDistance = _controller?.Parameters[RIGHT_COLLIDER]?.GetValue<float>() ?? 0f;
                break;
            case LEFT_COLLIDER:
                LeftDistance = _controller?.Parameters[LEFT_COLLIDER]?.GetValue<float>() ?? 0f;
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
