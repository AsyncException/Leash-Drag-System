using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using LDS.Messages;
using LDS.Models;
using LDS.Services.VRChatOSC;
using LDS.Utilities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LDS.Services;

public interface IBackgroundLeashUpdater
{
    Task StopProcess();
}

public sealed class BackgroundLeashUpdater : IRecipient<EmergencyStopMessage>, IRecipient<StartLeashUpdater>, IRecipient<StopLeashUpdater>, IBackgroundLeashUpdater
{
    private IVRChatOscClient Client { get; }
    private OSCParameters Leash { get; }
    private ThresholdSettings Thresholds { get; }
    private LeashData LeashData { get; }
    private ApplicationSettings Settings { get; }
    private IDebugLogger DebugLogger { get; }

    private CancellationTokenSource f_cancellationTokenSource = new();

    private Task f_leashUpdateTask;

    public BackgroundLeashUpdater(IVRChatOscClient client, OSCParameters leashContext, ThresholdSettings thresholds, LeashData leashData, ApplicationSettings settings, IDebugLogger debugLogger) {
        (Client, Leash, Thresholds, LeashData, Settings, DebugLogger) = (client, leashContext, thresholds, leashData, settings, debugLogger);

        StrongReferenceMessenger.Default.Register<EmergencyStopMessage>(this);
        StrongReferenceMessenger.Default.Register<StartLeashUpdater>(this);
        StrongReferenceMessenger.Default.Register<StopLeashUpdater>(this);

        if (Settings.GlobalEnableLeash) {
            f_leashUpdateTask = LeashTask(f_cancellationTokenSource.Token);
            Log.Information("Started Leash background service");
            DebugLogger.LogApp("Started Leash background service");
        }
        else {
            f_leashUpdateTask = Task.CompletedTask;
            Log.Information("Leash background service is disabled");
            DebugLogger.LogApp("Leash background service is disabled");
        }
    }

    /// <summary>
    /// Main task that runs in the background to update the leash data and send it to the client.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    private async Task LeashTask(CancellationToken stoppingToken) {
        try {
            using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(50));
            while (await timer.WaitForNextTickAsync(stoppingToken)) {

                if (!Thresholds.LeashEnabled) {
                    continue;
                }

                if (Settings.EnableToggleOnNullInput && await IsLeashReset()) {
                    continue;
                }

                LeashData currentData = LeashCalculator.GetLeashData(Leash, Thresholds, LeashData);

                if (currentData.Equals(LeashData)) {
                    continue;
                }

                LeashData.CopyFrom(currentData);
                Client.SendMovement(LeashData);
            }
        }
        catch (TaskCanceledException) { } // Ignore cancellation exceptions
        catch (OperationCanceledException) { } // Ignore cancellation exceptions
        catch (Exception ex) {
            Log.Error(ex, "An error occurred in the Leash background service");
            throw;
        }
    }


    private int f_resetCounter = 0;
    private async ValueTask<bool> IsLeashReset() {
        if (LeashCalculator.IsZeroColliderDistance(Leash)) {

            if (f_resetCounter < 3) {
                Client.SendParameter(OSCParameters.ENABLED, false);
                await Task.Delay(TimeSpan.FromSeconds(1));
                Client.SendParameter(OSCParameters.ENABLED, true);
                await Task.Delay(TimeSpan.FromSeconds(1));

                f_resetCounter++;

                Log.Information("Leash reset attempt {attempt}", f_resetCounter);
                DebugLogger.LogApp($"Leash reset attempt {f_resetCounter}");
                return false;
            }
            else if (f_resetCounter == 3) {
                Log.Information("Unable to reset leash");
                DebugLogger.LogApp($"Unable to reset leash");
                f_resetCounter++;

                return false;
            }

            return true;
        }
        else {
            f_resetCounter = 0;
            return false;
        }
    }

    /// <summary>
    /// This method is called when an EmergencyStopMessage is received. Cancelling the current movemnt and disabling both the leash and timer.
    /// </summary>
    /// <param name="message"></param>
    async void IRecipient<EmergencyStopMessage>.Receive(EmergencyStopMessage message) {
        Thresholds.LeashEnabled = false;
        Thresholds.TimerEnabled = false;

        await Task.Delay(50);

        LeashData data = new(); // Empty movement
        Client.SendMovement(data);

        Log.Warning("Emergency stop received");
        DebugLogger.LogApp($"Emergency stop received");
    }

    private readonly SemaphoreSlim f_semaphore = new(1, 1);

    /// <summary>
    /// Starts the leash updater task when a StartLeashUpdater message is received.
    /// </summary>
    /// <param name="message"></param>
    void IRecipient<StartLeashUpdater>.Receive(StartLeashUpdater message) {
        f_semaphore.Wait();

        try {
            if(!f_cancellationTokenSource.TryReset()){
                f_cancellationTokenSource = new CancellationTokenSource();
            }

            f_leashUpdateTask = LeashTask(f_cancellationTokenSource.Token);

            Log.Information("Started Leash background service");
            DebugLogger.LogApp("Started Leash background service");
        }
        catch (Exception ex) {
            Log.Error(ex, "Failed to start Leash background service");
            DebugLogger.LogApp("Failed to start Leash background service");
        }
        finally {
            f_semaphore.Release();
        }
    }

    /// <summary>
    /// Stops the leash updater task when a StopLeashUpdater message is received.
    /// </summary>
    /// <param name="message"></param>
    async void IRecipient<StopLeashUpdater>.Receive(StopLeashUpdater message) {
        await f_semaphore.WaitAsync();

        try {
            f_cancellationTokenSource.Cancel();
            await f_leashUpdateTask;
            f_leashUpdateTask = Task.CompletedTask;

            Log.Information("Stopped Leash background service");
            DebugLogger.LogApp("Stopped Leash background service");
        }
        catch (Exception ex) {
            Log.Error(ex, "Failed to stop Leash background service");
            DebugLogger.LogApp("Failed to stop Leash background service");
        }
        finally {
            f_semaphore.Release();
        }
    }

    /// <summary>
    /// Stops the background process and unregisters all messages.
    /// </summary>
    /// <returns></returns>
    public async Task StopProcess() {
        try {
            await f_cancellationTokenSource.CancelAsync();
            await f_leashUpdateTask;

            StrongReferenceMessenger.Default.UnregisterAll(this);

            Log.Information("Shutdown Leash backgound service");
        }
        catch (Exception ex) {
            Log.Error(ex, "Failed to stop Leash background service");
        }
    }
}

file class LeashCalculator
{
    /// <summary>
    /// Calculates the vertical offset based on the difference between the front and back distances, scaled by the stretch factor.
    /// </summary>
    /// <param name="leash">An <see cref="OSCParameters"/> object containing the front and back distances and the stretch factor used in the calculation</param>
    /// <returns>A value between -1.0 and 1,0 representing the vertical offset, where negative values indicate a backwards offset, positive values indicate a forward offset, and 0 represents no offset.</returns>
    public static float GetVerticalOffset(OSCParameters leash) => Math.Clamp((leash.FrontDistance - leash.BackDistance) * leash.Stretch, -1f, 1f);

    /// <summary>
    /// Calculates the horizontal offset based on the difference between the right and left distances, scaled by the stretch factor.
    /// </summary>
    /// <param name="leash">An <see cref="OSCParameters"/> object containing the right and left distances and the stretch factor used in the calculation.</param>
    /// <returns>A value between -1.0 and 1.0 representing the horizontal offset, where negative values indicate a leftward offset, positive values indicate a rightward offset, and 0 represents no offset.</returns>
    public static float GetHorizontalOffset(OSCParameters leash) => Math.Clamp((leash.RightDistance - leash.LeftDistance) * leash.Stretch, -1f, 1f);

    /// <summary>
    /// Calculates the horizontal turning adjustment based on leash parameters, thresholds, and a horizontal offset.
    /// </summary>
    /// <remarks>The method calculates the turning adjustment by applying the turning multiplier to the horizontal offset  and further adjusts the value based on the leash's back distance and the relative distances to the left and right. The result is clamped to the range [-1.0, 1.0].</remarks>
    /// <param name="leash">The leash parameters, including stretch, front distance, right distance, left distance, and back distance.</param>
    /// <param name="thresholds">The threshold settings that define turning behavior, including the turning threshold, goal, and multiplier.</param>
    /// <param name="horizontalOffset">The horizontal offset used to influence the turning adjustment.</param>
    /// <returns>A value between -1.0 and 1.0 representing the horizontal turning adjustment.  Returns 0.0 if the leash stretch is below the turning threshold or the front distance exceeds the turning goal.</returns>
    public static float GetHorizontalLook(OSCParameters leash, ThresholdSettings thresholds, float horizontalOffset) {
        if (leash.Stretch <= thresholds.TurningThreshold || leash.FrontDistance >= thresholds.TurningGoal) {
            return 0f;
        }

        float turn = thresholds.TurningMultiplier * horizontalOffset;

        turn = leash.RightDistance > leash.LeftDistance ? (turn += leash.BackDistance) : (turn -= leash.BackDistance);
        return Math.Clamp(turn, -1f, 1f);
    }

    /// <summary>
    /// Determines whether the system should transition to a running state based on the provided parameters.
    /// </summary>
    /// <param name="leash">The current leash parameters, including the stretch value.</param>
    /// <param name="thresholds">The threshold settings that define the minimum and maximum stretch values for running.</param>
    /// <param name="leashData">Additional leash data, including the current running state.</param>
    /// <returns><see langword="true"/> if the system should transition to a running state; otherwise, <see langword="false"/>.</returns>
    public static bool ShouldRun(OSCParameters leash, ThresholdSettings thresholds, LeashData leashData) {
        bool shouldRun = leash.Stretch > thresholds.RunningMaxThreshold;

        if (leashData.ShouldRun && !shouldRun && leash.Stretch > thresholds.RunningMinThreshold) {
            shouldRun = true;
        }

        return shouldRun;
    }

    /// <summary>
    /// Determines whether the leash is active based on its grabbed state and stretch threshold.
    /// </summary>
    /// <param name="leash">The leash parameters, including its grabbed state and stretch value.</param>
    /// <param name="thresholds">The threshold settings that define the stretch limit for activation.</param>
    /// <returns><see langword="true"/> if the leash is grabbed and its stretch exceeds the specified threshold; otherwise, <see langword="false"/>.</returns>
    public static bool LeashActive(OSCParameters leash, ThresholdSettings thresholds) => leash.IsGrabbed && leash.Stretch > thresholds.StretchThreshold;

    /// <summary>
    /// Gets the leash data based on the current leash parameters, threshold settings, and previous leash data.
    /// </summary>
    /// <param name="leash">The current leash parameters.</param>
    /// <param name="thresholds">The threshold settings.</param>
    /// <param name="previous">Previous set of <see cref="LeashData"/> that this instance should be compared to.</param>
    /// <returns>An instance of <see cref="LeashData"/> that contains the movement data.</returns>
    public static LeashData GetLeashData(OSCParameters leash, ThresholdSettings thresholds, LeashData previous) {
        if (!LeashActive(leash, thresholds)) {
            return new LeashData();
        }

        float verticalOffset = GetVerticalOffset(leash);
        float horizontalOffset = GetHorizontalOffset(leash);
        float horizontalLook = GetHorizontalLook(leash, thresholds, horizontalOffset);
        bool shouldRun = ShouldRun(leash, thresholds, previous);
        return new LeashData { HorizontalLook = horizontalLook, HorizontalOffset = horizontalOffset, VerticalOffset = verticalOffset, ShouldRun = shouldRun };
    }

    /// <summary>
    /// Determines whether the leash has zero collider distances, indicating that it is not currently interacting with any colliders.
    /// </summary>
    /// <param name="leash">The leash parameters, including the distances to the colliders.</param>
    /// <returns><see langword="true" /> if the distances are 0; otherwise, <see langword="false"/>.</returns>
    public static bool IsZeroColliderDistance(OSCParameters leash) => leash.RightDistance == 0 && leash.LeftDistance == 0 && leash.FrontDistance == 0 && leash.BackDistance == 0;
}