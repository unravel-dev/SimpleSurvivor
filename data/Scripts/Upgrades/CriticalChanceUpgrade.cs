using Unravel.Core;

/// <summary>
/// Upgrade that increases critical strike chance by a percentage.
/// Affects the probability of dealing critical hits.
/// </summary>
public class CriticalChanceUpgrade : Upgrade
{
    /// <summary>
    /// Percentage critical chance increase this upgrade provides.
    /// </summary>
    public float ChancePercent { get; set; }
    
    /// <summary>
    /// Create a new critical chance upgrade with specified parameters.
    /// </summary>
    /// <param name="chancePercent">Percentage critical chance increase (e.g., 5 for 5%).</param>
    public CriticalChanceUpgrade(float chancePercent = 5.0f) 
        : base("Critical Strike Chance", $"Increases critical strike chance by {chancePercent:F1}%")
    {
        ChancePercent = chancePercent;
    }


    /// <summary>
    /// Generate a new CriticalChanceUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minPercent">Minimum critical chance percent value.</param>
    /// <param name="maxPercent">Maximum critical chance percent value.</param>
    /// <returns>A new CriticalChanceUpgrade with a random value from the range.</returns>
    public static CriticalChanceUpgrade Generate(float minPercent, float maxPercent)
    {
        float chancePercent = Random.Range(minPercent, maxPercent);
        return new CriticalChanceUpgrade(chancePercent);
    }
}
