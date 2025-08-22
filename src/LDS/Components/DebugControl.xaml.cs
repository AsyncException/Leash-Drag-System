using CommunityToolkit.Mvvm.DependencyInjection;
using LDS.Logger;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LDS.Components;
public sealed partial class DebugControl : UserControl
{
    private readonly DebugLoggingStore _loggingStore = Ioc.Default.GetRequiredService<DebugLoggingStore>();

    public DebugControl() {
        _loggingStore.DebuggerAttached = true;
        InitializeComponent();

        _loggingStore.OnLogMessageAdd += OnLogMessageAdd;
        _loggingStore.OnDebugMessageAdd += OnDebugMessageAdd;

        this.Unloaded += Unattach;
    }



    public void Unattach(object? sender, RoutedEventArgs args) {
        _loggingStore.OnLogMessageAdd -= OnLogMessageAdd;
        _loggingStore.OnDebugMessageAdd -= OnDebugMessageAdd;
        _loggingStore.DebuggerAttached = false;
    }



    private void OnLogMessageAdd(object? sender, LogEntry e) => LogListView.ScrollIntoView(e);
    private void OnDebugMessageAdd(object? sender, LogEntry e) => DebugLogListView.ScrollIntoView(e);
}
