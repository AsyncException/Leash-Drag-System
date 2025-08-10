using System;
using System.Collections.ObjectModel;

namespace LDS.Utilities;

public interface IDebugLogger
{
    void LogApp(string message);
    void LogReceive(string message);
    void LogSend(string message);
}

public class DebugLogger(DebugLoggerContext context) : IDebugLogger
{
    private readonly DebugLoggerContext f_context = context;
    public void LogSend(string message) => f_context.AddLogSend(message);
    public void LogReceive(string message) => f_context.AddLogReceive(message);
    public void LogApp(string message) => f_context.AddLogApp(message);
}

public partial class DebugLoggerContext {

    public const int SEND_LOGS_MAX_LINES = 41;
    public ObservableCollection<string> SendLogs { get; set; } = [];

    public const int RECEIVE_LOGS_MAX_LINES = 41;
    public ObservableCollection<string> ReceiveLogs { get; set; } = [];

    public const int APP_LOGS_MAX_LINES = 100;
    public ObservableCollection<string> AppLogs { get; set; } = [];

    private static string AddLog(ObservableCollection<string> log, int maxLines, string message) {
        if (log.Count >= maxLines) {
            log.RemoveAt(0);
        }

        string debugMessage = $"[{DateTime.Now:yyyy-MM-dd hh:mm:ss.fff}] " + message;

        log.Add(debugMessage);
        return debugMessage;
    }

    public void AddLogSend(string message) {
        string newMessage = AddLog(SendLogs, SEND_LOGS_MAX_LINES, message);
        LogReceived?.Invoke(this, (newMessage, nameof(SendLogs)));
    }

    public void AddLogReceive(string message) {
        string newMessage = AddLog(ReceiveLogs, RECEIVE_LOGS_MAX_LINES, message);
        LogReceived?.Invoke(this, (newMessage, nameof(ReceiveLogs)));
    }

    public void AddLogApp(string message) {
        string newMessage = AddLog(AppLogs, APP_LOGS_MAX_LINES, message);
        LogReceived?.Invoke(this, (newMessage, nameof(AppLogs)));
    }

    public event EventHandler<(string, string)>? LogReceived;
}