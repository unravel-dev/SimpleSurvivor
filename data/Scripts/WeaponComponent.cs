using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// WeaponComponent that automatically targets and shoots at enemies within range.
/// Handles automatic firing, enemy detection, and projectile spawning.
/// </summary>
[ScriptSourceFile]
public class WeaponComponent : ScriptComponent
{
    //[Header("Weapon Settings")]
    [Tooltip("Rate of fire in shots per second")]
    public float fireRate = 2.0f;
    [Tooltip("Maximum shooting range (also used for enemy detection)")]
    public float range = 15.0f;
    [Tooltip("Damage dealt by projectiles")]
    public float damage = 25.0f;
    
    //[Header("Projectile Settings")]
    [Tooltip("Projectile prefab to instantiate when shooting")]
    public Prefab projectile;
    [Tooltip("Container entity for organizing projectiles in hierarchy")]
    public Entity projectileContainer;
    [Tooltip("Force applied to projectiles when fired")]
    public float projectileForce = 25.0f;
    [Tooltip("Projectile lifetime in seconds before auto-destruction")]
    public float projectileLifetime = 5.0f;
    [Tooltip("Spread angle for projectile accuracy (0 = perfect accuracy)")]
    public float spread = 0.1f;
    [Tooltip("Number of projectiles to fire per shot")]
    public int projectileCount = 1;
    
    //[Header("Targeting Settings")]
    [Tooltip("Layer mask for enemy detection (which layers contain enemies)")]
    public bool autoDetectEnemies = true;
    [Tooltip("Require line of sight to target (raycast check)")]
    public bool requireLineOfSight = false;
    
    //[Header("Debug Settings")]
    [Tooltip("Enable debug logging for weapon operations")]
    public bool debugWeapon = false;
    [Tooltip("Draw debug spheres for weapon range")]
    public bool debugDrawRanges = false;
    
    // Component references
    private TransformComponent transformComponent;
    
    // Weapon state
    private float timeSinceLastShot = 0.0f;
    private Entity currentTarget;
    
    /// <summary>
    /// Called when the script is created. Initialize component references.
    /// </summary>
    public override void OnCreate()
    {
        // Get required components
        transformComponent = owner.GetComponent<TransformComponent>();
        
        if (transformComponent == null)
        {
            Log.Error($"WeaponComponent on {owner.name}: TransformComponent not found!");
        }
        
        // Validate projectile
        if (projectile == null)
        {
            Log.Warning($"WeaponComponent on {owner.name}: No projectile prefab assigned!");
        }
        
        Log.Info($"WeaponComponent initialized on {owner.name}");
    }
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        // Validate components
        if (transformComponent == null)
        {
            Log.Error($"WeaponComponent on {owner.name}: Missing required components. Disabling script.");
            return;
        }
        
        // Auto-find projectile container if not assigned
        if (!projectileContainer)
        {
            // Try to find or create a projectile container
            var containerEntity = Scene.FindEntityByName("ProjectileContainer");
            if (containerEntity)
            {
                projectileContainer = containerEntity;
                if (debugWeapon)
                {
                    Log.Info($"WeaponComponent: Found existing ProjectileContainer");
                }
            }
            else
            {
                if (debugWeapon)
                {
                    Log.Info($"WeaponComponent: No ProjectileContainer found, projectiles will spawn without parent");
                }
            }
        }
        
        Log.Info($"WeaponComponent started on {owner.name}");
    }
    
    /// <summary>
    /// Called every frame to update weapon logic.
    /// </summary>
    public override void OnUpdate()
    {
        if (transformComponent == null || projectile == null)
            return;
            
        // Update firing timer
        timeSinceLastShot += Time.deltaTime;
        
        // Check if we can fire
        if (CanFire())
        {
            // Find target
            Entity target = FindClosestEnemy();
            
            if (target)
            {
                currentTarget = target;
                
                // Calculate shooting direction
                Vector3 shootDirection = CalculateShootDirection(target);
                
                // Fire weapon
                FireWeapon(shootDirection);
                
                // Reset fire timer
                timeSinceLastShot = 0.0f;
                
                if (debugWeapon)
                {
                    Log.Info($"WeaponComponent: Fired at {target.name} at distance {Vector3.Distance(transformComponent.position, target.transform.position):F2}");
                }
            }
        }
        
        // Debug drawing
        if (debugDrawRanges)
        {
            DrawDebugRanges();
        }
    }
    
    /// <summary>
    /// Check if the weapon can fire based on fire rate.
    /// </summary>
    /// <returns>True if weapon can fire.</returns>
    private bool CanFire()
    {
        float fireInterval = 1.0f / fireRate;
        return timeSinceLastShot >= fireInterval;
    }
    
    /// <summary>
    /// Find the closest enemy within range using SphereOverlap.
    /// </summary>
    /// <returns>The closest enemy entity, or Entity.Invalid if none found.</returns>
    private Entity FindClosestEnemy()
    {
        Vector3 weaponPosition = transformComponent.position;
        
        // Perform sphere overlap to find potential targets
        var overlaps = Physics.SphereOverlap(weaponPosition, range, LayerMask.GetMask("Enemy"));
        
        if (overlaps == null || overlaps.Length == 0)
        {
            return Entity.Invalid;
        }
        
        Entity closestEnemy = Entity.Invalid;
        float closestDistance = float.MaxValue;
        
        // Find the closest valid enemy
        foreach (var entity in overlaps)
        {            
            if (!entity) continue;
            
            // Skip self
            if (entity == owner) continue;
            
            // Check if it's an enemy (simple name-based check if auto-detect is enabled)
            if (autoDetectEnemies && !IsEnemyEntity(entity)) continue;
            
            // Calculate distance
            float distance = Vector3.Distance(weaponPosition, entity.transform.position);
            
            // Check if within range
            if (distance > range) continue;
            
            // Check line of sight if required
            if (requireLineOfSight && !HasLineOfSight(entity)) continue;
            
            // Check if this is the closest so far
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = entity;
            }
        }
        
        return closestEnemy;
    }
    
    /// <summary>
    /// Check if an entity is considered an enemy.
    /// </summary>
    /// <param name="entity">The entity to check.</param>
    /// <returns>True if the entity is an enemy.</returns>
    private bool IsEnemyEntity(Entity entity)
    {
        // Simple name-based detection
        if (entity.name.Contains("Enemy")) return true;
        
        // Check for Enemy component
        var enemyComponent = entity.GetComponent<Enemy>();
        if (enemyComponent != null) return true;
        
        return false;
    }
    
    /// <summary>
    /// Check if there's a clear line of sight to the target.
    /// </summary>
    /// <param name="target">The target entity.</param>
    /// <returns>True if line of sight is clear.</returns>
    private bool HasLineOfSight(Entity target)
    {
        Vector3 weaponPosition = transformComponent.position;
        Vector3 targetPosition = target.transform.position;
        Vector3 direction = (targetPosition - weaponPosition).normalized;
        float distance = Vector3.Distance(weaponPosition, targetPosition);
        
        // Raycast to check for obstacles
        var hit = Physics.Raycast(weaponPosition, direction, distance, -1, false);
        
        // If we hit something, check if it's the target
        if (hit.HasValue)
        {
            return hit.Value.entity == target;
        }
        
        // No obstacles found
        return true;
    }
    
    /// <summary>
    /// Calculate the shooting direction towards the target.
    /// </summary>
    /// <param name="target">The target entity.</param>
    /// <returns>The normalized shooting direction.</returns>
    private Vector3 CalculateShootDirection(Entity target)
    {
        Vector3 weaponPosition = transformComponent.position;
        Vector3 targetPosition = target.transform.position;
        
        // Calculate direction to target
        Vector3 direction = (targetPosition - weaponPosition).normalized;
        
        return direction;
    }
    
    /// <summary>
    /// Fire the weapon in the specified direction.
    /// </summary>
    /// <param name="shootDirection">The direction to shoot.</param>
    private void FireWeapon(Vector3 shootDirection)
    {
        Vector3 weaponPosition = transformComponent.position + Vector3.up * 2.0f;
        Shoot(weaponPosition, shootDirection, spread, projectileCount);
    }
    
    /// <summary>
    /// Shoot projectiles in the specified direction.
    /// </summary>
    /// <param name="source">The source position for projectiles.</param>
    /// <param name="shootDir">The shooting direction.</param>
    /// <param name="spread">The spread angle for accuracy.</param>
    /// <param name="count">The number of projectiles to spawn.</param>
    private void Shoot(Vector3 source, Vector3 shootDir, float spread, int count)
    {
        for (int i = 0; i < count; ++i)
        {
            var instance = Scene.Instantiate(projectile);
            
            // Set parent if container exists
            if (projectileContainer)
            {
                instance.transform.parent = projectileContainer;
            }
            
            // Set position with spread
            instance.transform.position = source + Random.insideUnitSphere * spread + shootDir * 0.5f;
            instance.transform.forward = shootDir;

            // Apply physics force
            var iphysics = instance.GetComponent<PhysicsComponent>();
            if (iphysics != null)
            {
                iphysics.excludeLayers = owner.layers;
                iphysics.ApplyForce(shootDir * projectileForce, ForceMode.Impulse);
            }
            
            // Auto-destroy after lifetime
            Scene.DestroyEntity(instance, projectileLifetime);
        }
    }
    
    /// <summary>
    /// Draw debug visualization for weapon ranges.
    /// </summary>
    private void DrawDebugRanges()
    {
        Vector3 weaponPosition = transformComponent.position;
        
        // This would typically use a debug drawing system
        // For now, we'll just log the ranges periodically
        if (Time.time % 2.0f < Time.deltaTime) // Every 2 seconds
        {
            if (debugWeapon)
            {
                Log.Info($"WeaponComponent Debug - Range: {range}, Position: {weaponPosition}");
            }
        }
    }
    
    /// <summary>
    /// Get the current target entity.
    /// </summary>
    /// <returns>The current target entity.</returns>
    public Entity GetCurrentTarget()
    {
        return currentTarget;
    }
    
    /// <summary>
    /// Get the time until the next shot can be fired.
    /// </summary>
    /// <returns>Time in seconds until next shot.</returns>
    public float GetTimeUntilNextShot()
    {
        float fireInterval = 1.0f / fireRate;
        return Mathf.Max(0.0f, fireInterval - timeSinceLastShot);
    }
    
    /// <summary>
    /// Check if the weapon is ready to fire.
    /// </summary>
    /// <returns>True if ready to fire.</returns>
    public bool IsReadyToFire()
    {
        return CanFire();
    }
    
    /// <summary>
    /// Set the fire rate of the weapon.
    /// </summary>
    /// <param name="newFireRate">New fire rate in shots per second.</param>
    public void SetFireRate(float newFireRate)
    {
        fireRate = Mathf.Max(0.1f, newFireRate);
    }
    
    /// <summary>
    /// Set the weapon range.
    /// </summary>
    /// <param name="newRange">New weapon range.</param>
    public void SetRange(float newRange)
    {
        range = Mathf.Max(0.1f, newRange);
    }
    
    /// <summary>
    /// Set the weapon damage.
    /// </summary>
    /// <param name="newDamage">New damage value.</param>
    public void SetDamage(float newDamage)
    {
        damage = Mathf.Max(0.0f, newDamage);
    }
}
