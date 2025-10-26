using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Interface for projectile effects that can be applied when projectiles collide with targets.
/// </summary>
public interface IWeaponEffect
{
    /// <summary>
    /// Apply the effect when the projectile collides with a target.
    /// </summary>
    /// <param name="projectile">The projectile entity that caused the collision.</param>
    /// <param name="target">The target entity that was hit.</param>
    /// <param name="collision">Details of the collision.</param>
    /// <returns>True if the effect was successfully applied.</returns>
    bool ApplyEffect(Entity projectile, Entity target, Collision collision);
    
    /// <summary>
    /// Get the name/type of this effect for debugging purposes.
    /// </summary>
    /// <returns>Effect name.</returns>
    string GetEffectName();
}
