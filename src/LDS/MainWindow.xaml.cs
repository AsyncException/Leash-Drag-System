using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Serilog;
using LDS.Services;
using Microsoft.Extensions.Hosting;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LDS;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly IHostLifetime _lifeTime = Ioc.Default.GetRequiredService<IHostLifetime>();
    private readonly IAppResizeService _appResizeService = Ioc.Default.GetRequiredService<IAppResizeService>()!;
    private readonly IBackDropController _backDropController = Ioc.Default.GetRequiredService<IBackDropController>()!;

    public MainWindow() {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        _backDropController.SetAcrylicBackdrop(this);
        _appResizeService.SetWindow(this);

        Closed += CleanUp;
    }

    public async void CleanUp(object sender, WindowEventArgs args) {
        await _lifeTime.StopAsync(CancellationToken.None);
        await Log.CloseAndFlushAsync();
    }
}
