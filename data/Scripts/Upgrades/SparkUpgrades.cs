using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Spark specific upgrades.
/// </summary>
[ScriptSourceFile]
public class LightningSplitUpgrade : Upgrade
{
    public int SplitCount { get; set; }

    public LightningSplitUpgrade(int splitCount = 2)
        : base("Lightning Split", $"Spark can split {splitCount} times, creating 2 projectiles each split")
    {
        SplitCount = splitCount;
    }

    /// <summary>
    /// Generate a new LightningSplitUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minSplitCount">Minimum split count.</param>
    /// <param name="maxSplitCount">Maximum split count.</param>
    /// <returns>A new LightningSplitUpgrade with random values from the ranges.</returns>
    public static LightningSplitUpgrade Generate(int minSplitCount = 2, int maxSplitCount = 3)
    {
        int splitCount = Random.Range(minSplitCount, maxSplitCount + 1);
        return new LightningSplitUpgrade(splitCount);
    }
}

/// <summary>
/// Upgrade that makes Spark chains create explosions.
/// </summary>
public class LightningChainExplosionUpgrade : Upgrade
{
    /// <summary>
    /// Explosion radius for chain explosions.
    /// </summary>
    public float ExplosionRadius { get; set; }
    
    /// <summary>
    /// Explosion damage percentage of base damage.
    /// </summary>
    public float ExplosionDamagePercent { get; set; }
    
    /// <summary>
    /// Create a new lightning chain explosion upgrade.
    /// </summary>
    /// <param name="explosionRadius">Radius of the explosion.</param>
    /// <param name="explosionDamagePercent">Damage percentage of base damage (e.g., 50 = 50% of base damage).</param>
    public LightningChainExplosionUpgrade(float explosionRadius = 2.0f, float explosionDamagePercent = 50.0f) 
        : base("Chain Explosion", $"Each chain creates a small explosion dealing {explosionDamagePercent:F0}% damage in a {explosionRadius:F1}m radius")
    {
        ExplosionRadius = explosionRadius;
        ExplosionDamagePercent = explosionDamagePercent;
    }
    
    /// <summary>
    /// Generate a new LightningChainExplosionUpgrade with values from the specified ranges.
    /// </summary>
    public static LightningChainExplosionUpgrade Generate(float minRadius, float maxRadius, float minDamagePercent, float maxDamagePercent)
    {
        float radius = Random.Range(minRadius, maxRadius);
        float damagePercent = Random.Range(minDamagePercent, maxDamagePercent);
        return new LightningChainExplosionUpgrade(radius, damagePercent);
    }
}

/// <summary>
/// Upgrade that makes Spark chains stun enemies.
/// </summary>
public class LightningStunUpgrade : Upgrade
{
    /// <summary>
    /// Stun duration in seconds.
    /// </summary>
    public float StunDuration { get; set; }
    
    /// <summary>
    /// Create a new lightning stun upgrade.
    /// </summary>
    /// <param name="stunDuration">Duration of the stun in seconds.</param>
    public LightningStunUpgrade(float stunDuration = 0.5f) 
        : base("Stun Chain", $"Chains stun enemies for {stunDuration:F1} seconds")
    {
        StunDuration = stunDuration;
    }
    
    /// <summary>
    /// Generate a new LightningStunUpgrade with a value from the specified range.
    /// </summary>
    public static LightningStunUpgrade Generate(float minDuration, float maxDuration)
    {
        float duration = Random.Range(minDuration, maxDuration);
        return new LightningStunUpgrade(duration);
    }
}

/// <summary>
/// Upgrade that makes Spark bounce between enemies instead of chaining normally.
/// </summary>
public class LightningBouncingUpgrade : Upgrade
{
    /// <summary>
    /// Create a new lightning bouncing upgrade.
    /// </summary>
    public LightningBouncingUpgrade() 
        : base("Bouncing Lightning", "Lightning bounces between enemies, can hit the same enemy multiple times")
    {
    }
    
    /// <summary>
    /// Generate a new LightningBouncingUpgrade.
    /// </summary>
    public static LightningBouncingUpgrade Generate()
    {
        return new LightningBouncingUpgrade();
    }
}

