using Unravel.Core;

/// <summary>
/// Upgrade that modifies Spark ability cooldown by a flat value (in seconds).
/// This is added to the base cooldown before applying percentage reduction.
/// Positive values reduce cooldown, negative values increase cooldown.
/// Spark-specific upgrade.
/// </summary>
public class SparkFlatCooldownModifierUpgrade : Upgrade
{
    /// <summary>
    /// Flat cooldown modifier in seconds. Positive values reduce cooldown, negative values increase cooldown.
    /// </summary>
    public float ModifierSeconds { get; set; }
    
    /// <summary>
    /// Create a new Spark flat cooldown modifier upgrade with specified parameters.
    /// </summary>
    /// <param name="modifierSeconds">Flat cooldown modifier in seconds (positive = reduce, negative = increase).</param>
    public SparkFlatCooldownModifierUpgrade(float modifierSeconds = -1.0f) 
        : base("Spark Cooldown Modifier", 
            modifierSeconds < 0 
                ? $"Increases Spark cooldown by {Mathf.Abs(modifierSeconds):F1}s" 
                : $"Reduces Spark cooldown by {modifierSeconds:F1}s")
    {
        ModifierSeconds = modifierSeconds;
    }
    

    /// <summary>
    /// Generate a new SparkFlatCooldownModifierUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minSeconds">Minimum modifier value in seconds.</param>
    /// <param name="maxSeconds">Maximum modifier value in seconds.</param>
    /// <returns>A new SparkFlatCooldownModifierUpgrade with a random value from the range.</returns>
    public static SparkFlatCooldownModifierUpgrade Generate(float minSeconds, float maxSeconds)
    {
        float modifierSeconds = Random.Range(minSeconds, maxSeconds);
        return new SparkFlatCooldownModifierUpgrade(modifierSeconds);
    }

}

