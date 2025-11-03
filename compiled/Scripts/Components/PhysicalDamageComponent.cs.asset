using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Component that defines physical damage properties for entities.
/// Used by the ContactSystem to determine damage amounts.
/// </summary>
[ScriptSourceFile]
public class PhysicalDamageComponent : ScriptComponent
{
    [Tooltip("Amount of physical damage this entity deals")]
    public int damage = 10;

    
    /// <summary>
    /// Get the damage amount this component deals.
    /// </summary>
    /// <returns>Damage amount as integer.</returns>
    public int GetDamage()
    {
        return damage;
    }
    
    /// <summary>
    /// Set the damage amount for this component.
    /// </summary>
    /// <param name="newDamage">New damage amount.</param>
    public void SetDamage(int newDamage)
    {
        damage = Mathf.Max(0, newDamage);
    }
    
    /// <summary>
    /// Modify the damage by a multiplier.
    /// </summary>
    /// <param name="multiplier">Damage multiplier.</param>
    public void ModifyDamage(float multiplier)
    {
        int oldDamage = damage;
        damage = Mathf.RoundToInt(damage * multiplier);
        damage = Mathf.Max(0, damage);
    }
    
    public override void OnStart()
    {

    }
}
