using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using LDS.Messages;
using LDS.Models;
using LDS.Services.VRChatOSC;
using LDS.Utilities;

namespace LDS.Services;

public interface IBackgroundTimerUpdater
{
    Task StopProcess();
}

public sealed class BackgroundTimerUpdater : IRecipient<StartTimerUpdater>, IRecipient<StopTimerUpdater>, IBackgroundTimerUpdater
{
    private IVRChatOscClient Client { get; }
    private OSCParameters Leash { get; }
    private ThresholdSettings Thresholds { get; }
    private TimerData TimerData { get; }
    private ApplicationSettings Settings { get; }
    private ITimeProvider TimeProvider { get; }
    private IDebugLogger DebugLogger { get; }

    private readonly CancellationTokenSource f_cancellationTokenSource = new();

    private Task f_timerUpdateTask;

    public BackgroundTimerUpdater(IVRChatOscClient client, OSCParameters leashContext, ThresholdSettings thresholds, TimerData timerData, ITimeProvider timeProvider, ApplicationSettings settings, IDebugLogger debugLogger) {
        (Client, Leash, Thresholds, TimerData, TimeProvider, Settings, DebugLogger) = (client, leashContext, thresholds, timerData, timeProvider, settings, debugLogger);

        StrongReferenceMessenger.Default.Register<StartTimerUpdater>(this);
        StrongReferenceMessenger.Default.Register<StopTimerUpdater>(this);

        if (Settings.GlobalEnableLeash) {
            f_timerUpdateTask = TimerTask(f_cancellationTokenSource.Token);
            Log.Information("Started Counter background service");
            DebugLogger.LogApp("Started Counter background service");
        }
        else {
            f_timerUpdateTask = Task.CompletedTask;
            Log.Information("Counter background service is disabled");
            DebugLogger.LogApp("Counter background service is disabled");
        }
    }

    private async Task TimerTask(CancellationToken stoppingToken) {
        try {
            TimerData.TimeSpan = TimeProvider.GetTime();

            using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync(stoppingToken)) {
                if (!Thresholds.TimerEnabled || !Leash.IsGrabbed || Leash.Stretch < Thresholds.TimerThreshold) {
                    continue;
                }

                TimerData.TimeSpan = new TimeSpan(TimerData.TimeSpan.Ticks + TimeSpan.TicksPerSecond);

                Client.SendParameter(OSCParameters.HOUR, TimerData.TimeSpan.Hours * 0.01f);
                Client.SendParameter(OSCParameters.MINUTE, TimerData.TimeSpan.Minutes * 0.01f);
                Client.SendParameter(OSCParameters.SECOND, TimerData.TimeSpan.Seconds * 0.01f);

                TimeProvider.SaveTime(TimerData.TimeSpan);
            }
        }
        catch (TaskCanceledException) { }
        catch (OperationCanceledException) { }
    }

    void IRecipient<StartTimerUpdater>.Receive(StartTimerUpdater message) {
        try {
            f_cancellationTokenSource.TryReset();
            f_timerUpdateTask = TimerTask(f_cancellationTokenSource.Token);

            Log.Information("Started Counter background service");
            DebugLogger.LogApp("Started Counter background service");
        }
        catch (Exception ex) {
            Log.Error(ex, "Failed to start Counter background service");
            DebugLogger.LogApp("Failed to start Counter background service");
            return;
        }
    }

    async void IRecipient<StopTimerUpdater>.Receive(StopTimerUpdater message) {
        try {
            f_cancellationTokenSource.Cancel();
            await f_timerUpdateTask;
            f_timerUpdateTask = Task.CompletedTask;

            Log.Information("Stopped Counter background service");
            DebugLogger.LogApp("Stopped Counter background service");
        }
        catch (Exception ex) {
            Log.Error(ex, "Failed to stop Counter background service");
            DebugLogger.LogApp("Failed to stop Counter background service");
            return;
        }
    }

    public async Task StopProcess() {
        try {
            await f_cancellationTokenSource.CancelAsync();
            await f_timerUpdateTask;

            StrongReferenceMessenger.Default.UnregisterAll(this);

            Log.Information("Shutdown Counter background service");
            DebugLogger.LogApp("Shutdown Counter background service");
        }
        catch (Exception ex) {
            Log.Error(ex, "Failed to stop Counter background service");
        }
    }
}