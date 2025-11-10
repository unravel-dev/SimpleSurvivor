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
        ability.maxRange = 20.0f;
        ability.spawnHeight = 15.0f;
        ability.projectileSpeed = 40.0f;
        ability.knockbackForce = 5.0f;
        ability.multicastPercent = 600.0f;
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
        QueryClosestTarget query = new QueryClosestTarget();
        query.source = owner;
        query.maxRange = UpgradeSystem.ApplyAreaOfEffectUpgrade(maxRange);
        return ContactSystem.FindClosestEnemies(query);
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

        if (targets.Length == 0)
        {
            return;
        }

        // Select random target
        int randomIndex = Random.Range(0, targets.Length);
        Entity target = targets[randomIndex];
        
        var targetTransform = target.GetComponent<TransformComponent>();
        if (targetTransform == null)
        {
            Log.Warning("MeteorShowerAbility: Target has no TransformComponent");
            return;
        }

        // Calculate impact position (where target currently is)
        Vector3 impactPosition = targetTransform.position;
        
        // Spawn meteor high above the impact point
        Vector3 spawnPosition = impactPosition + new Vector3(0, spawnHeight, 0);

        // Instantiate the meteor
        Entity meteorEntity = Scene.Instantiate(meteorPrefab);
        if (!meteorEntity)
        {
            Log.Error("MeteorShowerAbility: Failed to instantiate meteor prefab");
            return;
        }

        meteorEntity.transform.position = spawnPosition;
        meteorEntity.transform.forward = Vector3.down; // Point downward

        // Add projectile component
        var projectileComponent = meteorEntity.AddComponent<Projectile>();
        if (projectileComponent != null)
        {
            projectileComponent.SetSource(owner);
            projectileComponent.lifetime = spawnHeight / projectileSpeed + 1.0f; // Calculate lifetime based on fall distance
        }

        // Add auto-destroy component
        meteorEntity.AddComponent<AutoDestroyComponent>();

        // Add damage component
        var damageComponent = meteorEntity.AddComponent<PhysicalDamageComponent>();
        if (damageComponent != null)
        {
            int upgradedDamage = UpgradeSystem.ApplyDamageUpgrade(damage);
            damageComponent.SetDamage(upgradedDamage);
        }

        // Add area damage component with knockback
        var areaDamageComponent = meteorEntity.AddComponent<AreaDamageComponent>();
        if (areaDamageComponent != null)
        {
            areaDamageComponent.explosionRadius = UpgradeSystem.ApplyAreaOfEffectUpgrade(impactRadius);
            areaDamageComponent.damage = UpgradeSystem.ApplyDamageUpgrade(damage);
            areaDamageComponent.damageLayerMask = LayerMask.GetMask("Enemy");
            areaDamageComponent.knockbackForce = knockbackForce;
        }

        // Apply physics velocity downward
        var meteorPhysics = meteorEntity.GetComponent<PhysicsComponent>();
        if (meteorPhysics != null)
        {
            meteorPhysics.velocity = Vector3.down * projectileSpeed;
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

