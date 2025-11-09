using Unravel.Core;

/// <summary>
/// Upgrade that provides a chance to trigger abilities multiple times (multicast).
/// Each 100% multicast chance guarantees one additional cast, with remainder as probability.
/// </summary>
public class MulticastUpgrade : Upgrade
{
    /// <summary>
    /// Percentage chance for multicast. 100% = 1 guaranteed extra cast, 150% = 1 guaranteed + 50% chance for another.
    /// </summary>
    public float MulticastPercent { get; set; }
    
    /// <summary>
    /// Create a new multicast upgrade with specified parameters.
    /// </summary>
    /// <param name="multicastPercent">Percentage multicast chance (e.g., 150 for 150%).</param>
    public MulticastUpgrade(float multicastPercent = 50.0f) 
        : base("Multicast", $"Increases multicast chance by {multicastPercent:F1}%")
    {
        MulticastPercent = multicastPercent;
    }
    
    /// <summary>
    /// Generate a new MulticastUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minPercent">Minimum multicast percent value.</param>
    /// <param name="maxPercent">Maximum multicast percent value.</param>
    /// <returns>A new MulticastUpgrade with a random value from the range.</returns>
    public static MulticastUpgrade Generate(float minPercent, float maxPercent)
    {
        float multicastPercent = Random.Range(minPercent, maxPercent);
        return new MulticastUpgrade(multicastPercent);
    }
    
    /// <summary>
    /// Calculate the number of additional casts based on multicast percentage.
    /// </summary>
    /// <returns>Number of additional casts to perform</returns>
    public int CalculateAdditionalCasts()
    {
        if (MulticastPercent <= 0)
            return 0;
        
        // Guaranteed additional casts (every 100%)
        int guaranteedCasts = Mathf.FloorToInt(MulticastPercent / 100.0f);
        
        // Probability for one more cast (remainder percentage)
        float remainderPercent = MulticastPercent % 100.0f;
        int probabilityCast = (Random.Range(0f, 100f) < remainderPercent) ? 1 : 0;
        
        return guaranteedCasts + probabilityCast;
    }
    
    /// <summary>
    /// Gets the multicast multiplier for display purposes.
    /// </summary>
    /// <returns>Multicast multiplier value</returns>
    public float GetMulticastMultiplier()
    {
        return 1.0f + (MulticastPercent / 100.0f);
    }
}
