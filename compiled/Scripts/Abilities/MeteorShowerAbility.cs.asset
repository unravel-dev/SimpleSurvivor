using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Represents a single active meteor shower cast.
/// </summary>
public class ActiveMeteorShower
{
    public Vector3 centerPosition;
    public float remainingDuration;
    public float spawnTimer;
    public float spawnInterval;
    public float showerRadius;
    public float spawnHeight;
    public float meteorSpeed;
    public float trajectoryAngle;
    public int meteorDamage;
    public float impactRadius;
    public float knockbackForce;
}

/// <summary>
/// Meteor Shower ability that creates a sustained rain of meteors in an area.
/// Meteors fall continuously from the sky at an angle for a duration.
/// </summary>
[ScriptSourceFile]
public class MeteorShowerAbility : Ability
{
    [Tooltip("Prefab to instantiate as the meteor")]
    public Prefab meteorPrefab;


    [Tooltip("Prefab to instantiate as the meteor impact")]
    public Prefab meteorImpactPrefab;

    [Tooltip("Base damage per meteor impact")]
    public int damage = 10;

    [Tooltip("Impact radius for area damage")]
    public float impactRadius = 3.0f;

    [Tooltip("Duration of the meteor shower")]
    public float showerDuration = 5.0f;

    [Tooltip("Interval between meteor spawns")]
    public float spawnInterval = 0.3f;

    [Tooltip("Radius of the shower area")]
    public float showerRadius = 8.0f;

    [Tooltip("Spawn height above target")]
    public float spawnHeight = 15.0f;

    [Tooltip("Projectile fall speed")]
    public float meteorSpeed = 25.0f;

    [Tooltip("Trajectory angle offset in degrees")]
    public float trajectoryAngle = 15.0f;

    [Tooltip("Knockback force applied to enemies")]
    public float knockbackForce = 5.0f;

    // List of active meteor showers
    private List<ActiveMeteorShower> activeShowers = new List<ActiveMeteorShower>();

    /// <summary>
    /// Configure a Meteor Shower ability with default values.
    /// </summary>
    /// <param name="ability">The ability instance to configure.</param>
    public static void ConfigureAbility(MeteorShowerAbility ability)
    {
        if (ability == null)
            return;

        ability.damage = 8;
        ability.cooldown = 10.0f; // Bigger cooldown
        ability.impactRadius = 2.0f;
        ability.showerDuration = 2.0f;
        ability.spawnInterval = 0.05f;
        ability.showerRadius = 4.0f;
        ability.spawnHeight = 25.0f;
        ability.meteorSpeed = 25.0f;
        ability.trajectoryAngle = 15.0f;
        ability.knockbackForce = 5.0f;
    }

    public override void OnStart()
    {
        if (meteorPrefab == null)
        {
            // Try to load a default prefab
            meteorPrefab = Assets.GetAsset<Prefab>("app:/data/Abilities/Meteor.pfb");
            
            if (meteorPrefab == null)
            {
                Log.Warning($"MeteorShowerAbility on {owner.name}: No meteor prefab assigned and default not found!");
            }
        }

        if (meteorImpactPrefab == null)
        {
            meteorImpactPrefab = Assets.GetAsset<Prefab>("app:/data/Abilities/MeteorIndicator.pfb");
            if (meteorImpactPrefab == null)
            {
                Log.Warning($"MeteorShowerAbility on {owner.name}: No meteor impact prefab assigned and default not found!");
            }
        }

        // Set default cooldown if not set
        if (cooldown <= 0)
        {
            cooldown = 12.0f;
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        // Process active showers (backwards iteration to allow removal)
        for (int i = activeShowers.Count - 1; i >= 0; i--)
        {
            var shower = activeShowers[i];

            // Update duration
            shower.remainingDuration -= Time.deltaTime;

            // Remove expired showers
            if (shower.remainingDuration <= 0.0f)
            {
                activeShowers.RemoveAt(i);
                continue;
            }

            // Update spawn timer
            shower.spawnTimer -= Time.deltaTime;

            // Spawn meteor if timer expired
            if (shower.spawnTimer <= 0.0f)
            {
                SpawnMeteor(shower);
                shower.spawnTimer = shower.spawnInterval;
            }
        }

    }

    /// <summary>
    /// Gather targets - not used for Meteor Shower (area effect).
    /// </summary>
    /// <returns>Empty array</returns>
    protected override Entity[] GatherTargets()
    {
        return new Entity[] { Entity.Invalid };
    }

    /// <summary>
    /// Trigger the meteor shower ability - create a new active shower instance.
    /// </summary>
    /// <param name="targets">Not used for area effect.</param>
    /// <param name="castIndex">Not used for area effect.</param>
    /// <param name="totalCasts">The total number of casts in this trigger (including multicast).</param>
    protected override bool OnTriggerAbility(Entity[] targets, int castIndex, int totalCasts)
    {
        if (meteorPrefab == null)
        {
            Log.Warning("MeteorShowerAbility: No meteor prefab assigned - cannot create meteor shower!");
            return false;
        }

        // Apply upgrades
        int upgradedDamage = damage;
        float upgradedImpactRadius = UpgradeSystem.ApplyAreaOfEffectUpgrade(impactRadius);
        float upgradedShowerRadius = UpgradeSystem.ApplyAreaOfEffectUpgrade(showerRadius);
        float upgradedSpawnInterval = UpgradeSystem.ApplyCooldownReductionUpgrade(spawnInterval);
        float upgradedShowerDuration = UpgradeSystem.ApplyDurationUpgrade(showerDuration);
        // Create a new active shower instance
        ActiveMeteorShower shower = new ActiveMeteorShower
        {
            centerPosition = owner.transform.position,
            remainingDuration = upgradedShowerDuration,
            spawnTimer = 0.0f, // Spawn first meteor immediately
            spawnInterval = upgradedSpawnInterval,
            showerRadius = upgradedShowerRadius,
            spawnHeight = spawnHeight,
            meteorSpeed = meteorSpeed,
            trajectoryAngle = trajectoryAngle,
            meteorDamage = upgradedDamage,
            impactRadius = upgradedImpactRadius,
            knockbackForce = knockbackForce
        };
        
        // Add to active showers list
        activeShowers.Add(shower);
        return true;
    }

    /// <summary>
    /// Spawn a single meteor for an active shower.
    /// </summary>
    /// <param name="shower">The active shower to spawn a meteor for.</param>
    private void SpawnMeteor(ActiveMeteorShower shower)
    {
        if (meteorPrefab == null)
            return;
        
        // Calculate random position within shower radius
        Vector2 randomCircle = Random.insideUnitCircle * shower.showerRadius;
        Vector3 impactPosition = shower.centerPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
        
        // Calculate spawn position above impact point
        Vector3 spawnPosition = impactPosition + Vector3.up * shower.spawnHeight;
        
        // Add random horizontal offset based on trajectory angle
        float angleRad = shower.trajectoryAngle * Mathf.Deg2Rad;
        float horizontalOffset = shower.spawnHeight * Mathf.Tan(angleRad);
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        spawnPosition += new Vector3(randomDirection.x * horizontalOffset, 0, randomDirection.y * horizontalOffset);
        
        // Spawn meteor
        Entity meteorEntity = Scene.Instantiate(meteorPrefab, ContainerCache.EffectsContainer);
        if (!meteorEntity)
            return;
        
        meteorEntity.transform.position = spawnPosition;
        // meteorEntity.transform.scale *= shower.impactRadius;
        // Calculate velocity towards impact point
        Vector3 directionToImpact = (impactPosition - spawnPosition).normalized;
        Vector3 velocity = directionToImpact * shower.meteorSpeed;
        
        // Add projectile component
        var projectileComponent = meteorEntity.AddComponent<Projectile>();
        if (projectileComponent != null)
        {
            projectileComponent.lifetime = (shower.spawnHeight * 1.5f) / shower.meteorSpeed;
        }
        
        // Add damage source component to track damage statistics
        AddDamageSourceComponent(meteorEntity);
        
        // Add auto-destroy component
        meteorEntity.AddComponent<AutoDestroyComponent>();
        
        // Add damage component
        var damageComponent = meteorEntity.AddComponent<PhysicalDamageComponent>();
        if (damageComponent != null)
        {
            damageComponent.SetDamage(shower.meteorDamage);
        }
        
        // Add area damage component with knockback
        var areaDamageComponent = meteorEntity.AddComponent<AreaDamageComponent>();
        if (areaDamageComponent != null)
        {
            areaDamageComponent.explosionRadius = shower.impactRadius;
            areaDamageComponent.damage = shower.meteorDamage;
            areaDamageComponent.damageLayerMask = LayerMask.GetMask("Enemy");
            areaDamageComponent.knockbackForce = shower.knockbackForce;
        }
        
        // Add contact visual component
        var contactVisualComponent = meteorEntity.AddComponent<ContactVisualComponent>();
        if (contactVisualComponent != null)
        {
            contactVisualComponent.visualPrefab = meteorImpactPrefab;
            // contactVisualComponent.scaleMultiplier = shower.impactRadius;
        }
        // Apply physics velocity
        var meteorPhysics = meteorEntity.GetComponent<PhysicsComponent>();
        if (meteorPhysics != null)
        {
            meteorPhysics.velocity = velocity;
        }
    }

    /// <summary>
    /// Get display information for the Meteor Shower ability (static version).
    /// </summary>
    /// <returns>Display information for UI.</returns>
    public static new UpgradeDisplayInfo GetDisplayInfo()
    {
        UpgradeDisplayInfo info = new UpgradeDisplayInfo();
        info.iconType = "meteorshower";
        info.name = "Meteor Shower";
        info.icon = "M";
        info.color = "rgba(255, 80, 20, 180)"; // Bright orange/red
        info.description = GetDescription();
        return info;
    }
    
    public static string GetDescription()
    {
        return "Creates a sustained rain of meteors that fall continuously in an area, dealing area damage on impact.";
    }
}

