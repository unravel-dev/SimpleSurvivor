using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Boomerang Blade ability that creates orbiting blades around the player.
/// The blades circle the player for a duration, hitting enemies they pass through.
/// </summary>
[ScriptSourceFile]
public class BoomerangBladeAbility : Ability
{
    [Tooltip("Prefab to instantiate as the boomerang blade")]
    public Prefab bladePrefab;

    [Tooltip("Base damage per hit")]
    public int damage = 20;

    [Tooltip("Orbit radius around the player")]
    public float orbitRadius = 4.0f;

    [Tooltip("How long the blade orbits (in seconds)")]
    public float orbitDuration = 3.0f;

    [Tooltip("Rotation speed (degrees per second)")]
    public float rotationSpeed = 360.0f;

    [Tooltip("Spawn offset from the caster")]
    public Vector3 spawnOffset = Vector3.up;

    private TransformComponent transformComponent;

    /// <summary>
    /// Configure a Boomerang Blade ability with default values.
    /// </summary>
    /// <param name="ability">The ability instance to configure.</param>
    public static void ConfigureAbility(BoomerangBladeAbility ability)
    {
        if (ability == null)
            return;

        ability.damage = 20;
        ability.cooldown = 4.0f;
        ability.orbitRadius = 4.0f;
        ability.orbitDuration = 5.0f;
        ability.rotationSpeed = 360.0f;
        ability.spawnOffset = Vector3.up;
    }

    public override void OnStart()
    {
        transformComponent = owner.GetComponent<TransformComponent>();

        if (bladePrefab == null)
        {
            // Try to load a default prefab (you'll need to create this)
            bladePrefab = Assets.GetAsset<Prefab>("app:/data/Abilities/Boomerang.pfb");
            
            if (bladePrefab == null)
            {
                Log.Warning($"BoomerangBladeAbility on {owner.name}: No blade prefab assigned and default not found!");
            }
        }

        // Set default cooldown if not set
        if (cooldown <= 0)
        {
            cooldown = 4.0f;
        }
    }

    /// <summary>
    /// Boomerang blade doesn't need to gather targets - it hits whatever it touches.
    /// Return player as a dummy target to allow ability to trigger.
    /// </summary>
    protected override Entity[] GatherTargets()
    {
        // Return a dummy array with the player to allow ability to trigger
        return new Entity[] { owner };
    }

    /// <summary>
    /// Trigger the boomerang blade ability - spawn orbiting blades.
    /// </summary>
    /// <param name="targets">Ignored for this ability.</param>
    /// <param name="castIndex">Used to offset multiple blades when multicast is active.</param>
    protected override bool OnTriggerAbility(Entity[] targets, int castIndex)
    {
        if (bladePrefab == null)
        {
            Log.Warning("BoomerangBladeAbility: No blade prefab assigned - cannot spawn blade!");
            return false;
        }

        // Get upgrade values
        int totalBladeCount = UpgradeSystem.GetBoomerangBladeCount();
        float rotationSpeedMultiplier = UpgradeSystem.GetBoomerangRotationSpeedMultiplier();
        bool hasDualOrbit = UpgradeSystem.HasDualOrbit();
        bool hasPingPongOrbit = UpgradeSystem.HasPingPongOrbit();
        bool hasReturningBlade = UpgradeSystem.HasReturningBlade();
        float spinSpeedMultiplier = UpgradeSystem.GetBoomerangSpinSpeedMultiplier();
        float spinningSlashDamageMultiplier = UpgradeSystem.GetBoomerangSpinningSlashDamageMultiplier();
        float pingPongMaxRadiusMultiplier = UpgradeSystem.GetBoomerangPingPongMaxRadiusMultiplier();
        float pingPongSpeedMultiplier = UpgradeSystem.GetBoomerangPingPongSpeedMultiplier();

        // Apply area of effect upgrade to orbit radius
        float upgradedRadius = UpgradeSystem.ApplyAreaOfEffectUpgrade(orbitRadius);
        float baseRotationSpeed = rotationSpeed * rotationSpeedMultiplier;

        // Spawn blades
        for (int bladeIndex = 0; bladeIndex < totalBladeCount; bladeIndex++)
        {
            // Calculate starting angle offset
            // For multiple blades, spread them evenly around the circle
            float angleStep = 360.0f / totalBladeCount;
            float baseAngleOffset = bladeIndex * angleStep;
            
            // Add cast index offset for multicast
            float angleOffset = baseAngleOffset + (castIndex * (360.0f / Mathf.Max(1, castIndex + 1)));

            // Apply dual orbit - alternate blades rotate in opposite direction
            float directionMultiplier = 1.0f;
            if (hasDualOrbit && bladeIndex % 2 == 1)
            {
                directionMultiplier = -1.0f;
            }

            SpawnBlade(angleOffset, upgradedRadius, baseRotationSpeed * directionMultiplier, 
                      spawnOffset.y, hasPingPongOrbit, hasReturningBlade, 
                      spinSpeedMultiplier, spinningSlashDamageMultiplier, 
                      pingPongMaxRadiusMultiplier, pingPongSpeedMultiplier);
        }

        return true;
    }

    /// <summary>
    /// Spawn a single blade with the specified parameters.
    /// </summary>
    private void SpawnBlade(float angleOffset, float radius, float rotationSpeed, 
                           float heightOffset, bool hasPingPongOrbit, 
                           bool hasReturningBlade, float spinSpeedMultiplier, 
                           float spinningSlashDamageMultiplier, 
                           float pingPongMaxRadiusMultiplier, float pingPongSpeedMultiplier)
    {
        // Spawn the blade
        Entity bladeEntity = Scene.Instantiate(bladePrefab);
        if (!bladeEntity)
        {
            Log.Error("BoomerangBladeAbility: Failed to instantiate blade prefab");
            return;
        }

        // Position blade at starting angle
        Vector3 playerPosition = transformComponent.position + new Vector3(0, heightOffset, 0);
        float startAngleRad = angleOffset * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(
            Mathf.Cos(startAngleRad) * radius,
            0,
            Mathf.Sin(startAngleRad) * radius
        );
        bladeEntity.transform.position = playerPosition + offset;

        // Add the orbital movement component
        var orbitalComponent = bladeEntity.AddComponent<OrbitalMovementComponent>();
        if (orbitalComponent != null)
        {
            orbitalComponent.centerEntity = owner;
            orbitalComponent.orbitRadius = radius;
            orbitalComponent.rotationSpeed = rotationSpeed;
            orbitalComponent.currentAngle = angleOffset;
            orbitalComponent.heightOffset = heightOffset;
            
            // Configure ping-pong orbit
            orbitalComponent.enableRadiusPingPong = true;
            orbitalComponent.minRadius = 1.0f;
            
            // Apply ping-pong upgrades if active
            if (hasPingPongOrbit)
            {
                orbitalComponent.maxRadius = radius * pingPongMaxRadiusMultiplier;
                orbitalComponent.radiusPingPongSpeed = 1.0f * pingPongSpeedMultiplier;
            }
            else
            {
                orbitalComponent.maxRadius = radius;
                orbitalComponent.radiusPingPongSpeed = 1.0f;
            }
            
            orbitalComponent.visualSpinSpeed = orbitalComponent.visualSpinSpeed * spinSpeedMultiplier;
        }

        // Add projectile component for lifetime management
        var projectile = bladeEntity.AddComponent<Projectile>();
        if(projectile != null)
        {
            projectile.lifetime = orbitDuration;
        }

        // Add damage source component to track damage statistics
        AddDamageSourceComponent(bladeEntity);

        // Add auto-destroy component
        bladeEntity.AddComponent<AutoDestroyComponent>();

        // Add pierce component (blade can hit multiple enemies per pass)
        var pierceComponent = bladeEntity.AddComponent<PierceComponent>();
        if (pierceComponent != null)
        {
            pierceComponent.pierceCount = UpgradeSystem.ApplyPierceUpgrade(999); // High base pierce for orbital
        }

        // Add damage component with upgraded damage
        var damageComponent = bladeEntity.AddComponent<PhysicalDamageComponent>();
        if (damageComponent != null)
        {
            int baseUpgradedDamage = damage;
            // Apply spinning slash damage multiplier
            int finalDamage = Mathf.RoundToInt(baseUpgradedDamage * spinningSlashDamageMultiplier);
            damageComponent.SetDamage(finalDamage);
        }

        // Add returning blade component if upgrade is active
        if (hasReturningBlade)
        {
            var returningBlade = bladeEntity.AddComponent<ReturningBladeComponent>();
            if (returningBlade != null)
            {
                returningBlade.targetEntity = owner;
                returningBlade.returnSpeed = 15.0f;
                returningBlade.returnDistanceThreshold = 1.0f;
                returningBlade.returnPierceCount = 999; // High pierce for return journey
            }
        }

        // Make blade spin visually
        bladeEntity.transform.forward = Vector3.up;
    }

    /// <summary>
    /// Get display information for the Boomerang Blade ability.
    /// </summary>
    /// <returns>Display information for UI.</returns>
    public override AbilityDisplayInfo GetDisplayInfo()
    {
        AbilityDisplayInfo info = new AbilityDisplayInfo();
        info.type = "boomerang";
        info.name = "Boomerang Blade";
        info.icon = "B";
        info.color = "rgba(180, 180, 50, 180)"; // Yellow/gold
        return info;
    }

    public static string GetDescription()
    {
        return "Throws a blade that orbits around you, hitting enemies multiple times.";
    }
}

