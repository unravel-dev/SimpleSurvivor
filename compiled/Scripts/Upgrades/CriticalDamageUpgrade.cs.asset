using Unravel.Core;

/// <summary>
/// Upgrade that increases critical strike damage multiplier.
/// Affects the damage multiplier when a critical hit occurs.
/// </summary>
public class CriticalDamageUpgrade : Upgrade
{
    /// <summary>
    /// Percentage critical damage increase this upgrade provides.
    /// </summary>
    public float DamagePercent { get; set; }
    
    /// <summary>
    /// Create a new critical damage upgrade with specified parameters.
    /// </summary>
    /// <param name="damagePercent">Percentage critical damage increase (e.g., 25 for 25%).</param>
    public CriticalDamageUpgrade(float damagePercent = 25.0f) 
        : base("Critical Strike Damage", $"Increases critical strike damage by {damagePercent:F1}%")
    {
        DamagePercent = damagePercent;
    }
    
    /// <summary>
    /// Generate a new CriticalDamageUpgrade with random values based on rarity.
    /// </summary>
    /// <param name="rarity">The rarity level determining the value ranges.</param>
    /// <returns>A new CriticalDamageUpgrade with randomized values.</returns>
    public static CriticalDamageUpgrade Generate(UpgradeRarity rarity)
    {
        float damagePercent;
        
        switch (rarity)
        {
            case UpgradeRarity.Normal:
                damagePercent = Random.Range(15.0f, 25.0f);
                break;
            case UpgradeRarity.Common:
                damagePercent = Random.Range(20.0f, 35.0f);
                break;
            case UpgradeRarity.Epic:
                damagePercent = Random.Range(30.0f, 50.0f);
                break;
            case UpgradeRarity.Legendary:
                damagePercent = Random.Range(45.0f, 75.0f);
                break;
            default:
                damagePercent = 25.0f;
                break;
        }
        
        return new CriticalDamageUpgrade(damagePercent);
    }

    /// <summary>
    /// Generate a new CriticalDamageUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minPercent">Minimum critical damage percent value.</param>
    /// <param name="maxPercent">Maximum critical damage percent value.</param>
    /// <returns>A new CriticalDamageUpgrade with a random value from the range.</returns>
    public static CriticalDamageUpgrade Generate(float minPercent, float maxPercent)
    {
        float damagePercent = Random.Range(minPercent, maxPercent);
        return new CriticalDamageUpgrade(damagePercent);
    }

    /// <summary>
    /// Gets the critical damage multiplier (1.0 + percentage/100).
    /// </summary>
    /// <returns>Critical damage multiplier value.</returns>
    public float GetCriticalDamageMultiplier()
    {
        return 1.0f + (DamagePercent / 100.0f);
    }

}
