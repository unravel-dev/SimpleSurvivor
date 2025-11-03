using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unravel.Core;

/// <summary>
/// Example ability that targets the closest enemy and fires a projectile at them.
/// Demonstrates how to use the new ability system.
/// </summary>
[ScriptSourceFile]
public class ExampleAbility : Ability
{
    [Tooltip("Prefab to instantiate as projectile")]
    public Prefab projectilePrefab;
    
    [Tooltip("Damage amount to apply to the projectile")]
    public int damage = 25;
    
    [Tooltip("Maximum range to search for targets")]
    public float maxRange = 10.0f;
    
    [Tooltip("Speed of the projectile")]
    public float projectileSpeed = 15.0f;


    [Tooltip("Spawn offset from the caster")]
    public Vector3 spawnOffset = Vector3.up;
    
    
    private TransformComponent transformComponent;
    
     
    public override void OnStart()
    {
        transformComponent = owner.GetComponent<TransformComponent>();
        
        if (projectilePrefab == null)
        {
            Log.Warning($"ExampleAbility on {owner.name}: No projectile prefab assigned!");
        }
    }

    
    /// <summary>
    /// Gather targets for this ability. Finds all enemies within range and returns the closest one.
    /// </summary>
    /// <returns>List containing the closest enemy, or empty list if none found.</returns>
    protected override List<Entity> GatherTargets()
    {
        List<Entity> targets = new List<Entity>();

        QueryClosestTarget query = new QueryClosestTarget();
        query.source = owner;
        query.maxRange = maxRange;
        Entity closestEnemy = ContactSystem.FindClosestEnemy(query);

        // Add closest enemy to targets if found
        if (closestEnemy)
        {
            targets.Add(closestEnemy);
        }

        return targets;
    }
    
    /// <summary>
    /// Execute the ability by creating a projectile aimed at the target.
    /// </summary>
    /// <param name="targets">List of target entities (should contain one enemy).</param>
    protected override void OnTriggerAbility(List<Entity> targets)
    {
        if (targets == null || targets.Count == 0)
        {
            return;
        }
                
        if (projectilePrefab == null)
        {
            Log.Error("ExampleAbility: No projectile prefab assigned!");
            return;
        }
        foreach (var target in targets)
        {
        
            // Calculate spawn position
            Vector3 sourcePosition = transformComponent.position + spawnOffset;
            
            // Calculate direction to target
            Vector3 targetPosition = target.transform.position + spawnOffset;
            Vector3 direction = (targetPosition - sourcePosition).normalized;

            // Instantiate the projectile
            Entity projectileEntity = Scene.Instantiate(projectilePrefab);

            if (!projectileEntity)
            {
                Log.Error("ExampleAbility: Failed to instantiate projectile!");
                return;
            }

            projectileEntity.transform.position = sourcePosition;
            projectileEntity.transform.forward = direction;
                    
            // Configure the projectile

            // If no Projectile component, add one
            var projectileComponent = projectileEntity.AddComponent<Projectile>();
            if (projectileComponent != null)
            {
                projectileComponent.speed = projectileSpeed;
                projectileComponent.SetSource(owner);
            }

            // var pierceComponent = projectileEntity.AddComponent<PierceComponent>();
            // if (pierceComponent != null)
            // {
            //     pierceComponent.pierceCount = 1;
            // }

            var bounceComponent = projectileEntity.AddComponent<BounceComponent>();
            if (bounceComponent != null)
            {
                bounceComponent.bounceCount = 22;
                bounceComponent.bounceRange = maxRange;
                bounceComponent.bounceOffset = spawnOffset;
            }


            // Add PhysicalDamageComponent to the projectile
            var damageComponent = projectileEntity.AddComponent<PhysicalDamageComponent>();
            if (damageComponent != null)
            {
                damageComponent.SetDamage(damage);
            }

            // Apply physics force in the spread direction
            var iphysics = projectileEntity.GetComponent<PhysicsComponent>();
            if (iphysics != null)
            {
                iphysics.ApplyForce(direction * projectileSpeed, ForceMode.Impulse);
            }
        }
    }
   
    
    /// <summary>
    /// Update method to automatically trigger the ability when possible.
    /// </summary>
    public override void OnUpdate()
    {
        // Automatically trigger the ability when it's ready
        if (CanTriggerAbility())
        {
            TriggerAbility();
        }
    }
}
