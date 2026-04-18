using LDS.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Numerics;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace LDS.TUI.Views;

internal class PropertiesView : FrameView {
    public PropertiesView() {
        Title = "_Properties";
        CanFocus = true;

        var controller = ServiceProvider.Services.GetRequiredService<IController>();

        //Toggle Leash
        var leashToggle = new CheckBox {
            Value = controller.Thresholds.LeashEnabled ? CheckState.Checked : CheckState.UnChecked,
            Text = "Leash toggle",
        };

        leashToggle.ValueChanged += (s, a) => controller.Thresholds.LeashEnabled = a.NewValue == CheckState.Checked;
        controller.Thresholds.OnLeashEnabledChanged += (s, enabled) => leashToggle.Value = enabled ? CheckState.Checked : CheckState.UnChecked;

        //Toggle Counter

        var counterToggle = new CheckBox {
            Y = Pos.Bottom(leashToggle),
            Value = controller.Thresholds.CounterEnabled ? CheckState.Checked : CheckState.UnChecked,
            Text = "Counter toggle"
        };

        counterToggle.ValueChanged += (s, a) => controller.Thresholds.CounterEnabled = a.NewValue == CheckState.Checked;
        controller.Thresholds.OnCounterEnabledChanged += (s, enabled) => counterToggle.Value = enabled ? CheckState.Checked : CheckState.UnChecked;

        //Spacer

        var line1 = new Line {
            Y = Pos.Bottom(counterToggle),
            Width = Dim.Fill()
        };

        const int LABEL_WIDTH = 25;

        //Counter Threshold %
        var counterThreshold = new LabledNumberUpDown<int> {
            Y = Pos.Bottom(line1),
            LabelWidth = LABEL_WIDTH,
            Name = "Counter Threshold",
            MinValue = 0,
            MaxValue = 100,
            Increment = 5,
            Value = (int)Math.Round(controller.Thresholds.CounterThreshold * 100, 0),
            Action = value => controller.Thresholds.CounterThreshold = (float)Math.Round((decimal)value / 100, 2),
            HelpText = "The Threshold for when the counter should start counting based on stretch amount"
        };

        //Running Upper Threshold %
        var runningUpperThreshold = new LabledNumberUpDown<int> {
            Y = Pos.Bottom(counterThreshold),
            LabelWidth = LABEL_WIDTH,
            Name = "Running Upper Limit",
            MinValue = 0,
            MaxValue = 100,
            Increment = 5,
            Value = (int)Math.Round(controller.Thresholds.RunningUpperThreshold * 100, 0),
            Action = value => controller.Thresholds.RunningUpperThreshold = (float)Math.Round((decimal)value / 100, 2),
            HelpText = ""
        };

        //Running Lower THreshold %
        var runningLowerThreshold = new LabledNumberUpDown<int> {
            Y = Pos.Bottom(runningUpperThreshold),
            LabelWidth = LABEL_WIDTH,
            Name = "Running Lower Limit",
            MinValue = 0,
            MaxValue = 100,
            Increment = 5,
            Value = (int)Math.Round(controller.Thresholds.RunningLowerThreshold * 100, 0),
            Action = value => controller.Thresholds.RunningLowerThreshold = (float)Math.Round((decimal)value / 100, 2),
            HelpText = ""
        };

        //Stretch Threshold %
        var stretchThreshold = new LabledNumberUpDown<int> {
            Y = Pos.Bottom(runningLowerThreshold),
            LabelWidth = LABEL_WIDTH,
            Name = "Stretch Threshold",
            MinValue = 0,
            MaxValue = 100,
            Increment = 5,
            Value = (int)Math.Round(controller.Thresholds.StretchThreshold * 100, 0),
            Action = value => controller.Thresholds.StretchThreshold = (float)Math.Round((decimal)value / 100, 2),
            HelpText = ""
        };

        //Turning Threshold %
        var turningThreshold = new LabledNumberUpDown<int> {
            Y = Pos.Bottom(stretchThreshold),
            LabelWidth = LABEL_WIDTH,
            Name = "Turning Threshold",
            MinValue = 0,
            MaxValue = 100,
            Increment = 5,
            Value = (int)Math.Round(controller.Thresholds.TurningThreshold * 100, 0),
            Action = value => controller.Thresholds.TurningThreshold = (float)Math.Round((decimal)value / 100, 2),
            HelpText = ""
        };

        //Turning Goal %
        var turningGoal = new LabledNumberUpDown<int> {
            Y = Pos.Bottom(turningThreshold),
            LabelWidth = LABEL_WIDTH,
            Name = "Turning Goal",
            MinValue = 0,
            MaxValue = 100,
            Increment = 5,
            Value = (int)Math.Round(controller.Thresholds.TurningGoal * 100, 0),
            Action = value => controller.Thresholds.TurningGoal = (float)Math.Round((decimal)value / 100, 2),
            HelpText = ""
        };

        //Turning Multiplier 1-5
        var turningMultiplier = new LabledNumberUpDown<float> {
            Y = Pos.Bottom(turningGoal),
            LabelWidth = LABEL_WIDTH,
            Name = "Turning Multiplier",
            MinValue = 1,
            MaxValue = 5,
            Increment = 0.5f,
            Value = controller.Thresholds.TurningMultiplier,
            Action = value => controller.Thresholds.TurningMultiplier = (float)Math.Round((decimal)value / 100, 2),
            HelpText = ""
        };

        var spacer1 = new Line() { Y = Pos.Bottom(turningMultiplier), Height = Dim.Fill() };

        Add(leashToggle,
            counterToggle,
            line1,
            counterThreshold,
            runningUpperThreshold,
            runningLowerThreshold,
            stretchThreshold,
            turningThreshold,
            turningGoal,
            turningMultiplier,
            spacer1);
    }
}

internal class LabledNumberUpDown<T> : View where T : INumber<T>
{

    public required string Name { get => _label.Text; set => _label.Text = value; }
    public required T MinValue { get; set; }
    public required T MaxValue { get; set; }
    public required string HelpText { get; set; } = string.Empty;

    public T Value { get => _field.Value; set => _field.Value = value; }
    public T Increment { get => _field.Increment!; set => _field.Increment = value; }
    public Dim LabelWidth { get => _label.Width; set => _label.Width = value; }

    private readonly Label _label;
    private readonly NumericUpDown<T> _field;

    public Action<T> Action { get; set; } = delegate { };

    public LabledNumberUpDown() {
        CanFocus = true;
        Height = Dim.Auto(DimAutoStyle.Content);
        Width = Dim.Auto(DimAutoStyle.Content);

        _label = new Label() { Text = "Test" };
        _label.MouseEvent += OnClick;

        _field = new NumericUpDown<T> {
            X = Pos.Right(_label)
        };

        _field.ValueChanged += OnValueChanged;

        Add(_label, _field);
    }

    private void OnValueChanged(object? sender, ValueChangedEventArgs<T?> e) {
        if (e.NewValue! > MaxValue) {
            _field.Value = MaxValue;
        }

        if (e.NewValue! < MinValue) {
            _field.Value = MinValue;
        }

        Action(_field.Value!);
    }

    private void OnClick(object? sender, Mouse e) {
        if (e.IsPressed && e.Flags.HasFlag(MouseFlags.LeftButtonPressed)) {
            MessageBox.Query(App!, _label.Text, HelpText, "Ok");
        }
    }

}