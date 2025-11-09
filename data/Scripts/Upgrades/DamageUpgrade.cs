using Unravel.Core;

/// <summary>
/// Example upgrade that increases damage output.
/// Demonstrates how to create a specific upgrade type.
/// </summary>
public class DamageUpgrade : Upgrade
{
    /// <summary>
    /// Percentage damage increase this upgrade provides.
    /// </summary>
    public float DamagePercent { get; set; }
    
    /// <summary>
    /// Create a new damage upgrade with specified parameters.
    /// </summary>
    /// <param name="damagePercent">Percentage damage increase (e.g., 10 for 10%).</param>
    public DamageUpgrade(float damagePercent = 10.0f) 
        : base("Damage Boost", $"Increases damage by {damagePercent:F1}%")
    {
        DamagePercent = damagePercent;
    }
    

    /// <summary>
    /// Generate a new DamageUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minPercent">Minimum damage percent value.</param>
    /// <param name="maxPercent">Maximum damage percent value.</param>
    /// <returns>A new DamageUpgrade with a random value from the range.</returns>
    public static DamageUpgrade Generate(float minPercent, float maxPercent)
    {
        float damagePercent = Random.Range(minPercent, maxPercent);
        return new DamageUpgrade(damagePercent);
    }

    /// <summary>
    /// Gets the damage multiplier (1.0 + percentage/100).
    /// </summary>
    /// <returns>Damage multiplier value.</returns>
    public float GetDamageMultiplier()
    {
        return 1.0f + (DamagePercent / 100.0f);
    }
}
