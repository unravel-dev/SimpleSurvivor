using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Meteor Shower ability that calls down meteors from the sky at enemy positions.
/// Meteors have a delay before impact, dealing massive AoE damage and knockback.
/// </summary>
[ScriptSourceFile]
public class MeteorShowerAbility : Ability
{
    [Tooltip("Prefab to instantiate as the meteor")]
    public Prefab meteorPrefab;

    [Tooltip("Prefab to instantiate as ground indicator")]
    public Prefab indicatorPrefab;

    [Tooltip("Base damage per meteor impact")]
    public int damage = 50;

    [Tooltip("Impact radius for area damage")]
    public float impactRadius = 5.0f;

    [Tooltip("Maximum range to search for targets")]
    public float maxRange = 20.0f;

    [Tooltip("Spawn height above target")]
    public float spawnHeight = 15.0f;

    [Tooltip("Projectile fall speed")]
    public float projectileSpeed = 20.0f;

    [Tooltip("Knockback force applied to enemies")]
    public float knockbackForce = 10.0f;

    [Tooltip("Delay before meteor spawns (indicator warning time)")]
    public float spawnDelay = 0.8f;

    private TransformComponent transformComponent;

    /// <summary>
    /// Configure a Meteor Shower ability with default values.
    /// </summary>
    /// <param name="ability">The ability instance to configure.</param>
    public static void ConfigureAbility(MeteorShowerAbility ability)
    {
        if (ability == null)
            return;

        ability.damage = 5;
        ability.cooldown = 5.0f;
        ability.impactRadius = 5.0f;
        ability.maxRange = 8.0f;
        ability.spawnHeight = 15.0f;
        ability.projectileSpeed = 40.0f;
        ability.knockbackForce = 5.0f;
        ability.multicastPercent = 600.0f;
        ability.spawnDelay = 0.8f;
    }

    public override void OnStart()
    {
        transformComponent = owner.GetComponent<TransformComponent>();

        if (meteorPrefab == null)
        {
            // Try to load a default prefab
            meteorPrefab = Assets.GetAsset<Prefab>("app:/data/Abilities/Meteor.pfb");
            
            if (meteorPrefab == null)
            {
                Log.Warning($"MeteorShowerAbility on {owner.name}: No meteor prefab assigned and default not found!");
            }
        }

        if (indicatorPrefab == null)
        {
            // Try to load a default indicator prefab
            indicatorPrefab = Assets.GetAsset<Prefab>("app:/data/Abilities/MeteorIndicator.pfb");
            
            if (indicatorPrefab == null)
            {
                Log.Warning($"MeteorShowerAbility on {owner.name}: No indicator prefab assigned and default not found!");
            }
        }

        // Set default cooldown if not set
        if (cooldown <= 0)
        {
            cooldown = 5.0f;
        }
    }

    /// <summary>
    /// Gather targets for meteor strikes (finds enemies within range).
    /// </summary>
    /// <returns>Array of enemies to target with meteors</returns>
    protected override Entity[] GatherTargets()
    {
        // QueryClosestTarget query = new QueryClosestTarget();
        // query.source = owner;
        // query.maxRange = UpgradeSystem.ApplyAreaOfEffectUpgrade(maxRange);
        // return ContactSystem.FindClosestEnemies(query);
        return new Entity[] { Entity.Invalid };
    }

    /// <summary>
    /// Trigger the meteor shower ability - spawn meteors above targets.
    /// </summary>
    /// <param name="targets">List of targets to strike.</param>
    /// <param name="castIndex">Used to target different enemies when multicast is active.</param>
    protected override void OnTriggerAbility(Entity[] targets, int castIndex)
    {
        if (meteorPrefab == null)
        {
            Log.Warning("MeteorShowerAbility: No meteor prefab assigned - cannot spawn meteor!");
            return;
        }
        Vector2 randomCircle = Random.insideUnitCircle * maxRange;
        Vector3 randomPosition = new Vector3(randomCircle.x, 0, randomCircle.y);
        // Select random target
        // int randomIndex = Random.Range(0, targets.Length);
        // Entity target = targets[randomIndex];
        
        // var targetTransform = target.GetComponent<TransformComponent>();
        // if (targetTransform == null)
        // {
        //     Log.Warning("MeteorShowerAbility: Target has no TransformComponent");
        //     return;
        // }

        // Calculate impact position (where target currently is)
        // Vector3 impactPosition = targetTransform.position;
        Vector3 impactPosition = owner.transform.position + randomPosition;
        
        // Calculate fall time for indicator lifetime
        float fallTime = spawnHeight / projectileSpeed;
        
        // Calculate total warning time (indicator shows for delay + fall time)
        float totalWarningTime = spawnDelay + fallTime;
        
        // Spawn ground indicator at impact position
        if (indicatorPrefab != null)
        {
            Entity indicatorEntity = Scene.Instantiate(indicatorPrefab);
            if (indicatorEntity)
            {
                // Add ground indicator component
                var indicator = indicatorEntity.AddComponent<GroundIndicatorComponent>();
                if (indicator != null)
                {
                    indicator.lifetime = totalWarningTime; // Shows during delay + fall
                    indicator.radius = UpgradeSystem.ApplyAreaOfEffectUpgrade(impactRadius);
                    indicator.enablePulse = false;
                    indicator.pulseSpeed = 3.0f;
                }

                // Scale the indicator to match impact radius
                float upgradedRadius = UpgradeSystem.ApplyAreaOfEffectUpgrade(impactRadius);
                indicatorEntity.transform.scale = new Vector3(upgradedRadius, 0.1f, upgradedRadius);
                indicatorEntity.transform.position = impactPosition + Vector3.up * (0.15f * upgradedRadius * 0.5f);
            }
        }
        
        // Create a delayed spawner entity (empty entity to hold the delayed spawn component)
        Entity spawnerEntity = Scene.CreateEntity();
        spawnerEntity.name = "MeteorSpawner";

        // Add delayed spawn component
        var delayedSpawn = spawnerEntity.AddComponent<DelayedSpawnComponent>();
        if (delayedSpawn != null)
        {
            delayedSpawn.prefabToSpawn = meteorPrefab;
            delayedSpawn.spawnDelay = spawnDelay;
            delayedSpawn.spawnPosition = impactPosition + new Vector3(0, spawnHeight, 0);
            delayedSpawn.spawnDirection = Vector3.down;
            delayedSpawn.destroyAfterSpawn = true;

            // Store values for the callback
            int upgradedDamage = UpgradeSystem.ApplyDamageUpgrade(damage);
            float upgradedRadius = UpgradeSystem.ApplyAreaOfEffectUpgrade(impactRadius);
            float meteorLifetime = fallTime + 1.0f;
            Entity sourceEntity = owner;
            float meteorSpeed = projectileSpeed;
            float meteorKnockback = knockbackForce;

            // Set up callback to configure the meteor when it spawns
            delayedSpawn.onSpawnCallback = (Entity meteorEntity) =>
            {
                // Add projectile component
                var projectileComponent = meteorEntity.AddComponent<Projectile>();
                if (projectileComponent != null)
                {
                    projectileComponent.SetSource(sourceEntity);
                    projectileComponent.lifetime = meteorLifetime;
                }

                // Add auto-destroy component
                meteorEntity.AddComponent<AutoDestroyComponent>();

                // Add damage component
                var damageComponent = meteorEntity.AddComponent<PhysicalDamageComponent>();
                if (damageComponent != null)
                {
                    damageComponent.SetDamage(upgradedDamage);
                }

                // Add area damage component with knockback
                var areaDamageComponent = meteorEntity.AddComponent<AreaDamageComponent>();
                if (areaDamageComponent != null)
                {
                    areaDamageComponent.explosionRadius = upgradedRadius;
                    areaDamageComponent.damage = upgradedDamage;
                    areaDamageComponent.damageLayerMask = LayerMask.GetMask("Enemy");
                    areaDamageComponent.knockbackForce = meteorKnockback;
                }

                // Apply physics velocity downward
                var meteorPhysics = meteorEntity.GetComponent<PhysicsComponent>();
                if (meteorPhysics != null)
                {
                    meteorPhysics.velocity = Vector3.down * meteorSpeed;
                }
            };
        }
    }

    /// <summary>
    /// Get display information for the Meteor Shower ability.
    /// </summary>
    /// <returns>Display information for UI.</returns>
    public override AbilityDisplayInfo GetDisplayInfo()
    {
        AbilityDisplayInfo info = new AbilityDisplayInfo();
        info.type = "meteor";
        info.name = "Meteor Shower";
        info.icon = "M";
        info.color = "rgba(255, 80, 20, 180)"; // Bright orange/red
        return info;
    }

    public static string GetDescription()
    {
        return "Calls down meteors from the sky, dealing massive area damage after a short delay.";
    }
}

