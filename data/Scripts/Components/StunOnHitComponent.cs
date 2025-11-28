using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Component that applies a stun effect when the entity hits a target.
/// Used for abilities that stun enemies on hit.
/// </summary>
[ScriptSourceFile]
public class StunOnHitComponent : ScriptComponent
{
    [Tooltip("Duration of the stun in seconds")]
    public float stunDuration = 0.5f;
}

