using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using System;
using LDS.Models;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using LDS.Messages;
using LDS.Pages;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LDS.Components;

public sealed partial class MainContent : UserControl {
    public ILogger<MainContent> Logger { get; set; } = Ioc.Default.GetRequiredService<ILogger<MainContent>>();
    public ConnectionStatus ConnectionStatus { get; } = Ioc.Default.GetRequiredService<ConnectionStatus>();

    private DebugWindow? _debugWindow;

    public MainContent() {
        InitializeComponent();
        Navigation.SelectedItem = Navigation.MenuItems[0];
    }

    private void Navigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args) {
        string page = (args.SelectedItem as NavigationViewItem)?.Tag?.ToString() ?? "Home";

        //Filter out buttons
        if (page is "Unity") {
            Navigation.SelectedItem = GetPreviousPage();
            StartUnity();
            return;
        }

        if(page is "Console") {
            Navigation.SelectedItem = GetPreviousPage();
            ToggleConsole();
            return;
        }

        Type pageType = page switch {
            nameof(HomePage) => typeof(HomePage),
            nameof(ValuesPage) => typeof(ValuesPage),
            nameof(SettingsPage) => typeof(SettingsPage),
            _ => typeof(HomePage)
        };

        ContentFrame.Navigate(pageType);

        NavigationViewItem GetPreviousPage(){
            Type currentPage = ContentFrame.Content.GetType();
            string name = currentPage.Name;
            return Navigation.MenuItems.Cast<NavigationViewItem>().First(i => i.Tag?.ToString() == name);
        }
    }

    public void ToggleConsole() {
        if (_debugWindow is null) {
            _debugWindow = new DebugWindow();
            _debugWindow.Closed += (s, e) => {
                _debugWindow = null;
            };

            _debugWindow.Activate();
        }
        else {
            _debugWindow.Activate();
        }
    }


    public async void StartUnity() {
        try {
            bool success = await WeakReferenceMessenger.Default.Send(new ToggleUnityMessage()).Response;
        }
        catch (Exception ex) {
            Logger.LogError(ex, "Failed to toggle unity mode");
        }
    }
}