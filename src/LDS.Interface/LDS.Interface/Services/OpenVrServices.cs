using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LDS.Interface.Messages;
using Valve.VR;
using VRChatOSCClient.OpenVR;

namespace LDS.Interface.Services;

public class OpenVrServices(ILogger<OpenVrServices> logger, OpenVrWrapper openVR, OpenVRStatus status) : BackgroundService
{
    private readonly OpenVRStatus _status = status;
    private readonly VRChatOSCClient.OpenVR.OpenVrWrapper _openVR = openVR;
    private readonly ILogger<OpenVrServices> _logger = logger;
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
        _openVR.OnSteamVrFound += OnSteamVRFound;
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
            WeakReferenceMessenger.Default.Send(new InvokeExitMessage(new ExitMessageData("OpenVR AutoStart Service")));
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

public partial class OpenVRStatus : ObservableObject {
    [ObservableProperty] public partial bool IsOpenVRRunning { get; set; } = false;
    [ObservableProperty] public partial bool AutoStart { get; set; } = false;
}
