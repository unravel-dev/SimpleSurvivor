# Ability-Specific Upgrade Suggestions

This document contains suggestions for ability-specific upgrades that modify or enhance each ability's behavior. These upgrades can modify stats directly or add new behaviors through attached components/abilities.

---

## 🔥 Fireball Ability Upgrades

### Stat Modifiers
1. **Explosive Core** (Common/Epic)
   - Increases explosion radius by 50-100%
   - Description: "Fireball explodes with greater force, affecting a larger area"

2. **Rapid Fire** (Common/Epic)
   - Reduces cooldown by 30-50%
   - Description: "Fireball charges faster, allowing more frequent casts"

3. **Homing Fireball** (Epic)
   - Fireball tracks enemies within a cone
   - Description: "Fireball seeks out nearby enemies automatically"

4. **Split Fireball** (Epic/Legendary)
   - On impact, splits into 2-3 smaller fireballs
   - Description: "Fireball splits into multiple projectiles on impact"

5. **Fire Trail** (Epic)
   - Leaves burning ground that damages enemies over time
   - Description: "Fireball leaves a trail of fire that damages enemies"

6. **Double Explosion** (Legendary)
   - Explodes twice: on impact and after a short delay
   - Description: "Fireball creates a secondary explosion after impact"

---

## ⚡ Lightning Bolt Ability Upgrades

### Stat Modifiers
1. **Forked Lightning** (Common/Epic)
   - Lightning splits into 2-3 bolts on hit
   - Description: "Lightning forks into multiple bolts on impact"

2. **Chain Mastery** (Common/Epic)
   - Increases base chain count by 1-3
   - Description: "Lightning chains to more enemies"

3. **Overcharge** (Epic)
   - Damage increases with each chain (10-20% per chain)
   - Description: "Lightning grows stronger with each chain"

4. **Stun Effect** (Epic)
   - Chains stun enemies for 0.5-1.0 seconds
   - Description: "Lightning stuns enemies it chains to"

5. **Lightning Rod** (Epic/Legendary)
   - Lightning seeks out nearby enemies automatically
   - Description: "Lightning automatically targets nearby enemies"

6. **Storm Surge** (Legendary)
   - Each chain has a chance to spawn additional lightning bolts
   - Description: "Chains have a chance to create additional lightning strikes"

---

## 🌀 Boomerang Blade Ability Upgrades

### Stat Modifiers
1. **Multiple Blades** (Common/Epic)
   - Spawns 2-3 blades instead of 1
   - Description: "Throws multiple blades that orbit simultaneously"

2. **Faster Rotation** (Common/Epic)
   - Increases rotation speed by 50-100%
   - Description: "Blades orbit faster, hitting enemies more frequently"

3. **Expanding Orbit** (Epic)
   - Orbit radius gradually increases over duration
   - Description: "Blades orbit expands outward over time"

4. **Dual Orbit** (Epic)
   - Blades orbit in opposite directions
   - Description: "Blades orbit in counter-rotating directions"

5. **Blade Trail** (Epic)
   - Blades leave a damage trail behind them
   - Description: "Blades leave a trail of damage as they orbit"

6. **Returning Blade** (Epic/Legendary)
   - Blade returns to player after orbit duration, dealing damage on return
   - Description: "Blade returns to you after orbiting, damaging enemies on the way back"

7. **Blade Storm** (Legendary)
   - Spawns 4-6 blades that orbit at different heights
   - Description: "Creates a storm of blades orbiting at multiple levels"

8. **Spinning Slash** (Epic)
   - Increases visual spin speed and damage per hit
   - Description: "Blades spin faster, dealing more damage per rotation"

---

## ☄️ Meteor Shower Ability Upgrades

### Stat Modifiers
1. **Meteor Rain** (Common/Epic)
   - Spawns 2-4 meteors per cast
   - Description: "Calls down multiple meteors at once"

2. **Faster Impact** (Common/Epic)
   - Reduces spawn delay by 30-50%
   - Description: "Meteors fall faster, reducing warning time"

3. **Chain Meteors** (Epic)
   - Meteors spawn smaller meteors on impact
   - Description: "Meteor impacts spawn additional smaller meteors"

4. **Meteor Trail** (Epic)
   - Meteors leave a fire trail that damages enemies
   - Description: "Meteors leave a trail of fire as they fall"

5. **Staggered Impact** (Epic)
   - Meteors fall in sequence with slight delays
   - Description: "Meteors fall in a staggered pattern"

6. **Explosive Meteors** (Epic/Legendary)
   - Increases explosion radius by 50-100%
   - Description: "Meteors create larger explosions"

7. **Meteor Shower** (Legendary)
   - Spawns 5-8 meteors in a wide area
   - Description: "Calls down a devastating meteor shower"

8. **Impact Tremor** (Epic)
   - Meteors create a shockwave that knocks back enemies
   - Description: "Meteor impacts create powerful shockwaves"

---

## Implementation Notes

### Approach 1: Direct Stat Modification
- Modify ability properties directly when the upgrade is applied
- Example: `ability.rotationSpeed *= 1.5f` for Faster Rotation
- Store upgrade state in the ability component

### Approach 2: Component-Based Upgrades
- Add new components to ability entities when upgrades are active
- Example: Add `BladeTrailComponent` to blade entities
- Components handle the new behavior independently

### Approach 3: Upgrade System Integration
- Create ability-specific upgrade classes that modify ability stats
- Store upgrades in UpgradeSystem and query them in ability code
- Example: `BoomerangBladeUpgrade` class with `MultipleBladesCount` property

### Approach 4: Hybrid Approach (Recommended)
- Use upgrade classes for stat modifications
- Use components for new behaviors (trails, effects, etc.)
- Check for upgrades in ability's `OnTriggerAbility` method

---

## Suggested Upgrade Class Structure

```csharp
// Example: Ability-specific upgrade base class
public abstract class AbilitySpecificUpgrade : Upgrade
{
    public Type TargetAbilityType { get; protected set; }
    
    protected AbilitySpecificUpgrade(string name, string description, Type abilityType)
        : base(name, description)
    {
        TargetAbilityType = abilityType;
    }
}

// Example: Boomerang-specific upgrade
public class MultipleBladesUpgrade : AbilitySpecificUpgrade
{
    public int AdditionalBladeCount { get; set; }
    
    public MultipleBladesUpgrade(int bladeCount = 1)
        : base("Multiple Blades", $"Spawns {bladeCount + 1} blades instead of 1", typeof(BoomerangBladeAbility))
    {
        AdditionalBladeCount = bladeCount;
    }
}
```

---

## Priority Suggestions

### High Priority (Most Impactful)
1. **Multiple Blades** (Boomerang) - Dramatically changes gameplay
2. **Chain Meteors** (Meteor) - Creates interesting cascading effects
3. **Forked Lightning** (Lightning) - Visual and gameplay impact
4. **Split Fireball** (Fireball) - Adds complexity to simple ability

### Medium Priority (Nice to Have)
1. **Faster Rotation** (Boomerang) - Simple stat boost
2. **Meteor Rain** (Meteor) - More meteors = more fun
3. **Homing Fireball** (Fireball) - Quality of life improvement
4. **Stun Effect** (Lightning) - Adds utility

### Low Priority (Polish)
1. **Blade Trail** (Boomerang) - Visual effect
2. **Fire Trail** (Fireball) - Area denial
3. **Meteor Trail** (Meteor) - Visual effect

