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
    
    // Define the order of effect execution
    static readonly System.Func<Entity, Entity, ContactResult>[] effectHandlers = 
    {
        HandlePierce,
        HandleChain,
        // Add new effects here in order of priority
        // HandleBounce,
        // HandleSplit,
        // etc.
    };
    
    // Static collections to avoid allocations per call
    private static readonly List<(Entity entity, float distance, bool isVisited)> tempEnemyList = new List<(Entity, float, bool)>(64);
    private static readonly List<Entity> tempResultList = new List<Entity>(64);
    

    
    /// <summary>
    /// Find multiple enemies within range, sorted by distance with non-visited prioritized over visited.
    /// Optimized for frequent calls - uses static collections to avoid allocations.
    /// </summary>
    /// <param name="query">Query parameters including source, range, and visited targets.</param>
    /// <param name="results">Output list that will be populated with sorted entities.</param>
    /// <returns>Number of entities found.</returns>
    public static int FindClosestEnemies(QueryClosestTarget query, List<Entity> results)
    {
        results.Clear();
        
        var sourcePosition = query.source.transform.position;
        // Perform sphere overlap to find potential targets
        var overlaps = Physics.SphereOverlap(sourcePosition, query.maxRange, LayerMask.GetMask("Enemy"));

        if (overlaps == null || overlaps.Length == 0)
        {
            return 0;
        }

        // Clear and reuse static collections
        tempEnemyList.Clear();
        
        // Use squared distance to avoid expensive sqrt calculations
        float maxRangeSquared = query.maxRange * query.maxRange;

        // Collect valid enemies with their squared distances and visited status
        for (int i = 0; i < overlaps.Length; i++)
        {
            var entity = overlaps[i];
            if (!entity) continue;

            // Skip self
            if (entity == query.source) continue;

            // Calculate squared distance (faster than Vector3.Distance)
            var deltaPos = entity.transform.position - sourcePosition;
            float distanceSquared = Vector3.Dot(deltaPos, deltaPos);
            // Early distance check
            if (distanceSquared > maxRangeSquared) continue;

            // Check if this entity has been visited
            bool isVisited = query.visitedTargets != null && query.visitedTargets.Contains(entity);

            tempEnemyList.Add((entity, distanceSquared, isVisited));
        }

        if (tempEnemyList.Count == 0)
        {
            return 0;
        }

        // Sort by priority: non-visited first (isVisited = false), then by squared distance
        tempEnemyList.Sort((a, b) =>
        {
            // First priority: non-visited targets come before visited targets
            if (a.isVisited != b.isVisited)
            {
                return a.isVisited.CompareTo(b.isVisited); // false (non-visited) < true (visited)
            }
            
            // Second priority: sort by squared distance within the same visited status
            return a.distance.CompareTo(b.distance);
        });

        // Populate results list directly
        for (int i = 0; i < tempEnemyList.Count; i++)
        {
            results.Add(tempEnemyList[i].entity);
        }

        return tempEnemyList.Count;
    }
    
    /// <summary>
    /// Find multiple enemies within range, sorted by distance with non-visited prioritized over visited.
    /// Convenience method that allocates and returns an array (use sparingly for performance).
    /// </summary>
    /// <param name="query">Query parameters including source, range, and visited targets.</param>
    /// <returns>Array of entities sorted by priority (non-visited first) then by distance.</returns>
    public static Entity[] FindClosestEnemies(QueryClosestTarget query)
    {
        tempResultList.Clear();
        int count = FindClosestEnemies(query, tempResultList);
        
        if (count == 0)
        {
            return new Entity[0];
        }
        
        var result = new Entity[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = tempResultList[i];
        }
        
        return result;
    }

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

            if (query.visitedTargets != null)
            {
                if (distance < closestNonVisitedDistance && !query.visitedTargets.Contains(entity))
                {
                    closestNonVisitedDistance = distance;
                    closestNonVisitedEnemy = entity;
                }
            }
        }

        if (closestNonVisitedEnemy != Entity.Invalid)
        {
            return closestNonVisitedEnemy;
        }
        return closestEnemy;
    }
    /// <summary>
    /// Apply contact effects between a source entity and target entity.
    /// Processes effects in order and determines if the projectile should be destroyed.
    /// </summary>
    /// <param name="source">The entity initiating contact (e.g., projectile, weapon).</param>
    /// <param name="target">The entity being contacted (e.g., enemy, player).</param>
    public static void ApplyContact(Entity source, Entity target)
    {

        if(source.HasComponent<DestroyedComponent>())
        {
            return;
        }
        bool shouldExtendLifetime = false;
        
        // Execute effects in order until one succeeds
        foreach (var handler in effectHandlers)
        {
            ContactResult result = handler(source, target);
            
            if (result == ContactResult.Success)
            {
                shouldExtendLifetime = true;
                break; // Stop processing further effects
            }
            
            // If result is Exhausted, continue to next effect
            // If result is Failure, continue to next effect
        }
        
        // Always handle damage regardless of other effects
        HandlePhysicalDamage(source, target);
        

        if (!shouldExtendLifetime)
        {
            // Probably check for an auto destroy component
            var autoDestroyComponent = source.GetComponent<AutoDestroyComponent>();
            if (autoDestroyComponent != null)
            {
                source.AddComponent<DestroyedComponent>();
                Scene.DestroyEntity(source);
            }
        }
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
            int finalDamage = UpgradeSystem.CalculateDamage(damageAmount);
            DamageSystem.ApplyDamage(target, source, finalDamage);
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
        if (chainComponent.chainCount > 0)
        {
            chainComponent.chainCount--;
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
                // Scene.DestroyEntity(source);
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

        // Check exhaust condition
        if (chainComponent.chainCount <= 0)
        {
            return ContactResult.Exhausted;
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

        if(pierceComponent.pierceCount >= 0)
        {
            // apply pierce
            pierceComponent.pierceCount--;
        }

        // Check exhaust condition
        if (pierceComponent.pierceCount < 0)
        {
            return ContactResult.Exhausted;
        }

        return ContactResult.Success;
    }
}