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
                anySuccessful |= OnTriggerAbility(targets, i, totalCasts);
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
    /// <param name="castIndex">The index of the current cast (0-based).</param>
    /// <param name="totalCasts">The total number of casts in this trigger (including multicast).</param>
    protected abstract bool OnTriggerAbility(Entity[] targets, int castIndex, int totalCasts);

    /// <summary>
    /// Calculate a spread direction based on cast index and total casts using a total spread angle.
    /// Useful for abilities that want to spread projectiles in a cone pattern when multicast is active.
    /// The total spread angle is distributed evenly across all casts.
    /// </summary>
    /// <param name="baseDirection">The base direction to spread from.</param>
    /// <param name="castIndex">The current cast index (0-based).</param>
    /// <param name="totalCasts">The total number of casts in this trigger.</param>
    /// <param name="totalSpreadAngle">The total spread angle in degrees (e.g., 30 degrees creates a 30-degree cone from first to last cast).</param>
    /// <returns>A new direction vector rotated by the appropriate spread angle.</returns>
    static protected Vector3 CalculateSpreadDirection(Vector3 baseDirection, int castIndex, int totalCasts, float totalSpreadAngle)
    {
        // If only one cast, return the base direction unchanged
        if (totalCasts <= 1)
        {
            return baseDirection.normalized;
        }

        // Calculate the angle offset for this cast
        // Spread evenly from -totalSpreadAngle/2 to +totalSpreadAngle/2
        float normalizedPosition = (float)castIndex / (totalCasts - 1); // 0.0 to 1.0
        float angleOffset = (normalizedPosition - 0.5f) * totalSpreadAngle; // -totalSpreadAngle/2 to +totalSpreadAngle/2

        return ApplyDirectionRotation(baseDirection, angleOffset);
    }

    /// <summary>
    /// Calculate a spread direction based on cast index and total casts using an angle between casts.
    /// Useful for abilities that want consistent spacing between projectiles regardless of total cast count.
    /// The angle between casts is fixed, so total spread increases with more casts.
    /// </summary>
    /// <param name="baseDirection">The base direction to spread from.</param>
    /// <param name="castIndex">The current cast index (0-based).</param>
    /// <param name="totalCasts">The total number of casts in this trigger.</param>
    /// <param name="angleBetweenCasts">The angle in degrees between each cast (e.g., 15 degrees means 15 degrees between each projectile).</param>
    /// <returns>A new direction vector rotated by the appropriate spread angle.</returns>
    static protected Vector3 CalculateSpreadDirectionByAngleBetween(Vector3 baseDirection, int castIndex, int totalCasts, float angleBetweenCasts)
    {
        // If only one cast, return the base direction unchanged
        if (totalCasts <= 1)
        {
            return baseDirection.normalized;
        }

        // Calculate the angle offset for this cast
        // Center the spread around the base direction
        // For 3 casts with 15 degrees between: cast 0 = -15, cast 1 = 0, cast 2 = +15
        float centerOffset = (totalCasts - 1) * 0.5f; // Center position (e.g., 1.0 for 3 casts)
        float angleOffset = (castIndex - centerOffset) * angleBetweenCasts;

        return ApplyDirectionRotation(baseDirection, angleOffset);
    }

    /// <summary>
    /// Helper method to apply rotation to a direction vector around a perpendicular axis.
    /// Ensures spread happens on the XZ plane (horizontal plane) for top-down gameplay.
    /// </summary>
    /// <param name="baseDirection">The base direction to rotate.</param>
    /// <param name="angleOffset">The angle offset in degrees to rotate by.</param>
    /// <returns>A new direction vector rotated by the angle offset.</returns>
    static private Vector3 ApplyDirectionRotation(Vector3 baseDirection, float angleOffset)
    {
        // If the angle is zero, return the base direction
        if (Mathf.Abs(angleOffset) < 0.001f)
        {
            return baseDirection.normalized;
        }

        // Project base direction onto XZ plane to ensure horizontal spreading
        Vector3 horizontalDirection = new Vector3(baseDirection.x, 0, baseDirection.z);
        
        // If horizontal direction is zero (baseDirection is purely vertical), use forward as default
        if (horizontalDirection.magnitude < 0.001f)
        {
            horizontalDirection = Vector3.forward;
        }
        
        horizontalDirection = horizontalDirection.normalized;
        
        // Rotate the horizontal direction around the up axis (Y axis) by the angle offset
        // This ensures the spread stays on the XZ plane
        Quaternion rotation = Quaternion.AngleAxis(angleOffset, Vector3.up);
        Vector3 spreadHorizontal = rotation * horizontalDirection;
        
        // Preserve the original vertical component (Y) to maintain vertical aiming angle
        // The horizontal component is rotated, but vertical stays the same
        Vector3 spreadDirection = new Vector3(spreadHorizontal.x, baseDirection.y, spreadHorizontal.z);
        
        return spreadDirection.normalized;
    }

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
