using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace LDS.TUI.Views;

internal class MainView : View
{
    public MainView() {
        CanFocus = true;
        Width = Dim.Fill();
        Height = Dim.Fill();

        var statistics = new StatisticsView {
            Width = Dim.Percent(50),
            Height = Dim.Fill() - 7
        };

        var properties = new PropertiesView {
            Width = Dim.Percent(50),
            Height = Dim.Fill(),
            X = Pos.Right(statistics)
        };

        var control = new ControlView {
            Width = Dim.Percent(50),
            Height = 7,
            Y = Pos.Bottom(statistics)
        };

        Add(statistics, properties, control);
    }
}