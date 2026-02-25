using CommunityToolkit.Mvvm.ComponentModel;
using LDS.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace LDS.UI.Models;

public partial class MovementData : ObservableObject
{
    private IController? _controller;
    public MovementData Bind(IController controller) {
        _controller = controller;
        _controller.ControllerData.OnHorizontalOffsetChanged += (s, a) => HorizontalOffset = a;
        _controller.ControllerData.OnVerticalOffsetChanged += (s, a) => VerticalOffset = a;
        _controller.ControllerData.OnHorizontalLookChanged += (s, a) => HorizontalLook = a;
        _controller.ControllerData.OnShouldRunChanged += (s, a) => ShouldRun = a;

        return this;
    }

    [ObservableProperty]
    public partial float HorizontalOffset { get; set; }

    [ObservableProperty]
    public partial float VerticalOffset { get; set; }

    [ObservableProperty]
    public partial float HorizontalLook { get; set; }
     
    [ObservableProperty]
    public partial bool ShouldRun { get; set; }
}
