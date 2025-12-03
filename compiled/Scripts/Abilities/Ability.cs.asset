using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Base class for all ability types. Handles cooldown management and provides
/// virtual methods for derived classes to implement specific ability behavior.
/// </summary>
[ScriptSourceFile]
public abstract class Ability : ScriptComponent
{
    [Tooltip("Cooldown time in seconds between ability triggers")]
    public float cooldown = 1.0f;

    [Tooltip("Base multicast percentage")]
    public float multicastPercent = 0.0f;

    [Tooltip("Total damage dealt by this ability")]
    public int totalDamageDealt = 0;

    protected float modifiedCooldown = 0f;
    // Internal state
    protected float lastTriggerTime = -1f;

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

    public float GetCooldown()
    {
        return modifiedCooldown;
    }

    public virtual float GetBaseCooldown()
    {
        return cooldown;
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
    public virtual bool TriggerAbility()
    {
        if (!CanTriggerAbility())
        {
            return false;
        }

        // Calculate number of additional casts from multicast upgrades
        int additionalCasts = UpgradeSystem.ApplyMulticastUpgrade(multicastPercent);
        int totalCasts = 1 + additionalCasts; // Base cast + additional casts

        // Update cooldown timer (only once, regardless of multicast)
        bool anySuccessful = false;
        var targets = GatherTargets();
        if (targets != null && targets.Length > 0)
        {
            // Perform all casts
            for (int i = 0; i < totalCasts; i++)
            {
                // Gather targets for each cast (allows for dynamic targeting)
            
                // Execute the ability
                anySuccessful |= OnTriggerAbility(targets, i);
            }
        }

        if(anySuccessful)
        {
            ResetCooldown();
        }

        return anySuccessful;
    }

    /// <summary>
    /// Reset the cooldown, making the ability immediately available.
    /// </summary>
    public void ResetCooldown()
    {
        lastTriggerTime = Time.time;
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
    /// Static method to get display information for an ability type.
    /// Tries to call the static GetDisplayInfo method on the specific ability type,
    /// falls back to default implementation if not found.
    /// </summary>
    /// <param name="abilityType">The ability type</param>
    /// <returns>Display information for the ability</returns>
    public static UpgradeDisplayInfo GetDisplayInfo(Type abilityType)
    {
        if (abilityType == null)
        {
            UpgradeDisplayInfo defaultInfo = new UpgradeDisplayInfo();
            defaultInfo.iconType = "";
            defaultInfo.name = "Unknown";
            defaultInfo.icon = "?";
            defaultInfo.color = "rgba(150, 150, 150, 180)";
            defaultInfo.description = "";
            return defaultInfo;
        }
        
        // Try to call the static method on the specific ability type
        var method = abilityType.GetMethod("GetDisplayInfo", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        if (method != null)
        {
            return (UpgradeDisplayInfo)(method.Invoke(null, null) ?? GetDefaultDisplayInfo(abilityType));
        }
        
        // Fallback: use default implementation
        return GetDefaultDisplayInfo(abilityType);
    }
    
    /// <summary>
    /// Get default display information based on ability type name.
    /// </summary>
    /// <param name="abilityType">The ability type</param>
    /// <returns>Default display information</returns>
    private static UpgradeDisplayInfo GetDefaultDisplayInfo(Type abilityType)
    {
        string typeName = abilityType.Name.ToLower();
        if (typeName.EndsWith("ability"))
        {
            typeName = typeName.Substring(0, typeName.Length - 7);
        }
        
        UpgradeDisplayInfo info = new UpgradeDisplayInfo();
        info.iconType = typeName;
        info.name = abilityType.Name.Replace("Ability", "");
        info.icon = info.name.Length > 0 ? info.name[0].ToString() : "?";
        info.color = "rgba(150, 150, 150, 180)"; // Gray
        info.description = "";
        
        return info;
    }

    /// <summary>
    /// Virtual method for derived classes to provide display information for UI.
    /// Override this to customize how the ability appears in the UI.
    /// By default, calls the static GetDisplayInfo method.
    /// </summary>
    /// <returns>Display information for the ability.</returns>
    public virtual UpgradeDisplayInfo GetDisplayInfo()
    {
        // Call the static version
        return GetDisplayInfo(GetType());
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
    protected abstract bool OnTriggerAbility(Entity[] targets, int castIndex);

    /// <summary>
    /// Add DamageSourceComponent to an entity to track damage statistics.
    /// </summary>
    /// <param name="entity">The entity to add the component to.</param>
    protected void AddDamageSourceComponent(Entity entity)
    {
        if (!entity)
            return;
            
        var damageSource = entity.GetComponent<DamageSourceComponent>();
        if (damageSource == null)
        {
            damageSource = entity.AddComponent<DamageSourceComponent>();
        }
        
        if (damageSource != null)
        {
            damageSource.Initialize(owner, this);
        }
    }

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
        modifiedCooldown = UpgradeSystem.ApplyCooldownReductionUpgrade(GetBaseCooldown());
        // Automatically trigger the ability when it's ready
        if (CanTriggerAbility())
        {
            TriggerAbility();
        }
    }
    
}
