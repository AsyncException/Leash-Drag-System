using Microsoft.UI.Xaml.Controls;
using System;
using LDS.Models;
using CommunityToolkit.Mvvm.DependencyInjection;
using LDS.Pages;
using Microsoft.Extensions.Logging;
using System.Linq;
using LDS.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LDS.Components;

public sealed partial class MainContent : UserControl {
    public IController Controller { get; } = Ioc.Default.GetRequiredService<IController>();
    public ILogger<MainContent> Logger { get; set; } = Ioc.Default.GetRequiredService<ILogger<MainContent>>();
    public ConnectionDataModel ConnectionStatus { get; } = Ioc.Default.GetRequiredService<ConnectionDataModel>();

    public MainContent() {
        InitializeComponent();
        Navigation.SelectedItem = Navigation.MenuItems[0];
    }

    private void Navigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args) {
        string page = (args.SelectedItem as NavigationViewItem)?.Tag?.ToString() ?? "Home";

        //Filter out buttons
        if (page is "Unity") {
            Navigation.SelectedItem = GetPreviousPage();
            Controller.ToggleUnity();
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
}