// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel;
using LDS.Utilities;

namespace LDS.Services.VRChatOSC;

/// <summary>
/// This class holds the parameters received from VRChat OSC.
/// </summary>
public partial class OSCParameters(IDebugLogger debugLogger) : ObservableObject
{
    private readonly IDebugLogger f_debugLogger = debugLogger;

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

    [ObservableProperty] public partial bool IsGrabbed { get; set; }
    [ObservableProperty] public partial float Angle { get; set; } = 0f;
    [ObservableProperty] public partial float Stretch { get; set; } = 0f;
    [ObservableProperty] public partial float FrontDistance { get; set; } = 0f;
    [ObservableProperty] public partial float BackDistance { get; set; } = 0f;
    [ObservableProperty] public partial float RightDistance { get; set; } = 0f;
    [ObservableProperty] public partial float LeftDistance { get; set; } = 0f;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
        base.OnPropertyChanged(e);

        double angle = Math.Round(Angle, 2);
        double stretch = Math.Round(Stretch, 2);
        double front = Math.Round(FrontDistance, 2);
        double back = Math.Round(BackDistance, 2);
        double right = Math.Round(RightDistance, 2);
        double left = Math.Round(LeftDistance, 2);

        f_debugLogger.LogReceive($$"""{"IsGrabbed": {{IsGrabbed}}, "Angle": {{angle}}, "Stretch": {{stretch}}, "Front": {{front}}, "Back": {{back}}, "Right": {{right}}, "Left": {{left}}}""");
    }
}
