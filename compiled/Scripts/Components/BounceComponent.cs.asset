using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Component that defines physical damage properties for entities.
/// Used by the ContactSystem to determine damage amounts.
/// </summary>
[ScriptSourceFile]
public class BounceComponent : ScriptComponent
{
    [Tooltip("Amount of times this entity can bounce off of enemies")]
    public int bounceCount = 1;

    [Tooltip("Range to search for new targets")]
    public float bounceRange = 1f;

    public Vector3 bounceOffset = Vector3.zero;
}
