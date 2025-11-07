using Unravel.Core;

/// <summary>
/// Upgrade that increases the number of projectiles fired by abilities
/// </summary>
public class ProjectileCountUpgrade : Upgrade
{
    public int ProjectileCount { get; set; }

    public ProjectileCountUpgrade(int projectileCount = 1) 
        : base("Multi-Shot", $"Fire {projectileCount} additional projectile{(projectileCount > 1 ? "s" : "")}")
    {
        ProjectileCount = projectileCount;
    }

    /// <summary>
    /// Generates a random ProjectileCountUpgrade based on rarity
    /// </summary>
    /// <param name="rarity">The rarity level determining the upgrade power</param>
    /// <returns>A new ProjectileCountUpgrade with random values</returns>
    public static ProjectileCountUpgrade Generate(UpgradeRarity rarity)
    {
        int projectileCount;
        
        switch (rarity)
        {
            case UpgradeRarity.Normal:
                projectileCount = Random.Range(1, 2); // 1 additional projectile
                break;
            case UpgradeRarity.Common:
                projectileCount = Random.Range(1, 3); // 1-2 additional projectiles
                break;
            case UpgradeRarity.Epic:
                projectileCount = Random.Range(2, 4); // 2-3 additional projectiles
                break;
            case UpgradeRarity.Legendary:
                projectileCount = Random.Range(3, 6); // 3-5 additional projectiles
                break;
            default:
                projectileCount = 1;
                break;
        }

        var upgrade = new ProjectileCountUpgrade(projectileCount);
        return upgrade;
    }

    /// <summary>
    /// Generate a new ProjectileCountUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minCount">Minimum projectile count value.</param>
    /// <param name="maxCount">Maximum projectile count value.</param>
    /// <returns>A new ProjectileCountUpgrade with a random value from the range.</returns>
    public static ProjectileCountUpgrade Generate(int minCount, int maxCount)
    {
        int projectileCount = Random.Range(minCount, maxCount);
        return new ProjectileCountUpgrade(projectileCount);
    }
}
