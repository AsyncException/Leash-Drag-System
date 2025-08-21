using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using System;

namespace LDS;

//TODO: This whole mechanism needs to be rewritten. Looking into making a custom serilog.

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DebugWindow : Window
{

    public DebugWindow() {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
    }

    public void OnClose() {
    }

    private void ScrollIntoView(object? sender, (string latestMessage, string LogName)context) {
        (context.LogName switch {
            "AppLogs" => AppLogList,
            "ReceiveLogs" => ReceiveLogList,
            "SendLogs" => SendLogList,
            _ => throw new ArgumentException("Invalid log name", nameof(context))
        }).ScrollIntoView(context.latestMessage);
    }
}

public class DebugLogMessage(Guid id, string message) {
    public Guid Id { get; } = id;
    public string Message { get; } = message;
}