using LDS.Core;
using LDS.TUI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Net;
using Terminal.Gui.App;
using Terminal.Gui.Views;
using VRChatOSCClient;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
#if DEBUG
    .WriteTo.Debug()
#endif
    .CreateLogger();

var builder = Host.CreateApplicationBuilder();

builder.Services.AddSerilog();
builder.Services.AddVRChatClient("Leash Drag System", IPAddress.Loopback);

builder.Services.AddSingleton<BackgroundServiceController>();
builder.Services.AddSingleton<IController, BackgroundServiceController>(services => services.GetRequiredService<BackgroundServiceController>());
builder.Services.AddHostedService<VRChatBackgroundService>();

var app = builder.Build();

var controller = app.Services.GetRequiredService<IController>();
controller.Thresholds.CounterThreshold = 0.20f;
controller.Thresholds.RunningUpperThreshold = 0.90f;
controller.Thresholds.RunningLowerThreshold = 0.75f;
controller.Thresholds.StretchThreshold = 0.30f;
controller.Thresholds.TurningThreshold = 0.35f;
controller.Thresholds.TurningGoal = 0.90f;
controller.Thresholds.TurningMultiplier = 1.50f;
controller.Thresholds.LeashEnabled = true;
controller.Thresholds.CounterEnabled = true;

controller.Settings.GlobalEnableLeash = true;
controller.Settings.GlobalEnableCounter = true;

ServiceProvider.Initializer(app.Services);

await app.StartAsync();

using (IApplication application = Application.Create().Init()) {
    Window window = new() { BorderStyle = Terminal.Gui.Drawing.LineStyle.None };
    window.Add(new MainView());
    application.Run(window);
}

await app.WaitForShutdownAsync();