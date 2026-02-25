using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using LDS.Models;
using LDS.UI.Messages;
using LDS.UI.Models;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LDS;
public sealed partial class Settings : UserControl
{
    public ThresholdsDataModel Thresholds { get; set; } = Ioc.Default.GetRequiredService<ThresholdsDataModel>();
    public ApplicationSettingsDataModel ApplicationSettings { get; } = Ioc.Default.GetRequiredService<ApplicationSettingsDataModel>();

    public Settings() => InitializeComponent();
    [RelayCommand] public static void EmergencyStop() => WeakReferenceMessenger.Default.Send<EmergencyStopMessage>();
}