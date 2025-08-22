using CommunityToolkit.Mvvm.DependencyInjection;
using LDS.Logger;
using Microsoft.UI.Xaml;
using System;

namespace LDS;

//TODO: This whole mechanism needs to be rewritten. Looking into making a custom serilog.

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DebugWindow : Window
{

    public DebugWindow() {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
    }
}