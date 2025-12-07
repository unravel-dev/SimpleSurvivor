using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Black Hole ability that creates a gravitational pull at a random position.
/// Spawns a black hole prefab that pulls enemies toward it for a duration.
/// </summary>
[ScriptSourceFile]
public class BlackHoleAbility : Ability
{
    [Tooltip("Prefab to instantiate as the black hole")]
    public Prefab blackHolePrefab;

    [Tooltip("Maximum range from player to spawn black hole")]
    public float maxRange = 15.0f;

    [Tooltip("Pull radius of the black hole")]
    public float pullRadius = 6.0f;

    [Tooltip("Strength of the pull force")]
    public float pullStrength = 15.0f;

    [Tooltip("How long the black hole lasts (in seconds)")]
    public float duration = 4.0f;


    [Tooltip("Doom damage per second")]
    public float doomDamagePerSecond = 5.0f;



    [Tooltip("Spawn offset from the caster")]
    public Vector3 spawnOffset = Vector3.up;

    /// <summary>
    /// Configure a Black Hole ability with default values.
    /// </summary>
    /// <param name="ability">The ability instance to configure.</param>
    public static void ConfigureAbility(BlackHoleAbility ability)
    {
        if (ability == null)
            return;

        ability.cooldown = 6.0f;
        ability.maxRange = 6.0f;
        ability.pullRadius = 6.0f;
        ability.pullStrength = 25.0f;
        ability.duration = 4.0f;
        ability.spawnOffset = Vector3.up * 4.0f;
        ability.doomDamagePerSecond = 2.0f;
    }

    public override void OnStart()
    {
        if (blackHolePrefab == null)
        {
            // Try to load a default prefab (you'll need to create this)
            blackHolePrefab = Assets.GetAsset<Prefab>("app:/data/Abilities/BlackHole.pfb");

            if (blackHolePrefab == null)
            {
                Log.Warning($"BlackHoleAbility on {owner.name}: No black hole prefab assigned and default not found!");
            }
        }

        // Set default cooldown if not set
        if (cooldown <= 0)
        {
            cooldown = 6.0f;
        }
        
        AddDamageSourceComponent(owner);

    }

    /// <summary>
    /// Gather targets for the black hole ability (not needed, spawns at random position).
    /// </summary>
    /// <returns>Dummy array to allow ability to trigger</returns>
    protected override Entity[] GatherTargets()
    {
        // Black hole spawns at random position, doesn't need specific targets
        return new Entity[] { owner };
    }

    /// <summary>
    /// Trigger the black hole ability - spawn black hole at random position.
    /// </summary>
    /// <param name="targets">Ignored for this ability.</param>
    /// <param name="castIndex">Used to offset multiple black holes when multicast is active.</param>
    /// <param name="totalCasts">The total number of casts in this trigger (including multicast).</param>
    protected override bool OnTriggerAbility(Entity[] targets, int castIndex, int totalCasts)
    {
        if (blackHolePrefab == null)
        {
            Log.Warning("BlackHoleAbility: No black hole prefab assigned - cannot spawn black hole!");
            return false;
        }

        // Get upgrade values
        float pullStrengthMultiplier = UpgradeSystem.GetBlackHolePullStrengthMultiplier();
        int stacksToApply = 1 + UpgradeSystem.GetBlackHoleDoomAdditionalStacks(); // Number of stacks to apply at once
        float doomDamageMultiplier = UpgradeSystem.GetBlackHoleDoomDamagePerStackMultiplier();

        // Apply area of effect upgrade to pull radius
        float upgradedPullRadius = UpgradeSystem.ApplyAreaOfEffectUpgrade(pullRadius);
        float upgradedMaxRange = maxRange;
        
        // Apply pull strength upgrade
        float upgradedPullStrength = pullStrength * pullStrengthMultiplier;
        
        // Apply duration upgrade
        float upgradedDuration = UpgradeSystem.ApplyDurationUpgrade(duration);
        
        // Apply doom damage upgrade
        float upgradedDoomDamage = doomDamagePerSecond * doomDamageMultiplier;

        // Calculate random position within range
        Vector2 randomCircle = Random.insideUnitCircle * upgradedMaxRange;
        Vector3 randomPosition = new Vector3(randomCircle.x, spawnOffset.y, randomCircle.y);
        Vector3 spawnPosition = owner.transform.position + randomPosition;


        // Spawn black hole immediately
        Entity blackHoleEntity = Scene.Instantiate(blackHolePrefab, ContainerCache.EffectsContainer);
        if (blackHoleEntity)
        {
            blackHoleEntity.transform.position = spawnPosition;
            ConfigureBlackHole(blackHoleEntity, upgradedPullRadius, upgradedPullStrength, 
                             upgradedDuration, upgradedDoomDamage, stacksToApply, owner);
        }

        return true;
    }

    /// <summary>
    /// Configure a black hole entity with the pull component and other necessary components.
    /// </summary>
    /// <param name="blackHoleEntity">The black hole entity to configure</param>
    /// <param name="radius">Pull radius</param>
    /// <param name="strength">Pull strength</param>
    /// <param name="lifeDuration">How long the black hole lasts</param>
    /// <param name="doomDamage">Doom damage per second</param>
    /// <param name="stacksToApply">Number of Doom stacks to apply at once</param>
    /// <param name="source">Source entity that created the black hole</param>
    private void ConfigureBlackHole(Entity blackHoleEntity, float radius, float strength, float lifeDuration, 
                                   float doomDamage, int stacksToApply, Entity source)
    {
        if (!blackHoleEntity)
        {
            return;
        }

        // Add pull component - this is the core functionality
        var pullComponent = blackHoleEntity.AddComponent<PullComponent>();
        if (pullComponent != null)
        {
            pullComponent.pullRadius = radius;
            pullComponent.pullStrength = strength;
            pullComponent.duration = lifeDuration;
            pullComponent.distanceBasedStrength = true;
            pullComponent.minStrengthMultiplier = 0.3f;
            pullComponent.pullLayerMask = LayerMask.GetMask("Enemy");
            pullComponent.callbackInterval = 0.2f;
            pullComponent.onAffectedEntities = (entities) => {
                // Calculate damage per tick
                float damagePerTick = doomDamage * pullComponent.callbackInterval;
                float duration = 1.0f;
                foreach (var entity in entities)
                {
                    // Apply multiple stacks of Doom at once (based on upgrades)
                    EffectsSystem.AddOrRefreshEffect<DoomComponent>(entity, source, damagePerTick, duration, stacksToApply);
                }
            };
        }

        // Add auto-destroy component to clean up when duration expires
        // (PullComponent will destroy the entity, but this is a safety measure)
        // blackHoleEntity.AddComponent<AutoDestroyComponent>();

        // // Add projectile component for lifetime tracking (optional, for consistency)
        // var projectileComponent = blackHoleEntity.AddComponent<Projectile>();
        // if (projectileComponent != null)
        // {
        //     projectileComponent.SetSource(source);
        //     projectileComponent.lifetime = lifeDuration;
        // }
    }

    /// <summary>
    /// Get display information for the Black Hole ability (static version).
    /// </summary>
    /// <returns>Display information for UI.</returns>
    public static new UpgradeDisplayInfo GetDisplayInfo()
    {
        UpgradeDisplayInfo info = new UpgradeDisplayInfo();
        info.iconType = "blackhole";
        info.name = "Black Hole";
        info.icon = "B";
        info.color = "rgba(100, 50, 200, 180)"; // Purple/dark purple
        info.description = GetDescription();
        return info;
    }
    
    public static string GetDescription()
    {
        return "Creates a black hole at a random location that pulls enemies toward it.";
    }
}

