using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Simple enemy AI that moves towards the player's position.
/// Designed for top-down games with basic chase behavior.
/// </summary>
[ScriptSourceFile]
public class Enemy : ScriptComponent
{
    //[Header("Movement Settings")]
    [Tooltip("Maximum movement speed in units per second")]
    public float maxSpeed = 6.0f;
    [Tooltip("Maximum acceleration in units per second squared")]
    public float maxAcceleration = 20.0f;
    [Tooltip("Distance to start slowing down when approaching player")]
    public float slowRadius = 5.0f;
    [Tooltip("Distance considered as 'arrived' at player (stops moving)")]
    public float arriveRadius = 0.6f;
    [Tooltip("How quickly the enemy rotates to face the player")]
    public float rotationSpeed = 180.0f;
    [Tooltip("Use physics-based movement vs direct transform movement")]
    public bool usePhysicsMovement = false;
    
    //[Header("Player Detection")]
    [Tooltip("The player entity to chase (auto-found if not set)")]
    public Entity target;
    [Tooltip("Automatically find the player entity on start")]
    public bool autoFindPlayer = true;
    
    //[Header("Enemy Type")]
    [Tooltip("Enemy type identifier for loot configuration")]
    public EnemyType enemyType = EnemyType.Basic;
    
    //[Header("Contact Damage")]
    [Tooltip("Distance from player to start dealing contact damage")]
    public float contactDamageRadius = 1.5f;
    [Tooltip("Interval between contact damage ticks (seconds)")]
    public float contactDamageInterval = 0.5f;
    
    // Component references
    private PhysicsComponent physicsComponent;
    private Health Health;
    private EnemyStateMachine stateMachine;

    // Movement state
    private Vector3 lastPlayerPosition;
    
    // Contact damage state
    private float contactDamageTimer = 0.0f;
    
    /// <summary>
    /// Called when the script is created. Initialize component references.
    /// </summary>
    public override void OnCreate()
    {
        physicsComponent = owner.GetComponent<PhysicsComponent>();
        Health = owner.GetComponent<Health>();
        stateMachine = owner.GetComponent<EnemyStateMachine>();
        
        if (usePhysicsMovement && physicsComponent == null)
        {
            Log.Warning($"Enemy on {owner.name}: PhysicsComponent not found! Falling back to direct movement.");
            usePhysicsMovement = false;
        }
        
        if (Health == null)
        {
            Log.Warning($"Enemy on {owner.name}: Health not found! Enemy will not be able to take damage.");
        }
        else
        {
            // Subscribe to health events
            Health.OnDeath += OnEnemyDeath;
        }
        

    }
    
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        // Auto-find player if enabled
        if (autoFindPlayer && !target)
        {
            FindPlayer();
        }


        // Initialize last known player position and set initial state
        if (target)
        {
            lastPlayerPosition = target.transform.position;
            // Set state to Walking if we have a target
            if (stateMachine != null)
            {
                stateMachine.SetState(EnemyState.Walking);
            }
        }
        else
        {
            // No target, set to Idle
            if (stateMachine != null)
            {
                stateMachine.SetState(EnemyState.Idle);
            }
        }
    }
    
    /// <summary>
    /// Called every frame to handle enemy AI decisions.
    /// </summary>
    public override void OnUpdate()
    {
        if (!target || stateMachine == null)
            return;
        
        // Don't update if dead or dying
        if (stateMachine.IsDeadOrDying())
            return;
        
        // Check if stunned - update state accordingly
        bool isStunned = owner.HasComponent<StunnedComponent>();
        if (isStunned && !stateMachine.IsInState(EnemyState.Stunned))
        {
            stateMachine.SetState(EnemyState.Stunned);
        }
        else if (!isStunned && stateMachine.IsInState(EnemyState.Stunned) && target && (Health == null || !Health.IsDead()))
        {
            // Resume chasing when no longer stunned
            stateMachine.SetState(EnemyState.Walking);
        }
        
        // Update movement state based on whether we can move
        if (!isStunned && stateMachine.CanMove())
        {
            // Determine if we should be walking or idle based on movement
            // This will be handled in UpdateAI based on actual movement
        }
            
        // Update AI behavior (decision making only)
        UpdateAI();
        
        // Handle contact damage
        HandleContactDamage();
    }

    /// <summary>
    /// Called at fixed intervals for physics-based movement.
    /// </summary>
    public override void OnFixedUpdate()
    {
        if (!target || !usePhysicsMovement || physicsComponent == null || stateMachine == null)
            return;

        // Handle physics movement based on state
        if (stateMachine.IsInAnyState(EnemyState.Walking, EnemyState.Running))
        {
            UpdatePhysicsMovement();
        }
        else
        {
            // Stop horizontal movement when not in a movement state
            Vector3 currentVelocity = physicsComponent.velocity;
            physicsComponent.velocity = new Vector3(0, currentVelocity.y, 0);
        }
    }
    
    /// <summary>
    /// Update enemy AI decisions (called every frame).
    /// </summary>
    private void UpdateAI()
    {
        // Don't update AI if dead or dying
        if (stateMachine == null || !stateMachine.CanAct())
            return;
        
        // Only update movement if in a movement state
        if (!stateMachine.CanMove())
            return;
        
        Vector3 playerPosition = target.transform.position;
        Vector3 enemyPosition = transform.position;
        
        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(enemyPosition, playerPosition);
        
        // Calculate direction to player (only X and Z for top-down)
        Vector3 toTarget = playerPosition - enemyPosition;
        toTarget.y = 0; // Keep movement on horizontal plane
        
        // Update state based on movement
        if (toTarget.magnitude > 0.0001f)
        {
            // If we're moving, ensure we're in Walking state
            if (stateMachine.IsInState(EnemyState.Idle))
            {
                stateMachine.SetState(EnemyState.Walking);
            }
            
            // Handle non-physics movement here (direct transform)
            if (!usePhysicsMovement)
            {
                Vector3 directionToPlayer = toTarget.normalized;
                MoveTowardsPlayer(directionToPlayer, distanceToPlayer);
            }
            
            // Rotate to face movement direction (for physics) or player (for direct movement)
            if (usePhysicsMovement && physicsComponent != null)
            {
                // Rotate to face velocity direction for physics movement
                Vector3 velocity = physicsComponent.velocity;
                velocity.y = 0;
                if (velocity.sqrMagnitude > 0.01f)
                {
                    RotateTowardsDirection(velocity.normalized);
                }
            }
            else
            {
                // Rotate to face player for direct movement
                RotateTowardsDirection(toTarget.normalized);
            }
            
            lastPlayerPosition = playerPosition;
        }
        else
        {
            // Not moving, set to Idle if we're in a movement state
            if (stateMachine.IsInAnyState(EnemyState.Walking, EnemyState.Running))
            {
                stateMachine.SetState(EnemyState.Idle);
            }
        }
    }
    
    /// <summary>
    /// Update physics-based movement using steering forces (called at fixed intervals).
    /// </summary>
    private void UpdatePhysicsMovement()
    {
        Vector3 playerPosition = target.transform.position;
        Vector3 enemyPosition = transform.position;
        
        // Direction to target (flattened to XZ plane for top-down)
        Vector3 toTarget = playerPosition - enemyPosition;
        toTarget.y = 0f;
        
        float distance = toTarget.magnitude;
        if (distance < 0.0001f) return;
        
        Vector3 direction = toTarget / distance;
        
        // Target speed with "arrive" behavior (slow down near target)
        float targetSpeed = maxSpeed;
        if (distance < slowRadius)
        {
            targetSpeed = Mathf.Lerp(0f, maxSpeed, Mathf.InverseLerp(arriveRadius, slowRadius, distance));
        }
        if (distance <= arriveRadius)
        {
            targetSpeed = 0f;
        }
        
        Vector3 desiredVelocity = direction * targetSpeed;
        
        // Get current velocity (flattened to XZ plane)
        Vector3 currentVelocity = physicsComponent.velocity;
        Vector3 currentPlanarVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        
        // Steering = how much we want to change our current velocity
        Vector3 steering = desiredVelocity - currentPlanarVelocity;
        
        // Cap to what we can actually change this frame with our max acceleration
        float maxVelocityChange = maxAcceleration * Time.fixedDeltaTime;
        if (steering.sqrMagnitude > maxVelocityChange * maxVelocityChange)
        {
            steering = steering.normalized * maxVelocityChange;
        }
        
        // Apply as acceleration so it's mass-independent
        // Convert velocity change back to acceleration by dividing by deltaTime
        Vector3 accelerationForce = steering / Time.fixedDeltaTime;
        physicsComponent.ApplyForce(accelerationForce, ForceMode.Acceleration);
    }
    
    /// <summary>
    /// Move the enemy towards the player using direct transform movement.
    /// Includes arrive behavior for smooth stopping.
    /// </summary>
    /// <param name="direction">Normalized direction to move.</param>
    /// <param name="distance">Distance to the target.</param>
    private void MoveTowardsPlayer(Vector3 direction, float distance)
    {
        // Calculate target speed with arrive behavior
        float targetSpeed = maxSpeed;
        if (distance < slowRadius)
        {
            targetSpeed = Mathf.Lerp(0f, maxSpeed, Mathf.InverseLerp(arriveRadius, slowRadius, distance));
        }
        if (distance <= arriveRadius)
        {
            targetSpeed = 0f;
        }
        
        // Direct transform movement (for non-physics enemies)
        Vector3 movement = direction * targetSpeed * Time.deltaTime;
        transform.position += movement;
    }
    
    /// <summary>
    /// Rotate the enemy to face a specific direction.
    /// </summary>
    /// <param name="direction">Direction to face.</param>
    private void RotateTowardsDirection(Vector3 direction)
    {
        if (rotationSpeed <= 0 || direction.sqrMagnitude < 0.01f)
            return;
            
        // Calculate target rotation
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        
        // Smoothly rotate towards target
        float rotationStep = rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, 
            targetRotation, 
            rotationStep
        );
    }
    
    /// <summary>
    /// Automatically find the player entity.
    /// </summary>
    private void FindPlayer()
    {
        // Try to find by name first
        var playerEntity = Scene.FindEntityByName("Player");
        if (playerEntity)
        {
            target = playerEntity;
            return;
        }
        
        // Could also try to find by tag or component type here
        Log.Warning($"Enemy {owner.name}: Could not auto-find player. Please assign target manually.");
    }
    
    /// <summary>
    /// Set the target entity to chase.
    /// </summary>
    /// <param name="newTarget">The new target entity.</param>
    public void SetTarget(Entity newTarget)
    {
        target = newTarget;
        if (target)
        {
            Log.Info($"Enemy {owner.name}: Target set to {target.name}");
            lastPlayerPosition = target.transform.position;
        }
    }
    
    /// <summary>
    /// Set the enemy's maximum movement speed.
    /// </summary>
    /// <param name="speed">The new maximum movement speed.</param>
    public void SetMaxSpeed(float speed)
    {
        maxSpeed = Mathf.Max(0, speed);
    }
    
    /// <summary>
    /// Set the enemy's maximum acceleration.
    /// </summary>
    /// <param name="acceleration">The new maximum acceleration.</param>
    public void SetMaxAcceleration(float acceleration)
    {
        maxAcceleration = Mathf.Max(0, acceleration);
    }
    
    /// <summary>
    /// Get the distance to the current target.
    /// </summary>
    /// <returns>Distance to target, or -1 if no target is set.</returns>
    public float GetDistanceToTarget()
    {
        if (!target)
            return -1f;
            
        return Vector3.Distance(transform.position, target.transform.position);
    }
    
    /// <summary>
    /// Check if the enemy is currently chasing the player.
    /// </summary>
    /// <returns>True if chasing (walking or running), false otherwise.</returns>
    public bool IsChasing()
    {
        if (stateMachine == null)
            return false;
        return stateMachine.IsInAnyState(EnemyState.Walking, EnemyState.Running);
    }
    
    /// <summary>
    /// Check if the target is set and valid.
    /// </summary>
    /// <returns>True if target exists, false otherwise.</returns>
    public bool IsTargetInRange()
    {
        return target;
    }
    
    /// <summary>
    /// Stop chasing and remain stationary.
    /// </summary>
    public void StopChasing()
    {
        if (stateMachine == null)
            return;
        
        // Set to Idle state (animation controller will handle the visual)
        stateMachine.SetState(EnemyState.Idle);
    }
    
    /// <summary>
    /// Resume chasing the target.
    /// </summary>
    public void ResumeChasing()
    {
        if (stateMachine == null || !target)
            return;
        
        if (Health != null && Health.IsDead())
            return;
        
        // Set to Walking state (animation controller will handle the visual)
        stateMachine.SetState(EnemyState.Walking);
    }
    
    /// <summary>
    /// Called when the enemy dies.
    /// </summary>
    private void OnEnemyDeath()
    {
        // Set state to Dying (DeathSequence will handle the visual sequence)
        if (stateMachine != null)
        {
            stateMachine.SetState(EnemyState.Dying);
        }
        
        // Stop physics movement
        if (physicsComponent != null)
        {
            Vector3 currentVelocity = physicsComponent.velocity;
            physicsComponent.velocity = new Vector3(0, currentVelocity.y, 0);
        }
    }
    
    
    /// <summary>
    /// Get the enemy type for loot configuration.
    /// Used by the LootHandler to determine appropriate loot drops.
    /// </summary>
    /// <returns>Enemy type enum.</returns>
    public EnemyType GetEnemyType()
    {
        return enemyType;
    }
    
    /// <summary>
    /// Get the enemy's health component.
    /// </summary>
    /// <returns>Health if available, null otherwise.</returns>
    public Health GetHealth()
    {
        return Health;
    }
    
    /// <summary>
    /// Check if the enemy is alive.
    /// </summary>
    /// <returns>True if alive (not dead).</returns>
    public bool IsAlive()
    {
        return Health == null || !Health.IsDead();
    }
    
    /// <summary>
    /// Get the enemy's current health percentage.
    /// </summary>
    /// <returns>Health percentage (0.0 to 1.0), or 1.0 if no health component.</returns>
    public float GetHealthPercentage()
    {
        if (Health == null)
            return 1.0f;
            
        return Health.GetHealthPercentage();
    }
    
    /// <summary>
    /// Handle contact damage when enemy is close to player.
    /// </summary>
    private void HandleContactDamage()
    {
        // Don't deal contact damage if dead, dying, or stunned
        if (stateMachine == null || !stateMachine.CanAct())
            return;
       
        if (!target)
            return;
            
        // Calculate distance to player
        Vector3 enemyPosition = transform.position;
        Vector3 playerPosition = target.transform.position;
        float distanceToPlayer = Vector3.Distance(enemyPosition, playerPosition);
        // Update timer
        contactDamageTimer += Time.deltaTime;
        
        // Check if within contact damage range
        if (distanceToPlayer <= contactDamageRadius)
        {
            
            // Apply contact damage at interval
            if (contactDamageTimer >= contactDamageInterval)
            {
                // Calculate contact position (midpoint between enemy and player)
                Vector3 contactPosition = (enemyPosition + playerPosition) * 0.5f;
                
                // Apply contact damage (enemy is source, player is target)
                ContactSystem.ApplyContact(owner, target, contactPosition, Color.red);
                
                // Reset timer
                contactDamageTimer = 0.0f;
            }
        }
    }
}
