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
    private static readonly ProjectileCountUpgrade accumulatedProjectileCountUpgrade = new ProjectileCountUpgrade(0);
    private static readonly PierceUpgrade accumulatedPierceUpgrade = new PierceUpgrade(0);
    private static readonly ChainUpgrade accumulatedChainUpgrade = new ChainUpgrade(0);
    private static readonly CooldownReductionUpgrade accumulatedCooldownReductionUpgrade = new CooldownReductionUpgrade(0.0f);
    private static readonly MovementSpeedUpgrade accumulatedMovementSpeedUpgrade = new MovementSpeedUpgrade(0.0f);
    private static readonly MaxHealthUpgrade accumulatedMaxHealthUpgrade = new MaxHealthUpgrade(0);
    private static readonly CriticalChanceUpgrade accumulatedCriticalChanceUpgrade = new CriticalChanceUpgrade(0.0f);
    private static readonly CriticalDamageUpgrade accumulatedCriticalDamageUpgrade = new CriticalDamageUpgrade(0.0f);

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
        accumulatedProjectileCountUpgrade.ProjectileCount = 0;
        accumulatedPierceUpgrade.PierceCount = 0;
        accumulatedChainUpgrade.ChainCount = 0;
        accumulatedCooldownReductionUpgrade.ReductionPercent = 0.0f;
        accumulatedMovementSpeedUpgrade.SpeedPercent = 0.0f;
        accumulatedMaxHealthUpgrade.HealthIncrease = 0;
        accumulatedCriticalChanceUpgrade.ChancePercent = 0.0f;
        accumulatedCriticalDamageUpgrade.DamagePercent = 0.0f;
        
        // Accumulate values from all active upgrades
        float totalDamagePercent = 0.0f;
        int totalProjectileCount = 0;
        int totalPierceCount = 0;
        int totalChainCount = 0;
        float totalCooldownReduction = 0.0f;
        float totalMovementSpeed = 0.0f;
        int totalHealthIncrease = 0;
        float totalCriticalChance = 0.0f;
        float totalCriticalDamage = 0.0f;
        
        
        foreach (var upgrade in activeUpgrades)
        {
            if (upgrade is DamageUpgrade)
            {
                DamageUpgrade damageUpgrade = (DamageUpgrade)upgrade;
                totalDamagePercent += damageUpgrade.DamagePercent;
            }
            else if (upgrade is ProjectileCountUpgrade)
            {
                ProjectileCountUpgrade projectileUpgrade = (ProjectileCountUpgrade)upgrade;
                totalProjectileCount += projectileUpgrade.ProjectileCount;
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
        }
        
        // Update all accumulated upgrade instances
        accumulatedDamageUpgrade.DamagePercent = totalDamagePercent;
        accumulatedProjectileCountUpgrade.ProjectileCount = totalProjectileCount;
        accumulatedPierceUpgrade.PierceCount = totalPierceCount;
        accumulatedChainUpgrade.ChainCount = totalChainCount;
        accumulatedCooldownReductionUpgrade.ReductionPercent = totalCooldownReduction;
        accumulatedMovementSpeedUpgrade.SpeedPercent = totalMovementSpeed;
        accumulatedMaxHealthUpgrade.HealthIncrease = totalHealthIncrease;
        accumulatedCriticalChanceUpgrade.ChancePercent = totalCriticalChance;
        accumulatedCriticalDamageUpgrade.DamagePercent = totalCriticalDamage;
    }

    public static int ApplyDamageUpgrade(int baseDamage)
    {
        return Mathf.RoundToInt((float)baseDamage * accumulatedDamageUpgrade.GetDamageMultiplier());
    }

    public static int ApplyProjectileCountUpgrade(int baseProjectileCount)
    {
        return baseProjectileCount + accumulatedProjectileCountUpgrade.ProjectileCount;
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
        return baseCooldown * accumulatedCooldownReductionUpgrade.GetCooldownMultiplier();
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

    /// <summary>
    /// Calculate final damage applying damage upgrades and critical strike mechanics.
    /// </summary>
    /// <param name="baseDamage">Base damage value before any upgrades.</param>
    /// <param name="baseCriticalChance">Base critical chance percentage (0-100).</param>
    /// <param name="baseCriticalMultiplier">Base critical damage multiplier (e.g., 2.0 for 200% damage).</param>
    /// <returns>Final damage value after applying all upgrades and critical strike calculation.</returns>
    public static int CalculateDamage(int baseDamage, float baseCriticalChance = 0.0f, float baseCriticalMultiplier = 2.0f)
    {
        // Apply damage upgrades first
        int upgradedDamage = ApplyDamageUpgrade(baseDamage);
        
        // Calculate final critical chance
        float finalCriticalChance = ApplyCriticalChanceUpgrade(baseCriticalChance);
        
        // Calculate final critical multiplier
        float finalCriticalMultiplier = ApplyCriticalDamageUpgrade(baseCriticalMultiplier);
        
        // Roll for critical hit
        bool isCritical = Random.Range(0f, 100f) < finalCriticalChance;
        
        // Apply critical multiplier if it's a critical hit
        if (isCritical)
        {
            return Mathf.RoundToInt((float)upgradedDamage * finalCriticalMultiplier);
        }
        
        return upgradedDamage;
    }
}
