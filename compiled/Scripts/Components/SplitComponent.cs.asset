using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

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

}

