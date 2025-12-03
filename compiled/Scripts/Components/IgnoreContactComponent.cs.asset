using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Component that allows a projectile to ignore collisions with specific targets.
/// Used to prevent split projectiles from immediately hitting the same target they split from.
/// Targets are removed from the list after being ignored once, and the component is removed when empty.
/// </summary>
[ScriptSourceFile]
public class IgnoreContactComponent : ScriptComponent
{
    [Tooltip("List of targets to ignore collision with on the next contact")]
    public List<Entity> targetsToIgnore = new List<Entity>();

    /// <summary>
    /// Check if a target should be ignored and remove it from the list if found.
    /// </summary>
    /// <param name="target">Target entity to check.</param>
    /// <returns>True if the target should be ignored, false otherwise.</returns>
    public bool ShouldIgnoreAndRemove(Entity target)
    {
        if (targetsToIgnore == null || targetsToIgnore.Count == 0)
        {
            return false;
        }

        int index = targetsToIgnore.IndexOf(target);
        if (index >= 0)
        {
            targetsToIgnore.RemoveAt(index);
            
            // Remove component if list is now empty
            if (targetsToIgnore.Count == 0)
            {
                owner.RemoveComponent(this);
            }
            
            return true;
        }

        return false;
    }

    public override void OnStart()
    {
        // Initialize list if null
        if (targetsToIgnore == null)
        {
            targetsToIgnore = new List<Entity>();
        }
    }
}

