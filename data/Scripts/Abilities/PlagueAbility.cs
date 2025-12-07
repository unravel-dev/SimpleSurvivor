using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Represents a single active plague cast.
/// </summary>
public class ActivePlague
{
    public Vector3 centerPosition;
    public float remainingDuration;
    public float tickTimer;
    public float tickInterval;
    public float auraRadius;
    public int plagueDamage;
    public float poisonChancePercent;
    public int poisonStacks;
    public float healChancePercent;
    public int healAmount;
    public Entity effectEntity; // Visual effect entity attached to the player
}

/// <summary>
/// Plague ability that creates an aura around the player dealing damage over time to enemies in range.
/// </summary>
[ScriptSourceFile]
public class PlagueAbility : Ability
{
    [Tooltip("Base damage per tick")]
    public int damage = 8;

    [Tooltip("Duration of the plague aura")]
    public float plagueDuration = 6.0f;

    [Tooltip("Interval between damage ticks in seconds")]
    public float tickInterval = 0.5f;

    [Tooltip("Radius of the plague aura")]
    public float auraRadius = 5.0f;

    [Tooltip("Prefab to instantiate as the plague visual effect")]
    public Prefab plagueEffectPrefab;

    
    [Tooltip("Spawn offset from the caster")]
    public Vector3 spawnOffset = Vector3.up;

    // List of active plague casts
    private List<ActivePlague> activePlagues = new List<ActivePlague>();

    /// <summary>
    /// Configure a Plague ability with default values.
    /// </summary>
    /// <param name="ability">The ability instance to configure.</param>
    public static void ConfigureAbility(PlagueAbility ability)
    {
        if (ability == null)
            return;

        ability.damage = 8;
        ability.cooldown = 10.0f;
        ability.plagueDuration = 6.0f;
        ability.tickInterval = 0.5f;
        ability.auraRadius = 5.0f;
    }

    public override void OnStart()
    {
        if (plagueEffectPrefab == null)
        {
            plagueEffectPrefab = Assets.GetAsset<Prefab>("app:/data/Abilities/PlagueEffect.pfb");
        }

        // Set default cooldown if not set
        if (cooldown <= 0)
        {
            cooldown = 10.0f;
        }

        AddDamageSourceComponent(owner);
    }

    public override void OnDestroy()
    {
        // Clean up all remaining visual effects
        foreach (var plague in activePlagues)
        {
            if (plague.effectEntity)
            {
                Scene.DestroyEntity(plague.effectEntity);
            }
        }
        activePlagues.Clear();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        // Process active plagues (backwards iteration to allow removal)
        for (int i = activePlagues.Count - 1; i >= 0; i--)
        {
            var plague = activePlagues[i];

            // Update duration
            plague.remainingDuration -= Time.deltaTime;

            // Remove expired plagues
            if (plague.remainingDuration <= 0.0f)
            {
                // Destroy the visual effect
                if (plague.effectEntity)
                {
                    Scene.DestroyEntity(plague.effectEntity);
                }
                activePlagues.RemoveAt(i);
                continue;
            }

            // Update center position to follow player
            plague.centerPosition = owner.transform.position;

            // Update tick timer
            plague.tickTimer -= Time.deltaTime;

            // Apply damage if timer expired
            if (plague.tickTimer <= 0.0f)
            {
                ApplyPlagueDamage(plague);
                plague.tickTimer = plague.tickInterval;
            }
        }
    }

    /// <summary>
    /// Gather targets - not used for Plague (area effect).
    /// </summary>
    /// <returns>Empty array</returns>
    protected override Entity[] GatherTargets()
    {
        return new Entity[] { Entity.Invalid };
    }

    /// <summary>
    /// Trigger the plague ability - create a new active plague instance.
    /// </summary>
    /// <param name="targets">Not used for area effect.</param>
    /// <param name="castIndex">Not used for area effect.</param>
    /// <param name="totalCasts">The total number of casts in this trigger (including multicast).</param>
    protected override bool OnTriggerAbility(Entity[] targets, int castIndex, int totalCasts)
    {
        if (plagueEffectPrefab == null)
        {
            Log.Warning("PlagueAbility: No plague effect prefab assigned - cannot create plague!");
            return false;
        }

        // Apply upgrades
        int upgradedDamage = damage;
        float upgradedAuraRadius = UpgradeSystem.ApplyAreaOfEffectUpgrade(auraRadius);
        float upgradedTickInterval = UpgradeSystem.GetPlagueTickIntervalMultiplier() * tickInterval;
        float upgradedPlagueDuration = UpgradeSystem.ApplyDurationUpgrade(plagueDuration) * UpgradeSystem.GetPlagueDurationMultiplier();
        
        // Get ability-specific upgrades
        float poisonChance = UpgradeSystem.GetPlaguePoisonChancePercent();
        int poisonStacks = UpgradeSystem.GetPlaguePoisonStacks();
        float healChance = UpgradeSystem.GetPlagueHealChancePercent();
        int healAmount = UpgradeSystem.GetPlagueHealAmount();

        // Instantiate visual effect if prefab is available
        Entity effectEntity = Entity.Invalid;
        if (plagueEffectPrefab != null)
        {
            effectEntity = Scene.Instantiate(plagueEffectPrefab);
            if (effectEntity)
            {
                // Attach effect as child to the ability owner (player)
                effectEntity.transform.SetParent(owner, true);
                effectEntity.transform.localPosition = spawnOffset;
                effectEntity.transform.localScale *= upgradedAuraRadius;
            }
        }

        // Create a new active plague instance
        ActivePlague plague = new ActivePlague
        {
            centerPosition = owner.transform.position,
            remainingDuration = upgradedPlagueDuration,
            tickTimer = 0.0f, // Apply first tick immediately
            tickInterval = upgradedTickInterval,
            auraRadius = upgradedAuraRadius,
            plagueDamage = upgradedDamage,
            poisonChancePercent = poisonChance,
            poisonStacks = poisonStacks,
            healChancePercent = healChance,
            healAmount = healAmount,
            effectEntity = effectEntity
        };
        
        // Add to active plagues list
        activePlagues.Add(plague);
        return true;
    }

    /// <summary>
    /// Apply plague damage to all enemies in range.
    /// </summary>
    /// <param name="plague">The active plague to apply damage for.</param>
    private void ApplyPlagueDamage(ActivePlague plague)
    {
        // Find all enemies in range using sphere overlap
        var overlaps = Physics.SphereOverlap(plague.centerPosition, plague.auraRadius, LayerMask.GetMask("Enemy"));

        if (overlaps == null || overlaps.Length == 0)
            return;

        // Process each enemy in range
        foreach (var enemy in overlaps)
        {
            if (!enemy)
                continue;

            // Skip self
            if (enemy == owner)
                continue;

            // Calculate distance
            float distance = Vector3.Distance(plague.centerPosition, enemy.transform.position);
            if (distance > plague.auraRadius)
                continue;

            // Apply damage
            DamageBreakdown breakdown = UpgradeSystem.CalculateDamage(plague.plagueDamage);
            breakdown.color = Color.white;
            DamageSystem.ApplyDamage(enemy, owner, breakdown);

            // Check for poison application
            if (plague.poisonChancePercent > 0.0f && Random.Range(0.0f, 100.0f) < plague.poisonChancePercent)
            {
                EffectsSystem.AddOrRefreshEffect<PoisonComponent>(
                    enemy,
                    owner,
                    8.0f, // poison damage per second
                    6.0f, // poison duration
                    plague.poisonStacks,
                    5 // max poison stacks
                );
            }

            // Check for heal on player
            if (plague.healChancePercent > 0.0f && Random.Range(0.0f, 100.0f) < plague.healChancePercent)
            {
                var player = owner.GetComponent<Player>();
                if (player != null)
                {
                    player.HealPlayer(plague.healAmount);
                }
            }
        }
    }

    /// <summary>
    /// Get display information for the Plague ability (static version).
    /// </summary>
    /// <returns>Display information for UI.</returns>
    public static new UpgradeDisplayInfo GetDisplayInfo()
    {
        UpgradeDisplayInfo info = new UpgradeDisplayInfo();
        info.iconType = "plague";
        info.name = "Plague";
        info.icon = "P";
        info.color = "rgba(100, 50, 150, 180)"; // Purple/dark purple
        info.description = GetDescription();
        return info;
    }
    
    public static string GetDescription()
    {
        return "Creates a toxic aura around you that deals damage over time to all enemies in range.";
    }
}

