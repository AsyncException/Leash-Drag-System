using System.Numerics;
using System.Text;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace LDS.TUI.Views;

internal class GraphView : View
{
    public GraphView() {
        var scaleCanvas = new ScaleCanvasView() {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        Add(scaleCanvas);
    }
}


public class ScaleCanvasView : FrameView
{
    public ScaleCanvasView() {
        Title = "Graph";
    }

    protected override bool OnDrawingContent(DrawContext? ctx) {
        var contentSize = GetContentSize();
        var center = new Vector2(contentSize.Width / 2, contentSize.Height / 2);
        var size = Math.Min(contentSize.Width, contentSize.Height);
        CanvasContext context = new(this, center, size, 2.5f);

        context.DrawLine(new(-contentSize.Width, 0), new(contentSize.Width, 0), new(new Color(50, 169, 169, 169)));
        context.DrawLine(new(0, -contentSize.Height), new(0, contentSize.Width), new(new Color(50, 169, 169, 169)));

        context.DrawCircle(new(-1, 0), 1.5f, new(Theme.Colliders));
        context.DrawCircle(new(1, 0), 1.5f, new(Theme.Colliders));
        context.DrawCircle(new(0, -1f), 1.5f, new(Theme.Colliders));
        context.DrawCircle(new(0, 1f), 1.5f, new(Theme.Colliders));

        context.PlotPoint(new(0, 0), new(Theme.LeashPosition));

        return true;
    }
}

public record CanvasContext(View DrawView, Vector2 Center, int Size, float Zoom)
{
    public void DrawCircle(Vector2 norm, float radiusNorm, Attribute color) {
        DrawView.SetAttribute(color);

        float aspect = 0.5f;

        float half = Size / Zoom;

        int cx = (int)(Center.X + norm.X * half);
        int cy = (int)(Center.Y - norm.Y * half);

        int radius = (int)(radiusNorm * half);
        int yRadius = (int)(radius * aspect);

        for (int y = -yRadius; y <= yRadius; y++) {
            for (int x = -radius; x <= radius; x++) {
                float dx = x;
                float dy = y / aspect;

                float dist = MathF.Sqrt(dx * dx + dy * dy);

                if (Math.Abs(dist - radius) < 0.5f) {
                    DrawView.AddRune(cx + x, cy + y, new Rune('●'));
                }
            }
        }
    }

    public void PlotPoint(Vector2 norm, Attribute color) {
        DrawView.SetAttribute(color);

        float half = Size / Zoom;

        int cx = (int)(Center.X + norm.X * half);
        int cy = (int)(Center.Y - norm.Y * half);

        DrawView.AddRune(cx, cy, new Rune('●'));
    }

    public void DrawLine(Vector2 startNorm, Vector2 endNorm, Attribute color) {
        DrawView.SetAttribute(color);

        float half = Size / Zoom;

        int x0 = (int)(Center.X + startNorm.X * half);
        int y0 = (int)(Center.Y - startNorm.Y * half);

        int x1 = (int)(Center.X + endNorm.X * half);
        int y1 = (int)(Center.Y - endNorm.Y * half);

        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);

        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        int err = dx - dy;

        while (true) {
            DrawView.AddRune(x0, y0, new('█'));

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = err * 2;

            if (e2 > -dy) {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx) {
                err += dx;
                y0 += sy;
            }
        }
    }
}

public static class Theme
{
    public static Color BackgroundElements { get; } = new Color(50, 169, 169, 169);
    public static Color TimerThreshold { get; } = Color.BrightGreen;
    public static Color RunningMinThreshold { get; } = Color.Red;
    public static Color RunningMaxThreshold { get; } = Color.Red;
    public static Color TurningTreshold { get; } = Color.BrightBlue;
    public static Color StretchThreshold { get; } = Color.Green;
    public static Color TurningGoal { get; } = Color.Blue;
    public static Color Colliders { get; } = Color.BrightBlue;

    public static Color LeashPosition { get; } = Color.Yellow;
    public static Color StretchPosition { get; } = Color.Red;

    public static Color CurrentStretch { get; } = new Color(50, 255, 255, 255);
}