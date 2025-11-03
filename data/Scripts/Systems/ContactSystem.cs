using System.Runtime.CompilerServices;
using Unravel.Core;


public enum ContactResult
{
    Success = 0,
    Failure = 1,
    Exhausted,
}
/// <summary>
/// System that handles contact between entities and determines what effects to apply
/// based on the components attached to the source and target entities.
/// </summary>
[ScriptSourceFile]
public static class ContactSystem
{
    
    
    /// <summary>
    /// Find the closest enemy within range using SphereOverlap.
    /// </summary>
    /// <returns>The closest enemy entity, or Entity.Invalid if none found.</returns>
    public static Entity FindClosestEnemy(QueryClosestTarget query)
    {
        var sourcePosition = query.source.transform.position;
        // Perform sphere overlap to find potential targets
        var overlaps = Physics.SphereOverlap(sourcePosition, query.maxRange, LayerMask.GetMask("Enemy"));

        if (overlaps == null || overlaps.Length == 0)
        {
            return Entity.Invalid;
        }

        Entity closestEnemy = Entity.Invalid;
        float closestDistance = float.MaxValue;

        // Find the closest valid enemy
        foreach (var entity in overlaps)
        {
            if (!entity) continue;

            // Skip self
            if (entity == query.source) continue;

            // Calculate distance
            float distance = Vector3.Distance(sourcePosition, entity.transform.position);

            // Check if this is the closest so far
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = entity;
            }
        }

        return closestEnemy;
    }
    /// <summary>
    /// Apply contact effects between a source entity and target entity.
    /// Checks components on both entities to determine what should happen.
    /// </summary>
    /// <param name="source">The entity initiating contact (e.g., projectile, weapon).</param>
    /// <param name="target">The entity being contacted (e.g., enemy, player).</param>
    public static void ApplyContact(Entity source, Entity target)
    {

        // Handle pierce
        {
            ContactResult pierceResult = HandlePierce(source, target);
            if (pierceResult == ContactResult.Exhausted)
            {
                return;
            }
        }


        // Handle bounce
        {
            ContactResult bounceResult = HandleBounce(source, target);
            if (bounceResult == ContactResult.Exhausted)
            {
                return;
            }
        }

        // Handle physical damage
        HandlePhysicalDamage(source, target);
        // Handle other contact effects (can be extended)
        // HandleBounce(source, target);
        // HandleStatusEffects(source, target);
        // etc.
    }
    
    
    /// <summary>
    /// Handle physical damage application from source to target.
    /// </summary>
    /// <param name="source">Source entity with potential PhysicalDamageComponent.</param>
    /// <param name="target">Target entity to receive damage.</param>
    private static void HandlePhysicalDamage(Entity source, Entity target)
    {
        // Check if source has physical damage component
        var damageComponent = source.GetComponent<PhysicalDamageComponent>();
        if (damageComponent == null)
        {
            return; // No damage to apply
        }
        
        // Apply damage through the DamageSystem
        int damageAmount = damageComponent.GetDamage();
        if (damageAmount > 0)
        {
            DamageSystem.ApplyDamage(target, source, damageAmount);
        }
    }
    
    /// <summary>
    /// Example method for handling bounce effects (placeholder for future implementation).
    /// </summary>
    /// <param name="source">Source entity (e.g., projectile).</param>
    /// <param name="target">Target entity that was hit.</param>
    private static ContactResult HandleBounce(Entity source, Entity target)
    {
        // Check for BounceComponent on source
        var bounceComponent = source.GetComponent<BounceComponent>();
        if (bounceComponent == null)
        {
            return ContactResult.Failure; // No bounce to apply
        }
        // Redirect projectile to new target
        // Reduce bounce count, etc.    
        if(bounceComponent.bounceCount <= 0)
        {
            return ContactResult.Exhausted; // No bounce to apply
        }

        // Apply bounce through the DamageSystem
        bounceComponent.bounceCount--;


        if (bounceComponent.bounceCount <= 0)
        {
            Scene.DestroyEntity(source);
        }
        else
        {
            var sourcePosition = bounceComponent.owner.transform.position;

            QueryClosestTarget query = new QueryClosestTarget();
            query.source = target;
            query.maxRange = bounceComponent.bounceRange;
            Entity newTarget = FindClosestEnemy(query);

            if (!newTarget)
            {
                bounceComponent.bounceCount = 0;
                Scene.DestroyEntity(source);
                return ContactResult.Exhausted; // No new target to bounce to
            }
            var targetPosition = newTarget.transform.position + bounceComponent.bounceOffset;
            var direction = (targetPosition - sourcePosition).normalized;
            bounceComponent.owner.transform.forward = direction;

            var iphysics = bounceComponent.owner.GetComponent<PhysicsComponent>();
            if (iphysics != null)
            {
                var velocity = iphysics.velocity;
                velocity = direction * velocity.magnitude;
                iphysics.velocity = velocity;
            }
        }
        return ContactResult.Success;
    }

    private static ContactResult HandlePierce(Entity source, Entity target)
    {
        // Check if source has pierce component
        var pierceComponent = source.GetComponent<PierceComponent>();
        if (pierceComponent == null)
        {
            return ContactResult.Failure; // No pierce to apply
        }

        if(pierceComponent.pierceCount <= 0)
        {
            return ContactResult.Exhausted; // No pierce to apply
        }

        // Apply pierce through the DamageSystem
        pierceComponent.pierceCount--;
        if (pierceComponent.pierceCount <= 0)
        {
            Scene.DestroyEntity(source);
        }
        return ContactResult.Success;
    }
}