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
    public float DamagePercent { get; private set; }
    
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
    /// Generate a new DamageUpgrade with random values based on rarity.
    /// </summary>
    /// <param name="rarity">The rarity level determining the value ranges.</param>
    /// <returns>A new DamageUpgrade with randomized values.</returns>
    public static DamageUpgrade Generate(UpgradeRarity rarity)
    {
        float damagePercent;
        
        switch (rarity)
        {
            case UpgradeRarity.Normal:
                damagePercent = Random.Range(5.0f, 10.0f);
                break;
            case UpgradeRarity.Common:
                damagePercent = Random.Range(8.0f, 15.0f);
                break;
            case UpgradeRarity.Epic:
                damagePercent = Random.Range(12.0f, 20.0f);
                break;
            case UpgradeRarity.Legendary:
                damagePercent = Random.Range(18.0f, 30.0f);
                break;
            default:
                damagePercent = 10.0f;
                break;
        }
        
        return new DamageUpgrade(damagePercent);
    }

}
