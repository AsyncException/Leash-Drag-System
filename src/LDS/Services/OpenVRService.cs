using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Valve.VR;
using VRChatOSCClient.OpenVR;

namespace LDS.Services;

internal partial class OpenVRService(ILogger<OpenVRService> logger, OpenVRWrapper openVR, OpenVRStatus status) : BackgroundService
{
    private readonly OpenVRStatus _status = status;
    private readonly OpenVRWrapper _openVR = openVR;
    private readonly ILogger<OpenVRService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _openVR.OnSteamVRFound += OnSteamVRFound;
        _openVR.OnShutdownReceived += OnShutdownReceived;
        _status.PropertyChanged += OnStatusPropertyChanged;

        try {
            await _openVR.StartAndWaitAsync(stoppingToken);
        }
        catch (OpenVRException ex) when (ex.Error == Valve.VR.EVRInitError.Init_PathRegistryNotFound) {
            //This most likely means SteamVR isnt installed. This should not crash the app.
            _logger.LogError(ex, "Could not init OpenVR. OpenVRService will not be running");
        }
        catch (OpenVRException ex) {
            _logger.LogCritical(ex, "Could not start OpenVRService");
        }
    }

    private void OnStatusPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName != nameof(OpenVRStatus.AutoStart) || !_status.IsOpenVRRunning) {
            return;
        }

        _openVR.AutoLaunch = _status.AutoStart;
    }

    private Task OnShutdownReceived(VREvent_t t, CancellationToken token) {
        if (_status.AutoStart) {
            WeakReferenceMessenger.Default.Send<InvokeExitMessage>();
        }

        _status.IsOpenVRRunning = false;
        return Task.CompletedTask;
    }

    private Task OnSteamVRFound(CancellationToken arg) {
        _status.IsOpenVRRunning = true;
        _status.AutoStart = _openVR.AutoLaunch;
        return Task.CompletedTask;
    }
}

/// <summary>
/// A class used to communicate with the OpenVR background service.
/// </summary>
public partial class OpenVRStatus : ObservableObject {
    [ObservableProperty] public partial bool IsOpenVRRunning { get; set; } = false;
    [ObservableProperty] public partial bool AutoStart { get; set; } = false;
}