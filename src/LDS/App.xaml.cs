using CommunityToolkit.Mvvm.DependencyInjection;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Serilog;
using System;
using LDS.Models;
using LDS.Services;
using Microsoft.Extensions.Hosting;
using VRChatOSCClient;
using System.Net;
using LDS.TimerSystem;
using LDS.Logger;
using LDS.LeashSystem;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LDS;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? Window { get; set; }
    private IHost AppHost { get; }

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App() {
        Environment.SetEnvironmentVariable("MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY", AppContext.BaseDirectory);

        StorageLocation.EnsureAppdataPathExists();

        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        DebugLoggingStore loggingStore = new();
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.DebugWindowSink(loggingStore)
            .CreateLogger();

        builder.Logging.AddSerilog();
        builder.Services.AddSerilog();

        builder.Services.AddVRChatClient("Leash Drag System", IPAddress.Loopback);
        builder.Services.AddHostedService<BackgroundUpdater>();

        builder.Services.AddTransient<IBackDropController, BackDropController>();
        builder.Services.AddTransient<IAppResizeService, AppResizeService>();
        builder.Services.AddSingleton<ILiteDatabase>(s => new LiteDatabase(StorageLocation.GetDatabasePath()));

        builder.Services.AddSingleton<IApplicationSettingsProvider, ApplicationSettingsProvider>();
        builder.Services.AddSingleton<ApplicationSettings>(services => services.GetRequiredService<IApplicationSettingsProvider>().GetSettings());

        builder.Services.AddSingleton<IThresholdSettingsProvider, ThresholdSettingsProvider>();
        builder.Services.AddSingleton<ThresholdSettings>(services => services.GetRequiredService<IThresholdSettingsProvider>().GetSettings());

        builder.Services.AddSingleton<ITimeDataProvider, TimeDataProvider>();
        builder.Services.AddSingleton<TimerStorage>(service => service.GetRequiredService<ITimeDataProvider>().GetTime());

        builder.Services.AddSingleton<ConnectionStatus>();
        builder.Services.AddSingleton<OSCParameters>();
        builder.Services.AddSingleton<MovementDataViewModel>();
        builder.Services.AddSingleton(loggingStore);

        AppHost = builder.Build();

        Ioc.Default.ConfigureServices(AppHost.Services);

        AppHost.Start();

        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args) {
        Window = new MainWindow();
        Window.Activate();
    }
}
