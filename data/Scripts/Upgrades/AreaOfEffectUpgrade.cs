using Unravel.Core;

/// <summary>
/// Upgrade that increases the area of effect radius for abilities.
/// Improves coverage and ability to hit multiple enemies.
/// </summary>
public class AreaOfEffectUpgrade : Upgrade
{
    /// <summary>
    /// Percentage area of effect increase this upgrade provides.
    /// </summary>
    public float AoePercent { get; set; }
    
    /// <summary>
    /// Create a new area of effect upgrade with specified parameters.
    /// </summary>
    /// <param name="aoePercent">Percentage AOE increase (e.g., 20 for 20%).</param>
    public AreaOfEffectUpgrade(float aoePercent = 20.0f) 
        : base("Area of Effect", $"Increases area of effect by {aoePercent:F1}%")
    {
        AoePercent = aoePercent;
    }
    

    /// <summary>
    /// Generate a new AreaOfEffectUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minPercent">Minimum AOE percent value.</param>
    /// <param name="maxPercent">Maximum AOE percent value.</param>
    /// <returns>A new AreaOfEffectUpgrade with a random value from the range.</returns>
    public static AreaOfEffectUpgrade Generate(float minPercent, float maxPercent)
    {
        float aoePercent = Random.Range(minPercent, maxPercent);
        return new AreaOfEffectUpgrade(aoePercent);
    }

    /// <summary>
    /// Get the AOE multiplier (1.0 + percentage/100).
    /// </summary>
    /// <returns>AOE multiplier value.</returns>
    public float GetAoeMultiplier()
    {
        return 1.0f + (AoePercent / 100.0f);
    }
}

