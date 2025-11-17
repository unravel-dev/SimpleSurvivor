using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Component that applies burn stacks on hit with a chance.
/// Attach to projectiles or weapons to enable burn on hit.
/// </summary>
[ScriptSourceFile]
public class BurnOnHitComponent : ScriptComponent
{
    [Tooltip("Percent chance to apply burn on hit (0-100)")]
    public float burnChancePercent = 50.0f;
    
    [Tooltip("Number of burn stacks to apply when triggered")]
    public int burnStacks = 1;
    
    [Tooltip("Damage per second for the burn effect")]
    public float burnDamagePerSecond = 10.0f;
    
    [Tooltip("Duration of the burn effect in seconds")]
    public float burnDuration = 5.0f;
    
    [Tooltip("Maximum burn stacks allowed")]
    public int maxBurnStacks = 3;
}

