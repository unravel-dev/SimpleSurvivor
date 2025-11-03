using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

public struct QueryClosestTarget
{
    public Entity source;
    public float maxRange;}
/// <summary>
/// Base class for all ability types. Handles cooldown management and provides
/// virtual methods for derived classes to implement specific ability behavior.
/// </summary>
[ScriptSourceFile]
public abstract class Ability : ScriptComponent
{
    [Tooltip("Cooldown time in seconds between ability triggers")]
    public float cooldown = 1.0f;

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
        bool canTrigger = timeSinceLastTrigger >= cooldown;

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
        return Mathf.Max(0f, cooldown - timeSinceLastTrigger);
    }

    /// <summary>
    /// Get the cooldown progress as a percentage (0-1).
    /// </summary>
    /// <returns>Cooldown progress where 1 = ready, 0 = just triggered.</returns>
    public float GetCooldownProgress()
    {
        if (lastTriggerTime < 0 || cooldown <= 0)
            return 1f;

        float timeSinceLastTrigger = Time.time - lastTriggerTime;
        return Mathf.Clamp01(timeSinceLastTrigger / cooldown);
    }

    /// <summary>
    /// Attempt to trigger the ability. Checks cooldown and calls OnTriggerAbility if ready.
    /// </summary>
    /// <returns>True if ability was successfully triggered.</returns>
    public bool TriggerAbility()
    {
        if (!CanTriggerAbility())
        {
            return false;
        }

        // Gather targets for the ability
        List<Entity> targets = GatherTargets();

        if (targets == null || targets.Count == 0)
        {
            return false;
        }

        // Update cooldown timer
        lastTriggerTime = Time.time;

        // Execute the ability
        OnTriggerAbility(targets);

        return true;
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
        lastTriggerTime = Time.time - (cooldown - remainingTime);
    }

    /// <summary>
    /// Virtual method for derived classes to implement target gathering logic.
    /// </summary>
    /// <returns>List of entities that this ability should affect.</returns>
    protected abstract List<Entity> GatherTargets();

    /// <summary>
    /// Virtual method for derived classes to implement the actual ability effect.
    /// </summary>
    /// <param name="targets">List of target entities to affect.</param>
    protected abstract void OnTriggerAbility(List<Entity> targets);

    /// <summary>
    /// Virtual method called when the ability starts (can be overridden for setup).
    /// </summary>
    public override void OnStart()
    {

    }
    
    
}
