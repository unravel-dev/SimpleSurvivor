# Effects System Usage Guide

The Effects System handles damage over time (DoT) effects in a centralized manner. Effects are applied as components to entities and processed by the EffectsSystem every frame.

---

## Setup

1. **Add EffectsSystemUpdater to your level**
   - Add the `EffectsSystemUpdater` component to a persistent game object in your scene (e.g., Level, GameManager)
   - This component calls `EffectsSystem.Tick()` every frame

---

## Available DoT Effects

### 1. **Doom** (`DoomComponent`)
- **Description:** High damage curse/death mark
- **Stacking:** Cannot stack (max 1)
- **Duration:** Short
- **Use case:** Powerful single-target debuff

### 2. **Burn** (`BurnComponent`)
- **Description:** Fire/heat damage
- **Stacking:** Stacks increase damage (max 3 by default)
- **Duration:** Medium
- **Use case:** Fire-based abilities, continuous damage

### 3. **Poison** (`PoisonComponent`)
- **Description:** Toxin/venom damage
- **Stacking:** Stacks increase duration (max 5 by default)
- **Duration:** Long
- **Use case:** Poison-based abilities, area denial

### 4. **Bleed** (`BleedComponent`)
- **Description:** Physical bleeding damage
- **Stacking:** Stacks increase damage with 1.1x multiplier (max 10 by default)
- **Duration:** Medium
- **Use case:** Physical attacks, sustained damage

---

## Usage Examples

### Example 1: Apply Burn on Hit

```csharp
public class FireballAbility : Ability
{
    protected override void OnTriggerAbility(Entity[] targets, int castIndex)
    {
        Entity target = targets[castIndex];
        
        // Apply burn effect: 10 damage per second for 5 seconds
        EffectsSystem.AddOrRefreshEffect<BurnComponent>(
            target, 
            owner,           // source
            10.0f,          // damage per second
            5.0f,           // duration
            3               // max stacks
        );
    }
}
```

### Example 2: Check if Entity has Effect

```csharp
// Check if enemy is poisoned
if (EffectsSystem.HasEffect<PoisonComponent>(enemy))
{
    // Enemy is poisoned, apply additional effect
    Log.Info("Enemy is poisoned!");
}
```

### Example 3: Get All Effects on Entity

```csharp
// Get all burn effects
var burnEffects = EffectsSystem.GetEffects<BurnComponent>(enemy);

foreach (var burn in burnEffects)
{
    Log.Info($"Burn duration remaining: {burn.GetRemainingDuration()}");
    Log.Info($"Burn stacks: {burn.currentStacks}");
}
```

### Example 4: Apply DoT in Contact System

```csharp
// In ContactSystem or similar
private static void HandlePoisonDamage(Entity source, Entity target)
{
    // Check if source has a PoisonComponent
    var poisonSource = source.GetComponent<AppliesPoisonComponent>();
    if (poisonSource == null)
        return;
        
    // Apply poison to target
    EffectsSystem.AddOrRefreshEffect<PoisonComponent>(
        target,
        source,
        poisonSource.damagePerSecond,
        poisonSource.duration,
        poisonSource.maxStacks
    );
}
```

### Example 5: Custom DoT Effect

```csharp
[ScriptSourceFile]
public class FrostbiteComponent : DamageOverTimeComponent
{
    public override void Initialize(Entity source, float dps, float effectDuration, int maxStackCount = 3)
    {
        base.Initialize(source, dps, effectDuration, maxStackCount);
        stacksDamage = true;
        stacksDuration = false;
    }
    
    public override string GetEffectName()
    {
        return "Frostbite";
    }
    
    public override string GetEffectColor()
    {
        return "rgba(135, 206, 235, 255)"; // Light blue
    }
    
    public override void OnDamageApplied(int damageAmount)
    {
        base.OnDamageApplied(damageAmount);
        
        // Slow the enemy when frostbite damages them
        var enemy = owner.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Apply slow effect (would need to implement this)
            // enemy.ApplySlow(0.5f); // 50% slow
        }
    }
}
```

---

## System Architecture

### EffectsSystem (Static)
- Central system that processes all DoT effects
- Gathers all entities with `DamageOverTimeComponent`
- Applies damage on a fixed interval (0.5 seconds)
- Removes expired effects automatically

### DamageOverTimeComponent (Base)
- Base component for all DoT effects
- Properties:
  - `damagePerSecond` - Base damage per second
  - `duration` - Total duration of effect
  - `currentStacks` - Current number of stacks
  - `maxStacks` - Maximum stacks allowed
  - `stacksDamage` - Whether stacks increase damage
  - `stacksDuration` - Whether stacks increase duration

### Specific DoT Components
- `DoomComponent` - High damage, no stacking
- `BurnComponent` - Medium damage, stacks increase damage
- `PoisonComponent` - Low damage, stacks increase duration
- `BleedComponent` - Medium damage, stacks increase damage exponentially

---

## Integration with Black Hole

Here's an example of how to add DoT effects to the Black Hole ability:

```csharp
// In BlackHoleAbility.cs
private void ConfigureBlackHole(Entity blackHoleEntity, float radius, float strength, float lifeDuration, Entity source)
{
    // ... existing code ...
    
    // Add a component that applies DoT to pulled enemies
    var dotApplier = blackHoleEntity.AddComponent<BlackHoleDoTComponent>();
    if (dotApplier != null)
    {
        dotApplier.damagePerSecond = 5.0f;
        dotApplier.effectType = BlackHoleDoTComponent.EffectType.Doom;
    }
}
```

```csharp
// New component: BlackHoleDoTComponent.cs
[ScriptSourceFile]
public class BlackHoleDoTComponent : ScriptComponent
{
    public enum EffectType { Doom, Burn, Poison, Bleed }
    
    public float damagePerSecond = 5.0f;
    public float duration = 3.0f;
    public EffectType effectType = EffectType.Doom;
    
    private float applyInterval = 1.0f; // Apply effect every second
    private float timeSinceLastApply = 0.0f;
    
    public override void OnUpdate()
    {
        timeSinceLastApply += Time.deltaTime;
        
        if (timeSinceLastApply >= applyInterval)
        {
            timeSinceLastApply = 0.0f;
            ApplyEffectToNearbyEnemies();
        }
    }
    
    private void ApplyEffectToNearbyEnemies()
    {
        var pullComponent = owner.GetComponent<PullComponent>();
        if (pullComponent == null)
            return;
            
        var nearbyEnemies = Physics.SphereOverlap(
            owner.transform.position, 
            pullComponent.pullRadius, 
            LayerMask.GetMask("Enemy")
        );
        
        foreach (var enemy in nearbyEnemies)
        {
            if (!enemy)
                continue;
                
            // Apply the appropriate DoT effect
            switch (effectType)
            {
                case EffectType.Doom:
                    EffectsSystem.AddOrRefreshEffect<DoomComponent>(enemy, owner, damagePerSecond, duration);
                    break;
                case EffectType.Burn:
                    EffectsSystem.AddOrRefreshEffect<BurnComponent>(enemy, owner, damagePerSecond, duration, 3);
                    break;
                case EffectType.Poison:
                    EffectsSystem.AddOrRefreshEffect<PoisonComponent>(enemy, owner, damagePerSecond, duration, 5);
                    break;
                case EffectType.Bleed:
                    EffectsSystem.AddOrRefreshEffect<BleedComponent>(enemy, owner, damagePerSecond, duration, 10);
                    break;
            }
        }
    }
}
```

---

## Performance Considerations

- **Damage Tick Interval:** Damage is applied every 0.5 seconds to reduce performance impact
- **Static Collections:** The system uses static collections to avoid allocations
- **Automatic Cleanup:** Expired effects are automatically removed
- **Entity Queries:** The system queries all entities once per tick, which is efficient for reasonable entity counts

---

## Notes

- DoT effects use the DamageSystem for applying damage, so all damage events are triggered properly
- Effects can be stacked multiple times based on their `maxStacks` property
- Different effect types have different stacking behaviors (damage vs duration)
- The EffectsSystemUpdater must be added to your scene for effects to work

