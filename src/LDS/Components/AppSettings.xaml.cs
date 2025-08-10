using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using LDS.Messages;
using LDS.Models;
using LDS.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LDS;
public sealed partial class AppSettings : UserControl
{
    public ApplicationSettings Settings { get; } = Ioc.Default.GetRequiredService<ApplicationSettings>();
    public AppSettings() => InitializeComponent();

    [RelayCommand] void ResetReceivePort() => Settings.ReceivePort = ApplicationSettings.DEFAULT_RECEIVE_PORT;
    [RelayCommand] void ResetSendPort() => Settings.SendPort = ApplicationSettings.DEFAULT_SEND_PORT;

    [RelayCommand]
    async Task RestartClient() {
        RestartButton.IsEnabled = false;

        // Show spinner
        RestartProgressRing.Visibility = Visibility.Visible;
        RestartProgressRing.IsActive = true;

        bool success = StrongReferenceMessenger.Default.Send<ReconnectClientMessage>();

        // Hide spinner
        RestartProgressRing.IsActive = false;
        RestartProgressRing.Visibility = Visibility.Collapsed;

        // Show icon
        if (success) {
            RestartCheckIcon.Visibility = Visibility.Visible;
            await Task.Delay(1000);
            RestartCheckIcon.Visibility = Visibility.Collapsed;
        }
        else {
            RestartFailIcon.Visibility = Visibility.Visible;
            await Task.Delay(1000);
            RestartFailIcon.Visibility = Visibility.Collapsed;
        }

        RestartButton.IsEnabled = true;
    }

    [RelayCommand] public static void CloseSettings() => StrongReferenceMessenger.Default.Send<CloseSettings>();

    [RelayCommand]
    private static void OpenLogDirectory() {
        Process.Start(new ProcessStartInfo {
            FileName = StorageLocation.GetLogPath(),
            UseShellExecute = true
        });
    }
}
