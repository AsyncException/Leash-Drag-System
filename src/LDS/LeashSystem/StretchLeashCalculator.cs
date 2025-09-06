using LDS.Models;
using LDS.Services.VRChatOSC;
using System;
using System.Numerics;

namespace LDS.LeashSystem;
internal class StretchLeashCalculator : LeashCalculator
{
    /// <summary>
    /// Gets the leash data based on the current leash parameters, threshold settings, and previous leash data.
    /// </summary>
    /// <param name="leash">The current leash parameters.</param>
    /// <param name="thresholds">The threshold settings.</param>
    /// <param name="previous">Previous set of <see cref="MovementData"/> that this instance should be compared to.</param>
    /// <returns>An instance of <see cref="MovementData"/> that contains the movement data.</returns>
    public static new MovementData GetLeashData(OSCParameters leash, ThresholdSettings thresholds, MovementData previous) {
        Vector2 direction = new(GetHorizontalOffset(leash), GetVerticalOffset(leash));
        direction = Vector2.Normalize(direction);
        if(float.IsNaN(direction.X)) { direction.X = 0; }
        if(float.IsNaN(direction.Y)) { direction.Y = 0; }

        direction *= leash.Stretch;

        if(direction == Vector2.NaN) {
            direction = Vector2.Zero;
        }

        float horizontalLook = GetHorizontalLook(leash, thresholds, direction.X);
        bool shouldRun = ShouldRun(leash, thresholds, previous);
        return new MovementData { HorizontalLook = horizontalLook, HorizontalOffset = direction.X, VerticalOffset = direction.Y, ShouldRun = shouldRun };
    }
}
