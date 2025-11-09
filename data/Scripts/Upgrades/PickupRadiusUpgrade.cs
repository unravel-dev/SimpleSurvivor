using Unravel.Core;

/// <summary>
/// Upgrade that increases pickup radius for items and experience orbs.
/// Affects the distance at which items are automatically collected.
/// </summary>
public class PickupRadiusUpgrade : Upgrade
{
    /// <summary>
    /// Percentage pickup radius increase this upgrade provides.
    /// </summary>
    public float RadiusPercent { get; set; }
    
    /// <summary>
    /// Create a new pickup radius upgrade with specified parameters.
    /// </summary>
    /// <param name="radiusPercent">Percentage pickup radius increase (e.g., 20 for 20%).</param>
    public PickupRadiusUpgrade(float radiusPercent = 20.0f) 
        : base("Pickup Radius", $"Increases pickup radius by {radiusPercent:F1}%")
    {
        RadiusPercent = radiusPercent;
    }

    /// <summary>
    /// Generate a new PickupRadiusUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minPercent">Minimum radius percent value.</param>
    /// <param name="maxPercent">Maximum radius percent value.</param>
    /// <returns>A new PickupRadiusUpgrade with a random value from the range.</returns>
    public static PickupRadiusUpgrade Generate(float minPercent, float maxPercent)
    {
        float radiusPercent = Random.Range(minPercent, maxPercent);
        return new PickupRadiusUpgrade(radiusPercent);
    }

    /// <summary>
    /// Gets the radius multiplier (1.0 + percentage/100).
    /// </summary>
    /// <returns>Radius multiplier value.</returns>
    public float GetRadiusMultiplier()
    {
        return 1.0f + (RadiusPercent / 100.0f);
    }
}
