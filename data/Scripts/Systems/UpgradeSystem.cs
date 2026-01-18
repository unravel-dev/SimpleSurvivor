using System;
using System.Collections.Generic;
using Unravel.Core;

/// <summary>
/// Centralized static system for managing all player upgrades. Simple list-based storage
/// with type-based categorization using actual C# types.
/// </summary>
public static class UpgradeSystem
{
    // Enable debug logging for upgrade operations
    public static bool DebugUpgrades = false;

    // Simple storage for all active upgrades
    private static readonly List<Upgrade> activeUpgrades = new List<Upgrade>();

    // Dictionary for fast lookup by C# type
    private static readonly Dictionary<System.Type, List<Upgrade>> upgradesByType = new Dictionary<System.Type, List<Upgrade>>();
    
    // Static accumulated upgrade instances - direct access, no searches needed
    private static readonly DamageUpgrade accumulatedDamageUpgrade = new DamageUpgrade(0.0f);
    private static readonly MulticastUpgrade accumulatedMulticastUpgrade = new MulticastUpgrade(0.0f);
    private static readonly PierceUpgrade accumulatedPierceUpgrade = new PierceUpgrade(0);
    private static readonly ChainUpgrade accumulatedChainUpgrade = new ChainUpgrade(0);
    private static readonly CooldownReductionUpgrade accumulatedCooldownReductionUpgrade = new CooldownReductionUpgrade(0.0f);
    private static readonly MovementSpeedUpgrade accumulatedMovementSpeedUpgrade = new MovementSpeedUpgrade(0.0f);
    private static readonly MaxHealthUpgrade accumulatedMaxHealthUpgrade = new MaxHealthUpgrade(0);
    private static readonly CriticalChanceUpgrade accumulatedCriticalChanceUpgrade = new CriticalChanceUpgrade(0.0f);
    private static readonly CriticalDamageUpgrade accumulatedCriticalDamageUpgrade = new CriticalDamageUpgrade(0.0f);
    private static readonly PickupRadiusUpgrade accumulatedPickupRadiusUpgrade = new PickupRadiusUpgrade(0.0f);
    private static readonly LuckUpgrade accumulatedLuckUpgrade = new LuckUpgrade(0.0f);
    
    // Temporary magnet effect (pickup range multiplier)
    private static float magnetMultiplier = 1.0f;
    private static float magnetRemainingDuration = 0.0f;
    private static readonly AreaOfEffectUpgrade accumulatedAreaOfEffectUpgrade = new AreaOfEffectUpgrade(0.0f);
    private static readonly DurationUpgrade accumulatedDurationUpgrade = new DurationUpgrade(0.0f);
    
    // Ability-specific accumulated upgrade instances
    private static readonly IncreaseDoomStacksUpgrade accumulatedBlackHoleDoomStacksUpgrade = new IncreaseDoomStacksUpgrade(0);
    private static readonly IncreasePullStrengthUpgrade accumulatedBlackHolePullStrengthUpgrade = new IncreasePullStrengthUpgrade(0.0f);
    private static readonly IncreaseDoomDamagePerStackUpgrade accumulatedBlackHoleDoomDamageUpgrade = new IncreaseDoomDamagePerStackUpgrade(0.0f);
    private static readonly MultipleBladesUpgrade accumulatedBoomerangMultipleBladesUpgrade = new MultipleBladesUpgrade(0);
    private static readonly FasterRotationUpgrade accumulatedBoomerangFasterRotationUpgrade = new FasterRotationUpgrade(0.0f);
    private static readonly PingPongOrbitUpgrade accumulatedBoomerangPingPongOrbitUpgrade = new PingPongOrbitUpgrade(0.0f, 0.0f);
    private static readonly SpinningSlashUpgrade accumulatedBoomerangSpinningSlashUpgrade = new SpinningSlashUpgrade(0.0f, 0.0f);
    private static readonly BurnOnHitUpgrade accumulatedBurnOnHitUpgrade = new BurnOnHitUpgrade(0.0f, 0);
    private static readonly LightningSplitUpgrade accumulatedLightningSplitUpgrade = new LightningSplitUpgrade(0);
    private static readonly LightningChainExplosionUpgrade accumulatedLightningChainExplosionUpgrade = new LightningChainExplosionUpgrade(0.0f, 0.0f);
    private static readonly LightningStunUpgrade accumulatedLightningStunUpgrade = new LightningStunUpgrade(0.0f);
    private static bool hasLightningBouncingUpgrade = false;
    private static readonly FasterPlagueTickUpgrade accumulatedPlagueFasterTickUpgrade = new FasterPlagueTickUpgrade(0.0f);
    private static readonly PlaguePoisonUpgrade accumulatedPlaguePoisonUpgrade = new PlaguePoisonUpgrade(0.0f, 0);
    private static readonly ExtendedPlagueDurationUpgrade accumulatedPlagueExtendedDurationUpgrade = new ExtendedPlagueDurationUpgrade(0.0f);
    private static readonly PlagueLifeDrainUpgrade accumulatedPlagueLifeDrainUpgrade = new PlagueLifeDrainUpgrade(0.0f, 0);
    private static readonly SparkFlatCooldownModifierUpgrade accumulatedSparkFlatCooldownModifierUpgrade = new SparkFlatCooldownModifierUpgrade(0.0f);
    private static readonly FireballNovaUpgrade accumulatedFireballNovaUpgrade = new FireballNovaUpgrade(0, 0.0f);

    /// <summary>
    /// Get the total number of active upgrades.
    /// </summary>
    public static int TotalUpgradeCount => activeUpgrades.Count;

    /// <summary>
    /// Add an upgrade to the system.
    /// </summary>
    /// <param name="upgrade">The upgrade to add.</param>
    public static void AddUpgrade(Upgrade upgrade)
    {
        if (upgrade == null)
        {
            Log.Warning("UpgradeSystem: Cannot add null upgrade");
            return;
        }

        // Add to main list
        activeUpgrades.Add(upgrade);

        // Add to type dictionary
        System.Type upgradeType = upgrade.GetType();
        if (!upgradesByType.ContainsKey(upgradeType))
        {
            upgradesByType[upgradeType] = new List<Upgrade>();
        }
        upgradesByType[upgradeType].Add(upgrade);

        // Recalculate accumulated upgrade values
        RecalculateAccumulatedUpgrades();

        if (DebugUpgrades)
        {
            Log.Info($"UpgradeSystem: Added upgrade {upgrade.Name} ({upgradeType.Name}) - Total: {activeUpgrades.Count}");
        }
    }

    /// <summary>
    /// Remove an upgrade from the system.
    /// </summary>
    /// <param name="upgrade">The upgrade to remove.</param>
    /// <returns>True if the upgrade was successfully removed.</returns>
    public static bool RemoveUpgrade(Upgrade upgrade)
    {
        if (upgrade == null)
            return false;

        // Remove from main list
        bool removed = activeUpgrades.Remove(upgrade);

        if (removed)
        {
            // Remove from type dictionary
            System.Type upgradeType = upgrade.GetType();
            if (upgradesByType.ContainsKey(upgradeType))
            {
                upgradesByType[upgradeType].Remove(upgrade);

                // Clean up empty type lists
                if (upgradesByType[upgradeType].Count == 0)
                {
                    upgradesByType.Remove(upgradeType);
                }
            }

            // Recalculate accumulated upgrade values
            RecalculateAccumulatedUpgrades();

            if (DebugUpgrades)
            {
                Log.Info($"UpgradeSystem: Removed upgrade {upgrade.Name} - Total: {activeUpgrades.Count}");
            }
        }

        return removed;
    }

    /// <summary>
    /// Get all upgrades of a specific C# type.
    /// </summary>
    /// <typeparam name="T">The upgrade type to get.</typeparam>
    /// <returns>List of upgrades of the specified type.</returns>
    public static List<T> GetUpgradesByType<T>() where T : Upgrade
    {
        var result = new List<T>();
        System.Type targetType = typeof(T);

        if (upgradesByType.ContainsKey(targetType))
        {
            foreach (var upgrade in upgradesByType[targetType])
            {
                if (upgrade is T typedUpgrade)
                {
                    result.Add(typedUpgrade);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Get all upgrades of a specific C# type (non-generic version).
    /// </summary>
    /// <param name="upgradeType">The upgrade type to get.</param>
    /// <returns>List of upgrades of the specified type.</returns>
    public static List<Upgrade> GetUpgradesByType(System.Type upgradeType)
    {
        if (upgradesByType.ContainsKey(upgradeType))
        {
            return new List<Upgrade>(upgradesByType[upgradeType]);
        }

        return new List<Upgrade>();
    }

    /// <summary>
    /// Get the count of upgrades of a specific type.
    /// </summary>
    /// <typeparam name="T">The upgrade type to count.</typeparam>
    /// <returns>Number of upgrades of the specified type.</returns>
    public static int GetUpgradeCount<T>() where T : Upgrade
    {
        return GetUpgradesByType<T>().Count;
    }

    /// <summary>
    /// Check if any upgrade of a specific type is active.
    /// </summary>
    /// <typeparam name="T">The upgrade type to check for.</typeparam>
    /// <returns>True if at least one upgrade of the specified type is active.</returns>
    public static bool HasUpgradeOfType<T>() where T : Upgrade
    {
        return upgradesByType.ContainsKey(typeof(T)) && upgradesByType[typeof(T)].Count > 0;
    }

    /// <summary>
    /// Get all active upgrades.
    /// </summary>
    /// <returns>Read-only list of all active upgrades.</returns>
    public static IReadOnlyList<Upgrade> GetAllUpgrades()
    {
        return activeUpgrades.AsReadOnly();
    }


    /// <summary>
    /// Clear all upgrades from the system.
    /// </summary>
    public static void ClearAllUpgrades()
    {
        activeUpgrades.Clear();
        upgradesByType.Clear();
        
        // Reset all accumulated upgrade instances
        RecalculateAccumulatedUpgrades();
        
        // Reset card pool when clearing upgrades (new game/restart)
        UpgradeCardGenerator.ResetCardPool();

        if (DebugUpgrades)
        {
            Log.Info("UpgradeSystem: Cleared all upgrades");
        }
    }

    /// <summary>
    /// Recalculate all accumulated upgrade values from active upgrades
    /// </summary>
    private static void RecalculateAccumulatedUpgrades()
    {
        // Reset all accumulated values
        accumulatedDamageUpgrade.DamagePercent = 0.0f;
        accumulatedMulticastUpgrade.MulticastPercent = 0.0f;
        accumulatedPierceUpgrade.PierceCount = 0;
        accumulatedChainUpgrade.ChainCount = 0;
        accumulatedCooldownReductionUpgrade.ReductionPercent = 0.0f;
        accumulatedMovementSpeedUpgrade.SpeedPercent = 0.0f;
        accumulatedMaxHealthUpgrade.HealthIncrease = 0;
        accumulatedCriticalChanceUpgrade.ChancePercent = 0.0f;
        accumulatedCriticalDamageUpgrade.DamagePercent = 0.0f;
        accumulatedPickupRadiusUpgrade.RadiusPercent = 0.0f;
        accumulatedLuckUpgrade.LuckPercent = 0.0f;
        accumulatedAreaOfEffectUpgrade.AoePercent = 0.0f;
        accumulatedDurationUpgrade.DurationPercent = 0.0f;
        
        // Reset ability-specific accumulated values
        accumulatedBlackHoleDoomStacksUpgrade.AdditionalStacks = 0;
        accumulatedBlackHolePullStrengthUpgrade.PullStrengthPercent = 0.0f;
        accumulatedBlackHoleDoomDamageUpgrade.DamagePerStackPercent = 0.0f;
        accumulatedBoomerangMultipleBladesUpgrade.AdditionalBladeCount = 0;
        accumulatedBoomerangFasterRotationUpgrade.RotationSpeedPercent = 0.0f;
        accumulatedBoomerangPingPongOrbitUpgrade.MaxRadiusPercent = 0.0f;
        accumulatedBoomerangPingPongOrbitUpgrade.PingPongSpeedPercent = 0.0f;
        accumulatedBoomerangSpinningSlashUpgrade.SpinSpeedPercent = 0.0f;
        accumulatedBoomerangSpinningSlashUpgrade.DamagePercent = 0.0f;
        accumulatedBurnOnHitUpgrade.BurnChancePercent = 0.0f;
        accumulatedBurnOnHitUpgrade.BurnStacks = 0;
        accumulatedLightningSplitUpgrade.SplitCount = 0;
        accumulatedLightningChainExplosionUpgrade.ExplosionRadius = 0.0f;
        accumulatedLightningChainExplosionUpgrade.ExplosionDamagePercent = 0.0f;
        accumulatedLightningStunUpgrade.StunDuration = 0.0f;
        hasLightningBouncingUpgrade = false;
        accumulatedPlagueFasterTickUpgrade.TickSpeedPercent = 0.0f;
        accumulatedPlaguePoisonUpgrade.PoisonChancePercent = 0.0f;
        accumulatedPlaguePoisonUpgrade.PoisonStacks = 0;
        accumulatedPlagueExtendedDurationUpgrade.DurationPercent = 0.0f;
        accumulatedPlagueLifeDrainUpgrade.HealChancePercent = 0.0f;
        accumulatedPlagueLifeDrainUpgrade.HealAmount = 0;
        accumulatedSparkFlatCooldownModifierUpgrade.ModifierSeconds = 0.0f;
        accumulatedFireballNovaUpgrade.NovaProjectileCount = 0;
        accumulatedFireballNovaUpgrade.NovaProjectileScale = 0.0f;
        
        // Accumulate values from all active upgrades
        float totalDamagePercent = 0.0f;
        float totalMulticastPercent = 0.0f;
        int totalPierceCount = 0;
        int totalChainCount = 0;
        float totalCooldownReduction = 0.0f;
        float totalMovementSpeed = 0.0f;
        int totalHealthIncrease = 0;
        float totalCriticalChance = 0.0f;
        float totalCriticalDamage = 0.0f;
        float totalPickupRadius = 0.0f;
        float totalLuck = 0.0f;
        float totalAreaOfEffect = 0.0f;
        float totalDuration = 0.0f;
        
        
        foreach (var upgrade in activeUpgrades)
        {
            if (upgrade is DamageUpgrade)
            {
                DamageUpgrade damageUpgrade = (DamageUpgrade)upgrade;
                totalDamagePercent += damageUpgrade.DamagePercent;
            }
            else if (upgrade is MulticastUpgrade)
            {
                MulticastUpgrade multicastUpgrade = (MulticastUpgrade)upgrade;
                totalMulticastPercent += multicastUpgrade.MulticastPercent;
            }
            else if (upgrade is PierceUpgrade)
            {
                PierceUpgrade pierceUpgrade = (PierceUpgrade)upgrade;
                totalPierceCount += pierceUpgrade.PierceCount;
            }
            else if (upgrade is ChainUpgrade)
            {
                ChainUpgrade chainUpgrade = (ChainUpgrade)upgrade;
                totalChainCount += chainUpgrade.ChainCount;
            }
            else if (upgrade is CooldownReductionUpgrade)
            {
                CooldownReductionUpgrade cooldownUpgrade = (CooldownReductionUpgrade)upgrade;
                totalCooldownReduction += cooldownUpgrade.ReductionPercent;
            }
            else if (upgrade is MovementSpeedUpgrade)
            {
                MovementSpeedUpgrade speedUpgrade = (MovementSpeedUpgrade)upgrade;
                totalMovementSpeed += speedUpgrade.SpeedPercent;
            }
            else if (upgrade is MaxHealthUpgrade)
            {
                MaxHealthUpgrade healthUpgrade = (MaxHealthUpgrade)upgrade;
                totalHealthIncrease += healthUpgrade.HealthIncrease;
            }
            else if (upgrade is CriticalChanceUpgrade)
            {
                CriticalChanceUpgrade criticalChanceUpgrade = (CriticalChanceUpgrade)upgrade;
                totalCriticalChance += criticalChanceUpgrade.ChancePercent;
            }
            else if (upgrade is CriticalDamageUpgrade)
            {
                CriticalDamageUpgrade criticalDamageUpgrade = (CriticalDamageUpgrade)upgrade;
                totalCriticalDamage += criticalDamageUpgrade.DamagePercent;
            }
            else if (upgrade is PickupRadiusUpgrade)
            {
                PickupRadiusUpgrade pickupRadiusUpgrade = (PickupRadiusUpgrade)upgrade;
                totalPickupRadius += pickupRadiusUpgrade.RadiusPercent;
            }
            else if (upgrade is LuckUpgrade)
            {
                LuckUpgrade luckUpgrade = (LuckUpgrade)upgrade;
                totalLuck += luckUpgrade.LuckPercent;
            }
            else if (upgrade is AreaOfEffectUpgrade)
            {
                AreaOfEffectUpgrade aoeUpgrade = (AreaOfEffectUpgrade)upgrade;
                totalAreaOfEffect += aoeUpgrade.AoePercent;
            }
            else if (upgrade is DurationUpgrade)
            {
                DurationUpgrade durationUpgrade = (DurationUpgrade)upgrade;
                totalDuration += durationUpgrade.DurationPercent;
            }
            // Black Hole upgrades
            else if (upgrade is IncreaseDoomStacksUpgrade)
            {
                IncreaseDoomStacksUpgrade doomStacksUpgrade = (IncreaseDoomStacksUpgrade)upgrade;
                accumulatedBlackHoleDoomStacksUpgrade.AdditionalStacks += doomStacksUpgrade.AdditionalStacks;
            }
            else if (upgrade is IncreasePullStrengthUpgrade)
            {
                IncreasePullStrengthUpgrade pullStrengthUpgrade = (IncreasePullStrengthUpgrade)upgrade;
                accumulatedBlackHolePullStrengthUpgrade.PullStrengthPercent += pullStrengthUpgrade.PullStrengthPercent;
            }
            else if (upgrade is IncreaseDoomDamagePerStackUpgrade)
            {
                IncreaseDoomDamagePerStackUpgrade doomDamageUpgrade = (IncreaseDoomDamagePerStackUpgrade)upgrade;
                accumulatedBlackHoleDoomDamageUpgrade.DamagePerStackPercent += doomDamageUpgrade.DamagePerStackPercent;
            }
            // Boomerang upgrades
            else if (upgrade is MultipleBladesUpgrade)
            {
                MultipleBladesUpgrade multipleBladesUpgrade = (MultipleBladesUpgrade)upgrade;
                accumulatedBoomerangMultipleBladesUpgrade.AdditionalBladeCount += multipleBladesUpgrade.AdditionalBladeCount;
            }
            else if (upgrade is FasterRotationUpgrade)
            {
                FasterRotationUpgrade fasterRotationUpgrade = (FasterRotationUpgrade)upgrade;
                accumulatedBoomerangFasterRotationUpgrade.RotationSpeedPercent += fasterRotationUpgrade.RotationSpeedPercent;
            }
            else if (upgrade is PingPongOrbitUpgrade)
            {
                PingPongOrbitUpgrade pingPongOrbitUpgrade = (PingPongOrbitUpgrade)upgrade;
                accumulatedBoomerangPingPongOrbitUpgrade.MaxRadiusPercent += pingPongOrbitUpgrade.MaxRadiusPercent;
                accumulatedBoomerangPingPongOrbitUpgrade.PingPongSpeedPercent += pingPongOrbitUpgrade.PingPongSpeedPercent;
            }
            else if (upgrade is SpinningSlashUpgrade)
            {
                SpinningSlashUpgrade spinningSlashUpgrade = (SpinningSlashUpgrade)upgrade;
                accumulatedBoomerangSpinningSlashUpgrade.SpinSpeedPercent += spinningSlashUpgrade.SpinSpeedPercent;
                accumulatedBoomerangSpinningSlashUpgrade.DamagePercent += spinningSlashUpgrade.DamagePercent;
            }
            // Burn on hit upgrades
            else if (upgrade is BurnOnHitUpgrade)
            {
                BurnOnHitUpgrade burnOnHitUpgrade = (BurnOnHitUpgrade)upgrade;
                // Sum burn chances (will be capped at 100% in getter)
                accumulatedBurnOnHitUpgrade.BurnChancePercent += burnOnHitUpgrade.BurnChancePercent;
                // Use stacks from all upgrades
                accumulatedBurnOnHitUpgrade.BurnStacks += burnOnHitUpgrade.BurnStacks;
            }
            // Fireball nova upgrades
            else if (upgrade is FireballNovaUpgrade)
            {
                FireballNovaUpgrade novaUpgrade = (FireballNovaUpgrade)upgrade;
                accumulatedFireballNovaUpgrade.NovaProjectileCount += novaUpgrade.NovaProjectileCount;
                accumulatedFireballNovaUpgrade.NovaProjectileScale += novaUpgrade.NovaProjectileScale;
            }
            // Lightning split upgrades
            else if (upgrade is LightningSplitUpgrade)
            {
                LightningSplitUpgrade splitUpgrade = (LightningSplitUpgrade)upgrade;
                // Use split count from all upgrades
                accumulatedLightningSplitUpgrade.SplitCount += splitUpgrade.SplitCount;
            }
            // Lightning chain explosion upgrades
            else if (upgrade is LightningChainExplosionUpgrade)
            {
                LightningChainExplosionUpgrade explosionUpgrade = (LightningChainExplosionUpgrade)upgrade;
                accumulatedLightningChainExplosionUpgrade.ExplosionRadius += explosionUpgrade.ExplosionRadius;
                accumulatedLightningChainExplosionUpgrade.ExplosionDamagePercent += explosionUpgrade.ExplosionDamagePercent;
            }
            // Lightning stun upgrades
            else if (upgrade is LightningStunUpgrade)
            {
                LightningStunUpgrade stunUpgrade = (LightningStunUpgrade)upgrade;
                accumulatedLightningStunUpgrade.StunDuration += stunUpgrade.StunDuration;
            }
            // Lightning bouncing upgrade
            else if (upgrade is LightningBouncingUpgrade)
            {
                hasLightningBouncingUpgrade = true;
            }
            // Plague upgrades
            else if (upgrade is FasterPlagueTickUpgrade)
            {
                FasterPlagueTickUpgrade fasterTickUpgrade = (FasterPlagueTickUpgrade)upgrade;
                accumulatedPlagueFasterTickUpgrade.TickSpeedPercent += fasterTickUpgrade.TickSpeedPercent;
            }
            else if (upgrade is PlaguePoisonUpgrade)
            {
                PlaguePoisonUpgrade poisonUpgrade = (PlaguePoisonUpgrade)upgrade;
                // Sum poison chances (will be capped at 100% in getter)
                accumulatedPlaguePoisonUpgrade.PoisonChancePercent += poisonUpgrade.PoisonChancePercent;
                accumulatedPlaguePoisonUpgrade.PoisonStacks += poisonUpgrade.PoisonStacks;
            }
            else if (upgrade is ExtendedPlagueDurationUpgrade)
            {
                ExtendedPlagueDurationUpgrade durationUpgrade = (ExtendedPlagueDurationUpgrade)upgrade;
                accumulatedPlagueExtendedDurationUpgrade.DurationPercent += durationUpgrade.DurationPercent;
            }
            else if (upgrade is PlagueLifeDrainUpgrade)
            {
                PlagueLifeDrainUpgrade lifeDrainUpgrade = (PlagueLifeDrainUpgrade)upgrade;
                // Sum heal chances (will be capped at 100% in getter)
                accumulatedPlagueLifeDrainUpgrade.HealChancePercent += lifeDrainUpgrade.HealChancePercent;
                accumulatedPlagueLifeDrainUpgrade.HealAmount    += lifeDrainUpgrade.HealAmount;
            }
            // Spark flat cooldown modifier upgrades
            else if (upgrade is SparkFlatCooldownModifierUpgrade)
            {
                SparkFlatCooldownModifierUpgrade sparkCooldownUpgrade = (SparkFlatCooldownModifierUpgrade)upgrade;
                accumulatedSparkFlatCooldownModifierUpgrade.ModifierSeconds += sparkCooldownUpgrade.ModifierSeconds;
            }
        }
        
        // Update all accumulated upgrade instances
        accumulatedDamageUpgrade.DamagePercent = totalDamagePercent;
        accumulatedMulticastUpgrade.MulticastPercent = totalMulticastPercent;
        accumulatedPierceUpgrade.PierceCount = totalPierceCount;
        accumulatedChainUpgrade.ChainCount = totalChainCount;
        accumulatedCooldownReductionUpgrade.ReductionPercent = totalCooldownReduction;
        accumulatedMovementSpeedUpgrade.SpeedPercent = totalMovementSpeed;
        accumulatedMaxHealthUpgrade.HealthIncrease = totalHealthIncrease;
        accumulatedCriticalChanceUpgrade.ChancePercent = totalCriticalChance;
        accumulatedCriticalDamageUpgrade.DamagePercent = totalCriticalDamage;
        accumulatedPickupRadiusUpgrade.RadiusPercent = totalPickupRadius;
        accumulatedLuckUpgrade.LuckPercent = totalLuck;
        accumulatedAreaOfEffectUpgrade.AoePercent = totalAreaOfEffect;
        accumulatedDurationUpgrade.DurationPercent = totalDuration;
    }

    public static int ApplyDamageUpgrade(int baseDamage)
    {
        return Mathf.RoundToInt((float)baseDamage * accumulatedDamageUpgrade.GetDamageMultiplier());
    }

    public static int ApplyMulticastUpgrade(float baseMulticastPercent)
    {
        float totalMulticastPercent = baseMulticastPercent + accumulatedMulticastUpgrade.MulticastPercent;
        
        if (totalMulticastPercent <= 0)
            return 0;
        
        // Guaranteed additional casts (every 100%)
        int guaranteedCasts = Mathf.FloorToInt(totalMulticastPercent / 100.0f);
        
        // Probability for one more cast (remainder percentage)
        float remainderPercent = totalMulticastPercent % 100.0f;
        int probabilityCast = (Random.Range(0f, 100f) < remainderPercent) ? 1 : 0;
        
        return guaranteedCasts + probabilityCast;
    }
    
    public static float GetMulticastPercent(float baseMulticastPercent)
    {
        return baseMulticastPercent + accumulatedMulticastUpgrade.MulticastPercent;
    }
    
    public static int ApplyPierceUpgrade(int basePierceCount)
    {
        return basePierceCount + accumulatedPierceUpgrade.PierceCount;
    }

    public static int ApplyChainUpgrade(int baseChainCount)
    {
        return baseChainCount + accumulatedChainUpgrade.ChainCount;
    }

    public static float ApplyCooldownReductionUpgrade(float baseCooldown)
    {
        // Apply percentage reduction with diminishing returns
        float reductionPercent = accumulatedCooldownReductionUpgrade.ReductionPercent;
        
        // Apply diminishing returns formula to prevent performance issues
        // Formula: finalReduction = reductionPercent / (reductionPercent + 100)
        // This approaches 100% but never reaches it
        float effectiveReduction = reductionPercent / (reductionPercent + 100.0f);
        
        // Calculate final cooldown with effective reduction
        float reducedCooldown = baseCooldown * (1.0f - effectiveReduction);
        
        // Enforce minimum cooldown to prevent performance issues
        const float minimumCooldown = 0.05f; // 50ms minimum
        return Mathf.Max(reducedCooldown, minimumCooldown);
    }
    
    /// <summary>
    /// Get the effective cooldown reduction percentage after diminishing returns.
    /// </summary>
    /// <returns>Effective cooldown reduction percentage (0-100)</returns>
    public static float GetEffectiveCooldownReduction()
    {
        float reductionPercent = accumulatedCooldownReductionUpgrade.ReductionPercent;
        float effectiveReduction = reductionPercent / (reductionPercent + 100.0f);
        return effectiveReduction * 100.0f;
    }

    public static float ApplyMovementSpeedUpgrade(float baseMovementSpeed)
    {
        return baseMovementSpeed * accumulatedMovementSpeedUpgrade.GetSpeedMultiplier();
    }
    
    public static int ApplyMaxHealthUpgrade(int baseMaxHealth)
    {
        return baseMaxHealth + accumulatedMaxHealthUpgrade.HealthIncrease;
    }

    public static float ApplyCriticalChanceUpgrade(float baseCriticalChance)
    {
        return baseCriticalChance + accumulatedCriticalChanceUpgrade.ChancePercent;
    }

    public static float ApplyCriticalDamageUpgrade(float baseCriticalDamage)
    {
        return baseCriticalDamage * accumulatedCriticalDamageUpgrade.GetCriticalDamageMultiplier();
    }

    public static float ApplyPickupRadiusUpgrade(float basePickupRadius)
    {
        float upgradedRadius = basePickupRadius * accumulatedPickupRadiusUpgrade.GetRadiusMultiplier();
        // Apply temporary magnet multiplier
        upgradedRadius *= magnetMultiplier;
        return upgradedRadius;
    }
    
    /// <summary>
    /// Apply a temporary magnet effect that multiplies pickup range.
    /// </summary>
    /// <param name="multiplier">Pickup range multiplier (e.g., 5.0 = 5x range).</param>
    /// <param name="duration">Duration of the effect in seconds.</param>
    public static void ApplyMagnetEffect(float multiplier, float duration)
    {
        magnetMultiplier = Mathf.Max(magnetMultiplier, multiplier); // Use maximum if multiple magnets
        magnetRemainingDuration = Mathf.Max(magnetRemainingDuration, duration); // Use maximum duration
    }
    
    /// <summary>
    /// Get the current magnet multiplier.
    /// </summary>
    /// <returns>Current magnet multiplier (1.0 = no effect).</returns>
    public static float GetMagnetMultiplier()
    {
        return magnetMultiplier;
    }
    
    /// <summary>
    /// Update the magnet effect duration. Should be called every frame.
    /// </summary>
    /// <param name="deltaTime">Time since last frame.</param>
    public static void UpdateMagnetEffect(float deltaTime)
    {
        if (magnetRemainingDuration > 0.0f)
        {
            magnetRemainingDuration -= deltaTime;
            if (magnetRemainingDuration <= 0.0f)
            {
                magnetMultiplier = 1.0f;
                magnetRemainingDuration = 0.0f;
            }
        }
    }

    public static float ApplyLuckUpgrade(float baseLuck)
    {
        return baseLuck * accumulatedLuckUpgrade.GetLuckMultiplier();
    }

    public static float ApplyAreaOfEffectUpgrade(float baseAoe)
    {
        return baseAoe * accumulatedAreaOfEffectUpgrade.GetAoeMultiplier();
    }

    public static float ApplyDurationUpgrade(float baseDuration)
    {
        return baseDuration * accumulatedDurationUpgrade.GetDurationMultiplier();
    }

    /// <summary>
    /// Calculate final damage applying damage upgrades and critical strike mechanics.
    /// </summary>
    /// <param name="baseDamage">Base damage value before any upgrades.</param>
    /// <param name="baseCriticalChance">Base critical chance percentage (0-100).</param>
    /// <param name="baseCriticalMultiplier">Base critical damage multiplier (e.g., 2.0 for 200% damage).</param>
    /// <returns>Final damage value after applying all upgrades and critical strike calculation.</returns>
    public static DamageBreakdown CalculateDamage(int baseDamage, float baseCriticalChance = 0.0f, float baseCriticalMultiplier = 2.0f)
    {
        DamageBreakdown damageInfo = new DamageBreakdown();
        // Apply damage upgrades first
        damageInfo.amount = ApplyDamageUpgrade(baseDamage);
        damageInfo.color = Color.white;
        // Calculate final critical chance
        float finalCriticalChance = ApplyCriticalChanceUpgrade(baseCriticalChance);
        
        // Calculate final critical multiplier
        float finalCriticalMultiplier = ApplyCriticalDamageUpgrade(baseCriticalMultiplier);
        
        // Roll for critical hit
        damageInfo.isCritical = Random.Range(0f, 100f) < finalCriticalChance;
        
        // Apply critical multiplier if it's a critical hit
        if (damageInfo.isCritical)
        {
            damageInfo.amount = Mathf.RoundToInt((float)damageInfo.amount * finalCriticalMultiplier);
        }
        
        return damageInfo;
    }

    // ========== PUBLIC GETTERS FOR ACCUMULATED VALUES ==========
    // These methods provide access to accumulated upgrade values for UI display

    /// <summary>
    /// Get the accumulated damage percentage.
    /// </summary>
    public static float GetAccumulatedDamagePercent()
    {
        return accumulatedDamageUpgrade.DamagePercent;
    }

    /// <summary>
    /// Get the accumulated multicast percentage.
    /// </summary>
    public static float GetAccumulatedMulticastPercent()
    {
        return accumulatedMulticastUpgrade.MulticastPercent;
    }

    /// <summary>
    /// Get the accumulated pierce count.
    /// </summary>
    public static int GetAccumulatedPierceCount()
    {
        return accumulatedPierceUpgrade.PierceCount;
    }

    /// <summary>
    /// Get the accumulated chain count.
    /// </summary>
    public static int GetAccumulatedChainCount()
    {
        return accumulatedChainUpgrade.ChainCount;
    }

    /// <summary>
    /// Get the accumulated cooldown reduction percentage.
    /// </summary>
    public static float GetAccumulatedCooldownReductionPercent()
    {
        return accumulatedCooldownReductionUpgrade.ReductionPercent;
    }

    /// <summary>
    /// Get the accumulated movement speed percentage.
    /// </summary>
    public static float GetAccumulatedMovementSpeedPercent()
    {
        return accumulatedMovementSpeedUpgrade.SpeedPercent;
    }

    /// <summary>
    /// Get the accumulated max health increase.
    /// </summary>
    public static int GetAccumulatedMaxHealthIncrease()
    {
        return accumulatedMaxHealthUpgrade.HealthIncrease;
    }

    /// <summary>
    /// Get the accumulated critical chance percentage.
    /// </summary>
    public static float GetAccumulatedCriticalChancePercent()
    {
        return accumulatedCriticalChanceUpgrade.ChancePercent;
    }

    /// <summary>
    /// Get the accumulated critical damage percentage.
    /// </summary>
    public static float GetAccumulatedCriticalDamagePercent()
    {
        return accumulatedCriticalDamageUpgrade.DamagePercent;
    }

    /// <summary>
    /// Get the accumulated pickup radius percentage.
    /// </summary>
    public static float GetAccumulatedPickupRadiusPercent()
    {
        return accumulatedPickupRadiusUpgrade.RadiusPercent;
    }

    /// <summary>
    /// Get the accumulated luck percentage.
    /// </summary>
    public static float GetAccumulatedLuckPercent()
    {
        return accumulatedLuckUpgrade.LuckPercent;
    }

    /// <summary>
    /// Get the accumulated area of effect percentage.
    /// </summary>
    public static float GetAccumulatedAreaOfEffectPercent()
    {
        return accumulatedAreaOfEffectUpgrade.AoePercent;
    }

    /// <summary>
    /// Get the accumulated duration percentage.
    /// </summary>
    public static float GetAccumulatedDurationPercent()
    {
        return accumulatedDurationUpgrade.DurationPercent;
    }

    // ========== BOOMERANG BLADE UPGRADE HELPERS ==========

    /// <summary>
    /// Get the total number of blades to spawn (1 + all MultipleBladesUpgrade bonuses) (cached).
    /// </summary>
    public static int GetBoomerangBladeCount()
    {
        return 1 + accumulatedBoomerangMultipleBladesUpgrade.AdditionalBladeCount;
    }

    /// <summary>
    /// Get the rotation speed multiplier from all FasterRotationUpgrade bonuses (cached).
    /// </summary>
    public static float GetBoomerangRotationSpeedMultiplier()
    {
        return accumulatedBoomerangFasterRotationUpgrade.GetRotationSpeedMultiplier();
    }

    /// <summary>
    /// Check if DualOrbitUpgrade is active.
    /// </summary>
    public static bool HasDualOrbit()
    {
        return HasUpgradeOfType<DualOrbitUpgrade>();
    }

    /// <summary>
    /// Check if PingPongOrbitUpgrade is active.
    /// </summary>
    public static bool HasPingPongOrbit()
    {
        return HasUpgradeOfType<PingPongOrbitUpgrade>();
    }

    /// <summary>
    /// Get the maximum radius multiplier from all PingPongOrbitUpgrade bonuses (cached).
    /// </summary>
    public static float GetBoomerangPingPongMaxRadiusMultiplier()
    {
        return accumulatedBoomerangPingPongOrbitUpgrade.GetMaxRadiusMultiplier();
    }

    /// <summary>
    /// Get the ping-pong speed multiplier from all PingPongOrbitUpgrade bonuses (cached).
    /// </summary>
    public static float GetBoomerangPingPongSpeedMultiplier()
    {
        return accumulatedBoomerangPingPongOrbitUpgrade.GetPingPongSpeedMultiplier();
    }

    /// <summary>
    /// Check if ReturningBladeUpgrade is active.
    /// </summary>
    public static bool HasReturningBlade()
    {
        return HasUpgradeOfType<ReturningBladeUpgrade>();
    }

    /// <summary>
    /// Check if SpinningSlashUpgrade is active.
    /// </summary>
    public static bool HasSpinningSlash()
    {
        return HasUpgradeOfType<SpinningSlashUpgrade>();
    }

    /// <summary>
    /// Get the visual spin speed multiplier from SpinningSlashUpgrade (cached).
    /// </summary>
    public static float GetBoomerangSpinSpeedMultiplier()
    {
        return accumulatedBoomerangSpinningSlashUpgrade.GetSpinSpeedMultiplier();
    }

    /// <summary>
    /// Get the damage multiplier from SpinningSlashUpgrade (cached).
    /// </summary>
    public static float GetBoomerangSpinningSlashDamageMultiplier()
    {
        return accumulatedBoomerangSpinningSlashUpgrade.GetDamageMultiplier();
    }

    // ========== BLACK HOLE UPGRADE HELPERS ==========

    /// <summary>
    /// Get the total additional Doom stacks from all IncreaseDoomStacksUpgrade bonuses (cached).
    /// </summary>
    public static int GetBlackHoleDoomAdditionalStacks()
    {
        return accumulatedBlackHoleDoomStacksUpgrade.AdditionalStacks;
    }

    /// <summary>
    /// Get the pull strength multiplier from all IncreasePullStrengthUpgrade bonuses (cached).
    /// </summary>
    public static float GetBlackHolePullStrengthMultiplier()
    {
        return accumulatedBlackHolePullStrengthUpgrade.GetPullStrengthMultiplier();
    }

    /// <summary>
    /// Get the Doom damage per stack multiplier from all IncreaseDoomDamagePerStackUpgrade bonuses (cached).
    /// </summary>
    public static float GetBlackHoleDoomDamagePerStackMultiplier()
    {
        return accumulatedBlackHoleDoomDamageUpgrade.GetDamagePerStackMultiplier();
    }

    // ========== BURN ON HIT UPGRADE HELPERS ==========

    /// <summary>
    /// Get the burn chance percent from all BurnOnHitUpgrade bonuses (cached, capped at 100%).
    /// </summary>
    public static float GetBurnOnHitChancePercent()
    {
        return Mathf.Min(100.0f, accumulatedBurnOnHitUpgrade.BurnChancePercent);
    }

    /// <summary>
    /// Get the burn stacks to apply from all BurnOnHitUpgrade bonuses (cached, returns maximum).
    /// </summary>
    public static int GetBurnOnHitStacks()
    {
        return accumulatedBurnOnHitUpgrade.BurnStacks;
    }

    /// <summary>
    /// Check if burn should be applied based on accumulated upgrades (cached).
    /// </summary>
    /// <returns>True if burn should be applied, false otherwise.</returns>
    public static bool ShouldApplyBurnOnHit()
    {
        float chance = GetBurnOnHitChancePercent();
        if (chance <= 0.0f)
            return false;
        
        return Random.Range(0.0f, 100.0f) < chance;
    }

    // ========== FIREBALL NOVA UPGRADE HELPERS ==========

    /// <summary>
    /// Get the nova projectile count from all FireballNovaUpgrade bonuses (cached).
    /// </summary>
    public static int GetFireballNovaProjectileCount()
    {
        return accumulatedFireballNovaUpgrade.NovaProjectileCount;
    }

    /// <summary>
    /// Get the nova projectile scale from all FireballNovaUpgrade bonuses (cached).
    /// </summary>
    public static float GetFireballNovaProjectileScale()
    {
        return accumulatedFireballNovaUpgrade.NovaProjectileScale;
    }

    /// <summary>
    /// Check if fireball nova is enabled.
    /// </summary>
    public static bool HasFireballNova()
    {
        return accumulatedFireballNovaUpgrade.NovaProjectileCount > 0;
    }

    // ========== LIGHTNING SPLIT UPGRADE HELPERS ==========

    /// <summary>
    /// Get the split count from all LightningSplitUpgrade bonuses (cached).
    /// </summary>
    public static int GetLightningSplitCount()
    {
        return accumulatedLightningSplitUpgrade.SplitCount;
    }
    
    /// <summary>
    /// Get the chain explosion radius from all LightningChainExplosionUpgrade bonuses (cached).
    /// </summary>
    public static float GetLightningChainExplosionRadius()
    {
        return accumulatedLightningChainExplosionUpgrade.ExplosionRadius;
    }
    
    /// <summary>
    /// Get the chain explosion damage percent from all LightningChainExplosionUpgrade bonuses (cached).
    /// </summary>
    public static float GetLightningChainExplosionDamagePercent()
    {
        return accumulatedLightningChainExplosionUpgrade.ExplosionDamagePercent;
    }
    
    /// <summary>
    /// Check if chain explosions are enabled.
    /// </summary>
    public static bool HasLightningChainExplosion()
    {
        return accumulatedLightningChainExplosionUpgrade.ExplosionRadius > 0.0f;
    }
    
    /// <summary>
    /// Get the stun duration from all LightningStunUpgrade bonuses (cached).
    /// </summary>
    public static float GetLightningStunDuration()
    {
        return accumulatedLightningStunUpgrade.StunDuration;
    }
    
    /// <summary>
    /// Check if chain stuns are enabled.
    /// </summary>
    public static bool HasLightningStun()
    {
        return accumulatedLightningStunUpgrade.StunDuration > 0.0f;
    }
    
    /// <summary>
    /// Check if bouncing lightning is enabled.
    /// </summary>
    public static bool HasLightningBouncing()
    {
        return hasLightningBouncingUpgrade;
    }

    // ========== PLAGUE UPGRADE HELPERS ==========

    /// <summary>
    /// Get the tick interval multiplier from all FasterPlagueTickUpgrade bonuses (cached).
    /// Lower value = faster ticks. Applies diminishing returns similar to cooldown reduction.
    /// </summary>
    public static float GetPlagueTickIntervalMultiplier()
    {
        float tickSpeedPercent = accumulatedPlagueFasterTickUpgrade.TickSpeedPercent;
        
        if (tickSpeedPercent <= 0.0f)
            return 1.0f;
        
        // Apply diminishing returns formula to prevent performance issues
        // Formula: effectiveReduction = tickSpeedPercent / (tickSpeedPercent + 100)
        // This approaches 100% but never reaches it
        float effectiveReduction = tickSpeedPercent / (tickSpeedPercent + 100.0f);
        
        // Calculate final multiplier (1.0 - effectiveReduction)
        // Lower multiplier = faster ticks
        float multiplier = 1.0f - effectiveReduction;
        
        // Enforce minimum multiplier to prevent instant ticks and performance issues
        const float minimumMultiplier = 0.1f; // Can't go faster than 10x speed (0.1x interval)
        return Mathf.Max(multiplier, minimumMultiplier);
    }

    /// <summary>
    /// Get the poison chance percent from all PlaguePoisonUpgrade bonuses (cached, capped at 100%).
    /// </summary>
    public static float GetPlaguePoisonChancePercent()
    {
        return Mathf.Min(100.0f, accumulatedPlaguePoisonUpgrade.PoisonChancePercent);
    }

    /// <summary>
    /// Get the poison stacks to apply from all PlaguePoisonUpgrade bonuses (cached, returns maximum).
    /// </summary>
    public static int GetPlaguePoisonStacks()
    {
        return accumulatedPlaguePoisonUpgrade.PoisonStacks;
    }

    /// <summary>
    /// Get the duration multiplier from all ExtendedPlagueDurationUpgrade bonuses (cached).
    /// </summary>
    public static float GetPlagueDurationMultiplier()
    {
        if (accumulatedPlagueExtendedDurationUpgrade.DurationPercent <= 0.0f)
            return 1.0f;
        
        return accumulatedPlagueExtendedDurationUpgrade.GetDurationMultiplier();
    }

    /// <summary>
    /// Get the heal chance percent from all PlagueLifeDrainUpgrade bonuses (cached, capped at 100%).
    /// </summary>
    public static float GetPlagueHealChancePercent()
    {
        return Mathf.Min(100.0f, accumulatedPlagueLifeDrainUpgrade.HealChancePercent);
    }

    /// <summary>
    /// Get the heal amount from all PlagueLifeDrainUpgrade bonuses (cached, returns maximum).
    /// </summary>
    public static int GetPlagueHealAmount()
    {
        return accumulatedPlagueLifeDrainUpgrade.HealAmount;
    }

    // ========== SPARK UPGRADE HELPERS ==========

    /// <summary>
    /// Get the flat cooldown modifier from all SparkFlatCooldownModifierUpgrade bonuses (cached).
    /// </summary>
    public static float GetSparkFlatCooldownModifier()
    {
        return accumulatedSparkFlatCooldownModifierUpgrade.ModifierSeconds;
    }

}
