using LDS.Core.CommunicationObjects;
using System.Numerics;

namespace LDS.Core;

internal class BaseLeashCalculator
{
    /// <summary>
    /// Calculates the vertical offset based on the difference between the front and back distances, scaled by the stretch factor.
    /// </summary>
    /// <param name="leash">An <see cref="IParameters"/> object containing the front and back distances and the stretch factor used in the calculation</param>
    /// <returns>A value between -1.0 and 1,0 representing the vertical offset, where negative values indicate a backwards offset, positive values indicate a forward offset, and 0 represents no offset.</returns>
    public static float GetVerticalOffset(ParameterMapping parameters) => Math.Clamp((parameters.FrontDistance - parameters.BackDistance) * parameters.Stretch, -1f, 1f);

    /// <summary>
    /// Calculates the horizontal offset based on the difference between the right and left distances, scaled by the stretch factor.
    /// </summary>
    /// <param name="leash">An <see cref="IParameters"/> object containing the right and left distances and the stretch factor used in the calculation.</param>
    /// <returns>A value between -1.0 and 1.0 representing the horizontal offset, where negative values indicate a leftward offset, positive values indicate a rightward offset, and 0 represents no offset.</returns>
    public static float GetHorizontalOffset(ParameterMapping parameters) => Math.Clamp((parameters.RightDistance - parameters.LeftDistance) * parameters.Stretch, -1f, 1f);

    /// <summary>
    /// Calculates the horizontal turning adjustment based on leash parameters, thresholds, and a horizontal offset.
    /// </summary>
    /// <remarks>The method calculates the turning adjustment by applying the turning multiplier to the horizontal offset  and further adjusts the value based on the leash's back distance and the relative distances to the left and right. The result is clamped to the range [-1.0, 1.0].</remarks>
    /// <param name="leash">The leash parameters, including stretch, front distance, right distance, left distance, and back distance.</param>
    /// <param name="thresholds">The threshold settings that define turning behavior, including the turning threshold, goal, and multiplier.</param>
    /// <param name="horizontalOffset">The horizontal offset used to influence the turning adjustment.</param>
    /// <returns>A value between -1.0 and 1.0 representing the horizontal turning adjustment.  Returns 0.0 if the leash stretch is below the turning threshold or the front distance exceeds the turning goal.</returns>
    public static float GetHorizontalLook(ParameterMapping parameters, Thresholds thresholds, float horizontalOffset) {
        if (parameters.Stretch <= thresholds.TurningThreshold || parameters.FrontDistance >= thresholds.TurningGoal) {
            return 0f;
        }

        float turn = thresholds.TurningMultiplier * horizontalOffset;

        turn = parameters.RightDistance > parameters.LeftDistance ? (turn += parameters.BackDistance) : (turn -= parameters.BackDistance);
        return Math.Clamp(turn, -1f, 1f);
    }

    /// <summary>
    /// Determines whether the system should transition to a running state based on the provided parameters.
    /// </summary>
    /// <param name="leash">The current leash parameters, including the stretch value.</param>
    /// <param name="thresholds">The threshold settings that define the minimum and maximum stretch values for running.</param>
    /// <param name="leashData">Additional leash data, including the current running state.</param>
    /// <returns><see langword="true"/> if the system should transition to a running state; otherwise, <see langword="false"/>.</returns>
    public static bool ShouldRun(ParameterMapping parameters, Thresholds thresholds, bool wasRunningPreviously) {
        bool shouldRun = parameters.Stretch > thresholds.RunningUpperThreshold;

        if (wasRunningPreviously && !shouldRun && parameters.Stretch > thresholds.RunningLowerThreshold) {
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
    public static bool LeashActive(ParameterMapping parameters, Thresholds thresholds) => parameters.IsGrabbed && parameters.Stretch > thresholds.StretchThreshold;

    /// <summary>
    /// Determines whether the leash has zero collider distances, indicating that it is not currently interacting with any colliders.
    /// </summary>
    /// <param name="leash">The leash parameters, including the distances to the colliders.</param>
    /// <returns><see langword="true" /> if the distances are 0; otherwise, <see langword="false"/>.</returns>
    public static bool IsZeroColliderDistance(ParameterMapping parameters) => parameters.RightDistance == 0 && parameters.LeftDistance == 0 && parameters.FrontDistance == 0 && parameters.BackDistance == 0;

    public record struct MovementData(float VerticalOffset, float HorizontalOffset, float HorizontalLook, bool ShouldRun);
}

internal class PositionLeashCalculator : BaseLeashCalculator
{
    /// <summary>
    /// Gets the leash data based on the current leash parameters, threshold settings, and previous leash data.
    /// </summary>
    /// <param name="leash">The current leash parameters.</param>
    /// <param name="thresholds">The threshold settings.</param>
    /// <param name="previous">Previous set of <see cref="MovementData"/> that this instance should be compared to.</param>
    /// <returns>An instance of <see cref="MovementData"/> that contains the movement data.</returns>
    public static MovementData GetLeashData(IController controller, ParameterMapping parameters, ref MovementData previous) {
        if (!LeashActive(parameters, controller.Thresholds)) {
            return new MovementData();
        }

        float verticalOffset = GetVerticalOffset(parameters);
        float horizontalOffset = GetHorizontalOffset(parameters);
        float horizontalLook = GetHorizontalLook(parameters, controller.Thresholds, horizontalOffset);
        bool shouldRun = ShouldRun(parameters, controller.Thresholds, previous.ShouldRun);
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
    public static MovementData GetLeashData(IController controller, ParameterMapping parameters, ref MovementData previous) {
        if (!LeashActive(parameters, controller.Thresholds)) {
            return new MovementData();
        }

        Vector2 direction = new(GetHorizontalOffset(parameters), GetVerticalOffset(parameters));
        direction = Vector2.Normalize(direction);
        if (float.IsNaN(direction.X)) { direction.X = 0; }
        if (float.IsNaN(direction.Y)) { direction.Y = 0; }

        direction *= parameters.Stretch;

        float horizontalLook = GetHorizontalLook(parameters, controller.Thresholds, direction.X);
        bool shouldRun = ShouldRun(parameters, controller.Thresholds, previous.ShouldRun);
        return new MovementData { HorizontalLook = horizontalLook, HorizontalOffset = direction.X, VerticalOffset = direction.Y, ShouldRun = shouldRun };
    }
}

internal class StretchPositionLeashCalculator : BaseLeashCalculator
{
    /// <summary>
    /// Gets the leash data based on the current leash parameters, threshold settings, and previous leash data.
    /// </summary>
    /// <param name="leash">The current leash parameters.</param>
    /// <param name="thresholds">The threshold settings.</param>
    /// <param name="previous">Previous set of <see cref="MovementData"/> that this instance should be compared to.</param>
    /// <returns>An instance of <see cref="MovementData"/> that contains the movement data.</returns>
    public static MovementData GetLeashData(IController controller, ParameterMapping parameters, ref MovementData previous) {
        return parameters.Stretch > 0.90f
            ? PositionLeashCalculator.GetLeashData(controller, parameters, ref previous)
            : StretchLeashCalculator.GetLeashData(controller, parameters, ref previous);
    }
}

internal class ParameterMapping(IController controller)
{
    public const string ENABLED = "Leash_Enabled";
    public const string IS_GRABBED = "Leash_IsGrabbed";
    public const string ANGLE = "Leash_Angle";
    public const string STRETCH = "Leash_Stretch";
    public const string FRONT_COLLIDER = "Leash_Front";
    public const string BACK_COLLIDER = "Leash_Back";
    public const string RIGHT_COLLIDER = "Leash_Right";
    public const string LEFT_COLLIDER = "Leash_Left";

    public const string HOUR = "timer_hour";
    public const string MINUTE = "timer_minute";
    public const string SECOND = "timer_second";

    public bool Enabled { get; } = controller.Parameters.GetValueOrDefault(ENABLED)?.GetValue<bool>() ?? false;
    public bool IsGrabbed { get; } = controller.Parameters.GetValueOrDefault(IS_GRABBED)?.GetValue<bool>() ?? false;
    public float Angle { get; } = controller.Parameters.GetValueOrDefault(ANGLE)?.GetValue<float>() ?? 0;
    public float Stretch { get; } = controller.Parameters.GetValueOrDefault(STRETCH)?.GetValue<float>() ?? 0;
    public float FrontDistance { get; } = controller.Parameters.GetValueOrDefault(FRONT_COLLIDER)?.GetValue<float>() ?? 0;
    public float BackDistance { get; } = controller.Parameters.GetValueOrDefault(BACK_COLLIDER)?.GetValue<float>() ?? 0;
    public float RightDistance { get; } = controller.Parameters.GetValueOrDefault(RIGHT_COLLIDER)?.GetValue<float>() ?? 0;
    public float LeftDistance { get; } = controller.Parameters.GetValueOrDefault(LEFT_COLLIDER)?.GetValue<float>() ?? 0;

    public float TimerHour { get; } = controller.Parameters.GetValueOrDefault(HOUR)?.GetValue<float>() ?? 0;
    public float TimerMinute { get; } = controller.Parameters.GetValueOrDefault(MINUTE)?.GetValue<float>() ?? 0;
    public float TimerSecond { get; } = controller.Parameters.GetValueOrDefault(SECOND)?.GetValue<float>() ?? 0;
}