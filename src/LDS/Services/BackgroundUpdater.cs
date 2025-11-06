using CommunityToolkit.Mvvm.Messaging;
using LDS.LeashSystem;
using LDS.Messages;
using LDS.Models;
using LDS.TimerSystem;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VRChatOSCClient;
using VRChatOSCClient.OSCConnections;
using VRChatOSCClient.OSCQuery;

namespace LDS.Services;

internal partial class BackgroundUpdater : BackgroundService, IRecipient<EmergencyStopMessage>, IRecipient<StartLeashUpdater>, IRecipient<StopLeashUpdater>, IRecipient<StartTimerUpdater>, IRecipient<StopTimerUpdater>, IRecipient<ToggleUnityMessage>
{
    private readonly DispatcherQueue _dispatcherQueue;

    private readonly ILogger _logger;
    private readonly IVRChatClient _client;
    private readonly ITimeDataProvider _timeProvider;

    private OSCParameters Parameters { get; init; }
    private TimerStorage TimerData { get; init; }
    private ThresholdSettings ThresholdSettings { get; init; }
    private ConnectionStatus ConnectionStatus { get; init; }
    private ApplicationSettings ApplicationSettings { get; init; }
    private MovementDataViewModel MovementViewModel { get; init; }

    private CancellationTokenSource _leashCts = new();
    private CancellationTokenSource _timerCts = new();

    private int _retryCount = 0;
    private Task _leashTask = Task.CompletedTask;
    private Task _timerTask = Task.CompletedTask;
    private readonly SemaphoreSlim _stopStartSemaphore = new(1, 1);

    public BackgroundUpdater(
        IVRChatClient client,
        ILogger<BackgroundUpdater> logger,
        ITimeDataProvider timeProvider,
        DispatcherQueue dispatcherQueue,

        OSCParameters parameters,
        TimerStorage timerData,
        MovementDataViewModel movementViewModel,

        ApplicationSettings applicationSettings,
        ThresholdSettings thresholdSettings,
        ConnectionStatus connectionStatus
        ) {
        _client = client;
        _logger = logger;
        _timeProvider = timeProvider;
        _dispatcherQueue = dispatcherQueue;

        Parameters = parameters;
        TimerData = timerData;
        MovementViewModel = movementViewModel;

        ApplicationSettings = applicationSettings;
        ThresholdSettings = thresholdSettings;
        ConnectionStatus = connectionStatus;
        _client.OnParameterReceived += UpdateParameters;
        _client.OnAvatarChanged += UpdateParameters;
        _client.OnVRChatClientFound += ClientConnected;

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    private Task ClientConnected(VRChatConnectionInfo info, CancellationToken token) {
        _logger.LogInformation("VRChat client connected on Send Port {sendPort} and Receive Port {receivePort}", info.SendEndpoint.Port, info.ReceiveEndpoint.Port);

        _dispatcherQueue.TryEnqueue(() => {
            ConnectionStatus.IsConnected = true;
            ConnectionStatus.SendPort = info.SendEndpoint.Port;
            ConnectionStatus.ReceivePort = info.ReceiveEndpoint.Port;
        });

        if (_leashCts.IsCancellationRequested) {
            _leashCts = new CancellationTokenSource();
        }

        if (_timerCts.IsCancellationRequested) {
            _timerCts = new CancellationTokenSource();
        }

        if (ApplicationSettings.GlobalEnableLeash && !_leashTask.IsCanceled) {
            _leashTask = LeashTask();
        }

        if (ApplicationSettings.GlobalEnableLeash && !_leashTask.IsCanceled) {
            _timerTask = TimerTask();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Main entry point of the background task. Initializes the  <see cref="IVRChatClient"/> and starts the loops if theyre enabled.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
        MessageFilter filter = new();
        filter.SetParameterPattern("^([Ll]eash|[Tt]imer)");
        _client.Start(filter, stoppingToken);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Exit point of the background task. Disconnects the <see cref="IVRChatClient"/> and disables the loops.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task StopAsync(CancellationToken cancellationToken) {
        await _client.StopAsync(cancellationToken).ConfigureAwait(false);
        await StopLeash().ConfigureAwait(false);
        await StopTimer().ConfigureAwait(false);

        WeakReferenceMessenger.Default.UnregisterAll(this);
        _logger.LogInformation("Shutdown background task");

        await base.StopAsync(cancellationToken);
    }

    /// <summary>
    /// Updates the parameters in the <see cref="OSCParameters"/> once theyre received from the <see cref="IVRChatClient"/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private Task UpdateParameters(ParameterChangedMessage message, CancellationToken token) {
        Parameters.UpdateParameter(_dispatcherQueue, message);
        return Task.CompletedTask;
    }

    private Task UpdateParameters(Dictionary<string, object?> parameters, CancellationToken token) {
        Parameters.UpdateParameters(_dispatcherQueue, parameters);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disables the current client and switches to a new Unity client on localhost ports 9000 and 9001.
    /// </summary>
    /// <param name="message"></param>
    void IRecipient<ToggleUnityMessage>.Receive(ToggleUnityMessage message) {
        if (ConnectionStatus.IsUnityMode) {
            _logger.LogInformation("Received StartUnityMessage, restarting restarting with VRChat client");

            _dispatcherQueue.TryEnqueue(() => {
                ConnectionStatus.IsConnected = false;
                ConnectionStatus.SendPort = 0;
                ConnectionStatus.ReceivePort = 0;
            });
        }

        ConnectionStatus.IsUnityMode = !ConnectionStatus.IsUnityMode;

        message.Reply(Task.Run<bool>(async () => {
            try {
                await _client.StopAsync();
                await StopLeash();
                await StopTimer();

                MessageFilter filter = new();
                filter.SetParameterPattern("^([Ll]eash|[Tt]imer)");

                if (ConnectionStatus.IsUnityMode) {
                    await _client.Start(new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 9000), new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 9001), filter, CancellationToken.None);
                }
                else {
                    _client.Start(filter, CancellationToken.None);
                }

                return true;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to switch to VRChat client");
                return false;
            }
        }));

        
    }

    #region Leash stuff

    /// <summary>
    /// The main leash loop task that will update send data to vrchat.
    /// </summary>
    /// <returns></returns>
    private async Task LeashTask() {
        try {
            using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(50));

            MovementData previousData = new();
            while(await timer.WaitForNextTickAsync(_leashCts.Token)) {
                if(!ThresholdSettings.LeashEnabled) {
                    continue;
                }

                if (ShouldReset()) {
                    await ResetLeash();
                    continue;
                }

                MovementData currentData = ApplicationSettings.CalculatorType switch {
                    MovementCalculatorType.Location => PositionLeashCalculator.GetLeashData(Parameters, ThresholdSettings, ref previousData),
                    MovementCalculatorType.Stretch => StretchLeashCalculator.GetLeashData(Parameters, ThresholdSettings, ref previousData),
                    MovementCalculatorType.Combined => StretchPositionLeashCalculator.GetLeashData(Parameters, ThresholdSettings, ref previousData),
                    _ => PositionLeashCalculator.GetLeashData(Parameters, ThresholdSettings, ref previousData),
                };

                //Send latest data to UI. Pass currentData without ref so struct is copied.
                _dispatcherQueue.TryEnqueue(() => MovementViewModel.RenewData(currentData));

                if (currentData.Equals(previousData)) {
                    continue;
                }
                
                previousData = currentData;
                _client.SendMovement(ref previousData);
            }
        }
        catch (TaskCanceledException) { }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            _logger.LogError(ex, "An error occured in the LeashTask of the background updater");
            throw;
        }
    }

    /// <summary>
    /// Calculates if the leash colliders are in a null position cause the leash to require a reset.
    /// </summary>
    /// <returns></returns>
    private bool ShouldReset() {
        bool shouldReset = ApplicationSettings.EnableToggleOnNullInput && BaseLeashCalculator.IsZeroColliderDistance(Parameters);
        if(!shouldReset && _retryCount > 0) {
            _retryCount = 0;
        }

        return shouldReset;
    }

    /// <summary>
    /// Attempts to reset the colliders by toggling the leash off and on up to 3 times.
    /// </summary>
    /// <returns></returns>
    private async Task ResetLeash() {
        if(_retryCount < 3) {
            _client.SendParameterChange(OSCParameters.ENABLED, false);
            await Task.Delay(TimeSpan.FromSeconds(2));
            _client.SendParameterChange(OSCParameters.ENABLED, true);
            await Task.Delay(TimeSpan.FromSeconds(2));
            _retryCount++;
            _logger.LogInformation("Leash reset attempt {attempt}", _retryCount);
        }
        else if(_retryCount == 3) {
            _retryCount++;
            _logger.LogInformation("Unable to automatically reset the leash");
        }
    }

    /// <summary>
    /// Attempts to start the leash task after it has been stopped or not started.
    /// </summary>
    private void RestartLeash() {
        _stopStartSemaphore.Wait();

        try {
            if (!_leashCts.TryReset()) {
                _leashCts = new CancellationTokenSource();
            }

            _leashTask = LeashTask();

            _logger.LogInformation("Started Leash background service");
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to start Leash background service");
        }
        finally {
            _stopStartSemaphore.Release();
        }
    }

    /// <summary>
    /// Attempts to stop the leash task.
    /// </summary>
    /// <returns></returns>
    private async Task StopLeash() {
        await _stopStartSemaphore.WaitAsync();

        try {
            await _leashCts.CancelAsync();
            await _leashTask;

            _leashTask = Task.CompletedTask;

            _logger.LogInformation("Stopped Leash background service");
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to stop Leash background service");
        }
        finally {
            _stopStartSemaphore.Release();
        }
    }

    /// <summary>
    /// This method is called when an EmergencyStopMessage is received. Cancelling the current movemnt and disabling both the leash and timer.
    /// </summary>
    /// <param name="message"></param>
    async void IRecipient<EmergencyStopMessage>.Receive(EmergencyStopMessage message) {
        ThresholdSettings.LeashEnabled = false;
        ThresholdSettings.TimerEnabled = false;

        await Task.Delay(50);

        _client.SendMovement(0, 0, 0, false);
        _logger.LogError("Emergency stop received");
    }

    /// <summary>
    /// Starts the leash updater task when a StartLeashUpdater message is received.
    /// </summary>
    /// <param name="message"></param>
    void IRecipient<StartLeashUpdater>.Receive(StartLeashUpdater message) => RestartLeash();

    /// <summary>
    /// Stops the leash updater task when a StopLeashUpdater message is received.
    /// </summary>
    /// <param name="message"></param>
    async void IRecipient<StopLeashUpdater>.Receive(StopLeashUpdater message) => await StopLeash().ConfigureAwait(false);
    #endregion

    #region counter stuff

    /// <summary>
    /// The main timer loop task that will update send data to vrchat.
    /// </summary>
    /// <returns></returns>
    private async Task TimerTask() {
        try {
            using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync(_timerCts.Token)) {
                if (!ThresholdSettings.TimerEnabled || !Parameters.IsGrabbed || Parameters.Stretch < ThresholdSettings.TimerThreshold) {
                    continue;
                }

                _dispatcherQueue.TryEnqueue(() => {
                    TimerData.FromTimeSpan(new(TimerData.GetTimeSpan().Ticks + TimeSpan.TicksPerSecond)); //Add a second to the timer
                });

                _timeProvider.SaveTime(TimerData);

                _client.SendTime(TimerData);
            }
        }
        catch(TaskCanceledException) { }
        catch(OperationCanceledException) { }
        catch (Exception ex) {
            _logger.LogError(ex, "Error occured in the TimerTask of the background updater");
        }
    }

    /// <summary>
    /// Attempts to start the timer task after it has been stopped or not started.
    /// </summary>
    public void RestartTimer() {
        _stopStartSemaphore.Wait();

        try {
            if (!_timerCts.TryReset()) {
                _timerCts = new CancellationTokenSource();
            }

            _timerTask = TimerTask();

            _logger.LogInformation("Started Timer background service");
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to start Timer background service");
        }
        finally {
            _stopStartSemaphore.Release();
        }
    }

    /// <summary>
    /// Attempts to stop the leash task.
    /// </summary>
    /// <returns></returns>
    private async Task StopTimer() {
        await _stopStartSemaphore.WaitAsync();

        try {
            await _timerCts.CancelAsync();
            await _timerTask;

            _timerTask = Task.CompletedTask;

            _logger.LogInformation("Stopped Timer background service");
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to stop Timer background service");
        }
        finally {
            _stopStartSemaphore.Release();
        }
    }

    /// <summary>
    /// Starts the timer updater task when a StartTimerUpdater message is received.
    /// </summary>
    /// <param name="message"></param>
    void IRecipient<StartTimerUpdater>.Receive(StartTimerUpdater message) => RestartTimer();

    /// <summary>
    /// Stops the timer updater task when a StopTimerUpdater message is received.
    /// </summary>
    /// <param name="message"></param>
    async void IRecipient<StopTimerUpdater>.Receive(StopTimerUpdater message) => await StopTimer().ConfigureAwait(false);
    #endregion
}
