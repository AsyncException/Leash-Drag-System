using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Serilog;
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
    private readonly IBackgroundTimerUpdater f_backgroundTimerUpdater = Ioc.Default.GetRequiredService<IBackgroundTimerUpdater>();

    public MainWindow() {
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
}
