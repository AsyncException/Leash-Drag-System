using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using LDS.LeashSystem;
using LDS.Logger;
using LDS.Models;
using LDS.Services;
using LDS.TimerSystem;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Serilog;
using System;
using System.Net;
using System.Threading.Tasks;
using VRChatOSCClient;
using VRChatOSCClient.OpenVR;
using Application = Microsoft.UI.Xaml.Application;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LDS;

public partial class App : Application, IRecipient<InvokeExitMessage>
{
    private IHost AppHost { get; init; }
    public ILogger<App> Logger { get; init; }
    private DispatcherQueue DispatcherQueue { get; init; } = DispatcherQueue.GetForCurrentThread();

    private Window? Window { get; set; }

    public App() {
        Environment.SetEnvironmentVariable("MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY", AppContext.BaseDirectory);

        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        DebugLoggingStore loggingStore = new();
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.DebugWindowSink(loggingStore)
            .CreateLogger();

        StorageLocation.EnsureAppdataPathExists();

        if (!StorageLocation.ManifestPathExists()) {
            Log.Logger.Debug("Creating Manifest file");
            AppManifest.Create("builtin", "leashdragsystem.async", "binary", "LDS.exe", true, "Leash Drag System", "Enable Leashes in VRChat over OSC")
            .WriteToFile(StorageLocation.GetManifestPath());
        }

        builder.Logging.AddSerilog();
        builder.Services.AddSerilog();

        builder.Services.AddVRChatClient("Leash Drag System", IPAddress.Loopback);
        builder.Services.AddOpenVRClient(settings => settings.ManifestPath = StorageLocation.GetManifestPath());
        builder.Services.AddHostedService<BackgroundUpdater>();
        builder.Services.AddHostedService<OpenVRService>();
        builder.Services.AddTransient(services => DispatcherQueue);

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
        builder.Services.AddSingleton<OpenVRStatus>();
        builder.Services.AddSingleton(loggingStore);

        AppHost = builder.Build();

        Log.Logger.Debug("AppHost was created");

        Ioc.Default.ConfigureServices(AppHost.Services);

        Log.Logger.Debug("Waiting for background services to start");
        Task.Run(async () => await AppHost.StartAsync()).GetAwaiter().GetResult();
        Log.Logger.Debug("background services started");

        Logger = AppHost.Services.GetRequiredService<ILogger<App>>();

        WeakReferenceMessenger.Default.RegisterAll(this);

        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args) {
        Logger.LogDebug("OnLaunched called, MainWindow is starting");
        Window = new MainWindow();
        Window.Activate();
        Logger.LogDebug("MainWindow activated");
    }


    private bool _exitReceived = false;
    void IRecipient<InvokeExitMessage>.Receive(InvokeExitMessage message) {
        if (_exitReceived) { return; }
        _exitReceived = true;

        Logger.LogInformation("Exit requested by {SenderName}", message.Value.SenderName);

        _ = Task.Run(async () => {
            await AppHost.StopAsync();
            await Log.CloseAndFlushAsync();

            DispatcherQueue.TryEnqueue(() => {
                Window?.Close();
                Exit();
            });
        });
    }
}

internal class InvokeExitMessage(ExitMessageData value) : ValueChangedMessage<ExitMessageData>(value);
internal record ExitMessageData(string SenderName);