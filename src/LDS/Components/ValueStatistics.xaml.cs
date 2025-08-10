using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using LDS.Models;
using LDS.Services.VRChatOSC;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LDS;

public sealed partial class ValueStatistics : UserControl
{
    public ApplicationSettings ApplicationSettings { get; } = Ioc.Default.GetRequiredService<ApplicationSettings>();
    public OSCParameters OscParameters { get; } = Ioc.Default.GetRequiredService<OSCParameters>();
    private LeashData LeashData { get; } = Ioc.Default.GetRequiredService<LeashData>();
    private TimerData TimerData { get; } = Ioc.Default.GetRequiredService<TimerData>();

    public ValueStatistics() => InitializeComponent();
}
