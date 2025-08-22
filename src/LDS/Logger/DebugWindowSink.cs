using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using System;
using System.Collections.ObjectModel;

namespace LDS.Logger;
internal class DebugWindowSink(DebugLoggingStore loggingStore) : ILogEventSink
{
    private readonly DebugLoggingStore _loggingStore = loggingStore;

    public void Emit(LogEvent logEvent) {
        if(LogEventLevel.Verbose == logEvent.Level) {
            return;
        }

        LogEntry entry = new() {
            Timestamp = logEvent.Timestamp.ToString("s"),
            Level = logEvent.Level switch {
                LogEventLevel.Verbose => "VRB",
                LogEventLevel.Debug => "DBG",
                LogEventLevel.Information => "INF",
                LogEventLevel.Warning => "WRN",
                LogEventLevel.Error => "ERR",
                LogEventLevel.Fatal => "FTL",
                _ => "UNK"
            },
            Message = logEvent.RenderMessage(),
            Exception = logEvent.Exception?.ToString()
        };

        switch (logEvent.Level) {
            case LogEventLevel.Debug:
                _loggingStore.AddDebugMessage(entry);
                break;
            default:
                _loggingStore.AddLogMessage(entry);
                break;
        }
    }
}

public static class DebugWindowSinkExtensions
{
    public static LoggerConfiguration DebugWindowSink(this LoggerSinkConfiguration loggerConfiguration, DebugLoggingStore loggingStore) {
        DebugWindowSink sink = new(loggingStore);
        return loggerConfiguration.Sink(sink);
    }
}

public class LogEntry
{
    public string Timestamp { get; set; } = "";
    public string Level { get; set; } = "";
    public string Message { get; set; } = "";
    public string? Exception { get; set; }
}

public partial class  DebugLoggingStore : ObservableObject
{
    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    private const int MAX_MESSAGES = 100;

    public event EventHandler<LogEntry>? OnLogMessageAdd;
    public event EventHandler<LogEntry>? OnDebugMessageAdd;

    public ObservableCollection<LogEntry> LogMessages { get; } = [];
    public ObservableCollection<LogEntry> DebugMessages { get; } = [];

    public bool DebuggerAttached { get; set; } = false;

    public void AddLogMessage(LogEntry message) {
        if (DebuggerAttached) {
            _dispatcherQueue.TryEnqueue(Add);
        }
        else {
            Add();
        }

        void Add() {
            if (LogMessages.Count >= MAX_MESSAGES) {
                LogMessages.RemoveAt(0);
            }

            LogMessages.Add(message);
            OnLogMessageAdd?.Invoke(this, message);
        }
    }

    public void AddDebugMessage(LogEntry message) {
        if (DebuggerAttached) {
            _dispatcherQueue.TryEnqueue(Add);
        }
        else {
            Add();
        }

        void Add() {
            if (LogMessages.Count >= MAX_MESSAGES) {
                LogMessages.RemoveAt(0);
            }

            DebugMessages.Add(message);
            OnDebugMessageAdd?.Invoke(this, message);
        }
    }
}