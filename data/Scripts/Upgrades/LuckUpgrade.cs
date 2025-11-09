using Unravel.Core;

/// <summary>
/// Upgrade that increases luck for better loot drops and rare item chances.
/// Affects the probability of finding better items and experience bonuses.
/// </summary>
public class LuckUpgrade : Upgrade
{
    /// <summary>
    /// Percentage luck increase this upgrade provides.
    /// </summary>
    public float LuckPercent { get; set; }
    
    /// <summary>
    /// Create a new luck upgrade with specified parameters.
    /// </summary>
    /// <param name="luckPercent">Percentage luck increase (e.g., 15 for 15%).</param>
    public LuckUpgrade(float luckPercent = 15.0f) 
        : base("Luck", $"Increases luck by {luckPercent:F1}%")
    {
        LuckPercent = luckPercent;
    }

    /// <summary>
    /// Generate a new LuckUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minPercent">Minimum luck percent value.</param>
    /// <param name="maxPercent">Maximum luck percent value.</param>
    /// <returns>A new LuckUpgrade with a random value from the range.</returns>
    public static LuckUpgrade Generate(float minPercent, float maxPercent)
    {
        float luckPercent = Random.Range(minPercent, maxPercent);
        return new LuckUpgrade(luckPercent);
    }

    /// <summary>
    /// Gets the luck multiplier (1.0 + percentage/100).
    /// </summary>
    /// <returns>Luck multiplier value.</returns>
    public float GetLuckMultiplier()
    {
        return 1.0f + (LuckPercent / 100.0f);
    }
}
