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
    public int PierceCount { get; set; }
    
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
    /// Generate a new PierceUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minCount">Minimum pierce count value.</param>
    /// <param name="maxCount">Maximum pierce count value.</param>
    /// <returns>A new PierceUpgrade with a random value from the range.</returns>
    public static PierceUpgrade Generate(int minCount, int maxCount)
    {
        int pierceCount = Random.Range(minCount, maxCount);
        return new PierceUpgrade(pierceCount);
    }
}
