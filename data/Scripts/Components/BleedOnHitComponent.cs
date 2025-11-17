using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Component that applies bleed stacks on hit with a chance.
/// Attach to projectiles or weapons to enable bleed on hit.
/// </summary>
[ScriptSourceFile]
public class BleedOnHitComponent : ScriptComponent
{
    [Tooltip("Percent chance to apply bleed on hit (0-100)")]
    public float bleedChancePercent = 50.0f;
    
    [Tooltip("Number of bleed stacks to apply when triggered")]
    public int bleedStacks = 1;
    
    [Tooltip("Damage per second for the bleed effect")]
    public float bleedDamagePerSecond = 12.0f;
    
    [Tooltip("Duration of the bleed effect in seconds")]
    public float bleedDuration = 4.0f;
    
    [Tooltip("Maximum bleed stacks allowed")]
    public int maxBleedStacks = 10;
}

