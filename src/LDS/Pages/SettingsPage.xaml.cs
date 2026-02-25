using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using LDS.Core;
using LDS.Models;
using LDS.Services;
using LDS.UI.Services;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LDS.Pages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingsPage : Page
{
    public OpenVRStatus OpenVRStatus { get; } = Ioc.Default.GetRequiredService<OpenVRStatus>();
    public ApplicationSettingsDataModel Settings { get; } = Ioc.Default.GetRequiredService<ApplicationSettingsDataModel>();

    public MovementCalculatorType[] CalculatorOptions { get; } = Enum.GetValues<MovementCalculatorType>();
    public SettingsPage() => InitializeComponent();

    [RelayCommand]
    public static void OpenLogDirectory() {
        Process.Start(new ProcessStartInfo {
            FileName = StorageLocation.GetLogPath(),
            UseShellExecute = true
        });
    }
}
