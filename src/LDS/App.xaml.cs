using CommunityToolkit.Mvvm.DependencyInjection;
using FastOSC;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Serilog;
using System;
using System.Net.Http;
using LDS.Models;
using LDS.Services;
using LDS.Services.VRChatOSC;
using LDS.Utilities;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LDS;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? f_window;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App() {
        Environment.SetEnvironmentVariable("MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY", AppContext.BaseDirectory);

        StorageLocation.EnsureAppdataPathExists();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
#if DEBUG
            .WriteTo.Debug()
#endif
            .WriteTo.File(StorageLocation.GetLogFile(), restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 5)
            .CreateLogger();

        Ioc.Default.ConfigureServices(new ServiceCollection()
            .AddTransient<IBackDropController, BackDropController>()
            .AddSingleton<ILiteDatabase>(s => new LiteDatabase(StorageLocation.GetDatabasePath()))
            .AddSingleton<DebugLoggerContext>()
            .AddTransient<IDebugLogger, DebugLogger>()

            .AddSingleton<HttpClient>()
            .AddSingleton<OSCSender>()
            .AddSingleton<OSCReceiver>()
            .AddSingleton<IVRChatOscClient, VRChatOscClient>()

            .AddSingleton<IBackgroundLeashUpdater, BackgroundLeashUpdater>()
            .AddSingleton<IBackgroundTimerUpdater, BackgroundTimerUpdater>()

            .AddSingleton<IApplicationSettingsProvider, ApplicationSettingsProvider>()
            .AddSingleton<ApplicationSettings>(services => services.GetRequiredService<IApplicationSettingsProvider>().GetSettings())

            .AddSingleton<IThresholdSettingsProvider, ThresholdSettingsProvider>()
            .AddSingleton<ThresholdSettings>(services => services.GetRequiredService<IThresholdSettingsProvider>().GetSettings())

            .AddSingleton<ITimeProvider, Services.TimeProvider>()

            .AddSingleton<OSCParameters>()
            .AddSingleton<LeashData>()
            .AddSingleton<TimerData>()

            .BuildServiceProvider());

        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args) => (f_window = new MainWindow()).Activate();
}
