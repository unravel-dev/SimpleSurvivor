using Unravel.Core;

/// <summary>
/// Upgrade that increases the maximum number of Doom stacks applied by Black Hole.
/// </summary>
public class IncreaseDoomStacksUpgrade : Upgrade
{
    /// <summary>
    /// Number of additional Doom stacks that can be applied.
    /// </summary>
    public int AdditionalStacks { get; set; }
    
    /// <summary>
    /// Create a new increase doom stacks upgrade.
    /// </summary>
    /// <param name="additionalStacks">Number of additional stacks to add.</param>
    public IncreaseDoomStacksUpgrade(int additionalStacks = 1) 
        : base("Cursed Vortex", $"Black Hole applies {additionalStacks} additional Doom stack(s)")
    {
        AdditionalStacks = additionalStacks;
    }
    
    /// <summary>
    /// Generate a new IncreaseDoomStacksUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minStacks">Minimum additional stacks.</param>
    /// <param name="maxStacks">Maximum additional stacks.</param>
    /// <returns>A new IncreaseDoomStacksUpgrade with a random value from the range.</returns>
    public static IncreaseDoomStacksUpgrade Generate(int minStacks, int maxStacks)
    {
        int stacks = Random.Range(minStacks, maxStacks + 1);
        return new IncreaseDoomStacksUpgrade(stacks);
    }
}

/// <summary>
/// Upgrade that increases the pull strength of Black Hole.
/// </summary>
public class IncreasePullStrengthUpgrade : Upgrade
{
    /// <summary>
    /// Percentage increase to pull strength (e.g., 50 = 50% stronger pull).
    /// </summary>
    public float PullStrengthPercent { get; set; }
    
    /// <summary>
    /// Create a new increase pull strength upgrade.
    /// </summary>
    /// <param name="pullStrengthPercent">Percentage increase to pull strength.</param>
    public IncreasePullStrengthUpgrade(float pullStrengthPercent = 50.0f) 
        : base("Gravitational Pull", $"Black Hole pulls enemies {pullStrengthPercent:F0}% stronger")
    {
        PullStrengthPercent = pullStrengthPercent;
    }
    
    /// <summary>
    /// Generate a new IncreasePullStrengthUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minPercent">Minimum percentage increase.</param>
    /// <param name="maxPercent">Maximum percentage increase.</param>
    /// <returns>A new IncreasePullStrengthUpgrade with a random value from the range.</returns>
    public static IncreasePullStrengthUpgrade Generate(float minPercent, float maxPercent)
    {
        float percent = Random.Range(minPercent, maxPercent);
        return new IncreasePullStrengthUpgrade(percent);
    }
    
    /// <summary>
    /// Get the pull strength multiplier.
    /// </summary>
    /// <returns>Multiplier value (e.g., 1.5 for 50% increase).</returns>
    public float GetPullStrengthMultiplier()
    {
        return 1.0f + (PullStrengthPercent / 100.0f);
    }
}

/// <summary>
/// Upgrade that increases the damage per stack of Doom applied by Black Hole.
/// </summary>
public class IncreaseDoomDamagePerStackUpgrade : Upgrade
{
    /// <summary>
    /// Percentage increase to Doom damage per stack (e.g., 30 = 30% more damage per stack).
    /// </summary>
    public float DamagePerStackPercent { get; set; }
    
    /// <summary>
    /// Create a new increase doom damage per stack upgrade.
    /// </summary>
    /// <param name="damagePerStackPercent">Percentage increase to damage per stack.</param>
    public IncreaseDoomDamagePerStackUpgrade(float damagePerStackPercent = 30.0f) 
        : base("Amplified Doom", $"Black Hole's Doom deals {damagePerStackPercent:F0}% more damage per stack")
    {
        DamagePerStackPercent = damagePerStackPercent;
    }
    
    /// <summary>
    /// Generate a new IncreaseDoomDamagePerStackUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minPercent">Minimum percentage increase.</param>
    /// <param name="maxPercent">Maximum percentage increase.</param>
    /// <returns>A new IncreaseDoomDamagePerStackUpgrade with a random value from the range.</returns>
    public static IncreaseDoomDamagePerStackUpgrade Generate(float minPercent, float maxPercent)
    {
        float percent = Random.Range(minPercent, maxPercent);
        return new IncreaseDoomDamagePerStackUpgrade(percent);
    }
    
    /// <summary>
    /// Get the damage per stack multiplier.
    /// </summary>
    /// <returns>Multiplier value (e.g., 1.3 for 30% increase).</returns>
    public float GetDamagePerStackMultiplier()
    {
        return 1.0f + (DamagePerStackPercent / 100.0f);
    }
}


