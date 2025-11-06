using Unravel.Core;

/// <summary>
/// Upgrade that increases the number of projectiles fired by abilities
/// </summary>
public class ProjectileCountUpgrade : Upgrade
{
    public int ProjectileCount { get; private set; }

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
    /// Gets the additional projectile count
    /// </summary>
    /// <returns>Number of additional projectiles to fire</returns>
    public int GetProjectileCount()
    {
        return ProjectileCount;
    }
}
