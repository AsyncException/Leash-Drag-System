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
            WeakReferenceMessenger.Default.Send<StartTimerUpdater>();
        }
        else {
            WeakReferenceMessenger.Default.Send<StopTimerUpdater>();
        }
    }

    [ObservableProperty] public partial bool GlobalEnableLeash { get; set; } = true;
    partial void OnGlobalEnableLeashChanged(bool value) {
        if (value) {
            WeakReferenceMessenger.Default.Send<StartLeashUpdater>();
        }
        else {
            WeakReferenceMessenger.Default.Send<StopLeashUpdater>();
        }
    }

    [ObservableProperty] public partial bool EnableToggleOnNullInput { get; set; } = false;
}
