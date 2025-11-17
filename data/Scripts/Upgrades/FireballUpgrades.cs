using Unravel.Core;

/// <summary>
/// Upgrade that gives Fireball a chance to apply burn stacks on hit.
/// Works for both direct hits and area of effect damage.
/// </summary>
public class BurnOnHitUpgrade : Upgrade
{
    /// <summary>
    /// Percent chance to apply burn stacks on hit (0-100).
    /// </summary>
    public float BurnChancePercent { get; set; }
    
    /// <summary>
    /// Number of burn stacks to apply when triggered.
    /// </summary>
    public int BurnStacks { get; set; }
    
    /// <summary>
    /// Create a new burn on hit upgrade.
    /// </summary>
    /// <param name="burnChancePercent">Percent chance to apply burn (0-100).</param>
    /// <param name="burnStacks">Number of burn stacks to apply.</param>
    public BurnOnHitUpgrade(float burnChancePercent = 50.0f, int burnStacks = 1) 
        : base("Burn on Hit", $"Fireball has {burnChancePercent:F0}% chance to apply {burnStacks} burn stack(s) on hit")
    {
        BurnChancePercent = burnChancePercent;
        BurnStacks = burnStacks;
    }
    
    /// <summary>
    /// Generate a new BurnOnHitUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minChance">Minimum burn chance percent.</param>
    /// <param name="maxChance">Maximum burn chance percent.</param>
    /// <param name="minStacks">Minimum burn stacks to apply.</param>
    /// <param name="maxStacks">Maximum burn stacks to apply.</param>
    /// <returns>A new BurnOnHitUpgrade with random values from the ranges.</returns>
    public static BurnOnHitUpgrade Generate(float minChance, float maxChance, int minStacks = 1, int maxStacks = 1)
    {
        float chance = Random.Range(minChance, maxChance);
        int stacks = Random.Range(minStacks, maxStacks + 1);
        return new BurnOnHitUpgrade(chance, stacks);
    }
    
    /// <summary>
    /// Check if burn should be applied based on chance.
    /// </summary>
    /// <returns>True if burn should be applied, false otherwise.</returns>
    public bool ShouldApplyBurn()
    {
        return Random.Range(0.0f, 100.0f) < BurnChancePercent;
    }
}

