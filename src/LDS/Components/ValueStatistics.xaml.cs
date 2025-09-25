using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LDS.LeashSystem;
using LDS.Messages;
using LDS.Models;
using Microsoft.UI.Xaml.Controls;

namespace LDS;

public sealed partial class ValueStatistics : UserControl
{
    public ApplicationSettings ApplicationSettings { get; } = Ioc.Default.GetRequiredService<ApplicationSettings>();
    public OSCParameters OscParameters { get; } = Ioc.Default.GetRequiredService<OSCParameters>();
    public MovementDataViewModel MovementData { get; } = Ioc.Default.GetRequiredService<MovementDataViewModel>();
    public TimerStorage TimerStorage { get; } = Ioc.Default.GetRequiredService<TimerStorage>();

    public ValueStatistics() => InitializeComponent();

    [RelayCommand] public static void EmergencyStop() => WeakReferenceMessenger.Default.Send<EmergencyStopMessage>();
}
