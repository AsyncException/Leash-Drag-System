// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace LDS.Services.VRChatOSC;

//TODO: Rename this class to something more appropriate, like MovementData or similar.
public partial class LeashData : ObservableObject, IEquatable<LeashData?>
{
    [ObservableProperty] public partial float VerticalOffset { get; set; } = 0f;
    [ObservableProperty] public partial float HorizontalOffset { get; set; } = 0f;
    [ObservableProperty] public partial float HorizontalLook { get; set; } = 0f;
    [ObservableProperty] public partial bool ShouldRun { get; set; } = false;

    public void CopyFrom(LeashData other)
    {
        VerticalOffset = other.VerticalOffset;
        HorizontalOffset = other.HorizontalOffset;
        HorizontalLook = other.HorizontalLook;
        ShouldRun = other.ShouldRun;
    }

    public override bool Equals(object? obj) => Equals(obj as LeashData);
    public bool Equals(LeashData? other) => other is not null && VerticalOffset == other.VerticalOffset && HorizontalOffset == other.HorizontalOffset && HorizontalLook == other.HorizontalLook && ShouldRun == other.ShouldRun;

    public static bool operator ==(LeashData? left, LeashData? right) => EqualityComparer<LeashData>.Default.Equals(left, right);
    public static bool operator !=(LeashData? left, LeashData? right) => !(left == right);

    public override int GetHashCode() => HashCode.Combine(VerticalOffset, HorizontalOffset, HorizontalLook, ShouldRun);
}
