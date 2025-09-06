using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using LDS.Models;
using LDS.Services;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LDS.Components;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AppSettingsDialog : Page
{
    public ApplicationSettings Settings { get; } = Ioc.Default.GetRequiredService<ApplicationSettings>();
    public MovementCalculatorType[] CalculatorOptions { get; } = Enum.GetValues<MovementCalculatorType>();
    public AppSettingsDialog() => InitializeComponent();


    [RelayCommand]
    public static void OpenLogDirectory() {
        Process.Start(new ProcessStartInfo {
            FileName = StorageLocation.GetLogPath(),
            UseShellExecute = true
        });
    }

}