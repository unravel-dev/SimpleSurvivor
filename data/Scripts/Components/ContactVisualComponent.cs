using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Component that holds a visual effect prefab to spawn on contact.
/// The ContactSystem will instantiate this prefab at the contact position.
/// </summary>
[ScriptSourceFile]
public class ContactVisualComponent : ScriptComponent
{
    [Tooltip("Visual effect prefab to spawn on contact")]
    public Prefab visualPrefab;
    
    [Tooltip("Position offset from the contact point")]
    public Vector3 positionOffset = Vector3.zero;
    
    [Tooltip("Rotation offset (in degrees)")]
    public Vector3 rotationOffset = Vector3.zero;
    
    [Tooltip("Scale multiplier for the spawned visual")]
    public float scaleMultiplier = 1.0f;
}

