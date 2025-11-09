using System.Runtime.CompilerServices;
using Unravel.Core;


/// <summary>
/// Component that defines area damage properties for explosions and area effects.
/// The actual damage logic is handled by ContactSystem for consistency.
/// </summary>
[ScriptSourceFile]
public class AreaDamageComponent : ScriptComponent
{
    [Tooltip("Radius of the explosion")]
    public float explosionRadius = 3.0f;
    
    [Tooltip("Base damage dealt in the explosion area")]
    public int damage = 40;
    
    [Tooltip("Layer mask for entities that can be damaged")]
    public LayerMask damageLayerMask = LayerMask.GetMask("Enemy");
    
    [Tooltip("Whether to exclude the original target from area damage")]
    public bool excludeOriginalTarget = true;
    
    /// <summary>
    /// Get the current damage value.
    /// </summary>
    /// <returns>Base damage value</returns>
    public int GetDamage()
    {
        return damage;
    }
    
    /// <summary>
    /// Set the damage value.
    /// </summary>
    /// <param name="newDamage">New damage value</param>
    public void SetDamage(int newDamage)
    {
        damage = Mathf.Max(0, newDamage);
    }
    
    /// <summary>
    /// Get the explosion radius.
    /// </summary>
    /// <returns>Explosion radius</returns>
    public float GetExplosionRadius()
    {
        return explosionRadius;
    }
    
    /// <summary>
    /// Set the explosion radius.
    /// </summary>
    /// <param name="newRadius">New explosion radius</param>
    public void SetExplosionRadius(float newRadius)
    {
        explosionRadius = Mathf.Max(0.1f, newRadius);
    }
}
