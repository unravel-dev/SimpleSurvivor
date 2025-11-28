using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Component that defines physical damage properties for entities.
/// Used by the ContactSystem to determine damage amounts.
/// </summary>
[ScriptSourceFile]
public class ChainComponent : ScriptComponent
{
    [Tooltip("Amount of times this entity can bounce off of enemies")]
    public int chainCount = 1;

    [Tooltip("Range to search for new targets")]
    public float chainRange = 1f;

    public Vector3 chainOffset = Vector3.zero;

    public List<Entity> visitedTargets = new List<Entity>();
    
    [Tooltip("If true, allows revisiting already visited targets (bouncing behavior)")]
    public bool allowRevisitTargets = false;
}
