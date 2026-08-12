using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VRChatOSCClient;
using VRChatOSCClient.OSCConnections;

namespace LDS.Core;

public sealed class VrChatBackgroundService : BackgroundService
{
    private readonly IVrChatClient _client;
    private readonly BackgroundServiceController _controller;
    private readonly ILogger<VrChatBackgroundService> _logger;

    private LeashTaskController? _leashTaskController;
    private CounterTaskController? _counterTaskController;

    public VrChatBackgroundService(IVrChatClient client, BackgroundServiceController controller, ILogger<VrChatBackgroundService> logger) {
        _client = client;
        _logger = logger;
        _controller = controller;

        _controller.StopInvoked += StopInvoked;
        _controller.ToggleUnityInvoked += ToggleUnityInvoked;

        _controller.Settings.OnGlobalEnableLeashChanged += ToggleLeash;
        _controller.Settings.OnGlobalEnableCounterChanged += ToggleCounter;

        _client.OnParameterReceived += ParameterReceived;
        _client.OnVrChatClientFound += ClientFound;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
        _client.Start(new(), stoppingToken);
        return Task.CompletedTask;
    }

    private async void ToggleUnityInvoked(object? sender, EventArgs e) {
        await _client.StopAsync();
        _leashTaskController?.Stop();
        _counterTaskController?.Stop();

        _controller.ConnectionStatus.IsConnected = false;
        _controller.ConnectionStatus.SendPort = 0;
        _controller.ConnectionStatus.ReceivePort = 0;

        if(_controller.ConnectionStatus.IsUnityConnected) {
            _controller.ConnectionStatus.IsUnityConnected = false;
            _client.Start(new(), CancellationToken.None);
        }
        else {
            _controller.ConnectionStatus.IsUnityConnected = true;
            await _client.Start(new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 9000), new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 9001), new(), CancellationToken.None);
        }
    }

    private async void StopInvoked(object? sender, EventArgs e) {
        await _client.StopAsync();
        _leashTaskController?.Stop();

        _controller.ConnectionStatus.IsConnected = false;
        _controller.ConnectionStatus.SendPort = 0;
        _controller.ConnectionStatus.ReceivePort = 0;

        await base.StopAsync(CancellationToken.None);
    }

    private void ToggleLeash(object? sender, bool isEnabled) {
        if(isEnabled) {
            if(_leashTaskController is not null) {
                return;
            }

            _leashTaskController = new LeashTaskController(_controller, _client, _logger);
            _leashTaskController.Start();
        }
        else {
            if(_leashTaskController is null) {
                return;
            }

            _leashTaskController.Stop();
            _leashTaskController = null;
        }
    }

    private void ToggleCounter(object? sender, bool isEnabled) {
        if (isEnabled) {
            if (_counterTaskController is not null) {
                return;
            }

            _counterTaskController = new CounterTaskController(_controller, _client, _logger);
            _counterTaskController.Start();
        }
        else {
            if (_counterTaskController is null) {
                return;
            }

            _counterTaskController.Stop();
            _counterTaskController = null;
        }
    }

    private Task ClientFound(VRChatOSCClient.OSCQuery.VrChatConnectionInfo connectionInfo, CancellationToken cancellationToken) {
        _controller.ConnectionStatus.IsConnected = true;
        _controller.ConnectionStatus.SendPort = connectionInfo.SendEndpoint.Port;
        _controller.ConnectionStatus.ReceivePort = connectionInfo.ReceiveEndpoint.Port;

        if (_controller.Settings.GlobalEnableLeash) {
            _leashTaskController = new LeashTaskController(_controller, _client, _logger);
        }

        if(_controller.Settings.GlobalEnableCounter) {
            _counterTaskController = new CounterTaskController(_controller, _client, _logger);
        }

        return Task.CompletedTask;
    }

    private Task ParameterReceived(ParameterChangedMessage message, CancellationToken token) {
        Parameter parameter = message switch {
            { Value: bool value } => new BoolParameter(message.Name, message.Address, value),
            { Value: float value } => new FloatParameter(message.Name, message.Address, value),
            { Value: int value } => new FloatParameter(message.Name, message.Address, value),
            _ => throw new NotSupportedException($"Unknown type for {message.Address} with type {message.Value.GetType().Name}"),
        };

        _controller.Parameters[parameter.Name] = parameter;
        _controller.InvokeParameterChanged(parameter.Name);
        return Task.CompletedTask;
    }
}

public class LeashTaskController(BackgroundServiceController controller, IVrChatClient client, ILogger<VrChatBackgroundService> logger)
{
    private readonly CancellationTokenSource _cts = new();

    private Task _task = Task.CompletedTask;
    private int _retryCount = 0;

    public void Start()
    {
        _task = LeashTask();
    }

    public async Task Stop()
    {
        await _cts.CancelAsync();
        await _task;
    }

    private async Task LeashTask() {
        try {
            using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(50));

            var previousData = new BaseLeashCalculator.MovementData();
            while (await timer.WaitForNextTickAsync(_cts.Token)) {
                if (!controller.Thresholds.LeashEnabled) {
                    continue;
                }

                var parameters = new ParameterMapping(controller);

                if (ShouldReset(parameters)) {
                    await ResetLeash();
                    continue;
                }

                var currentData = controller.Settings.CalculatorType switch {
                    MovementCalculatorType.Location => PositionLeashCalculator.GetLeashData(controller, parameters, ref previousData),
                    MovementCalculatorType.Stretch => StretchLeashCalculator.GetLeashData(controller, parameters, ref previousData),
                    MovementCalculatorType.Combined => StretchPositionLeashCalculator.GetLeashData(controller, parameters, ref previousData),
                    _ => PositionLeashCalculator.GetLeashData(controller, parameters, ref previousData),
                };

                if (currentData.Equals(previousData)) {
                    continue;
                }

                previousData = currentData;
                client.Send(new Message("/input/Vertical", [currentData.VerticalOffset]));
                client.Send(new Message("/input/Horizontal", [currentData.HorizontalOffset]));
                client.Send(new Message("/input/LookHorizontal", [currentData.HorizontalLook]));
                client.Send(new Message("/input/Run", [currentData.ShouldRun]));
            }
        }
        catch (TaskCanceledException) { }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            logger.LogError(ex, "An error occured in the LeashTask of the background updater");
            throw;
        }
    }

    /// <summary>
    /// Calculates if the leash colliders are in a null position cause the leash to require a reset.
    /// </summary>
    /// <returns></returns>
    private bool ShouldReset(ParameterMapping parameters) {
        var shouldReset = controller.Settings.EnableToggleOnNullInput && BaseLeashCalculator.IsZeroColliderDistance(parameters);
        if (!shouldReset && _retryCount > 0) {
            _retryCount = 0;
        }

        return shouldReset;
    }

    /// <summary>
    /// Attempts to reset the colliders by toggling the leash off and on up to 3 times.
    /// </summary>
    /// <returns></returns>
    private async Task ResetLeash()
    {
        switch (_retryCount)
        {
            case < 3:
                client.SendParameterChange(ParameterMapping.ENABLED, false);
                await Task.Delay(TimeSpan.FromSeconds(2));
                client.SendParameterChange(ParameterMapping.ENABLED, true);
                await Task.Delay(TimeSpan.FromSeconds(2));
                _retryCount++;
                logger.LogInformation("Leash reset attempt {attempt}", _retryCount);
                break;
            case 3:
                _retryCount++;
                logger.LogInformation("Unable to automatically reset the leash");
                break;
        }
    }
}

public class CounterTaskController(BackgroundServiceController controller, IVrChatClient client, ILogger<VrChatBackgroundService> logger)
{
    private readonly CancellationTokenSource _cts = new();

    private Task _task = Task.CompletedTask;

    public void Start()
    {
        _task = TimerTask();
    }

    public async Task Stop()
    {
        await _cts.CancelAsync();
        await _task;
    }

    /// <summary>
    /// The main timer loop task that will update send data to vrchat.
    /// </summary>
    /// <returns></returns>
    private async Task TimerTask() {
        try {
            using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync(_cts.Token)) {
                var isGrabbed = controller.Parameters.GetValueOrDefault(ParameterMapping.IS_GRABBED)?.GetValue<bool>() ?? false;
                var stretch = controller.Parameters.GetValueOrDefault(ParameterMapping.STRETCH)?.GetValue<float>() ?? 0;

                if (!controller.Thresholds.CounterEnabled || !isGrabbed || stretch < controller.Thresholds.CounterThreshold) {
                    continue;
                }

                var timeSpan = new TimeSpan(controller.ControllerData.Hours, controller.ControllerData.Minutes, controller.ControllerData.Seconds);
                timeSpan = new TimeSpan(timeSpan.Ticks + TimeSpan.TicksPerSecond);

                controller.ControllerData.Hours = timeSpan.Hours;
                controller.ControllerData.Minutes = timeSpan.Minutes;
                controller.ControllerData.Seconds = timeSpan.Seconds;
            }
        }
        catch (TaskCanceledException) { }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            logger.LogError(ex, "Error occured in the TimerTask of the background updater");
        }
    }
}