using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unravel.Core;

/// <summary>
/// Spark ability that targets the closest enemy and fires a lightning projectile at them.
/// Fast-moving electric projectile with chain lightning capabilities.
/// </summary>
[ScriptSourceFile]
public class SparkAbility : Ability
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
    
    [Tooltip("Sound to play when casting the ability")]
    public AudioClip castSound;
        
     
    /// <summary>
    /// Configure a Spark ability with default values.
    /// </summary>
    /// <param name="ability">The ability instance to configure.</param>
    public static void ConfigureAbility(SparkAbility ability)
    {
        if (ability == null)
            return;
            
        ability.damage = 10;
        ability.cooldown = 1.0f;
        ability.maxRange = 10.0f;
        ability.projectileSpeed = 40.0f;
        ability.projectileCount = 1;
        ability.chainCount = 4;
        ability.spawnOffset = Vector3.up;
    }

    public override void OnStart()
    {
        
        if (projectilePrefab == null)
        {
            //Log.Warning($"SparkAbility on {owner.name}: No projectile prefab assigned!");
            projectilePrefab = Assets.GetAsset<Prefab>("app:/data/Abilities/Spark.pfb");
        }
        
        // Load default cast sound if not assigned
        if (castSound == null)
        {
            castSound = Assets.GetAsset<AudioClip>("app:/data/Sounds/spark/electric-cast.mp3");
        }
    }

    public override float GetBaseCooldown()
    {
        float sparkFlatModifier = UpgradeSystem.GetSparkFlatCooldownModifier();
        return cooldown - sparkFlatModifier;
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
        query.requireLineOfSight = true;
        query.obstacleLayerMask = LayerMask.GetMask("Environment");
        return ContactSystem.FindClosestEnemies(query, LayerMask.GetMask("Enemy"));
    }

    public override bool TriggerAbility()
    {
        if(!base.TriggerAbility())
        {
            return false;
        }

        if (castSound != null)
        {
            var source = AudioSourceComponent.PlayClipAtPoint(castSound, transform.position, 1.0f);
            source.maxDistance = 100.0f;
        }

        return true;
    }
    
    /// <summary>
    /// Execute the ability by creating a projectile aimed at the target.
    /// </summary>
    /// <param name="targets">List of target entities (should contain one enemy).</param>
    /// <param name="castIndex">The index of the current cast (0-based).</param>
    /// <param name="totalCasts">The total number of casts in this trigger (including multicast).</param>
    protected override bool OnTriggerAbility(Entity[] targets, int castIndex, int totalCasts)
    {
        if (projectilePrefab == null)
        {
            Log.Error("SparkAbility: No projectile prefab assigned!");
            return false;
        }

        if (castIndex >= targets.Length)
        {
            castIndex = 0;
        }

        var target = targets[castIndex];

        {
            // Calculate spawn position
            Vector3 sourcePosition = transform.position + spawnOffset;

            // Calculate direction to target
            Vector3 targetPosition = target.transform.position + spawnOffset;
            Vector3 direction = (targetPosition - sourcePosition).normalized;

            // Instantiate the projectile
            Entity projectileEntity = Scene.Instantiate(projectilePrefab, ContainerCache.EffectsContainer);

            if (!projectileEntity)
            {
                Log.Error("SparkAbility: Failed to instantiate projectile!");
                return false;
            }

            projectileEntity.transform.position = sourcePosition;
            projectileEntity.transform.forward = direction;

            // Configure the projectile

            // If no Projectile component, add one
            projectileEntity.AddComponent<Projectile>();

            // Add damage source component to track damage statistics
            AddDamageSourceComponent(projectileEntity);

            projectileEntity.AddComponent<AutoDestroyComponent>();

            var pierceComponent = projectileEntity.AddComponent<PierceComponent>();
            if (pierceComponent != null)
            {
                pierceComponent.pierceCount = UpgradeSystem.ApplyPierceUpgrade(0);
            }

            var chainComponent = projectileEntity.AddComponent<ChainComponent>();
            if (chainComponent != null)
            {
                chainComponent.chainCount = UpgradeSystem.ApplyChainUpgrade(chainCount);
                chainComponent.chainRange = UpgradeSystem.ApplyAreaOfEffectUpgrade(maxRange);
                chainComponent.chainOffset = spawnOffset;
                // Enable bouncing if upgrade is active
                chainComponent.allowRevisitTargets = UpgradeSystem.HasLightningBouncing();
            }

            // Add area damage component for chain explosions if upgrade is active
            if (UpgradeSystem.HasLightningChainExplosion())
            {
                var areaDamageComponent = projectileEntity.AddComponent<AreaDamageComponent>();
                if (areaDamageComponent != null)
                {
                    float explosionRadius = UpgradeSystem.GetLightningChainExplosionRadius();
                    int explosionDamage = Mathf.RoundToInt(damage * (UpgradeSystem.GetLightningChainExplosionDamagePercent() / 100.0f));
                    areaDamageComponent.explosionRadius = explosionRadius;
                    areaDamageComponent.damage = explosionDamage;
                    areaDamageComponent.excludeOriginalTarget = false; // Allow explosion to hit original target too
                }
            }

            // Add stun component if upgrade is active
            if (UpgradeSystem.HasLightningStun())
            {
                var stunOnHitComponent = projectileEntity.AddComponent<StunOnHitComponent>();
                if (stunOnHitComponent != null)
                {
                    stunOnHitComponent.stunDuration = UpgradeSystem.GetLightningStunDuration();
                }
            }

            // Add split component if upgrade is active
            int splitCount = UpgradeSystem.GetLightningSplitCount();
            if (splitCount > 0)
            {
                var splitComponent = projectileEntity.AddComponent<SplitComponent>();
                if (splitComponent != null)
                {
                    splitComponent.subsplit = false;
                    splitComponent.splitCount = splitCount;
                    splitComponent.splitRange = UpgradeSystem.ApplyAreaOfEffectUpgrade(maxRange);
                    splitComponent.splitOffset = spawnOffset;
                }
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

        return true;
    }

    /// <summary>
    /// Get display information for the Spark ability (static version).
    /// </summary>
    /// <returns>Display information for UI.</returns>
    public static new UpgradeDisplayInfo GetDisplayInfo()
    {
        UpgradeDisplayInfo info = new UpgradeDisplayInfo();
        info.iconType = "spark";
        info.name = "Spark";
        info.icon = "L";
        info.color = "rgba(100, 150, 255, 180)"; // Blue
        info.description = GetDescription();
        return info;
    }
    
    public static string GetDescription()
    {
        return "Shoots a spark at the nearest enemy, dealing damage and causing a chain reaction.";
    }
}

