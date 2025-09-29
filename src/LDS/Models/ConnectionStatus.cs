using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace LDS.Models;
public partial class ConnectionStatus : ObservableObject
{
    [ObservableProperty, NotifyPropertyChangedFor(nameof(ConnectionIcon), nameof(ConnectionColor))]
    public partial bool IsConnected { get; set; } = false;

    public string ConnectionIcon => IsConnected ? "\uE701" : "\uEB5E";
    public SolidColorBrush ConnectionColor => IsConnected ? new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)) : new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

    [ObservableProperty, NotifyPropertyChangedFor(nameof(UnityColor))]
    public partial bool IsUnityMode { get; set; } = false;

    public SolidColorBrush UnityColor => IsUnityMode ? new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)) : new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));


    [ObservableProperty, NotifyPropertyChangedFor(nameof(SendPortString))]
    public partial int SendPort { get; set; } = 0;
    public string SendPortString => SendPort.ToString();


    [ObservableProperty, NotifyPropertyChangedFor(nameof(ReceivePortString))]
    public partial int ReceivePort { get; set; } = 0;
    public string ReceivePortString => ReceivePort.ToString();
}
