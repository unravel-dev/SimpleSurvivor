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

        if (DebugUpgrades)
        {
            Log.Info("UpgradeSystem: Cleared all upgrades");
        }
    }


    public static int ApplyDamageUpgrade(int baseDamage)
    {
        float damageMultiplier = 1.0f;
        foreach (var upgrade in activeUpgrades)
        {
            if (upgrade is DamageUpgrade damageUpgrade)
            {
                damageMultiplier += damageUpgrade.DamagePercent;
            }
        }
        return Mathf.RoundToInt((float)baseDamage * (1.0f + damageMultiplier / 100.0f));
    }

    public static int ApplyPierceUpgrade(int basePierceCount)
    {
        int newPierceCount = basePierceCount;
        foreach (var upgrade in activeUpgrades)
        {
            if (upgrade is PierceUpgrade pierceUpgrade)
            {
                newPierceCount += pierceUpgrade.PierceCount;
            }
        }
        return newPierceCount;
    }


    public static int ApplyChainUpgrade(int baseChainCount)
    {
        int newChainCount = baseChainCount;
        foreach (var upgrade in activeUpgrades)
        {
            if (upgrade is ChainUpgrade chainUpgrade)
            {
                newChainCount += chainUpgrade.ChainCount;
            }
        }
        return newChainCount;
    }

    public static float ApplyCooldownReductionUpgrade(float baseCooldown)
    {
        float cooldownReduction = 0.0f;
        foreach (var upgrade in activeUpgrades)
        {
            if (upgrade is CooldownReductionUpgrade cooldownReductionUpgrade)
            {
                cooldownReduction += cooldownReductionUpgrade.ReductionPercent;
            }
        }

        return baseCooldown * (1.0f - cooldownReduction / 100.0f);
    }
    public static float ApplyMovementSpeedUpgrade(float baseMovementSpeed)
    {
        float movementSpeedIncrease = 0.0f;
        foreach (var upgrade in activeUpgrades)
        {
            if (upgrade is MovementSpeedUpgrade movementSpeedUpgrade)
            {
                movementSpeedIncrease += movementSpeedUpgrade.SpeedPercent;
            }
        }
        return baseMovementSpeed * (1.0f + movementSpeedIncrease / 100.0f);
    }
    
    public static int ApplyMaxHealthUpgrade(int baseMaxHealth)
    {
        int newMaxHealth = baseMaxHealth;
        foreach (var upgrade in activeUpgrades)
        {
            if (upgrade is MaxHealthUpgrade maxHealthUpgrade)
            {
                newMaxHealth += maxHealthUpgrade.HealthIncrease;
            }
        }
        return newMaxHealth;
    }
}
