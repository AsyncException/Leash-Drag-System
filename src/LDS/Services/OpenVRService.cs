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

    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
        _openVR.OnSteamVRFound += OnSteamVRFound;
        _openVR.OnShutdownReceived += OnShutdownReceived;
        _status.PropertyChanged += OnStatusPropertyChanged;
        _openVR.Start();

        _logger.LogDebug("OpenVR Service started");

        return Task.CompletedTask;
    }

    private void OnStatusPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName != nameof(OpenVRStatus.AutoStart) || !_status.IsOpenVRRunning) {
            return;
        }

        _openVR.AutoLaunch = _status.AutoStart;
    }

    private Task OnShutdownReceived(VREvent_t t, CancellationToken token) {
        _logger.LogDebug("OpenVR Shutdown received");
        if (_status.AutoStart) {
            _logger.LogDebug("Starting shutdown because autostart is turned on");
            WeakReferenceMessenger.Default.Send<InvokeExitMessage>(new(new("OpenVR AutoStart Service")));
        }

        _status.IsOpenVRRunning = false;
        return Task.CompletedTask;
    }

    private Task OnSteamVRFound(CancellationToken arg) {
        _status.IsOpenVRRunning = true;
        _status.AutoStart = _openVR.AutoLaunch;

        _logger.LogDebug("OpenVR Client found");

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