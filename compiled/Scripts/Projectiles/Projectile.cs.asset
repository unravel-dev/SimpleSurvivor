using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Simple projectile component that handles movement, collision detection,
/// and automatic destruction. Uses ContactSystem for all interaction logic.
/// </summary>
[ScriptSourceFile]
public class Projectile : ScriptComponent
{
    
    [Tooltip("Lifetime of the projectile in seconds")]
    public float lifetime = 5.0f;

    // Internal state
    private float timeAlive = 0.0f;

    private bool isDestroyed = false;
    
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

            if (!isDestroyed)
            {
                isDestroyed = true;
                var particleEmitter = owner.GetComponent<ParticleEmitterComponent>();
                if (particleEmitter != null)
                {
                    particleEmitter.Stop();
                }
                Scene.DestroyEntity(owner, 1.0f);
            }
            return;
        }
    }

    
    /// <summary>
    /// Handle collision with another entity.
    /// </summary>
    /// <param name="other">The entity this projectile collided with.</param>
    public override void OnSensorEnter(Collision collision)
    {
        if (collision == null)
            return;
            
        // Use collision contact point if available, otherwise use projectile position
        Vector3 contactPosition = collision.contacts.Length > 0 
            ? collision.contacts[0].point 
            : owner.transform.position;
            
        // Use ContactSystem to handle the interaction
        ContactSystem.ApplyContact(owner, collision.entity, contactPosition);
    }

    public override void OnCollisionEnter(Collision collision)
    {
        if (collision == null)
            return;
        
        // Use collision contact point if available, otherwise use projectile position
        Vector3 contactPosition = collision.contacts.Length > 0 
            ? collision.contacts[0].point 
            : owner.transform.position;
        
        // Use ContactSystem to handle the interaction
        ContactSystem.ApplyContact(owner, collision.entity, contactPosition);
    }
    
    /// <summary>
    /// Get the remaining lifetime of this projectile.
    /// </summary>
    /// <returns>Remaining lifetime in seconds.</returns>
    public float GetRemainingLifetime()
    {
        return Mathf.Max(0f, lifetime - timeAlive);
    }
}
