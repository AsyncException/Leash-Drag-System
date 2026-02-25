using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VRChatOSCClient;
using VRChatOSCClient.OSCConnections;

namespace LDS.Core;

public class VRChatBackgroundService : BackgroundService
{
    private readonly IVRChatClient _client;
    private readonly BackgroundServiceController _controller;
    private readonly ILogger<VRChatBackgroundService> _logger;

    private LeashTaskController? _leashTaskController;
    private CounterTaskController? _counterTaskController;

    public VRChatBackgroundService(IVRChatClient client, BackgroundServiceController controller, ILogger<VRChatBackgroundService> logger) {
        _client = client;
        _logger = logger;
        _controller = controller;

        _controller.StopInvoked += StopInvoked;
        _controller.ToggleUnityInvoked += ToggleUnityInvoked;

        _controller.Settings.OnGlobalEnableLeashChanged += ToggleLeash;
        _controller.Settings.OnGlobalEnableCounterChanged += ToggleCounter;

        _client.OnParameterReceived += ParameterReceived;
        _client.OnVRChatClientFound += ClientFound;
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

    private Task ClientFound(VRChatOSCClient.OSCQuery.VRChatConnectionInfo connectionInfo, CancellationToken cancellationToken) {
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

public class LeashTaskController(BackgroundServiceController controller, IVRChatClient client, ILogger<VRChatBackgroundService> logger)
{
    private readonly IVRChatClient _client = client;
    private readonly ILogger<VRChatBackgroundService> _logger = logger;
    private readonly BackgroundServiceController _controller = controller;

    private readonly CancellationTokenSource _cts = new();

    private Task _task = Task.CompletedTask;
    private int _retryCount = 0;

    public void Start() => _task = LeashTask();
    public void Stop() => _cts?.Cancel();

    private async Task LeashTask() {
        try {
            using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(50));

            BaseLeashCalculator.MovementData previousData = new();
            while (await timer.WaitForNextTickAsync(_cts.Token)) {
                if (!_controller.Thresholds.LeashEnabled) {
                    continue;
                }

                ParameterMapping parameters = new(_controller);

                if (ShouldReset(parameters)) {
                    await ResetLeash();
                    continue;
                }

                BaseLeashCalculator.MovementData currentData = _controller.Settings.CalculatorType switch {
                    MovementCalculatorType.Location => PositionLeashCalculator.GetLeashData(_controller, parameters, ref previousData),
                    MovementCalculatorType.Stretch => StretchLeashCalculator.GetLeashData(_controller, parameters, ref previousData),
                    MovementCalculatorType.Combined => StretchPositionLeashCalculator.GetLeashData(_controller, parameters, ref previousData),
                    _ => PositionLeashCalculator.GetLeashData(_controller, parameters, ref previousData),
                };

                if (currentData.Equals(previousData)) {
                    continue;
                }

                previousData = currentData;
                _client.Send(new Message("/input/Vertical", [currentData.VerticalOffset]));
                _client.Send(new Message("/input/Horizontal", [currentData.HorizontalOffset]));
                _client.Send(new Message("/input/LookHorizontal", [currentData.HorizontalLook]));
                _client.Send(new Message("/input/Run", [currentData.ShouldRun]));
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
    private bool ShouldReset(ParameterMapping parameters) {
        bool shouldReset = _controller.Settings.EnableToggleOnNullInput && BaseLeashCalculator.IsZeroColliderDistance(parameters);
        if (!shouldReset && _retryCount > 0) {
            _retryCount = 0;
        }

        return shouldReset;
    }

    /// <summary>
    /// Attempts to reset the colliders by toggling the leash off and on up to 3 times.
    /// </summary>
    /// <returns></returns>
    private async Task ResetLeash() {
        if (_retryCount < 3) {
            _client.SendParameterChange(ParameterMapping.ENABLED, false);
            await Task.Delay(TimeSpan.FromSeconds(2));
            _client.SendParameterChange(ParameterMapping.ENABLED, true);
            await Task.Delay(TimeSpan.FromSeconds(2));
            _retryCount++;
            _logger.LogInformation("Leash reset attempt {attempt}", _retryCount);
        }
        else if (_retryCount == 3) {
            _retryCount++;
            _logger.LogInformation("Unable to automatically reset the leash");
        }
    }
}

public class CounterTaskController(BackgroundServiceController controller, IVRChatClient client, ILogger<VRChatBackgroundService> logger)
{
    private readonly IVRChatClient _client = client;
    private readonly ILogger<VRChatBackgroundService> _logger = logger;
    private readonly BackgroundServiceController _controller = controller;

    private readonly CancellationTokenSource _cts = new();

    private Task _task = Task.CompletedTask;

    public void Start() => _task = TimerTask();
    public void Stop() => _cts?.Cancel();

    /// <summary>
    /// The main timer loop task that will update send data to vrchat.
    /// </summary>
    /// <returns></returns>
    private async Task TimerTask() {
        try {
            using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync(_cts.Token)) {
                var isGrabbed = _controller.Parameters.GetValueOrDefault(ParameterMapping.IS_GRABBED)?.GetValue<bool>() ?? false;
                var stretch = _controller.Parameters.GetValueOrDefault(ParameterMapping.STRETCH)?.GetValue<float>() ?? 0;

                if (!_controller.Thresholds.CounterEnabled || !isGrabbed || stretch < _controller.Thresholds.CounterThreshold) {
                    continue;
                }

                TimeSpan timeSpan = new TimeSpan(_controller.ControllerData.Hours, _controller.ControllerData.Minutes, _controller.ControllerData.Seconds);
                timeSpan = new(timeSpan.Ticks + TimeSpan.TicksPerSecond);

                _controller.ControllerData.Hours = timeSpan.Hours;
                _controller.ControllerData.Minutes = timeSpan.Minutes;
                _controller.ControllerData.Seconds = timeSpan.Seconds;
            }
        }
        catch (TaskCanceledException) { }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            _logger.LogError(ex, "Error occured in the TimerTask of the background updater");
        }
    }
}