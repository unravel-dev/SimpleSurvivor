using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Component that applies poison stacks on hit with a chance.
/// Attach to projectiles or weapons to enable poison on hit.
/// </summary>
[ScriptSourceFile]
public class PoisonOnHitComponent : ScriptComponent
{
    [Tooltip("Percent chance to apply poison on hit (0-100)")]
    public float poisonChancePercent = 50.0f;
    
    [Tooltip("Number of poison stacks to apply when triggered")]
    public int poisonStacks = 1;
    
    [Tooltip("Damage per second for the poison effect")]
    public float poisonDamagePerSecond = 8.0f;
    
    [Tooltip("Duration of the poison effect in seconds")]
    public float poisonDuration = 6.0f;
    
    [Tooltip("Maximum poison stacks allowed")]
    public int maxPoisonStacks = 5;
}

