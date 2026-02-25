using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using LDS.Core;
using LDS.Models;
using LDS.UI.Models;
using Microsoft.UI.Xaml.Controls;

namespace LDS;

public sealed partial class ValueStatistics : UserControl
{
    public MovementData MovementData { get; } = Ioc.Default.GetRequiredService<MovementData>();
    public OSCParameters OscParameters { get; } = Ioc.Default.GetRequiredService<OSCParameters>();
    public CounterDataModel TimerStorage { get; } = Ioc.Default.GetRequiredService<CounterDataModel>();
    public ThresholdsDataModel ThresholdsData { get; } = Ioc.Default.GetRequiredService<ThresholdsDataModel>();
    public ApplicationSettingsDataModel ApplicationSettings { get; } = Ioc.Default.GetRequiredService<ApplicationSettingsDataModel>();

    public ValueStatistics() => InitializeComponent();

    [RelayCommand] public void EmergencyStop() {
        ThresholdsData.LeashEnabled = false;
        ThresholdsData.CounterEnabled = false;
    }
}
