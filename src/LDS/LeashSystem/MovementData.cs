using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace LDS.Services.VRChatOSC;

public partial class MovementData : ObservableObject, IEquatable<MovementData?>
{
    [ObservableProperty] public partial float VerticalOffset { get; set; } = 0f;
    [ObservableProperty] public partial float HorizontalOffset { get; set; } = 0f;
    [ObservableProperty] public partial float HorizontalLook { get; set; } = 0f;
    [ObservableProperty] public partial bool ShouldRun { get; set; } = false;

    public void CopyFrom(MovementData other)
    {
        VerticalOffset = other.VerticalOffset;
        HorizontalOffset = other.HorizontalOffset;
        HorizontalLook = other.HorizontalLook;
        ShouldRun = other.ShouldRun;
    }

    public override bool Equals(object? obj) => Equals(obj as MovementData);
    public bool Equals(MovementData? other) => other is not null && VerticalOffset == other.VerticalOffset && HorizontalOffset == other.HorizontalOffset && HorizontalLook == other.HorizontalLook && ShouldRun == other.ShouldRun;

    public static bool operator ==(MovementData? left, MovementData? right) => EqualityComparer<MovementData>.Default.Equals(left, right);
    public static bool operator !=(MovementData? left, MovementData? right) => !(left == right);

    public override int GetHashCode() => HashCode.Combine(VerticalOffset, HorizontalOffset, HorizontalLook, ShouldRun);
}
