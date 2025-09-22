using LDS.Models;
using System;
using System.Numerics;

namespace LDS.LeashSystem;

/// <summary>
/// Base class for the calculators with some utility methods.
/// </summary>
internal class BaseLeashCalculator
{
    /// <summary>
    /// Calculates the vertical offset based on the difference between the front and back distances, scaled by the stretch factor.
    /// </summary>
    /// <param name="leash">An <see cref="OSCParameters"/> object containing the front and back distances and the stretch factor used in the calculation</param>
    /// <returns>A value between -1.0 and 1,0 representing the vertical offset, where negative values indicate a backwards offset, positive values indicate a forward offset, and 0 represents no offset.</returns>
    public static float GetVerticalOffset(OSCParameters leash) => Math.Clamp((leash.FrontDistance - leash.BackDistance) * leash.Stretch, -1f, 1f);

    /// <summary>
    /// Calculates the horizontal offset based on the difference between the right and left distances, scaled by the stretch factor.
    /// </summary>
    /// <param name="leash">An <see cref="OSCParameters"/> object containing the right and left distances and the stretch factor used in the calculation.</param>
    /// <returns>A value between -1.0 and 1.0 representing the horizontal offset, where negative values indicate a leftward offset, positive values indicate a rightward offset, and 0 represents no offset.</returns>
    public static float GetHorizontalOffset(OSCParameters leash) => Math.Clamp((leash.RightDistance - leash.LeftDistance) * leash.Stretch, -1f, 1f);

    /// <summary>
    /// Calculates the horizontal turning adjustment based on leash parameters, thresholds, and a horizontal offset.
    /// </summary>
    /// <remarks>The method calculates the turning adjustment by applying the turning multiplier to the horizontal offset  and further adjusts the value based on the leash's back distance and the relative distances to the left and right. The result is clamped to the range [-1.0, 1.0].</remarks>
    /// <param name="leash">The leash parameters, including stretch, front distance, right distance, left distance, and back distance.</param>
    /// <param name="thresholds">The threshold settings that define turning behavior, including the turning threshold, goal, and multiplier.</param>
    /// <param name="horizontalOffset">The horizontal offset used to influence the turning adjustment.</param>
    /// <returns>A value between -1.0 and 1.0 representing the horizontal turning adjustment.  Returns 0.0 if the leash stretch is below the turning threshold or the front distance exceeds the turning goal.</returns>
    public static float GetHorizontalLook(OSCParameters leash, ThresholdSettings thresholds, float horizontalOffset) {
        if (leash.Stretch <= thresholds.TurningThreshold || leash.FrontDistance >= thresholds.TurningGoal) {
            return 0f;
        }

        float turn = thresholds.TurningMultiplier * horizontalOffset;

        turn = leash.RightDistance > leash.LeftDistance ? (turn += leash.BackDistance) : (turn -= leash.BackDistance);
        return Math.Clamp(turn, -1f, 1f);
    }

    /// <summary>
    /// Determines whether the system should transition to a running state based on the provided parameters.
    /// </summary>
    /// <param name="leash">The current leash parameters, including the stretch value.</param>
    /// <param name="thresholds">The threshold settings that define the minimum and maximum stretch values for running.</param>
    /// <param name="leashData">Additional leash data, including the current running state.</param>
    /// <returns><see langword="true"/> if the system should transition to a running state; otherwise, <see langword="false"/>.</returns>
    public static bool ShouldRun(OSCParameters leash, ThresholdSettings thresholds, ref MovementData leashData) {
        bool shouldRun = leash.Stretch > thresholds.RunningMaxThreshold;

        if (leashData.ShouldRun && !shouldRun && leash.Stretch > thresholds.RunningMinThreshold) {
            shouldRun = true;
        }

        return shouldRun;
    }

    /// <summary>
    /// Determines whether the leash is active based on its grabbed state and stretch threshold.
    /// </summary>
    /// <param name="leash">The leash parameters, including its grabbed state and stretch value.</param>
    /// <param name="thresholds">The threshold settings that define the stretch limit for activation.</param>
    /// <returns><see langword="true"/> if the leash is grabbed and its stretch exceeds the specified threshold; otherwise, <see langword="false"/>.</returns>
    public static bool LeashActive(OSCParameters leash, ThresholdSettings thresholds) => leash.IsGrabbed && leash.Stretch > thresholds.StretchThreshold;

    /// <summary>
    /// Determines whether the leash has zero collider distances, indicating that it is not currently interacting with any colliders.
    /// </summary>
    /// <param name="leash">The leash parameters, including the distances to the colliders.</param>
    /// <returns><see langword="true" /> if the distances are 0; otherwise, <see langword="false"/>.</returns>
    public static bool IsZeroColliderDistance(OSCParameters leash) => leash.RightDistance == 0 && leash.LeftDistance == 0 && leash.FrontDistance == 0 && leash.BackDistance == 0;
}

internal class PositionLeashCalculator : BaseLeashCalculator {
    /// <summary>
    /// Gets the leash data based on the current leash parameters, threshold settings, and previous leash data.
    /// </summary>
    /// <param name="leash">The current leash parameters.</param>
    /// <param name="thresholds">The threshold settings.</param>
    /// <param name="previous">Previous set of <see cref="MovementData"/> that this instance should be compared to.</param>
    /// <returns>An instance of <see cref="MovementData"/> that contains the movement data.</returns>
    public static MovementData GetLeashData(OSCParameters leash, ThresholdSettings thresholds, ref MovementData previous) {
        if (!LeashActive(leash, thresholds)) {
            return new MovementData();
        }

        float verticalOffset = GetVerticalOffset(leash);
        float horizontalOffset = GetHorizontalOffset(leash);
        float horizontalLook = GetHorizontalLook(leash, thresholds, horizontalOffset);
        bool shouldRun = ShouldRun(leash, thresholds, ref previous);
        return new MovementData { HorizontalLook = horizontalLook, HorizontalOffset = horizontalOffset, VerticalOffset = verticalOffset, ShouldRun = shouldRun };
    }
}

internal class StretchLeashCalculator : BaseLeashCalculator
{
    /// <summary>
    /// Gets the leash data based on the current leash parameters, threshold settings, and previous leash data.
    /// </summary>
    /// <param name="leash">The current leash parameters.</param>
    /// <param name="thresholds">The threshold settings.</param>
    /// <param name="previous">Previous set of <see cref="MovementData"/> that this instance should be compared to.</param>
    /// <returns>An instance of <see cref="MovementData"/> that contains the movement data.</returns>
    public static MovementData GetLeashData(OSCParameters leash, ThresholdSettings thresholds, ref MovementData previous) {
        if (!LeashActive(leash, thresholds)) {
            return new MovementData();
        }

        Vector2 direction = new(GetHorizontalOffset(leash), GetVerticalOffset(leash));
        direction = Vector2.Normalize(direction);
        if (float.IsNaN(direction.X)) { direction.X = 0; }
        if (float.IsNaN(direction.Y)) { direction.Y = 0; }

        direction *= leash.Stretch;

        float horizontalLook = GetHorizontalLook(leash, thresholds, direction.X);
        bool shouldRun = ShouldRun(leash, thresholds, ref previous);
        return new MovementData { HorizontalLook = horizontalLook, HorizontalOffset = direction.X, VerticalOffset = direction.Y, ShouldRun = shouldRun };
    }
}

internal class StretchPositionLeashCalculator : BaseLeashCalculator {
    /// <summary>
    /// Gets the leash data based on the current leash parameters, threshold settings, and previous leash data.
    /// </summary>
    /// <param name="leash">The current leash parameters.</param>
    /// <param name="thresholds">The threshold settings.</param>
    /// <param name="previous">Previous set of <see cref="MovementData"/> that this instance should be compared to.</param>
    /// <returns>An instance of <see cref="MovementData"/> that contains the movement data.</returns>
    public static MovementData GetLeashData(OSCParameters leash, ThresholdSettings thresholds, ref MovementData previous) {
        if(leash.Stretch > 0.95f) {
            return PositionLeashCalculator.GetLeashData(leash, thresholds, ref previous);
        }
        else {
            return StretchLeashCalculator.GetLeashData(leash, thresholds, ref previous);
        }
    }
}