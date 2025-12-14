using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// MagnetLoot component that represents a collectible magnet item.
/// When collected, temporarily increases the player's pickup range significantly.
/// </summary>
[ScriptSourceFile]
public class MagnetLoot : LootComponent
{
    [Tooltip("Duration of the magnet effect in seconds")]
    public float duration = 0.5f;
    
    /// <summary>
    /// Collect this magnet and apply the pickup range boost to the player.
    /// </summary>
    protected override void CollectLoot()
    {
        if (!targetPlayer)
            return;
            
        UpgradeSystem.ApplyMagnetEffect(99999, duration);
        
        base.CollectLoot();
    }
}

