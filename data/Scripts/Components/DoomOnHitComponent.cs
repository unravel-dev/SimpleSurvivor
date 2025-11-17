using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Component that applies doom stacks on hit with a chance.
/// Attach to projectiles or weapons to enable doom on hit.
/// </summary>
[ScriptSourceFile]
public class DoomOnHitComponent : ScriptComponent
{
    [Tooltip("Percent chance to apply doom on hit (0-100)")]
    public float doomChancePercent = 50.0f;
    
    [Tooltip("Number of doom stacks to apply when triggered")]
    public int doomStacks = 1;
    
    [Tooltip("Damage per second for the doom effect")]
    public float doomDamagePerSecond = 20.0f;
    
    [Tooltip("Duration of the doom effect in seconds")]
    public float doomDuration = 3.0f;
    
    [Tooltip("Maximum doom stacks allowed (usually 1)")]
    public int maxDoomStacks = 1;
}

