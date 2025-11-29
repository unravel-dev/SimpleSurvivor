using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Example fireball ability that demonstrates how to create custom abilities for AbilityCard.
/// Fires a fireball projectile at the nearest enemy with area damage.
/// </summary>
[ScriptSourceFile]
public class FireballAbility : Ability
{
    [Tooltip("Prefab to instantiate as fireball projectile")]
    public Prefab fireballPrefab;

    [Tooltip("Base damage of the fireball")]
    public int damage = 15;

    [Tooltip("Explosion radius for area damage")]
    public float explosionRadius = 3.0f;

    [Tooltip("Maximum range to search for targets")]
    public float maxRange = 15.0f;

    [Tooltip("Speed of the fireball projectile")]
    public float projectileSpeed = 12.0f;

    [Tooltip("Spawn offset from the caster")]
    public Vector3 spawnOffset = Vector3.up;

    [Tooltip("Sound to play when casting the ability")]
    public AudioClip castSound;

    private TransformComponent transformComponent;


    /// <summary>
    /// Configure a Fireball ability with default values.
    /// </summary>
    /// <param name="ability">The ability instance to configure.</param>
    public static void ConfigureAbility(FireballAbility ability)
    {
        if (ability == null)
            return;

        ability.damage = 20;
        ability.cooldown = 3.5f;
        ability.explosionRadius = 3.0f;
        ability.maxRange = 15.0f;
        ability.projectileSpeed = 12.0f;
        ability.spawnOffset = Vector3.up;
    }

    public override void OnStart()
    {
        transformComponent = owner.GetComponent<TransformComponent>();

        if (fireballPrefab == null)
        {
            //Log.Warning($"FireballAbility on {owner.name}: No fireball prefab assigned!");
            fireballPrefab = Assets.GetAsset<Prefab>("app:/data/Abilities/Fireball.pfb");
        }

        // Load default cast sound if not assigned
        if (castSound == null)
        {
            castSound = Assets.GetAsset<AudioClip>("app:/data/Sounds/fireball/fireball-cast.mp3");
        }

        // Set default cooldown if not set
        if (cooldown <= 0)
        {
            cooldown = 3.0f;
        }
    }

    /// <summary>
    /// Gather targets for the fireball ability (finds closest enemy).
    /// </summary>
    /// <returns>List containing the closest enemy, or empty list if none found</returns>
    protected override Entity[] GatherTargets()
    {
        QueryClosestTarget query = new QueryClosestTarget();
        query.source = owner;
        query.maxRange = UpgradeSystem.ApplyAreaOfEffectUpgrade(maxRange);
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
            var source = AudioSourceComponent.PlayClipAtPoint(castSound, transformComponent.position, 1.0f);
            source.maxDistance = 100.0f;
        }

        return true;
    }

    /// <summary>
    /// Trigger the fireball ability - launch fireball at target.
    /// </summary>
    /// <param name="targets">List of targets (should contain one enemy)</param>
    protected override bool OnTriggerAbility(Entity[] targets, int castIndex)
    {
        if (fireballPrefab == null)
        {
            Log.Warning("FireballAbility: No fireball prefab assigned - cannot fire!");
            return false;
        }

        if (castIndex >= targets.Length)
        {
            castIndex = 0;
        }

        Entity target = targets[castIndex];
        var targetTransform = target.GetComponent<TransformComponent>();
        if (targetTransform == null)
        {
            Log.Warning("FireballAbility: Target has no TransformComponent");
            return false;
        }

        // Calculate spawn position and direction
        Vector3 spawnPosition = transformComponent.position + spawnOffset;
        Vector3 targetPosition = targetTransform.position + spawnOffset;
        Vector3 direction = (targetPosition - spawnPosition).normalized;

        // Instantiate the fireball projectile
        Entity fireballEntity = Scene.Instantiate(fireballPrefab);
        if (!fireballEntity)
        {
            Log.Error("FireballAbility: Failed to instantiate fireball prefab");
            return false;
        }
        fireballEntity.transform.position = spawnPosition;
        fireballEntity.transform.forward = direction;

        // Set up the projectile component
        var projectile = fireballEntity.AddComponent<Projectile>();
        if (projectile != null)
        {
            projectile.lifetime = UpgradeSystem.ApplyAreaOfEffectUpgrade(maxRange) / projectileSpeed; // Lifetime based on range
        }

        // Add damage source component to track damage statistics
        AddDamageSourceComponent(fireballEntity);

        fireballEntity.AddComponent<AutoDestroyComponent>();


        var pierceComponent = fireballEntity.AddComponent<PierceComponent>();
        if (pierceComponent != null)
        {
            pierceComponent.pierceCount = UpgradeSystem.ApplyPierceUpgrade(0);
        }

        var chainComponent = fireballEntity.AddComponent<ChainComponent>();
        if (chainComponent != null)
        {
            chainComponent.chainCount = UpgradeSystem.ApplyChainUpgrade(0);
            chainComponent.chainRange = UpgradeSystem.ApplyAreaOfEffectUpgrade(maxRange);
            chainComponent.chainOffset = spawnOffset;
        }
        // Add physical damage component with upgraded damage
        var damageComponent = fireballEntity.GetComponent<PhysicalDamageComponent>();
        if (damageComponent == null)
        {
            damageComponent = fireballEntity.AddComponent<PhysicalDamageComponent>();
        }

        int upgradedDamage = damage;
        damageComponent.SetDamage(upgradedDamage);

        // Add area damage component for explosion effect
        var areaDamageComponent = fireballEntity.AddComponent<AreaDamageComponent>();
        areaDamageComponent.explosionRadius = UpgradeSystem.ApplyAreaOfEffectUpgrade(explosionRadius);
        areaDamageComponent.damage = upgradedDamage;
        areaDamageComponent.damageLayerMask = LayerMask.GetMask("Enemy");

        // Add burn on hit component if upgrade is active
        float burnChance = UpgradeSystem.GetBurnOnHitChancePercent();
        if (burnChance > 0.0f)
        {
            var burnOnHit = fireballEntity.AddComponent<BurnOnHitComponent>();
            if (burnOnHit != null)
            {
                burnOnHit.burnChancePercent = burnChance;
                burnOnHit.burnStacks = UpgradeSystem.GetBurnOnHitStacks();
                burnOnHit.burnDamagePerSecond = 10.0f;
                burnOnHit.burnDuration = 5.0f;
                burnOnHit.maxBurnStacks = 3;
            }
        }

        var fireballPhysics = fireballEntity.GetComponent<PhysicsComponent>();
        if (fireballPhysics != null)
        {
            fireballPhysics.velocity = direction * projectileSpeed;
        }

        return true;
    }

    /// <summary>
    /// Get display information for the Fireball ability (static version).
    /// </summary>
    /// <returns>Display information for UI.</returns>
    public static new UpgradeDisplayInfo GetDisplayInfo()
    {
        UpgradeDisplayInfo info = new UpgradeDisplayInfo();
        info.iconType = "fireball";
        info.name = "Fireball";
        info.icon = "F";
        info.color = "rgba(255, 100, 50, 180)"; // Orange/red
        info.description = GetDescription();
        return info;
    }
    
    public static string GetDescription()
    {
        return "Shoots a fireball at the nearest enemy, dealing damage and causing an explosion.";
    }
}
