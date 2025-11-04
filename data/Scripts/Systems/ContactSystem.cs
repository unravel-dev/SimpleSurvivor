using System.Runtime.CompilerServices;
using System.Collections.Generic;

using Unravel.Core;


public enum ContactResult
{
    Success = 0,
    Failure = 1,
    Exhausted,
}


public struct QueryClosestTarget
{
    public Entity source;
    public float maxRange;

    public List<Entity> visitedTargets;
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

        Entity closestNonVisitedEnemy = Entity.Invalid;
        float closestNonVisitedDistance = float.MaxValue;

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

            if(query.visitedTargets != null)
            {
                if (distance < closestNonVisitedDistance && !query.visitedTargets.Contains(entity))
                {
                    closestNonVisitedDistance = distance;
                    closestNonVisitedEnemy = entity;
                }
            }
        }

        if(closestNonVisitedEnemy != Entity.Invalid)
        {
            return closestNonVisitedEnemy;
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


        // Handle chain
        {
            ContactResult chainResult = HandleChain(source, target);
            if (chainResult == ContactResult.Exhausted)
            {
                return;
            }
        }

        // Handle physical damage
        HandlePhysicalDamage(source, target);
        // Handle other contact effects (can be extended)
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
    private static ContactResult HandleChain(Entity source, Entity target)
    {
        // Check for BounceComponent on source
        var chainComponent = source.GetComponent<ChainComponent>();
        if (chainComponent == null)
        {
            return ContactResult.Failure; // No bounce to apply
        }
        // Redirect projectile to new target
        // Reduce bounce count, etc.    
        if(chainComponent.chainCount <= 0)
        {
            return ContactResult.Exhausted; // No bounce to apply
        }

        // Apply bounce through the DamageSystem
        chainComponent.chainCount--;

        if (chainComponent.chainCount <= 0)
        {
            Scene.DestroyEntity(source);
        }
        else
        {
            chainComponent.visitedTargets.Add(target);

            var sourcePosition = chainComponent.owner.transform.position;

            QueryClosestTarget query = new QueryClosestTarget();
            query.source = target;
            query.maxRange = chainComponent.chainRange;
            query.visitedTargets = chainComponent.visitedTargets;
            Entity newTarget = FindClosestEnemy(query);

            if (!newTarget)
            {
                chainComponent.chainCount = 0;
                Scene.DestroyEntity(source);
                return ContactResult.Exhausted; // No new target to bounce to
            }

            // if the new target is already in the visited targets list, we looped back to the same target, so we clear the visited targets list
            if (chainComponent.visitedTargets.Contains(newTarget))
            {
                chainComponent.visitedTargets.Clear();
            }


            var targetPosition = newTarget.transform.position + chainComponent.chainOffset;
            var direction = (targetPosition - sourcePosition).normalized;
            chainComponent.owner.transform.forward = direction;

            var iphysics = chainComponent.owner.GetComponent<PhysicsComponent>();
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