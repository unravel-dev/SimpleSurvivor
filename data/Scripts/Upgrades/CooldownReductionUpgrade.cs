using Unravel.Core;

/// <summary>
/// Upgrade that reduces ability cooldowns by a percentage.
/// Affects all abilities with cooldown timers.
/// </summary>
public class CooldownReductionUpgrade : Upgrade
{
    /// <summary>
    /// Percentage cooldown reduction this upgrade provides.
    /// </summary>
    public float ReductionPercent { get; set; }
    
    /// <summary>
    /// Create a new cooldown reduction upgrade with specified parameters.
    /// </summary>
    /// <param name="reductionPercent">Percentage cooldown reduction (e.g., 10 for 10%).</param>
    public CooldownReductionUpgrade(float reductionPercent = 10.0f) 
        : base("Cooldown Reduction", $"Reduces ability cooldowns by {reductionPercent:F1}%")
    {
        ReductionPercent = reductionPercent;
    }
    

    /// <summary>
    /// Generate a new CooldownReductionUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minPercent">Minimum reduction percent value.</param>
    /// <param name="maxPercent">Maximum reduction percent value.</param>
    /// <returns>A new CooldownReductionUpgrade with a random value from the range.</returns>
    public static CooldownReductionUpgrade Generate(float minPercent, float maxPercent)
    {
        float reductionPercent = Random.Range(minPercent, maxPercent);
        return new CooldownReductionUpgrade(reductionPercent);
    }

}
