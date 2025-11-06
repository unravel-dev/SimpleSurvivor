using Unravel.Core;

/// <summary>
/// Upgrade that increases the player's maximum health.
/// Provides additional survivability in combat.
/// </summary>
public class MaxHealthUpgrade : Upgrade
{
    /// <summary>
    /// Additional health points this upgrade provides.
    /// </summary>
    public int HealthIncrease { get; private set; }
    
    /// <summary>
    /// Create a new max health upgrade with specified parameters.
    /// </summary>
    /// <param name="healthIncrease">Additional health points to add.</param>
    public MaxHealthUpgrade(int healthIncrease = 20) 
        : base("Health Boost", $"Increases maximum health by {healthIncrease} points")
    {
        HealthIncrease = healthIncrease;
    }
    
    /// <summary>
    /// Generate a new MaxHealthUpgrade with random values based on rarity.
    /// </summary>
    /// <param name="rarity">The rarity level determining the value ranges.</param>
    /// <returns>A new MaxHealthUpgrade with randomized values.</returns>
    public static MaxHealthUpgrade Generate(UpgradeRarity rarity)
    {
        int healthIncrease;
        
        switch (rarity)
        {
            case UpgradeRarity.Normal:
                healthIncrease = Random.Range(10, 21);      // 10-20 health
                break;
            case UpgradeRarity.Common:
                healthIncrease = Random.Range(15, 31);      // 15-30 health
                break;
            case UpgradeRarity.Epic:
                healthIncrease = Random.Range(25, 46);      // 25-45 health
                break;
            case UpgradeRarity.Legendary:
                healthIncrease = Random.Range(40, 71);      // 40-70 health
                break;
            default:
                healthIncrease = 20;
                break;
        }
        
        return new MaxHealthUpgrade(healthIncrease);
    }

}
