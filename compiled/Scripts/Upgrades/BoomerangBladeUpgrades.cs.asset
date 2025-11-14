using Unravel.Core;

/// <summary>
/// Upgrade that allows Boomerang Blade to spawn multiple blades at once.
/// </summary>
public class MultipleBladesUpgrade : Upgrade
{
    /// <summary>
    /// Number of additional blades to spawn (total blades = 1 + AdditionalBladeCount).
    /// </summary>
    public int AdditionalBladeCount { get; set; }
    
    /// <summary>
    /// Create a new multiple blades upgrade.
    /// </summary>
    /// <param name="additionalBladeCount">Number of additional blades (1 = 2 total, 2 = 3 total, etc.)</param>
    public MultipleBladesUpgrade(int additionalBladeCount = 1) 
        : base("Multiple Blades", $"Spawns {additionalBladeCount + 1} blades instead of 1")
    {
        AdditionalBladeCount = additionalBladeCount;
    }
    
    /// <summary>
    /// Generate a new MultipleBladesUpgrade with a value from the specified range.
    /// </summary>
    /// <param name="minCount">Minimum additional blade count.</param>
    /// <param name="maxCount">Maximum additional blade count.</param>
    /// <returns>A new MultipleBladesUpgrade with a random value from the range.</returns>
    public static MultipleBladesUpgrade Generate(int minCount, int maxCount)
    {
        int bladeCount = Random.Range(minCount, maxCount + 1);
        return new MultipleBladesUpgrade(bladeCount);
    }
    
    /// <summary>
    /// Get the total number of blades to spawn (base 1 + additional).
    /// </summary>
    /// <returns>Total blade count.</returns>
    public int GetTotalBladeCount()
    {
        return 1 + AdditionalBladeCount;
    }
}

/// <summary>
/// Upgrade that increases the rotation speed of Boomerang Blade.
/// </summary>
public class FasterRotationUpgrade : Upgrade
{
    /// <summary>
    /// Percentage increase to rotation speed (e.g., 50 = 50% faster).
    /// </summary>
    public float RotationSpeedPercent { get; set; }
    
    /// <summary>
    /// Create a new faster rotation upgrade.
    /// </summary>
    /// <param name="rotationSpeedPercent">Percentage increase to rotation speed.</param>
    public FasterRotationUpgrade(float rotationSpeedPercent = 50.0f) 
        : base("Faster Rotation", $"Blades orbit {rotationSpeedPercent:F0}% faster, hitting enemies more frequently")
    {
        RotationSpeedPercent = rotationSpeedPercent;
    }
    
    /// <summary>
    /// Generate a new FasterRotationUpgrade with a value from the specified range.
    /// </summary>
    public static FasterRotationUpgrade Generate(float minPercent, float maxPercent)
    {
        float speedPercent = Random.Range(minPercent, maxPercent);
        return new FasterRotationUpgrade(speedPercent);
    }
    
    /// <summary>
    /// Get the rotation speed multiplier.
    /// </summary>
    /// <returns>Multiplier value (e.g., 1.5 for 50% increase).</returns>
    public float GetRotationSpeedMultiplier()
    {
        return 1.0f + (RotationSpeedPercent / 100.0f);
    }
}

/// <summary>
/// Upgrade that makes Boomerang Blade orbit in opposite directions.
/// </summary>
public class DualOrbitUpgrade : Upgrade
{
    /// <summary>
    /// Create a new dual orbit upgrade.
    /// </summary>
    public DualOrbitUpgrade() 
        : base("Dual Orbit", "Blades orbit in counter-rotating directions")
    {
    }
}

/// <summary>
/// Upgrade that enhances the ping-pong orbit effect of Boomerang Blade.
/// Increases the radius range and speed of the ping-pong oscillation.
/// </summary>
public class PingPongOrbitUpgrade : Upgrade
{
    /// <summary>
    /// Percentage increase to maximum ping-pong radius (e.g., 50 = 50% larger max radius).
    /// </summary>
    public float MaxRadiusPercent { get; set; }
    
    /// <summary>
    /// Percentage increase to ping-pong speed (e.g., 100 = 100% faster oscillation).
    /// </summary>
    public float PingPongSpeedPercent { get; set; }
    
    /// <summary>
    /// Create a new ping-pong orbit upgrade.
    /// </summary>
    /// <param name="maxRadiusPercent">Percentage increase to maximum radius.</param>
    /// <param name="pingPongSpeedPercent">Percentage increase to ping-pong speed.</param>
    public PingPongOrbitUpgrade(float maxRadiusPercent = 50.0f, float pingPongSpeedPercent = 50.0f) 
        : base("Ping-Pong Orbit", $"Blade orbit expands {maxRadiusPercent:F0}% further and oscillates {pingPongSpeedPercent:F0}% faster")
    {
        MaxRadiusPercent = maxRadiusPercent;
        PingPongSpeedPercent = pingPongSpeedPercent;
    }
    
    /// <summary>
    /// Generate a new PingPongOrbitUpgrade with random values.
    /// </summary>
    public static PingPongOrbitUpgrade Generate(float minRadiusPercent, float maxRadiusPercent, float minSpeedPercent, float maxSpeedPercent)
    {
        float radiusPercent = Random.Range(minRadiusPercent, maxRadiusPercent);
        float speedPercent = Random.Range(minSpeedPercent, maxSpeedPercent);
        return new PingPongOrbitUpgrade(radiusPercent, speedPercent);
    }
    
    /// <summary>
    /// Get the maximum radius multiplier.
    /// </summary>
    /// <returns>Multiplier value (e.g., 1.5 for 50% increase).</returns>
    public float GetMaxRadiusMultiplier()
    {
        return 1.0f + (MaxRadiusPercent / 100.0f);
    }
    
    /// <summary>
    /// Get the ping-pong speed multiplier.
    /// </summary>
    /// <returns>Multiplier value (e.g., 1.5 for 50% increase).</returns>
    public float GetPingPongSpeedMultiplier()
    {
        return 1.0f + (PingPongSpeedPercent / 100.0f);
    }
}

/// <summary>
/// Upgrade that makes Boomerang Blade return to player after orbit duration.
/// The blade can pierce through enemies on its return path.
/// </summary>
public class ReturningBladeUpgrade : Upgrade
{
    /// <summary>
    /// Create a new returning blade upgrade.
    /// </summary>
    public ReturningBladeUpgrade() 
        : base("Returning Blade", "Blade returns to you after orbiting, piercing through enemies on the way back")
    {
    }
    
    /// <summary>
    /// Generate a new ReturningBladeUpgrade.
    /// </summary>
    public static ReturningBladeUpgrade Generate()
    {
        return new ReturningBladeUpgrade();
    }
}

/// <summary>
/// Upgrade that increases visual spin speed and damage per hit.
/// </summary>
public class SpinningSlashUpgrade : Upgrade
{
    /// <summary>
    /// Percentage increase to visual spin speed.
    /// </summary>
    public float SpinSpeedPercent { get; set; }
    
    /// <summary>
    /// Percentage increase to damage per hit.
    /// </summary>
    public float DamagePercent { get; set; }
    
    /// <summary>
    /// Create a new spinning slash upgrade.
    /// </summary>
    /// <param name="spinSpeedPercent">Percentage increase to visual spin speed.</param>
    /// <param name="damagePercent">Percentage increase to damage.</param>
    public SpinningSlashUpgrade(float spinSpeedPercent = 100.0f, float damagePercent = 25.0f) 
        : base("Spinning Slash", $"Blades spin {spinSpeedPercent:F0}% faster and deal {damagePercent:F0}% more damage per hit")
    {
        SpinSpeedPercent = spinSpeedPercent;
        DamagePercent = damagePercent;
    }
    
    /// <summary>
    /// Generate a new SpinningSlashUpgrade with random values.
    /// </summary>
    public static SpinningSlashUpgrade Generate(float minSpinSpeed, float maxSpinSpeed, float minDamage, float maxDamage)
    {
        float spinSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);
        float damage = Random.Range(minDamage, maxDamage);
        return new SpinningSlashUpgrade(spinSpeed, damage);
    }
    
    /// <summary>
    /// Get the visual spin speed multiplier.
    /// </summary>
    public float GetSpinSpeedMultiplier()
    {
        return 1.0f + (SpinSpeedPercent / 100.0f);
    }
    
    /// <summary>
    /// Get the damage multiplier.
    /// </summary>
    public float GetDamageMultiplier()
    {
        return 1.0f + (DamagePercent / 100.0f);
    }
}

