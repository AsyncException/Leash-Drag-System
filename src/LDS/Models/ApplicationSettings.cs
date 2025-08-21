using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LiteDB;
using System;
using LDS.Messages;

namespace LDS.Models;

public partial class ApplicationSettings : ObservableObject
{
    [BsonIgnore] public static Guid Target { get; } = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
    [BsonId] public Guid Id { get; set; } = Target;

    [ObservableProperty] public partial bool GlobalEnableCounter { get; set; } = false;
    partial void OnGlobalEnableCounterChanged(bool value) {
        if (value) {
            StrongReferenceMessenger.Default.Send<StartTimerUpdater>();
        }
        else {
            StrongReferenceMessenger.Default.Send<StopTimerUpdater>();
        }
    }

    [ObservableProperty] public partial bool GlobalEnableLeash { get; set; } = true;
    partial void OnGlobalEnableLeashChanged(bool value) {
        if (value) {
            StrongReferenceMessenger.Default.Send<StartLeashUpdater>();
        }
        else {
            StrongReferenceMessenger.Default.Send<StopLeashUpdater>();
        }
    }

    [ObservableProperty] public partial bool EnableToggleOnNullInput { get; set; } = false;
}
