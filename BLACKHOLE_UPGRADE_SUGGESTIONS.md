# Black Hole Ability Upgrade Suggestions

This document contains suggestions for Black Hole-specific upgrades that modify or enhance the black hole's behavior.

---

## 🕳️ Black Hole Ability Upgrades

### Stat Modifiers

1. **Multiple Black Holes** (Common/Epic)
   - Spawns 2-3 black holes per cast
   - Description: "Creates multiple black holes at once"
   - Implementation: Modify spawn count in OnTriggerAbility

2. **Stronger Pull** (Common/Epic)
   - Increases pull strength by 50-100%
   - Description: "Black holes pull enemies with greater force"
   - Implementation: Modify pullStrength in ConfigureBlackHole

3. **Larger Event Horizon** (Common/Epic)
   - Increases pull radius by 40-80%
   - Description: "Black holes affect a larger area"
   - Implementation: Modify pullRadius in ConfigureBlackHole

4. **Longer Duration** (Common/Epic)
   - Increases black hole duration by 50-100%
   - Description: "Black holes persist longer"
   - Implementation: Modify duration in ConfigureBlackHole

5. **Faster Spawn** (Common/Epic)
   - Reduces cooldown by 30-50%
   - Description: "Black holes can be created more frequently"
   - Implementation: Modify cooldown

---

### Behavioral Upgrades

6. **Explosive Collapse** (Epic/Legendary)
   - When black hole expires, it explodes dealing area damage
   - Description: "Black holes explode when they collapse, dealing massive damage"
   - Implementation: Add AreaDamageComponent when PullComponent duration expires
   - Damage scales with number of enemies pulled in

7. **Event Horizon Damage** (Epic)
   - Enemies at the center of the black hole take damage over time
   - Description: "Enemies near the black hole center take continuous damage"
   - Implementation: Add damage over time to PullComponent (or create new component)
   - Damage increases the closer enemies are to center

8. **Chain Black Holes** (Epic/Legendary)
   - When a black hole expires, it spawns 2-3 smaller black holes
   - Description: "Collapsing black holes spawn smaller black holes"
   - Implementation: On black hole expiration, spawn smaller black holes with reduced radius/duration
   - Cascading effect

9. **Gravitational Well** (Epic)
   - Black holes also pull experience orbs toward them
   - Description: "Black holes attract experience orbs, making them easier to collect"
   - Implementation: Modify PullComponent to also affect "Experience" layer
   - Utility upgrade

10. **Black Hole Link** (Epic/Legendary)
    - Multiple black holes are connected by damaging energy beams
    - Description: "Black holes are linked by energy beams that damage enemies"
    - Implementation: Create component that draws beams between black holes
    - Enemies crossing beams take damage

11. **Spinning Vortex** (Epic)
    - Black hole rotates, creating a spiral pull pattern
    - Description: "Black holes spin, creating a spiral pull effect"
    - Implementation: Add rotation to black hole entity, modify pull direction based on rotation
    - Visual and gameplay enhancement

12. **Gravitational Slingshot** (Epic)
    - Enemies pulled to center are launched outward when black hole expires
    - Description: "Enemies are launched away when black holes collapse"
    - Implementation: Apply knockback force on expiration
    - Crowd control effect

---

### Advanced Upgrades

13. **Singularity** (Legendary)
    - Black hole grows stronger over time (pull strength increases)
    - Description: "Black holes grow stronger the longer they exist"
    - Implementation: Modify PullComponent to increase strength over time
    - Scaling effect

14. **Twin Black Holes** (Epic/Legendary)
    - Always spawns black holes in pairs that orbit each other
    - Description: "Black holes spawn in pairs that orbit around each other"
    - Implementation: Spawn two black holes that orbit a center point
    - Unique visual and gameplay

15. **Black Hole Armor** (Epic)
    - Player gains temporary armor when black holes are active
    - Description: "Active black holes grant you temporary armor"
    - Implementation: Track active black holes, grant armor based on count
    - Defensive utility

16. **Void Rift** (Legendary)
    - Black holes create portals that teleport enemies
    - Description: "Black holes create rifts that teleport enemies to random locations"
    - Implementation: Add teleportation effect when enemies reach center
    - Crowd control and chaos

17. **Accretion Disk** (Epic)
    - Enemies pulled in create a visible ring that damages other enemies
    - Description: "Enemies pulled in form a damaging ring around the black hole"
    - Implementation: Create visual ring effect that damages enemies
    - Area denial

18. **Hawking Radiation** (Epic/Legendary)
    - Black holes emit periodic damage pulses
    - Description: "Black holes emit periodic radiation that damages nearby enemies"
   - Implementation: Add periodic area damage component
   - Passive damage

---

## Priority Suggestions

### High Priority (Most Impactful)
1. **Explosive Collapse** - Adds satisfying payoff when black hole expires
2. **Multiple Black Holes** - Dramatically changes gameplay
3. **Event Horizon Damage** - Makes black holes more offensive
4. **Chain Black Holes** - Creates interesting cascading effects

### Medium Priority (Nice to Have)
1. **Stronger Pull** - Simple stat boost
2. **Larger Event Horizon** - More area coverage
3. **Gravitational Well** - Quality of life improvement
4. **Longer Duration** - More uptime

### Low Priority (Polish/Advanced)
1. **Black Hole Link** - Complex but visually interesting
2. **Spinning Vortex** - Visual enhancement
3. **Singularity** - Scaling mechanic
4. **Twin Black Holes** - Unique mechanic

---

## Implementation Notes

### Approach 1: Direct Stat Modification
- Modify ability properties directly when upgrade is applied
- Example: `ability.pullStrength *= 1.5f` for Stronger Pull
- Store upgrade state in the ability component

### Approach 2: Component-Based Upgrades
- Add new components to black hole entities when upgrades are active
- Example: Add `AreaDamageComponent` for Explosive Collapse
- Components handle the new behavior independently

### Approach 3: Upgrade System Integration
- Create black hole-specific upgrade classes that modify ability stats
- Store upgrades in UpgradeSystem and query them in ability code
- Example: `BlackHoleUpgrade` class with `PullStrengthMultiplier` property

### Approach 4: Hybrid Approach (Recommended)
- Use upgrade classes for stat modifications
- Use components for new behaviors (explosions, damage over time, etc.)
- Check for upgrades in ability's `OnTriggerAbility` method

---

## Suggested Upgrade Class Structure

```csharp
// Example: Black hole-specific upgrade base class
public abstract class BlackHoleSpecificUpgrade : Upgrade
{
    protected BlackHoleSpecificUpgrade(string name, string description)
        : base(name, description)
    {
    }
}

// Example: Multiple black holes upgrade
public class MultipleBlackHolesUpgrade : BlackHoleSpecificUpgrade
{
    public int AdditionalBlackHoleCount { get; set; }
    
    public MultipleBlackHolesUpgrade(int count = 1)
        : base("Multiple Black Holes", $"Spawns {count + 1} black holes instead of 1")
    {
        AdditionalBlackHoleCount = count;
    }
}

// Example: Explosive collapse upgrade
public class ExplosiveCollapseUpgrade : BlackHoleSpecificUpgrade
{
    public int ExplosionDamage { get; set; }
    public float ExplosionRadius { get; set; }
    
    public ExplosiveCollapseUpgrade(int damage = 50, float radius = 5.0f)
        : base("Explosive Collapse", "Black holes explode when they collapse, dealing massive damage")
    {
        ExplosionDamage = damage;
        ExplosionRadius = radius;
    }
}
```

---

## Notes

- Black holes are unique in that they're persistent area effects, not projectiles
- Upgrades should enhance both the pull mechanic and add new behaviors
- Consider visual feedback for upgrades (larger black holes, different colors, etc.)
- Balance around the fact that black holes are crowd control, not direct damage

