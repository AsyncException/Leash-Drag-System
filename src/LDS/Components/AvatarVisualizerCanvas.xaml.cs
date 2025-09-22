using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;
using LDS.Models;
using Windows.UI;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LDS;

public sealed partial class AvatarVisualizerCanvas : UserControl
{
    private OSCParameters OSCParameters { get; } = Ioc.Default.GetRequiredService<OSCParameters>();
    private ThresholdSettings Settings { get; } = Ioc.Default.GetRequiredService<ThresholdSettings>();
    private ApplicationSettings ApplicationSettings { get; } = Ioc.Default.GetRequiredService<ApplicationSettings>();

    public ObservableCollection<LegendItem> LegendItems { get; } = [
        new("Counter Threshold", Theme.TimerThreshold, "The threshold for the timer to start counting."),
        new("Running Upper Threshold", Theme.RunningMaxThreshold, "The upper range of where the avatar will be running."),
        new("Running Lower Threshold", Theme.RunningMinThreshold, "The lower range of where the avatar will start walking again."),
        new("Stretch Threshold", Theme.StretchThreshold, "The threshold for when the avatar will start moving."),
        new("Turning Threshold", Theme.TurningTreshold, "The threshold for when the avatar will be able to turn."),
        new("Turning Goal", Theme.TurningGoal, "The range where the avatar will stop turning. More is less turning."),
        new("Colliders", Theme.Colliders, "Visual representation of the colliders"),

        new("Leash Position", Theme.LeashPosition, "The current position of the leash"),
        new("Stretch Position", Theme.StretchPosition, "The current amount of stretch represented as a dot")
    ];

    public AvatarVisualizerCanvas() {
        InitializeComponent();
        Settings.PropertyChanged += Redraw;
        OSCParameters.PropertyChanged += Redraw;
        ApplicationSettings.PropertyChanged += Redraw;
    }

    private void Redraw(object? sender, PropertyChangedEventArgs e) {
        PlotCanvas.Invalidate();
        ScaleCanvas.Invalidate();
    }

    private void PlotCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args) {
        CanvasDrawingSession ds = args.DrawingSession;
        float width = (float)sender.ActualWidth;
        float height = (float)sender.ActualHeight;

        CanvasContext context = new(
            args.DrawingSession,
            Center: new(width / 2, height / 2),
            Size: Math.Min(width, height),
            Zoom: Settings.Zooming);

        ds.Clear(Colors.Transparent);
        ds.DrawLine(context.Center.X, 0, context.Center.X, height, Color.FromArgb(50, 169, 169, 169));
        ds.DrawLine(0, context.Center.Y, width, context.Center.Y, Color.FromArgb(50, 169, 169, 169));

        if (Settings.ShowPositionLayer && ApplicationSettings.GlobalEnableLeash) {
            //Circle representing Left Collider
            context.DrawCircle(new Vector2(-1f, 0f), 1.5f, Theme.Colliders);

            //Circle representing Right Collider
            context.DrawCircle(new Vector2(1f, 0f), 1.5f, Theme.Colliders);

            //Circle representing Back Collider
            context.DrawCircle(new Vector2(0f, -1f), 1.5f, Theme.Colliders);

            //Circle representing Front Collider
            context.DrawCircle(new Vector2(0f, 1f), 1.5f, Theme.Colliders);
            context.DrawCircle(new Vector2(0f, 1f), (1.5f - Settings.TurningGoal * 1.5f), Theme.TurningGoal);
        }

        //Plot the settings

        if(Settings.ShowStretchLayer && ApplicationSettings.GlobalEnableCounter) {
            //Plot counter threshold
            context.DrawCircle(Vector2.Zero, Settings.TimerThreshold, Theme.TimerThreshold);
        }
        
        if (Settings.ShowStretchLayer && ApplicationSettings.GlobalEnableLeash) {
            //Plot running range
            context.DrawCircle(Vector2.Zero, Settings.RunningMinThreshold, Theme.RunningMinThreshold);
            context.DrawCircle(Vector2.Zero, Settings.RunningMaxThreshold, Theme.RunningMaxThreshold);

            //Plot turning threshold
            context.DrawCircle(Vector2.Zero, Settings.TurningThreshold, Theme.TurningTreshold);

            //Plot Stretch threshold
            context.DrawCircle(Vector2.Zero, Settings.StretchThreshold, Theme.StretchThreshold);
        }

        //Plot avatar position
        context.PlotPoint(Vector2.Zero, 50, Theme.CurrentStretch);

        //Draw current leash position
        float vertical = Math.Clamp(OSCParameters.FrontDistance - OSCParameters.BackDistance, -1f, 1f);
        float horizontal = Math.Clamp(OSCParameters.RightDistance - OSCParameters.LeftDistance, -1f, 1f);

        if (Settings.ShowPositionLayer) {
            //Draw position
            context.PlotPoint(new Vector2(horizontal, vertical), 25, Theme.LeashPosition);
        }

        //Draw stretch position
        if (Settings.ShowStretchLayer) {
            Vector2 direction = new(horizontal, vertical);
            if (direction != Vector2.Zero) { direction = Vector2.Normalize(direction); }
            direction *= OSCParameters.Stretch;
            context.PlotPoint(direction, 15, Theme.StretchPosition);
        }
    }
    private void ScaleCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args) {
        ScaleCanvasContext context = new(args.DrawingSession, Width: (float)sender.ActualWidth, Height: (float)sender.ActualHeight);
        context.DrawingSession.Clear(Colors.Transparent);

        context.DrawMarker(10, Theme.BackgroundElements);
        context.DrawMarker(20, Theme.BackgroundElements);
        context.DrawMarker(30, Theme.BackgroundElements);
        context.DrawMarker(40, Theme.BackgroundElements);
        context.DrawMarker(50, Theme.BackgroundElements);
        context.DrawMarker(60, Theme.BackgroundElements);
        context.DrawMarker(70, Theme.BackgroundElements);
        context.DrawMarker(80, Theme.BackgroundElements);
        context.DrawMarker(90, Theme.BackgroundElements);

        if (ApplicationSettings.GlobalEnableCounter) {
            //Draw timer threshold
            context.DrawMarker(Settings.TimerThreshold * 100, Theme.TimerThreshold);
        }

        if(ApplicationSettings.GlobalEnableLeash) {
            //Draw running range
            context.DrawMarker(Settings.RunningMinThreshold * 100, Theme.RunningMinThreshold);
            context.DrawBlock(Settings.RunningMinThreshold * 100, Settings.RunningMaxThreshold * 100, Theme.RunningMinThreshold);
            context.DrawMarker(Settings.RunningMaxThreshold * 100, Theme.RunningMaxThreshold);

            //Draw turning threshold
            context.DrawMarker(Settings.TurningThreshold * 100, Theme.TurningTreshold);

            //Draw stretch threshold
            context.DrawMarker(Settings.StretchThreshold * 100, Theme.StretchThreshold);

            //Draw current stretch
            context.DrawBlock(0, OSCParameters.Stretch * 100, Theme.CurrentStretch);
        }
    }
}
file record CanvasContext(CanvasDrawingSession DrawingSession, Vector2 Center, float Size, float Zoom)
{
    public void DrawCircle(Vector2 normalizedCoord, float normalizedRadius, Windows.UI.Color color) {
        float halfSize = Size / Zoom;
        Vector2 pixelCenter = new(
            Center.X + normalizedCoord.X * halfSize,
            Center.Y - normalizedCoord.Y * halfSize
        );

        float pixelRadius = normalizedRadius * (Size / Zoom);
        DrawingSession.DrawCircle(pixelCenter, pixelRadius, color);
    }

    public void PlotPoint(Vector2 normalizedCoord, float dotSize, Windows.UI.Color color) {
        float halfSize = Size / Zoom;
        Vector2 pixelCenter = new(
            Center.X + normalizedCoord.X * halfSize,
            Center.Y - normalizedCoord.Y * halfSize // Invert Y-axis
        );

        DrawingSession.FillCircle(pixelCenter, dotSize, color);
    }
}

file record ScaleCanvasContext(CanvasDrawingSession DrawingSession, float Width, float Height)
{
    public void DrawMarker(float percent, Windows.UI.Color color) {
        float x = Width * (percent / 100f);
        DrawingSession.DrawLine(x, 0, x, Height, color, 2);
    }

    public void DrawBlock(float startPercent, float endPercent, Windows.UI.Color color) {
        float startX = Width * (startPercent / 100f);
        float endX = Width * (endPercent / 100f);
        float blockHeight = Height * 0.6f;
        float yOffset = (Height - blockHeight) / 2;

        DrawingSession.FillRectangle(startX, yOffset, endX - startX, blockHeight, color);
    }
}

public class LegendItem(string name, Windows.UI.Color color, string description)
{
    public string Name { get; set; } = name;
    public Color Color { get; set; } = color;
    public string Description { get; set; } = description;
}
