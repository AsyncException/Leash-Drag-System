using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using LDS.Services;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LDS;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly IAppResizeService _appResizeService = Ioc.Default.GetRequiredService<IAppResizeService>()!;
    private readonly IBackDropController _backDropController = Ioc.Default.GetRequiredService<IBackDropController>()!;

    public MainWindow() {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        _backDropController.SetAcrylicBackdrop(this);
        _appResizeService.SetWindow(this);

        Closed += CleanUp;
    }

    public void CleanUp(object sender, WindowEventArgs args) => WeakReferenceMessenger.Default.Send<InvokeExitMessage>(new(new("MainWindow")));
}
