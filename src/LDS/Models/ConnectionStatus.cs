using CommunityToolkit.Mvvm.ComponentModel;

namespace LDS.Models;
public partial class ConnectionStatus : ObservableObject
{
    [ObservableProperty] public partial bool IsConnected { get; set; } = false;

    public string SendPortString => SendPort.ToString();
    [ObservableProperty, NotifyPropertyChangedFor(nameof(SendPortString))] public partial int SendPort { get; set; } = 0;

    public string ReceivePortString => ReceivePort.ToString();
    [ObservableProperty, NotifyPropertyChangedFor(nameof(ReceivePortString))] public partial int ReceivePort { get; set; } = 0;
}
