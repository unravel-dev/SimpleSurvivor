using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Component that tracks the original damage source entity and ability component.
/// Should be added to projectiles, damage over time components, and other damage sources
/// to track which ability caused the damage for statistics purposes.
/// </summary>
[ScriptSourceFile]
public class DamageSourceComponent : ScriptComponent
{
    [Tooltip("The original ability entity (usually the player entity that has the ability)")]
    public Entity abilityEntity;
    
    [Tooltip("The ability component that created this damage source")]
    public Ability abilityComponent;
    
    /// <summary>
    /// Initialize the damage source component with the ability entity and component.
    /// </summary>
    /// <param name="abilityEntity">The entity that has the ability (usually the player).</param>
    /// <param name="abilityComponent">The ability component that created this damage source.</param>
    public void Initialize(Entity abilityEntity, Ability abilityComponent)
    {
        this.abilityEntity = abilityEntity;
        this.abilityComponent = abilityComponent;
    }
    
    /// <summary>
    /// Get the ability component that created this damage source.
    /// </summary>
    /// <returns>The ability component, or null if not set.</returns>
    public Ability GetAbilityComponent()
    {
        return abilityComponent;
    }
    
    /// <summary>
    /// Get the ability entity that has the ability component.
    /// </summary>
    /// <returns>The ability entity, or Entity.Invalid if not set.</returns>
    public Entity GetAbilityEntity()
    {
        return abilityEntity;
    }
}

