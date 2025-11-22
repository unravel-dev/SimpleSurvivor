using Unravel.Core;

/// <summary>
/// Upgrade that reduces the tick interval for Plague, making it deal damage more frequently.
/// </summary>
public class FasterPlagueTickUpgrade : Upgrade
{
    /// <summary>
    /// Percentage reduction to tick interval (e.g., 30 = 30% faster, meaning 30% shorter interval).
    /// </summary>
    public float TickSpeedPercent { get; set; }
    
    /// <summary>
    /// Create a new faster plague tick upgrade.
    /// </summary>
    /// <param name="tickSpeedPercent">Percentage reduction to tick interval.</param>
    public FasterPlagueTickUpgrade(float tickSpeedPercent = 30.0f) 
        : base("Rapid Decay", $"Plague deals damage {tickSpeedPercent:F0}% more frequently")
    {
        TickSpeedPercent = tickSpeedPercent;
    }
    
    /// <summary>
    /// Generate a new FasterPlagueTickUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minPercent">Minimum tick speed percent.</param>
    /// <param name="maxPercent">Maximum tick speed percent.</param>
    /// <returns>A new FasterPlagueTickUpgrade with random values from the range.</returns>
    public static FasterPlagueTickUpgrade Generate(float minPercent, float maxPercent)
    {
        float speedPercent = Random.Range(minPercent, maxPercent);
        return new FasterPlagueTickUpgrade(speedPercent);
    }
    
    /// <summary>
    /// Get the tick interval multiplier (lower = faster ticks).
    /// </summary>
    /// <returns>Multiplier value (e.g., 0.7 for 30% faster).</returns>
    public float GetTickIntervalMultiplier()
    {
        return 1.0f - (TickSpeedPercent / 100.0f);
    }
}

/// <summary>
/// Upgrade that gives Plague a chance to apply poison stacks on hit.
/// </summary>
public class PlaguePoisonUpgrade : Upgrade
{
    /// <summary>
    /// Percent chance to apply poison on hit (0-100).
    /// </summary>
    public float PoisonChancePercent { get; set; }
    
    /// <summary>
    /// Number of poison stacks to apply when triggered.
    /// </summary>
    public int PoisonStacks { get; set; }
    
    /// <summary>
    /// Create a new plague poison upgrade.
    /// </summary>
    /// <param name="poisonChancePercent">Percent chance to apply poison (0-100).</param>
    /// <param name="poisonStacks">Number of poison stacks to apply.</param>
    public PlaguePoisonUpgrade(float poisonChancePercent = 30.0f, int poisonStacks = 1) 
        : base("Toxic Strike", $"Plague has {poisonChancePercent:F0}% chance to apply {poisonStacks} poison stack(s) on hit")
    {
        PoisonChancePercent = poisonChancePercent;
        PoisonStacks = poisonStacks;
    }
    
    /// <summary>
    /// Generate a new PlaguePoisonUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minChance">Minimum poison chance percent.</param>
    /// <param name="maxChance">Maximum poison chance percent.</param>
    /// <param name="minStacks">Minimum poison stacks to apply.</param>
    /// <param name="maxStacks">Maximum poison stacks to apply.</param>
    /// <returns>A new PlaguePoisonUpgrade with random values from the ranges.</returns>
    public static PlaguePoisonUpgrade Generate(float minChance, float maxChance, int minStacks = 1, int maxStacks = 1)
    {
        float chance = Random.Range(minChance, maxChance);
        int stacks = Random.Range(minStacks, maxStacks + 1);
        return new PlaguePoisonUpgrade(chance, stacks);
    }
}

/// <summary>
/// Upgrade that increases the duration of Plague casts.
/// </summary>
public class ExtendedPlagueDurationUpgrade : Upgrade
{
    /// <summary>
    /// Percentage increase to plague duration (e.g., 50 = 50% longer duration).
    /// </summary>
    public float DurationPercent { get; set; }
    
    /// <summary>
    /// Create a new extended plague duration upgrade.
    /// </summary>
    /// <param name="durationPercent">Percentage increase to duration.</param>
    public ExtendedPlagueDurationUpgrade(float durationPercent = 50.0f) 
        : base("Enduring Plague", $"Plague lasts {durationPercent:F0}% longer")
    {
        DurationPercent = durationPercent;
    }
    
    /// <summary>
    /// Generate a new ExtendedPlagueDurationUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minPercent">Minimum duration percent.</param>
    /// <param name="maxPercent">Maximum duration percent.</param>
    /// <returns>A new ExtendedPlagueDurationUpgrade with random values from the range.</returns>
    public static ExtendedPlagueDurationUpgrade Generate(float minPercent, float maxPercent)
    {
        float durationPercent = Random.Range(minPercent, maxPercent);
        return new ExtendedPlagueDurationUpgrade(durationPercent);
    }
    
    /// <summary>
    /// Get the duration multiplier.
    /// </summary>
    /// <returns>Multiplier value (e.g., 1.5 for 50% increase).</returns>
    public float GetDurationMultiplier()
    {
        return 1.0f + (DurationPercent / 100.0f);
    }
}

/// <summary>
/// Upgrade that gives Plague a chance to heal the player for a small amount.
/// </summary>
public class PlagueLifeDrainUpgrade : Upgrade
{
    /// <summary>
    /// Percent chance to heal on hit (0-100).
    /// </summary>
    public float HealChancePercent { get; set; }
    
    /// <summary>
    /// Amount of health to restore when triggered.
    /// </summary>
    public int HealAmount { get; set; }
    
    /// <summary>
    /// Create a new plague life drain upgrade.
    /// </summary>
    /// <param name="healChancePercent">Percent chance to heal (0-100).</param>
    /// <param name="healAmount">Amount of health to restore.</param>
    public PlagueLifeDrainUpgrade(float healChancePercent = 20.0f, int healAmount = 2) 
        : base("Life Drain", $"Plague has {healChancePercent:F0}% chance to heal you for {healAmount} health per tick")
    {
        HealChancePercent = healChancePercent;
        HealAmount = healAmount;
    }
    
    /// <summary>
    /// Generate a new PlagueLifeDrainUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minChance">Minimum heal chance percent.</param>
    /// <param name="maxChance">Maximum heal chance percent.</param>
    /// <param name="minAmount">Minimum heal amount.</param>
    /// <param name="maxAmount">Maximum heal amount.</param>
    /// <returns>A new PlagueLifeDrainUpgrade with random values from the ranges.</returns>
    public static PlagueLifeDrainUpgrade Generate(float minChance, float maxChance, int minAmount, int maxAmount)
    {
        float chance = Random.Range(minChance, maxChance);
        int amount = Random.Range(minAmount, maxAmount + 1);
        return new PlagueLifeDrainUpgrade(chance, amount);
    }
}

