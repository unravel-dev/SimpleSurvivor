using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unravel.Core;

/// <summary>
/// Lightning bolt ability that targets the closest enemy and fires a lightning projectile at them.
/// Fast-moving electric projectile with chain lightning capabilities.
/// </summary>
[ScriptSourceFile]
public class LightningBoltAbility : Ability
{
    [Tooltip("Prefab to instantiate as projectile")]
    public Prefab projectilePrefab;
    
    [Tooltip("Damage amount to apply to the projectile")]
    public int damage = 25;
    
    [Tooltip("Maximum range to search for targets")]
    public float maxRange = 10.0f;
    
    [Tooltip("Speed of the projectile")]
    public float projectileSpeed = 35.0f;


    [Tooltip("Number of projectiles to fire")]
    public int projectileCount = 1;


    [Tooltip("Number of times the projectile can chain")]
    public int chainCount = 0;


    [Tooltip("Spawn offset from the caster")]
    public Vector3 spawnOffset = Vector3.up;
    
    
    private TransformComponent transformComponent;
    
     
    /// <summary>
    /// Configure a Lightning Bolt ability with default values.
    /// </summary>
    /// <param name="ability">The ability instance to configure.</param>
    public static void ConfigureAbility(LightningBoltAbility ability)
    {
        if (ability == null)
            return;
            
        ability.damage = 30;
        ability.cooldown = 1.0f;
        ability.maxRange = 10.0f;
        ability.projectileSpeed = 55.0f;
        ability.projectileCount = 1;
        ability.chainCount = 2;
        ability.spawnOffset = Vector3.up;
    }
    public override void OnStart()
    {
        transformComponent = owner.GetComponent<TransformComponent>();
        
        if (projectilePrefab == null)
        {
            //Log.Warning($"LightningBoltAbility on {owner.name}: No projectile prefab assigned!");
            projectilePrefab = Assets.GetAsset<Prefab>("app:/data/Abilities/Spark.pfb");
        }
    }

    
    /// <summary>
    /// Gather targets for this ability. Finds all enemies within range and returns the closest one.
    /// </summary>
    /// <returns>List containing the closest enemy, or empty list if none found.</returns>
    protected override Entity[] GatherTargets()
    {
        QueryClosestTarget query = new QueryClosestTarget();
        query.source = owner;
        query.maxRange = UpgradeSystem.ApplyAreaOfEffectUpgrade(maxRange);
        return ContactSystem.FindClosestEnemies(query);
    }
    
    /// <summary>
    /// Execute the ability by creating a projectile aimed at the target.
    /// </summary>
    /// <param name="targets">List of target entities (should contain one enemy).</param>
    protected override void OnTriggerAbility(Entity[] targets, int castIndex)
    {                
        if (projectilePrefab == null)
        {
            Log.Error("LightningBoltAbility: No projectile prefab assigned!");
            return;
        }

        if(castIndex >= targets.Length)
        {
            castIndex = 0;
        }

        var target = targets[castIndex];

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
                Log.Error("LightningBoltAbility: Failed to instantiate projectile!");
                return;
            }

            projectileEntity.transform.position = sourcePosition;
            projectileEntity.transform.forward = direction;

            // Configure the projectile

            // If no Projectile component, add one
            var projectileComponent = projectileEntity.AddComponent<Projectile>();
            if (projectileComponent != null)
            {
                projectileComponent.SetSource(owner);
            }

            projectileEntity.AddComponent<AutoDestroyComponent>();

            // var pierceComponent = projectileEntity.AddComponent<PierceComponent>();
            // if (pierceComponent != null)
            // {
            //     pierceComponent.pierceCount = UpgradeSystem.ApplyPierceUpgrade(0);
            // }

            var chainComponent = projectileEntity.AddComponent<ChainComponent>();
            if (chainComponent != null)
            {
                chainComponent.chainCount = UpgradeSystem.ApplyChainUpgrade(chainCount);
                chainComponent.chainRange = UpgradeSystem.ApplyAreaOfEffectUpgrade(maxRange);
                chainComponent.chainOffset = spawnOffset;
            }


            // Add PhysicalDamageComponent to the projectile
            var damageComponent = projectileEntity.AddComponent<PhysicalDamageComponent>();
            if (damageComponent != null)
            {
                damageComponent.SetDamage(UpgradeSystem.ApplyDamageUpgrade(damage));
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
    /// Get display information for the Lightning Bolt ability.
    /// </summary>
    /// <returns>Display information for UI.</returns>
    public override AbilityDisplayInfo GetDisplayInfo()
    {
        AbilityDisplayInfo info = new AbilityDisplayInfo();
        info.type = "spark";
        info.name = "Lightning Bolt";
        info.icon = "L";
        info.color = "rgba(100, 150, 255, 180)"; // Blue
        return info;
    }

    public static string GetDescription()
    {
        return "Shoots a lightning bolt at the nearest enemy, dealing damage and causing a chain reaction.";
    }
}

