using Unravel.Core;

/// <summary>
/// Upgrade that increases the duration of abilities and effects.
/// Useful for orbital abilities, DoT effects, turrets, etc.
/// </summary>
public class DurationUpgrade : Upgrade
{
    public float DurationPercent { get; set; }

    public DurationUpgrade(float durationPercent = 20.0f)
        : base("Duration Boost", $"Increases ability duration by {durationPercent:F1}%")
    {
        DurationPercent = durationPercent;
    }

    public static DurationUpgrade Generate(float minPercent, float maxPercent)
    {
        float durationPercent = Random.Range(minPercent, maxPercent);
        return new DurationUpgrade(durationPercent);
    }

    /// <summary>
    /// Get the duration multiplier (e.g., 20% = 1.2x duration).
    /// </summary>
    public float GetDurationMultiplier()
    {
        return 1.0f + (DurationPercent / 100.0f);
    }
}

