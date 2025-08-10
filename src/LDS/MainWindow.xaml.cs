using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Serilog;
using LDS.Messages;
using LDS.Services;
using LDS.Services.VRChatOSC;
using LDS.Utilities;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LDS;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly IVRChatOscClient f_vrchatOscClient = Ioc.Default.GetRequiredService<IVRChatOscClient>();
    private readonly IBackDropController f_backDropController = Ioc.Default.GetRequiredService<IBackDropController>()!;
    private readonly IBackgroundLeashUpdater f_leashDirectionService = Ioc.Default.GetRequiredService<IBackgroundLeashUpdater>(); //Just creating these services is enough to run them.
    private readonly IBackgroundTimerUpdater f_backgroundTimerUpdater = Ioc.Default.GetRequiredService<IBackgroundTimerUpdater>(); // Just creating these service is enough to run them.

    public MainWindow() {
        StrongReferenceMessenger.Default.Register<CloseSettings>(this, (r, m) => SettingsPopup.Visibility = Visibility.Collapsed);

        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        f_backDropController.SetAcrylicBackdrop(this);
        _ = f_vrchatOscClient.InitializeClient();

        Closed += CleanUp;
    }

    public async void CleanUp(object sender, WindowEventArgs args) {
        await f_vrchatOscClient.StopClient();

        await f_leashDirectionService.StopProcess();
        await f_backgroundTimerUpdater.StopProcess();

        await Log.CloseAndFlushAsync();
    }

    [RelayCommand]
    public void InvokeSettings() => SettingsPopup.Visibility = SettingsPopup.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

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
