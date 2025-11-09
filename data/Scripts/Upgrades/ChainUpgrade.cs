using Unravel.Core;

/// <summary>
/// Upgrade that increases chain count for projectiles.
/// Allows projectiles to jump between multiple enemies.
/// </summary>
public class ChainUpgrade : Upgrade
{
    /// <summary>
    /// Additional chain count this upgrade provides.
    /// </summary>
    public int ChainCount { get; set; }
    
    /// <summary>
    /// Create a new chain upgrade with specified parameters.
    /// </summary>
    /// <param name="chainCount">Additional chain count to add.</param>
    public ChainUpgrade(int chainCount = 2) 
        : base("Chain Lightning", $"Projectiles chain to {chainCount} additional enemies")
    {
        ChainCount = chainCount;
    }
    

    /// <summary>
    /// Generate a new ChainUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minCount">Minimum chain count value.</param>
    /// <param name="maxCount">Maximum chain count value.</param>
    /// <returns>A new ChainUpgrade with a random value from the range.</returns>
    public static ChainUpgrade Generate(int minCount, int maxCount)
    {
        int chainCount = Random.Range(minCount, maxCount);
        return new ChainUpgrade(chainCount);
    }
}
