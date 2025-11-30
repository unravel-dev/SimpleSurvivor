using Unravel.Core;

/// <summary>
/// Upgrade that modifies ability cooldowns by a flat value (in seconds).
/// This is added to the base cooldown before applying percentage reduction.
/// Positive values reduce cooldown, negative values increase cooldown.
/// Affects all abilities with cooldown timers.
/// </summary>
public class FlatCooldownModifierUpgrade : Upgrade
{
    /// <summary>
    /// Flat cooldown modifier in seconds. Positive values reduce cooldown, negative values increase cooldown.
    /// </summary>
    public float ModifierSeconds { get; set; }
    
    /// <summary>
    /// Create a new flat cooldown modifier upgrade with specified parameters.
    /// </summary>
    /// <param name="modifierSeconds">Flat cooldown modifier in seconds (positive = reduce, negative = increase).</param>
    public FlatCooldownModifierUpgrade(float modifierSeconds = -1.0f) 
        : base("Cooldown Modifier", 
            modifierSeconds < 0 
                ? $"Increases ability cooldowns by {Mathf.Abs(modifierSeconds):F1}s" 
                : $"Reduces ability cooldowns by {modifierSeconds:F1}s")
    {
        ModifierSeconds = modifierSeconds;
    }
    

    /// <summary>
    /// Generate a new FlatCooldownModifierUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minSeconds">Minimum modifier value in seconds.</param>
    /// <param name="maxSeconds">Maximum modifier value in seconds.</param>
    /// <returns>A new FlatCooldownModifierUpgrade with a random value from the range.</returns>
    public static FlatCooldownModifierUpgrade Generate(float minSeconds, float maxSeconds)
    {
        float modifierSeconds = Random.Range(minSeconds, maxSeconds);
        return new FlatCooldownModifierUpgrade(modifierSeconds);
    }

}

