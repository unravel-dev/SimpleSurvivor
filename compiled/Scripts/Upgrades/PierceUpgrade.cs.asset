using Unravel.Core;

/// <summary>
/// Example upgrade that increases pierce count for projectiles.
/// Demonstrates how to create upgrades that affect projectile behavior.
/// </summary>
public class PierceUpgrade : Upgrade
{
    /// <summary>
    /// Additional pierce count this upgrade provides.
    /// </summary>
    public int PierceCount { get; private set; }
    
    /// <summary>
    /// Create a new pierce upgrade with specified parameters.
    /// </summary>
    /// <param name="pierceCount">Additional pierce count to add.</param>
    public PierceUpgrade(int pierceCount = 1) 
        : base("Piercing Shot", $"Projectiles pierce through {pierceCount} additional enemies")
    {
        PierceCount = pierceCount;
    }
    
    /// <summary>
    /// Generate a new PierceUpgrade with random values based on rarity.
    /// </summary>
    /// <param name="rarity">The rarity level determining the value ranges.</param>
    /// <returns>A new PierceUpgrade with randomized values.</returns>
    public static PierceUpgrade Generate(UpgradeRarity rarity)
    {
        int pierceCount;
        
        switch (rarity)
        {
            case UpgradeRarity.Normal:
                pierceCount = Random.Range(1, 2);      // 1 pierce
                break;
            case UpgradeRarity.Common:
                pierceCount = Random.Range(1, 3);      // 1-2 pierce
                break;
            case UpgradeRarity.Epic:
                pierceCount = Random.Range(2, 4);      // 2-3 pierce
                break;
            case UpgradeRarity.Legendary:
                pierceCount = Random.Range(3, 6);      // 3-5 pierce
                break;
            default:
                pierceCount = 1;
                break;
        }
        
        return new PierceUpgrade(pierceCount);
    }
    
    /// <summary>
    /// Get the additional pierce count this upgrade provides.
    /// </summary>
    /// <returns>Additional pierce count.</returns>
    public int GetPierceCount()
    {
        return PierceCount;
    }
}
