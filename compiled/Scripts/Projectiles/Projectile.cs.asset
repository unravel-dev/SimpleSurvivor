using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Simple projectile component that handles movement, collision detection,
/// and automatic destruction. Uses ContactSystem for all interaction logic.
/// </summary>
[ScriptSourceFile]
public class Projectile : ScriptComponent
{
    [Tooltip("Speed of the projectile")]
    public float speed = 10.0f;
    
    [Tooltip("Lifetime of the projectile in seconds")]
    public float lifetime = 5.0f;

    // Internal state
    private float timeAlive = 0.0f;
    private Entity sourceEntity;

    
    /// <summary>
    /// Set the source entity for this projectile.
    /// </summary>
    /// <param name="source">Entity that created this projectile.</param>
    public void SetSource(Entity source)
    {
        sourceEntity = source;
    }
    
    public override void OnStart()
    {
        timeAlive = 0.0f;
    }
    
    public override void OnUpdate()
    {
        // Update lifetime
        timeAlive += Time.deltaTime;
        
        // Check if projectile should be destroyed due to lifetime
        if (timeAlive >= lifetime)
        {
            
            Scene.DestroyEntity(owner);
            return;
        }
    }

    
    /// <summary>
    /// Handle collision with another entity.
    /// </summary>
    /// <param name="other">The entity this projectile collided with.</param>
    public override void OnSensorEnter(Entity other)
    {
        if (!other)
            return;
            
        // Don't collide with the source entity
        if (sourceEntity && other == sourceEntity)
            return;
            
        // Use ContactSystem to handle the interaction
        ContactSystem.ApplyContact(owner, other);
    }

    public override void OnCollisionEnter(Collision collision)
    {
        if (collision == null)
            return;
        // Use ContactSystem to handle the interaction
        ContactSystem.ApplyContact(owner, collision.entity);
    }
    
    /// <summary>
    /// Get the remaining lifetime of this projectile.
    /// </summary>
    /// <returns>Remaining lifetime in seconds.</returns>
    public float GetRemainingLifetime()
    {
        return Mathf.Max(0f, lifetime - timeAlive);
    }
    
    /// <summary>
    /// Get the source entity that created this projectile.
    /// </summary>
    /// <returns>Source entity, or null if none set.</returns>
    public Entity GetSource()
    {
        return sourceEntity;
    }
}
