using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Policy for how split projectiles should be spawned.
/// </summary>
public enum SplitPolicy
{
    /// <summary>
    /// Find closest enemies as targets for split projectiles (default behavior).
    /// </summary>
    ClosestTarget,
    
    /// <summary>
    /// Spawn split projectiles in a radial pattern around the split point.
    /// </summary>
    Radial
}

/// <summary>
/// Component that allows a projectile to split into duplicates on contact.
/// Used for split/duplication effects.
/// </summary>
[ScriptSourceFile]
public class SplitComponent : ScriptComponent
{
    [Tooltip("Number of times this entity can split")]
    public int splitCount = 1;

    [Tooltip("Range to search for new targets for split projectiles")]
    public float splitRange = 10.0f;

    [Tooltip("Offset from target position when spawning split projectiles")]
    public Vector3 splitOffset = Vector3.zero;

    [Tooltip("List of targets already hit (to avoid re-hitting same targets)")]
    public List<Entity> visitedTargets = new List<Entity>();

    [Tooltip("Policy for how split projectiles should be spawned")]
    public SplitPolicy splitPolicy = SplitPolicy.ClosestTarget;

    [Tooltip("Number of projectiles to spawn per split (for radial policy)")]
    public int projectilesPerSplit = 2;

    [Tooltip("Starting angle offset for radial splits (in degrees)")]
    public float radialStartAngle = 0.0f;

    [Tooltip("Scale multiplier for split projectiles (1.0 = same size, <1.0 = smaller)")]
    public float splitProjectileScale = 1.0f;

    [Tooltip("Whether to allow sub-splitting of split projectiles")]
    public bool subsplit = true;

}

