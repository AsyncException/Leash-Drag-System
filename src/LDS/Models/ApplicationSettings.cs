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
    [ObservableProperty, NotifyPropertyChangedFor(nameof(CalculatorDescription))] public partial MovementCalculatorType CalculatorType { get; set; } = MovementCalculatorType.Location;

    [BsonIgnore]
    public string CalculatorDescription => CalculatorType switch {
        MovementCalculatorType.Location => "Calculates direction and speed based on the position of the leash. This depends on the correct sizing of the colliders but gives a smoother and more accurate movement when pulling slowly",
        MovementCalculatorType.Stretch => "Calculates the direction based on the position of the leash and the speed on the amount of stretch. Does not require perfect collider placement to reach full speed. Less accureate with slow movements",
        MovementCalculatorType.Combined => "Best of both the Position and Stretch calculators. This will normally use the Position calculator for the accuracy till the stretch of the leash goes above 0.99. It will switch over to the Stretch calculator to allow for full speed.",
        _ => "Calculates direction and speed based on the position of the leash. This depends on the correct sizing of the colliders but gives a smoother and more accurate movement when pulling slowly",
    };
}

public enum MovementCalculatorType : byte {
    Location = 0,
    Stretch = 1,
    Combined = 2,
}