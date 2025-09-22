using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using System;
using LDS.Models;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using LDS.Messages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LDS.Components;
public sealed partial class MainContent : UserControl {
    public ConnectionStatus ConnectionStatus { get; } = Ioc.Default.GetRequiredService<ConnectionStatus>();
    public MainContent() => InitializeComponent();


    [RelayCommand]
    public async Task InvokeSettings() {
        AppSettingsDialog content = new();

        ContentDialog contentDialog = new() {
            XamlRoot = XamlRoot,
            Content = content,
            Title = "App Settings",
            CloseButtonText = "Close",
            IsPrimaryButtonEnabled = false,
            IsSecondaryButtonEnabled = false,
            DefaultButton = ContentDialogButton.Close
        };

        _ = await contentDialog.ShowAsync();
    }

    private DebugWindow? _debugWindow;

    [RelayCommand]
    public void ToggleConsole() {
        if (_debugWindow is null) {
            _debugWindow = new DebugWindow();
            _debugWindow.Closed += (s, e) => {
                _debugWindow = null;
            };

            _debugWindow.Activate();
        }
        else {
            _debugWindow.Activate();
        }
    }

    [RelayCommand]
    public async Task StartUnity() {
        UnityProgress.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        bool success = await WeakReferenceMessenger.Default.Send(new ToggleUnityMessage()).Response;
        
        UnityProgress.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

        if (success) {
            UnitySuccess.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            await Task.Delay(2000);
            UnitySuccess.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
        else {
            UnityError.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            await Task.Delay(2000);
            UnityError.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
    }
}