using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Structure to hold ability display information for UI.
/// </summary>
public struct AbilityDisplayInfo
{
    public string type;
    public string name;
    public string icon;
    public string color;
}

/// <summary>
/// Base class for all ability types. Handles cooldown management and provides
/// virtual methods for derived classes to implement specific ability behavior.
/// </summary>
[ScriptSourceFile]
public abstract class Ability : ScriptComponent
{
    [Tooltip("Cooldown time in seconds between ability triggers")]
    public float cooldown = 1.0f;

    private float modifiedCooldown = 0f;
    // Internal state
    private float lastTriggerTime = -1f;

    /// <summary>
    /// Check if the ability can be triggered based on cooldown.
    /// </summary>
    /// <returns>True if ability is ready to trigger.</returns>
    public bool CanTriggerAbility()
    {
        if (lastTriggerTime < 0)
        {
            // First time triggering
            return true;
        }

        float timeSinceLastTrigger = Time.time - lastTriggerTime;
        bool canTrigger = timeSinceLastTrigger >= modifiedCooldown;

        return canTrigger;
    }

    /// <summary>
    /// Get the remaining cooldown time in seconds.
    /// </summary>
    /// <returns>Remaining cooldown time, or 0 if ready.</returns>
    public float GetRemainingCooldown()
    {
        if (lastTriggerTime < 0)
            return 0f;

        float timeSinceLastTrigger = Time.time - lastTriggerTime;
        return Mathf.Max(0f, modifiedCooldown - timeSinceLastTrigger);
    }

    /// <summary>
    /// Get the cooldown progress as a percentage (0-1).
    /// </summary>
    /// <returns>Cooldown progress where 1 = ready, 0 = just triggered.</returns>
    public float GetCooldownProgress()
    {
        if (lastTriggerTime < 0 || modifiedCooldown <= 0)
            return 1f;

        float timeSinceLastTrigger = Time.time - lastTriggerTime;
        return Mathf.Clamp01(timeSinceLastTrigger / modifiedCooldown);
    }

    /// <summary>
    /// Attempt to trigger the ability. Checks cooldown and calls OnTriggerAbility if ready.
    /// Handles multicast upgrades automatically.
    /// </summary>
    /// <returns>True if ability was successfully triggered.</returns>
    public bool TriggerAbility()
    {
        if (!CanTriggerAbility())
        {
            return false;
        }

        // Calculate number of additional casts from multicast upgrades
        float baseMulticastPercent = GetBaseMulticastPercent();
        int additionalCasts = UpgradeSystem.ApplyMulticastUpgrade(baseMulticastPercent);
        int totalCasts = 1 + additionalCasts; // Base cast + additional casts

        // Update cooldown timer (only once, regardless of multicast)
        lastTriggerTime = Time.time;
        bool anySuccessful = false;
        var targets = GatherTargets();
        if (targets != null && targets.Length > 0)
        {
            // Perform all casts
            for (int i = 0; i < totalCasts; i++)
            {
                // Gather targets for each cast (allows for dynamic targeting)
            
                // Execute the ability
                OnTriggerAbility(targets, i);
                anySuccessful = true;
            }
        }

        return anySuccessful;
    }

    /// <summary>
    /// Reset the cooldown, making the ability immediately available.
    /// </summary>
    public void ResetCooldown()
    {
        lastTriggerTime = -1f;
    }

    /// <summary>
    /// Set the cooldown to a specific remaining time.
    /// </summary>
    /// <param name="remainingTime">Remaining cooldown time in seconds.</param>
    public void SetRemainingCooldown(float remainingTime)
    {
        lastTriggerTime = Time.time - (modifiedCooldown - remainingTime);
    }

    /// <summary>
    /// Virtual method for derived classes to define base multicast percentage.
    /// Override this to give abilities inherent multicast chance.
    /// </summary>
    /// <returns>Base multicast percentage (default 0%).</returns>
    protected virtual float GetBaseMulticastPercent()
    {
        return 0.0f;
    }

    /// <summary>
    /// Virtual method for derived classes to provide display information for UI.
    /// Override this to customize how the ability appears in the UI.
    /// </summary>
    /// <returns>Display information for the ability.</returns>
    public virtual AbilityDisplayInfo GetDisplayInfo()
    {
        // Default implementation - uses class name
        string typeName = GetType().Name;
        AbilityDisplayInfo info = new AbilityDisplayInfo();
        info.type = typeName.ToLower().Replace("ability", "");
        info.name = typeName.Replace("Ability", "");
        info.icon = info.name.Length > 0 ? info.name[0].ToString() : "?";
        info.color = "rgba(150, 150, 150, 180)"; // Gray
        return info;
    }

    /// <summary>
    /// Virtual method for derived classes to implement target gathering logic.
    /// </summary>
    /// <returns>List of entities that this ability should affect.</returns>
    protected abstract Entity[] GatherTargets();

    /// <summary>
    /// Virtual method for derived classes to implement the actual ability effect.
    /// </summary>
    /// <param name="targets">List of target entities to affect.</param>
    protected abstract void OnTriggerAbility(Entity[] targets, int castIndex);

    /// <summary>
    /// Virtual method called when the ability starts (can be overridden for setup).
    /// </summary>
    public override void OnStart()
    {

    }

        
    /// <summary>
    /// Update method to automatically trigger the ability when possible.
    /// </summary>
    public override void OnUpdate()
    {
        modifiedCooldown = UpgradeSystem.ApplyCooldownReductionUpgrade(cooldown);
        // Automatically trigger the ability when it's ready
        if (CanTriggerAbility())
        {
            TriggerAbility();
        }
    }
    
}
