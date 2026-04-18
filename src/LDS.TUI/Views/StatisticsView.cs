using LDS.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace LDS.TUI.Views;

internal class StatisticsView : FrameView
{
    public StatisticsView() {
        Title = "_Statistics";
        HotKey = Key.S.WithAlt;
        CanFocus = false;

        var controller = ServiceProvider.Services.GetRequiredService<IController>();

        // Horizontal
        var horizontalFrame = new FrameView { Title = "Horizontal", Width = 15, Height = Dim.Auto() };
        var horizontalLabel = new Label { Text = "0", TextAlignment = Alignment.Center, Width = Dim.Fill() };
        horizontalFrame.Add(horizontalLabel);

        // Forward backward
        var verticalFrame = new FrameView { X = Pos.Right(horizontalFrame), Title = "Vertical", Width = 15, Height = Dim.Auto() };
        var verticalLabel = new Label { Text = "0", TextAlignment = Alignment.Center, Width = Dim.Fill() };
        verticalFrame.Add(verticalLabel);

        // Turning
        var turningFrame = new FrameView { X = Pos.Right(verticalFrame), Title = "Turning", Width = 15, Height = Dim.Auto() };
        var turningLabel = new Label { Text = "0", TextAlignment = Alignment.Center, Width = Dim.Fill() };
        turningFrame.Add(turningLabel);

        // Running
        var runningFrame = new FrameView() { X = Pos.Right(turningFrame), Title = "Running", Width = 15, Height = Dim.Auto() };
        var runningLabel = new Label { Text = "False", TextAlignment = Alignment.Center, Width = Dim.Fill() };
        runningFrame.Add(runningLabel);

        var splitter1 = new Line() { Y = Pos.Bottom(horizontalFrame) };

        Add(horizontalFrame, verticalFrame, turningFrame, runningFrame, splitter1);


        //Top
        var topFrame = new FrameView { Y = Pos.Bottom(splitter1), X = Pos.Center(), Width = 15, Height = Dim.Auto() };
        var topLabel = new Label { Text = "0", TextAlignment = Alignment.Center, Y = Pos.Center(), Width = Dim.Fill() };
        topFrame.Add(topLabel);

        var centerLabel = new Label { Text = "+", TextAlignment = Alignment.Center, X = Pos.Center(), Y = Pos.Bottom(topFrame) + 1 };

        //left
        var leftFrame = new FrameView { Y = Pos.Bottom(topFrame), X = Pos.Left(centerLabel) - 20, Width = 15, Height = Dim.Auto() };
        var leftLabel = new Label { Text = "0", TextAlignment = Alignment.Center, Y = Pos.Center(), Width = Dim.Fill() };
        leftFrame.Add(leftLabel);


        //right
        var rightFrame = new FrameView { Y = Pos.Bottom(topFrame), X = Pos.Right(centerLabel) + 5, Width = 15, Height = Dim.Auto() };
        var rightLabel = new Label { Text = "0", TextAlignment = Alignment.Center, Y = Pos.Center(), Width = Dim.Fill() };
        rightFrame.Add(rightLabel);

        //bottom
        var bottomFrame = new FrameView { Y = Pos.Bottom(centerLabel) + 1, X = Pos.Center(), Width = 15, Height = Dim.Auto() };
        var bottomLabel = new Label { Text = "0", TextAlignment = Alignment.Center, Y = Pos.Center(), Width = Dim.Fill() };
        bottomFrame.Add(bottomLabel);

        var splitter2 = new Line() { Y = Pos.Bottom(bottomFrame) };

        Add(centerLabel, topFrame, leftFrame, rightFrame, bottomFrame, splitter2);


        var hourFrame = new FrameView { Title = "Hours", Width = Dim.Percent(33), Height = Dim.Auto(), Y = Pos.Bottom(splitter2) };
        var hoursLabel = new Label { Text = "0", TextAlignment = Alignment.Center, Width = Dim.Fill() };
        hourFrame.Add(hoursLabel);

        var minutesFrame = new FrameView { Title = "Minutes", Width = Dim.Percent(33), Height = Dim.Auto(), X = Pos.Right(hourFrame), Y = Pos.Bottom(splitter2) };
        var minutesLabel = new Label { Text = "0", TextAlignment = Alignment.Center, Width = Dim.Fill() };
        minutesFrame.Add(minutesLabel);

        var secondsFrame = new FrameView { Title = "Seconds", Width = Dim.Percent(33), Height = Dim.Auto(), X = Pos.Right(minutesFrame), Y = Pos.Bottom(splitter2) };
        var secondsLabel = new Label { Text = "0", TextAlignment = Alignment.Center, Width = Dim.Fill() };
        secondsFrame.Add(secondsLabel);

        var splitter3 = new Line() { Y = Pos.Bottom(hourFrame) };

        Add(hourFrame, minutesFrame, secondsFrame, splitter3);


        var grabbedFrame = new FrameView { Title = "Grabbed", Width = Dim.Percent(33), Height = Dim.Auto(), Y = Pos.Bottom(splitter3) };
        var grabbedLabel = new Label { Text = "0", TextAlignment = Alignment.Center, Width = Dim.Fill() };
        grabbedFrame.Add(grabbedLabel);

        var angleFrame = new FrameView { Title = "Angle", Width = Dim.Percent(33), Height = Dim.Auto(), X = Pos.Right(grabbedFrame), Y = Pos.Bottom(splitter3) };
        var angleLabel = new Label { Text = "0", TextAlignment = Alignment.Center, Width = Dim.Fill() };
        angleFrame.Add(angleLabel);

        var stretchFrame = new FrameView { Title = "Stretch", Width = Dim.Percent(33), Height = Dim.Auto(), X = Pos.Right(angleFrame), Y = Pos.Bottom(splitter3) };
        var stretchLabel = new Label { Text = "0", TextAlignment = Alignment.Center, Width = Dim.Fill() };
        stretchFrame.Add(stretchLabel);

        Add(grabbedFrame, angleFrame, stretchFrame);


        controller.ControllerData.OnHorizontalOffsetChanged += (s, a) => horizontalLabel.Text = a.ToString();
        controller.ControllerData.OnVerticalOffsetChanged += (s, a) => verticalLabel.Text = a.ToString();
        controller.ControllerData.OnHorizontalLookChanged += (s, a) => turningLabel.Text = a.ToString();
        controller.ControllerData.OnShouldRunChanged += (s, a) => runningLabel.Text = a.ToString();

        controller.ControllerData.OnHoursChanged += (s, a) => hoursLabel.Text = a.ToString();
        controller.ControllerData.OnMinutesChanged += (s, a) => minutesLabel.Text = a.ToString();
        controller.ControllerData.OnSecondsChanged += (s, a) => secondsLabel.Text = a.ToString();

        controller.ParameterChanged += (s, a) => {
            switch (a.Name) {
                case "Leash_Front":
                    topLabel.Text = a.GetValue<float>().ToString();
                    return;
                case "Leash_Back":
                    bottomLabel.Text = a.GetValue<float>().ToString();
                    return;
                case "Leash_Left":
                    leftLabel.Text = a.GetValue<float>().ToString();
                    return;
                case "Leash_Right":
                    rightLabel.Text = a.GetValue<float>().ToString();
                    return;
                case "Leash_IsGrabbed":
                    grabbedLabel.Text = a.GetValue<bool>().ToString();
                    return;
                case "Leash_Angle":
                    angleLabel.Text = a.GetValue<float>().ToString();
                    return;
                case "Leash_Stretch":
                    stretchLabel.Text = a.GetValue<float>().ToString();
                    return;
                default:
                    return;
            }
        };
    }
}
