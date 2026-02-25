using Microsoft.UI;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LDS.UI.Models;

public static class Theme
{
    public static Windows.UI.Color BackgroundElements { get; } = Windows.UI.Color.FromArgb(50, 169, 169, 169);
    public static Windows.UI.Color TimerThreshold { get; } = Colors.LightGreen;
    public static Windows.UI.Color RunningMinThreshold { get; } = Colors.DarkRed;
    public static Windows.UI.Color RunningMaxThreshold { get; } = Colors.Red;
    public static Windows.UI.Color TurningTreshold { get; } = Colors.Purple;
    public static Windows.UI.Color StretchThreshold { get; } = Colors.Green;
    public static Windows.UI.Color TurningGoal { get; } = Colors.Blue;
    public static Windows.UI.Color Colliders { get; } = Colors.LightBlue;

    public static Windows.UI.Color LeashPosition { get; } = Colors.Yellow;
    public static Windows.UI.Color StretchPosition { get; } = Colors.Red;

    public static Windows.UI.Color CurrentStretch { get; } = Windows.UI.Color.FromArgb(50, 255, 255, 255);
}