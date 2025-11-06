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
    public float ReductionPercent { get; private set; }
    
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
    /// Generate a new CooldownReductionUpgrade with random values based on rarity.
    /// </summary>
    /// <param name="rarity">The rarity level determining the value ranges.</param>
    /// <returns>A new CooldownReductionUpgrade with randomized values.</returns>
    public static CooldownReductionUpgrade Generate(UpgradeRarity rarity)
    {
        float reductionPercent;
        
        switch (rarity)
        {
            case UpgradeRarity.Normal:
                reductionPercent = Random.Range(5.0f, 10.0f);
                break;
            case UpgradeRarity.Common:
                reductionPercent = Random.Range(8.0f, 15.0f);
                break;
            case UpgradeRarity.Epic:
                reductionPercent = Random.Range(12.0f, 20.0f);
                break;
            case UpgradeRarity.Legendary:
                reductionPercent = Random.Range(18.0f, 25.0f);
                break;
            default:
                reductionPercent = 10.0f;
                break;
        }
        
        return new CooldownReductionUpgrade(reductionPercent);
    }

}
