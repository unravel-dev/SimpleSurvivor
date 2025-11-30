using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Dash ability that allows the entity to quickly dash in the movement direction.
/// Activated manually with the Space key, has a cooldown.
/// Unlike other abilities, this is manually triggered rather than automatic.
/// </summary>
[ScriptSourceFile]
public class DashAbility : Ability
{
    [Tooltip("Dash force/speed multiplier")]
    public float dashForce = 22.0f;
    
    [Tooltip("Duration of invulnerability frames during dash (in seconds)")]
    public float invulnerabilityDuration = 0.5f;
    
    // Component references
    private PhysicsComponent physicsComponent;
    
    /// <summary>
    /// Called when the script is created.[]
    /// </summary>
    public override void OnCreate()
    {
        // Cache component references
        physicsComponent = owner.GetComponent<PhysicsComponent>();
        
        if (physicsComponent == null && owner.transform.parent != null)
        {
            physicsComponent = owner.transform.parent.GetComponent<PhysicsComponent>();
        }
    }
 
    
    /// <summary>
    /// Gather targets for the dash ability (not used, but required by base class).
    /// </summary>
    /// <returns>Empty array since dash doesn't target enemies.</returns>
    protected override Entity[] GatherTargets()
    {
        // Dash doesn't need targets
        return new Entity[] { owner };
    }
    
    /// <summary>
    /// Trigger the dash ability.
    /// </summary>
    /// <param name="targets">Target entities (not used for dash).</param>
    /// <param name="castIndex">Cast index for multicast (not used for dash).</param>
    protected override bool OnTriggerAbility(Entity[] targets, int castIndex)
    {
        if(castIndex > 0)
        {
            return false;
        }
        if (Input.IsPressed(KeyCode.Space))
        {
            return PerformDash();
        }
        return false;
    }
    
    /// <summary>
    /// Perform the dash action.
    /// </summary>
    private bool PerformDash()
    {
        if (physicsComponent == null)
            return false;

        Vector3 currentVelocity = physicsComponent.velocity;
        Vector3 dashDirection = currentVelocity.normalized;

        if (dashDirection.magnitude < 0.1f)
        {
            return false;
        }
        
        // Cancel current velocity in dash direction for consistent dash distance
        // This prevents the dash from being affected by current movement speed
        float currentSpeedInDashDirection = Vector3.Dot(currentVelocity, dashDirection);
        Vector3 velocityToCancel = dashDirection * currentSpeedInDashDirection;
        // physicsComponent.velocity -= velocityToCancel;
        
        float upgradedDashForce = UpgradeSystem.ApplyMovementSpeedUpgrade(dashForce);
        // Apply dash impulse
        Vector3 force = dashDirection * (upgradedDashForce- velocityToCancel.magnitude);
        physicsComponent.ApplyForce(force, ForceMode.Impulse);
        
        // Grant invulnerability during dash
        GrantDashInvulnerability();
        
        return true;
    }

    
    /// <summary>
    /// Get display information for the Dash ability (static version).
    /// </summary>
    /// <returns>Display information for UI.</returns>
    public static new UpgradeDisplayInfo GetDisplayInfo()
    {
        UpgradeDisplayInfo info = new UpgradeDisplayInfo();
        info.iconType = "dash";
        info.name = "Dash";
        info.icon = "➤";
        info.color = "rgba(100, 180, 255, 220)";
        info.description = GetDescription();
        return info;
    }
    
    /// <summary>
    /// Grant invulnerability frames during dash.
    /// </summary>
    private void GrantDashInvulnerability()
    {
        if (invulnerabilityDuration <= 0.0f)
            return;
            
        // Get or add InvulnerabilityComponent to the owner entity
        // Check parent entity first (player entity), then owner itself
        Entity targetEntity = owner;
        if (owner.transform.parent != null)
        {
            targetEntity = owner.transform.parent;
        }
        
        if (!targetEntity)
            return;
            
        var invulnerabilityComponent = targetEntity.GetComponent<InvulnerabilityComponent>();
        if (invulnerabilityComponent == null)
        {
            invulnerabilityComponent = targetEntity.AddComponent<InvulnerabilityComponent>();
        }
        
        invulnerabilityComponent.GrantInvulnerability(invulnerabilityDuration);
    }
    
    /// <summary>
    /// Get a description of the dash ability.
    /// </summary>
    /// <returns>Description string.</returns>
    public static string GetDescription()
    {
        return "Quickly dash in the movement direction, gaining brief invulnerability. Activated with Space key. 3 second cooldown.";
    }
    
    /// <summary>
    /// Configure the dash ability with default values.
    /// </summary>
    /// <param name="ability">The dash ability to configure.</param>
    public static void ConfigureAbility(DashAbility ability)
    {
        ability.cooldown = 3.0f;
        ability.dashForce = 22.0f;
        ability.invulnerabilityDuration = 0.5f;
    }
}

