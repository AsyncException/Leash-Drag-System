using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using LDS.Models;
using LDS.LeashSystem;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LDS;

public sealed partial class ValueStatistics : UserControl
{
    public ApplicationSettings ApplicationSettings { get; } = Ioc.Default.GetRequiredService<ApplicationSettings>();
    public OSCParameters OscParameters { get; } = Ioc.Default.GetRequiredService<OSCParameters>();
    public MovementDataViewModel MovementData { get; } = Ioc.Default.GetRequiredService<MovementDataViewModel>();
    public TimerStorage TimerStorage { get; } = Ioc.Default.GetRequiredService<TimerStorage>();

    public ValueStatistics() => InitializeComponent();
}
