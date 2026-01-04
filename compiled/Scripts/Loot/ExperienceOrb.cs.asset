using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// ExperienceOrb component that represents a collectible experience point.
/// Handles attraction to player when in pickup range and provides experience value.
/// </summary>
[ScriptSourceFile]
public class ExperienceOrb : LootComponent
{
    //[Header("Experience Settings")]
    [Tooltip("Experience value this orb provides when collected")]
    public float experienceValue = 10.0f;
    // [Tooltip("Lifetime of the orb in seconds (0 = infinite)")]
    // public float lifetime = 30.0f;
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        base.OnStart();
    }
    
    /// <summary>
    /// Called every frame to update orb behavior.
    /// </summary>
    public override void OnUpdate()
    {
        // Update lifetime
        timeAlive += Time.deltaTime;
        
        // Check if orb should expire
        // if (lifetime > 0 && timeAlive >= lifetime)
        // {
        //     Scene.DestroyEntity(owner);
        //     return;
        // }
        
        // Call base update for attraction and floating logic
        base.OnUpdate();
    }
    
    /// <summary>
    /// Collect this orb and give experience to the player.
    /// </summary>
    protected override void CollectLoot()
    {
        if (!targetPlayer)
            return;
            
        // Try to give experience to player
        var Experience = targetPlayer.GetComponent<Experience>();
        if (Experience != null)
        {
            Experience.CollectExperience(experienceValue, owner);
        }

        base.CollectLoot();
    }
    
    /// <summary>
    /// Get the experience value of this orb.
    /// </summary>
    /// <returns>Experience value.</returns>
    public float GetExperienceValue()
    {
        return experienceValue;
    }
    
    /// <summary>
    /// Set the experience value of this orb.
    /// </summary>
    /// <param name="value">New experience value.</param>
    public void SetExperienceValue(float value)
    {
        experienceValue = Mathf.Max(0, value);
    }
    
}
