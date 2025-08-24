using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using LDS.Messages;
using LDS.Models;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LDS;
public sealed partial class Settings : UserControl
{
    public ThresholdSettings Thresholds { get; set; } = Ioc.Default.GetRequiredService<ThresholdSettings>();
    public ApplicationSettings ApplicationSettings { get; } = Ioc.Default.GetRequiredService<ApplicationSettings>();

    public Settings() => InitializeComponent();
    [RelayCommand] public static void EmergencyStop() => WeakReferenceMessenger.Default.Send<EmergencyStopMessage>();
}