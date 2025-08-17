using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LDS.Messages;
using LDS.Models;
using LDS.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LDS.Components;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AppSettingsDialog : Page
{
    public ApplicationSettings Settings { get; } = Ioc.Default.GetRequiredService<ApplicationSettings>();

    public AppSettingsDialog() => InitializeComponent();

    [RelayCommand] 
    public void ResetReceivePort() => Settings.ReceivePort = ApplicationSettings.DEFAULT_RECEIVE_PORT;
    
    [RelayCommand] 
    public void ResetSendPort() => Settings.SendPort = ApplicationSettings.DEFAULT_SEND_PORT;

    [RelayCommand]
    public async Task RestartClient() {
        RestartButton.IsEnabled = false;

        // Show spinner
        RestartProgressRing.Visibility = Visibility.Visible;
        RestartProgressRing.IsActive = true;

        bool success = false;
        try {
             success = StrongReferenceMessenger.Default.Send<ReconnectClientMessage>();
        }
        catch {
            success = false;
        }

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

    [RelayCommand]
    public static void OpenLogDirectory() {
        Process.Start(new ProcessStartInfo {
            FileName = StorageLocation.GetLogPath(),
            UseShellExecute = true
        });
    }

}