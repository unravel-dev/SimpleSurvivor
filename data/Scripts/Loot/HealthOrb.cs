using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// HealthOrb component that represents a collectible health restoration item.
/// Handles attraction to player when in pickup range and provides health restoration.
/// </summary>
[ScriptSourceFile]
public class HealthOrb : LootComponent
{
    [Tooltip("Health amount this orb restores when collected")]
    public int healAmount = 10;
    
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
        
        // Call base update for attraction and floating logic
        base.OnUpdate();
    }
    
    /// <summary>
    /// Collect this orb and heal the player.
    /// </summary>
    protected override void CollectLoot()
    {
        if (!targetPlayer)
            return;
            
        // Try to heal the player
        var player = targetPlayer.GetComponent<Player>();
        if (player != null)
        {
            player.HealPlayer(healAmount);
        }

        base.CollectLoot();
    }
    
    /// <summary>
    /// Get the heal amount of this orb.
    /// </summary>
    /// <returns>Heal amount.</returns>
    public int GetHealAmount()
    {
        return healAmount;
    }
    
    /// <summary>
    /// Set the heal amount of this orb.
    /// </summary>
    /// <param name="amount">New heal amount.</param>
    public void SetHealAmount(int amount)
    {
        healAmount = Mathf.Max(1, amount);
    }
    
}
