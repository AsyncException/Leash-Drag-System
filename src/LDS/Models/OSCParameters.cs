using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using System.Collections.Generic;
using VRChatOSCClient.OSCConnections;
using Windows.Devices.SerialCommunication;
using Windows.Foundation.Collections;

namespace LDS.Services.VRChatOSC;

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

    [ObservableProperty] public partial bool Enabled { get; set; } = false;
    [ObservableProperty] public partial bool IsGrabbed { get; set; } = false;
    [ObservableProperty] public partial float Angle { get; set; } = 0f;
    [ObservableProperty] public partial float Stretch { get; set; } = 0f;
    [ObservableProperty] public partial float FrontDistance { get; set; } = 0f;
    [ObservableProperty] public partial float BackDistance { get; set; } = 0f;
    [ObservableProperty] public partial float RightDistance { get; set; } = 0f;
    [ObservableProperty] public partial float LeftDistance { get; set; } = 0f;

    public bool UpdateParameter(DispatcherQueue dispatcherQueue, ParameterChangedMessage message) {
        return message.Name switch {
            ENABLED => dispatcherQueue.TryEnqueue(() => Enabled = (bool)message.Value),
            IS_GRABBED => dispatcherQueue.TryEnqueue(() => IsGrabbed = (bool)message.Value),
            ANGLE => dispatcherQueue.TryEnqueue(() => Angle = (float)message.Value),
            STRETCH => dispatcherQueue.TryEnqueue(() => Stretch = (float)message.Value),
            FRONT_COLLIDER => dispatcherQueue.TryEnqueue(() => FrontDistance = (float)message.Value),
            BACK_COLLIDER => dispatcherQueue.TryEnqueue(() => BackDistance = (float)message.Value),
            RIGHT_COLLIDER => dispatcherQueue.TryEnqueue(() => RightDistance = (float)message.Value),
            LEFT_COLLIDER => dispatcherQueue.TryEnqueue(() => LeftDistance = (float)message.Value),
            _ => true, // Ignore unknown parameters
        };
    }

    public void UpdateParameters(DispatcherQueue dispatcherQueue, Dictionary<string, object?> parameters) {
        foreach(KeyValuePair<string, object?> pair in parameters) {
            if(pair.Value is null) {
                continue;
            }

            UpdateParameter(dispatcherQueue, new ParameterChangedMessage(pair.Key, pair.Value));
        }
    }
}
