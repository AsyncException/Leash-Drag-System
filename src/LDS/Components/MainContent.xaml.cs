using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LDS.Components;
public sealed partial class MainContent : UserControl
{
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

    private DebugWindow? f_debugWindow;

    [RelayCommand]
    public void ToggleConsole() {
        if (f_debugWindow is null) {
            f_debugWindow = new DebugWindow();
            f_debugWindow.Closed += (s, e) => {
                f_debugWindow.OnClose();
                f_debugWindow = null;
            };

            f_debugWindow.Activate();
        }
        else {
            f_debugWindow.Activate();
        }
    }
}
