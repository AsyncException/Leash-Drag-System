using LDS.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace LDS.TUI.Views;

internal class ControlView : FrameView
{
    public ControlView() {
        Title = "Controls";
        CanFocus = true;
        HotKey = Key.C.WithAlt;

        var controller = ServiceProvider.Services.GetRequiredService<IController>();

        var unityButton = new Button { Text = "Switch To Unity", Width = Dim.Percent(50) };
        unityButton.Accepting += (s, a) => {
            controller.ToggleUnity();
            a.Handled = true;
        };

        controller.ConnectionStatus.OnIsUnityConnectedChanged += (s, unityConnected) => {
            unityButton.Text = unityConnected ? "Switch to VRChat" : "Switch To Unity";
        };

        var emergencyButton = new Button { Text = "Emergency Stop", X = Pos.Right(unityButton), Width = Dim.Percent(50) };
        emergencyButton.Accepting += (s, a) => {
            controller.EmergencyStop();
            a.Handled = true;
        };

        var connectedFrame = new FrameView { Title = "Status", Y = Pos.Bottom(emergencyButton), Width = Dim.Percent(33), Height = Dim.Auto(), };
        var connectionLabel = new Label { Text = "Disconnected", TextAlignment = Alignment.Center, Width = Dim.Fill() };
        connectedFrame.Add(connectionLabel);

        var sendFrame = new FrameView { Title = "Send", Y = Pos.Bottom(emergencyButton), X = Pos.Right(connectedFrame), Width = Dim.Percent(33), Height = Dim.Auto(), };
        var sendLabel = new Label { Text = "0", TextAlignment = Alignment.Center, Width = Dim.Fill() };
        sendFrame.Add(sendLabel);

        var receiveFrame = new FrameView { Title = "Send", Y = Pos.Bottom(emergencyButton), X = Pos.Right(sendFrame), Width = Dim.Percent(33), Height = Dim.Auto(), };
        var receiveLabel = new Label { Text = "0", TextAlignment = Alignment.Center, Width = Dim.Fill() };
        receiveFrame.Add(receiveLabel);

        controller.ConnectionStatus.OnIsConnectedChanged += (s, connected) => connectionLabel.Text = connected ? "Connected" : "Disconnected";
        controller.ConnectionStatus.OnSendPortChanged += (s, connected) => sendLabel.Text = connected.ToString();
        controller.ConnectionStatus.OnReceivePortChanged += (s, connected) => receiveLabel.Text = connected.ToString();

        Add(unityButton, emergencyButton, connectedFrame, sendFrame, receiveFrame);
    }
}
