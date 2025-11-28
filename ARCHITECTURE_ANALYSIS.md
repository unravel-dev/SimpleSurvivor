# Codebase Architecture Analysis & Improvement Recommendations

## Executive Summary

Your codebase follows a **hybrid component-system architecture** with both strengths and areas for improvement. The current approach mixes:
- **Data Components** (pure data, processed by systems) ✅ Good
- **Behavior Components** (have their own OnUpdate logic) ⚠️ Mixed
- **Entity Components** (complex game logic like Player, Enemy) ⚠️ Needs refactoring
- **Static Systems** (process components, handle cross-cutting concerns) ✅ Good

## Current Architecture Overview

### ✅ **What's Working Well**

#### 1. **ContactSystem Pattern** (Excellent)
- **Location**: `data/Scripts/Systems/ContactSystem.cs`
- **Pattern**: Pure data components (`ChainComponent`, `PierceComponent`, `SplitComponent`) are processed by a centralized system
- **Benefits**:
  - Clear separation: Components = data, System = logic
  - Easy to add new contact effects (just add handler to array)
  - Dependencies between components are explicit in the system
  - No component-to-component coupling

#### 2. **EffectsSystem Pattern** (Good, but has coupling issues)
- **Location**: `data/Scripts/Systems/EffectsSystem.cs`
- **Pattern**: Centralized system processes `EffectOverTime` components
- **Benefits**:
  - Single place to manage all time-based effects
  - Consistent update loop
  - Easy to query effects
- **Issues**:
  - **Tight coupling**: Directly calls `enemy.StopChasing()`/`ResumeChasing()` (lines 50-54, 68-72)
  - Should use events or a more generic approach

#### 3. **DamageSystem Pattern** (Excellent)
- **Location**: `data/Scripts/Systems/DamageSystem.cs`
- **Pattern**: Centralized damage application with events
- **Benefits**:
  - Single point of control
  - Event-driven architecture allows loose coupling
  - Easy to track damage statistics

#### 4. **Component Data Separation** (Good)
- Components like `ChainComponent`, `PierceComponent`, `PhysicalDamageComponent` are pure data
- No logic in these components, making them easy to reason about

---

## ⚠️ **Areas Needing Improvement**

### 1. **Component Update Logic Inconsistency**

#### Problem
Some components have their own `OnUpdate()` logic, creating inconsistent patterns:

**Components with OnUpdate():**
- `OrbitalMovementComponent` - Updates position/rotation every frame
- `ReturningBladeComponent` - Checks lifetime, moves toward target
- `Projectile` - Tracks lifetime, destroys self
- `Player` - Handles input, movement, upgrades
- `Enemy` - Handles AI, movement, contact damage
- `Experience` - Detects nearby orbs, manages attraction
- `ExperienceOrb` - Handles attraction, floating animation, lifetime

**Components without OnUpdate():**
- `ChainComponent`, `PierceComponent`, `SplitComponent` - Pure data, processed by ContactSystem ✅

#### Recommendation: **Create Movement System**

**Current Pattern (Bad):**
```csharp
// OrbitalMovementComponent.cs
public override void OnUpdate() {
    // Updates position directly
    transformComponent.position = CalculatePosition();
}
```

**Recommended Pattern (Good):**
```csharp
// MovementSystem.cs (new)
public static class MovementSystem {
    public static void Tick(float deltaTime) {
        var orbitals = Scene.FindEntitiesWithComponent<OrbitalMovementComponent>();
        foreach (var entity in orbitals) {
            var orbital = entity.GetComponent<OrbitalMovementComponent>();
            var transform = entity.GetComponent<TransformComponent>();
            // Update position
            transform.position = CalculateOrbitalPosition(orbital, transform);
        }
    }
}

// OrbitalMovementComponent.cs (data only)
public class OrbitalMovementComponent : ScriptComponent {
    public Entity centerEntity;
    public float orbitRadius;
    public float rotationSpeed;
    // No OnUpdate() - just data
}
```

**Benefits:**
- All movement logic in one place
- Easier to optimize (batch updates, spatial partitioning)
- Clear dependencies
- Components become pure data

---

### 2. **Tight Coupling in EffectsSystem**

#### Problem
`EffectsSystem` directly manipulates `Enemy` behavior:

```csharp
// EffectsSystem.cs (lines 48-55, 66-73)
if (isStun) {
    var enemy = entity.GetComponent<Enemy>();
    if (enemy != null) {
        enemy.StopChasing(); // Direct coupling!
    }
}
```

#### Recommendation: **Use Events or Component-Based Approach**

**Option A: Event-Based (Recommended)**
```csharp
// EffectOverTime.cs
public abstract class EffectOverTime : ScriptComponent {
    public System.Action<Entity> OnEffectApplied;
    public System.Action<Entity> OnEffectExpired;
}

// StunComponent.cs
public override void OnStart() {
    OnEffectApplied?.Invoke(owner);
}

// Enemy.cs
public override void OnStart() {
    // Subscribe to stun events via a system or component
    EffectsSystem.OnStunApplied += HandleStunApplied;
    EffectsSystem.OnStunExpired += HandleStunExpired;
}
```

**Option B: Component-Based (More ECS-like)**
```csharp
// StunnedComponent.cs (new)
public class StunnedComponent : ScriptComponent {
    // Just a marker component
}

// MovementSystem.cs (or EnemySystem.cs)
public static void Tick(float deltaTime) {
    var enemies = Scene.FindEntitiesWithComponent<Enemy>();
    foreach (var enemyEntity in enemies) {
        var enemy = enemyEntity.GetComponent<Enemy>();
        bool isStunned = enemyEntity.HasComponent<StunnedComponent>();
        
        if (isStunned) {
            enemy.StopChasing();
        } else {
            enemy.ResumeChasing();
        }
    }
}

// EffectsSystem.cs
if (effect is StunComponent) {
    entity.AddComponent<StunnedComponent>();
}
```

**Benefits:**
- `EffectsSystem` doesn't need to know about `Enemy`
- `Enemy` doesn't need to know about `StunComponent`
- More flexible and extensible

---

### 3. **Component-to-Component Direct Manipulation**

#### Problem
`ReturningBladeComponent` directly removes other components:

```csharp
// ReturningBladeComponent.cs (line 103)
owner.RemoveComponent<OrbitalMovementComponent>(); // Direct manipulation!
```

#### Recommendation: **Use Systems or Events**

**Current (Bad):**
```csharp
private void StartReturning() {
    owner.RemoveComponent<OrbitalMovementComponent>();
    // ...
}
```

**Recommended (Good):**
```csharp
// ReturningBladeComponent.cs
public class ReturningBladeComponent : ScriptComponent {
    public bool shouldReturn = false; // Flag
}

// MovementSystem.cs
public static void Tick(float deltaTime) {
    var returning = Scene.FindEntitiesWithComponent<ReturningBladeComponent>();
    foreach (var entity in returning) {
        var returningComp = entity.GetComponent<ReturningBladeComponent>();
        if (returningComp.shouldReturn) {
            // Handle return movement
            // Remove orbital if needed
            entity.RemoveComponent<OrbitalMovementComponent>();
        }
    }
}
```

**Benefits:**
- Components don't manipulate each other
- System controls component lifecycle
- Clear ownership and dependencies

---

### 4. **Mixed Responsibilities in Entity Components**

#### Problem
`Player` and `Enemy` have too many responsibilities:
- Input handling
- Movement logic
- Upgrade management
- Animation control
- Health/Experience management

#### Recommendation: **Split into Systems**

**Current Structure:**
```
Player.cs
├── HandleInput()
├── HandleMovement()
├── UpdatePickupRange()
├── ApplyMagnetEffect()
└── OnUpgradeSelected()
```

**Recommended Structure:**
```
Player.cs (data only)
├── baseMaxSpeed
├── baseMaxHealth
└── basePickupRange

InputSystem.cs (new)
└── Tick() - processes all input

MovementSystem.cs (new)
└── Tick() - processes Player, Enemy movement

PickupSystem.cs (new)
└── Tick() - processes Experience, ExperienceOrb attraction
```

**Benefits:**
- Single Responsibility Principle
- Easier to test
- Better performance (batch processing)
- Clear dependencies

---

### 5. **Lifetime Management Scattered**

#### Problem
Lifetime is managed in multiple places:
- `Projectile.OnUpdate()` - checks lifetime, destroys self
- `ExperienceOrb.OnUpdate()` - checks lifetime, destroys self
- `AutoDestroyComponent` - checked by ContactSystem

#### Recommendation: **Unified Lifetime System**

```csharp
// LifetimeComponent.cs (new)
public class LifetimeComponent : ScriptComponent {
    public float lifetime;
    private float timeAlive = 0.0f;
}

// LifetimeSystem.cs (new)
public static class LifetimeSystem {
    public static void Tick(float deltaTime) {
        var entities = Scene.FindEntitiesWithComponent<LifetimeComponent>();
        foreach (var entity in entities) {
            var lifetime = entity.GetComponent<LifetimeComponent>();
            lifetime.timeAlive += deltaTime;
            
            if (lifetime.timeAlive >= lifetime.lifetime) {
                Scene.DestroyEntity(entity);
            }
        }
    }
}
```

**Benefits:**
- Single place for lifetime logic
- Consistent behavior
- Easy to add lifetime modifiers

---

## 📋 **Recommended Refactoring Priority**

### **Phase 1: High Priority (Immediate Impact)**

1. **Decouple EffectsSystem from Enemy**
   - Use events or `StunnedComponent` marker
   - **Impact**: Reduces coupling, makes stun system reusable

2. **Create MovementSystem**
   - Move `OrbitalMovementComponent` logic to system
   - Move `ReturningBladeComponent` movement to system
   - **Impact**: Centralizes movement, easier to optimize

3. **Create LifetimeSystem**
   - Unify `Projectile`, `ExperienceOrb`, `AutoDestroyComponent` lifetime logic
   - **Impact**: Consistent lifetime management

### **Phase 2: Medium Priority (Architectural Improvements)**

4. **Split Player/Enemy Responsibilities**
   - Create `InputSystem` for player input
   - Create `AISystem` for enemy AI
   - Keep `Player`/`Enemy` as data containers
   - **Impact**: Better separation of concerns

5. **Create PickupSystem**
   - Move `Experience.DetectNearbyOrbs()` to system
   - Move `ExperienceOrb` attraction to system
   - **Impact**: Centralized pickup logic

### **Phase 3: Low Priority (Polish)**

6. **Component Cleanup**
   - Ensure all components are pure data where possible
   - Move remaining logic to systems
   - **Impact**: Cleaner architecture

---

## 🎯 **System Design Principles**

### **When to Use Systems:**

✅ **Use Systems For:**
- Cross-entity operations (movement, collisions, effects)
- Batch processing (performance)
- Complex interactions between multiple components
- Gameplay logic that affects multiple entities

### **When to Keep Logic in Components:**

✅ **Keep Logic in Components For:**
- Simple, entity-specific behavior (e.g., `Health.TakeDamage()`)
- Data validation/initialization
- Component-specific state management
- **BUT**: Prefer systems when logic needs to interact with other components

### **Component Guidelines:**

✅ **Good Components:**
- Pure data (`ChainComponent`, `PierceComponent`)
- Simple getters/setters (`Health.GetCurrentHealth()`)
- Self-contained behavior that doesn't affect other components

❌ **Bad Components:**
- Components that manipulate other components directly
- Components with complex update logic that could be batched
- Components that know about specific other component types

---

## 🔍 **Specific Code Examples**

### Example 1: Refactoring OrbitalMovementComponent

**Before:**
```csharp
public class OrbitalMovementComponent : ScriptComponent {
    public override void OnUpdate() {
        // 50+ lines of movement logic
        currentAngle += rotationSpeed * Time.deltaTime;
        transformComponent.position = CalculatePosition();
    }
}
```

**After:**
```csharp
// Component (data only)
public class OrbitalMovementComponent : ScriptComponent {
    public Entity centerEntity;
    public float orbitRadius;
    public float rotationSpeed;
    public float currentAngle;
    // No OnUpdate()
}

// System (logic)
public static class MovementSystem {
    public static void Tick(float deltaTime) {
        var orbitals = Scene.FindEntitiesWithComponent<OrbitalMovementComponent>();
        foreach (var entity in orbitals) {
            UpdateOrbitalMovement(entity, deltaTime);
        }
    }
}
```

### Example 2: Decoupling Stun from Enemy

**Before:**
```csharp
// EffectsSystem.cs
if (effect is StunComponent) {
    var enemy = entity.GetComponent<Enemy>();
    if (enemy != null) {
        enemy.StopChasing(); // Tight coupling
    }
}
```

**After:**
```csharp
// EffectsSystem.cs
if (effect is StunComponent) {
    entity.AddComponent<StunnedComponent>(); // Marker component
}

// EnemySystem.cs (or MovementSystem.cs)
var enemies = Scene.FindEntitiesWithComponent<Enemy>();
foreach (var enemyEntity in enemies) {
    var enemy = enemyEntity.GetComponent<Enemy>();
    bool isStunned = enemyEntity.HasComponent<StunnedComponent>();
    
    if (isStunned && enemy.isChasing) {
        enemy.StopChasing();
    } else if (!isStunned && !enemy.isChasing) {
        enemy.ResumeChasing();
    }
}
```

---

## 📊 **Current vs. Recommended Architecture**

### **Current Architecture:**
```
Components (mixed)
├── Data Components (ChainComponent, etc.) ✅
├── Behavior Components (OrbitalMovementComponent) ⚠️
└── Entity Components (Player, Enemy) ⚠️

Systems
├── ContactSystem ✅
├── EffectsSystem ⚠️ (coupling issues)
├── DamageSystem ✅
└── UpgradeSystem ✅
```

### **Recommended Architecture:**
```
Components (data only)
├── Data Components (ChainComponent, etc.) ✅
├── Movement Components (OrbitalMovementComponent) ✅
└── Entity Components (Player, Enemy) ✅ (data only)

Systems
├── ContactSystem ✅
├── EffectsSystem ✅ (decoupled)
├── DamageSystem ✅
├── UpgradeSystem ✅
├── MovementSystem ✅ (new)
├── LifetimeSystem ✅ (new)
├── InputSystem ✅ (new)
├── PickupSystem ✅ (new)
└── AISystem ✅ (new)
```

---

## ✅ **What's Already Good (Don't Change)**

1. **ContactSystem** - Perfect example of system-based architecture
2. **DamageSystem** - Excellent event-driven design
3. **Data Components** - `ChainComponent`, `PierceComponent`, etc. are well-designed
4. **UpgradeSystem** - Good centralized upgrade management
5. **Component-System separation** - The foundation is solid

---

## 🚀 **Migration Strategy**

1. **Start Small**: Pick one component (e.g., `OrbitalMovementComponent`)
2. **Extract to System**: Create `MovementSystem`, move logic there
3. **Test**: Ensure behavior is identical
4. **Repeat**: Move to next component
5. **Refactor Systems**: Once components are data-only, refactor system interactions

**Key Principle**: Make changes incrementally, test after each change.

---

## 📝 **Summary**

Your codebase has a **solid foundation** with good system patterns. The main improvements needed are:

1. **Move update logic from components to systems** (especially movement, lifetime)
2. **Decouple systems from specific component types** (EffectsSystem → Enemy)
3. **Split large entity components** (Player, Enemy) into data + systems
4. **Unify scattered logic** (lifetime management, pickup logic)

The **ContactSystem** is your best example - aim for that pattern everywhere!

