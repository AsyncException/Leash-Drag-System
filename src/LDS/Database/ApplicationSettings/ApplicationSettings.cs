using CommunityToolkit.Mvvm.ComponentModel;
using LiteDB;
using System;
using LDS.Core;

namespace LDS.Models;

public partial class ApplicationSettingsDataModel : ObservableObject
{
    [BsonIgnore] public static Guid Target { get; } = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
    [BsonId] public Guid Id { get; set; } = Target;

    private IController? _controller;
    public ApplicationSettingsDataModel Bind(IController controller) {
        _controller = controller;
        _controller?.Settings.GlobalEnableCounter = GlobalEnableCounter;
        _controller?.Settings.GlobalEnableLeash = GlobalEnableLeash;
        _controller?.Settings.EnableToggleOnNullInput = EnableToggleOnNullInput;
        _controller?.Settings.CalculatorType = CalculatorType;

        return this;
    }

    [ObservableProperty] public partial bool GlobalEnableCounter { get; set; } = false;
    partial void OnGlobalEnableCounterChanged(bool value) => _controller?.Settings.GlobalEnableCounter = value;

    [ObservableProperty] public partial bool GlobalEnableLeash { get; set; } = true;
    partial void OnGlobalEnableLeashChanged(bool value) => _controller?.Settings.GlobalEnableLeash = value;

    [ObservableProperty] public partial bool EnableToggleOnNullInput { get; set; } = false;
    partial void OnEnableToggleOnNullInputChanged(bool value) => _controller?.Settings.EnableToggleOnNullInput = value;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(CalculatorDescription))] public partial MovementCalculatorType CalculatorType { get; set; } = MovementCalculatorType.Location;
    partial void OnCalculatorTypeChanged(MovementCalculatorType value) => _controller?.Settings.CalculatorType = value;


    [BsonIgnore]
    public string CalculatorDescription => CalculatorType switch {
        MovementCalculatorType.Location => "Calculates direction and speed based on the position of the leash. This depends on the correct sizing of the colliders but gives a smoother and more accurate movement when pulling slowly",
        MovementCalculatorType.Stretch => "Calculates the direction based on the position of the leash and the speed on the amount of stretch. Does not require perfect collider placement to reach full speed. Less accureate with slow movements",
        MovementCalculatorType.Combined => "Best of both the Position and Stretch calculators. This will normally use the Position calculator for the accuracy till the stretch of the leash goes above 0.99. It will switch over to the Stretch calculator to allow for full speed.",
        _ => "Calculates direction and speed based on the position of the leash. This depends on the correct sizing of the colliders but gives a smoother and more accurate movement when pulling slowly",
    };
}

