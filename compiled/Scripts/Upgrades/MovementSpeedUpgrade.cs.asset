using Unravel.Core;

/// <summary>
/// Upgrade that increases the player's movement speed.
/// Improves mobility and ability to dodge attacks.
/// </summary>
public class MovementSpeedUpgrade : Upgrade
{
    /// <summary>
    /// Percentage movement speed increase this upgrade provides.
    /// </summary>
    public float SpeedPercent { get; set; }
    
    /// <summary>
    /// Create a new movement speed upgrade with specified parameters.
    /// </summary>
    /// <param name="speedPercent">Percentage movement speed increase (e.g., 15 for 15%).</param>
    public MovementSpeedUpgrade(float speedPercent = 15.0f) 
        : base("Speed Boost", $"Increases movement speed by {speedPercent:F1}%")
    {
        SpeedPercent = speedPercent;
    }
    

    /// <summary>
    /// Generate a new MovementSpeedUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minPercent">Minimum speed percent value.</param>
    /// <param name="maxPercent">Maximum speed percent value.</param>
    /// <returns>A new MovementSpeedUpgrade with a random value from the range.</returns>
    public static MovementSpeedUpgrade Generate(float minPercent, float maxPercent)
    {
        float speedPercent = Random.Range(minPercent, maxPercent);
        return new MovementSpeedUpgrade(speedPercent);
    }

    /// <summary>
    /// Get the speed multiplier (1.0 + percentage/100).
    /// </summary>
    /// <returns>Speed multiplier value.</returns>
    public float GetSpeedMultiplier()
    {
        return 1.0f + (SpeedPercent / 100.0f);
    }
}
