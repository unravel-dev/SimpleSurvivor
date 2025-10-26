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
    [Tooltip("Maximum distance to chase the player (0 = unlimited)")]
    public float maxChaseDistance = 0.0f;
    
    //[Header("Debug")]
    [Tooltip("Enable debug logging for enemy behavior")]
    public bool debugMovement = false;
    
    // Component references
    private TransformComponent transformComponent;
    private PhysicsComponent physicsComponent;
    
    // Movement state
    private Vector3 lastPlayerPosition;
    private bool isChasing = false;
    
    /// <summary>
    /// Called when the script is created. Initialize component references.
    /// </summary>
    public override void OnCreate()
    {
        transformComponent = owner.GetComponent<TransformComponent>();
        physicsComponent = owner.GetComponent<PhysicsComponent>();
        
        if (transformComponent == null)
        {
            Log.Error($"Enemy on {owner.name}: TransformComponent not found!");
        }
        
        if (usePhysicsMovement && physicsComponent == null)
        {
            Log.Warning($"Enemy on {owner.name}: PhysicsComponent not found! Falling back to direct movement.");
            usePhysicsMovement = false;
        }
        
        Log.Info($"Enemy initialized on {owner.name}");
    }
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        // Validate components
        if (transformComponent == null)
        {
            Log.Error($"Enemy on {owner.name}: Missing TransformComponent. Disabling script.");
            return;
        }
        
        // Auto-find player if enabled
        if (autoFindPlayer && !target)
        {
            FindPlayer();
        }
        
        // Initialize last known player position
        if (target)
        {
            lastPlayerPosition = target.transform.position;
        }
        
        Log.Info($"Enemy started on {owner.name}");
    }
    
    /// <summary>
    /// Called every frame to handle enemy AI decisions.
    /// </summary>
    public override void OnUpdate()
    {
        if (transformComponent == null || !target)
            return;
            
        // Update AI behavior (decision making only)
        UpdateAI();
    }
    
    /// <summary>
    /// Called at fixed intervals for physics-based movement.
    /// </summary>
    public override void OnFixedUpdate()
    {
        if (transformComponent == null || !target || !usePhysicsMovement || physicsComponent == null)
            return;
            
        // Handle physics movement
        if (isChasing)
        {
            UpdatePhysicsMovement();
        }
        else
        {
            // Stop horizontal movement when not chasing
            Vector3 currentVelocity = physicsComponent.velocity;
            physicsComponent.velocity = new Vector3(0, currentVelocity.y, 0);
        }
    }
    
    /// <summary>
    /// Update enemy AI decisions (called every frame).
    /// </summary>
    private void UpdateAI()
    {
        Vector3 playerPosition = target.transform.position;
        Vector3 enemyPosition = transformComponent.position;
        
        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(enemyPosition, playerPosition);
        
        // Check if player is within chase range
        bool shouldChase = maxChaseDistance <= 0 || distanceToPlayer <= maxChaseDistance;
        
        // Check if we should stop (arrived at player)
        bool shouldStop = distanceToPlayer <= arriveRadius;
        
        if (shouldChase && !shouldStop)
        {
            // Calculate direction to player (only X and Z for top-down)
            Vector3 toTarget = playerPosition - enemyPosition;
            toTarget.y = 0; // Keep movement on horizontal plane
            
            if (toTarget.magnitude > 0.0001f)
            {
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
                
                isChasing = true;
                lastPlayerPosition = playerPosition;
                
                if (debugMovement)
                {
                    Log.Info($"Enemy {owner.name}: Chasing player (distance: {distanceToPlayer:F2})");
                }
            }
        }
        else
        {
            if (isChasing && debugMovement)
            {
                if (shouldStop)
                    Log.Info($"Enemy {owner.name}: Arrived at player");
                else
                    Log.Info($"Enemy {owner.name}: Stopped chasing - player out of range");
            }
            
            isChasing = false;
        }
    }
    
    /// <summary>
    /// Update physics-based movement using steering forces (called at fixed intervals).
    /// </summary>
    private void UpdatePhysicsMovement()
    {
        Vector3 playerPosition = target.transform.position;
        Vector3 enemyPosition = transformComponent.position;
        
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
        
        if (debugMovement)
        {
            Log.Info($"Enemy {owner.name}: Applying force {accelerationForce.magnitude:F2}, target speed: {targetSpeed:F2}");
        }
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
        transformComponent.position += movement;
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
        transformComponent.rotation = Quaternion.RotateTowards(
            transformComponent.rotation, 
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
            Log.Info($"Enemy {owner.name}: Found player target: {target.name}");
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
        
        if (debugMovement)
        {
            Log.Info($"Enemy {owner.name}: Max speed set to {maxSpeed:F2}");
        }
    }
    
    /// <summary>
    /// Set the enemy's maximum acceleration.
    /// </summary>
    /// <param name="acceleration">The new maximum acceleration.</param>
    public void SetMaxAcceleration(float acceleration)
    {
        maxAcceleration = Mathf.Max(0, acceleration);
        
        if (debugMovement)
        {
            Log.Info($"Enemy {owner.name}: Max acceleration set to {maxAcceleration:F2}");
        }
    }
    
    /// <summary>
    /// Get the distance to the current target.
    /// </summary>
    /// <returns>Distance to target, or -1 if no target is set.</returns>
    public float GetDistanceToTarget()
    {
        if (!target || transformComponent == null)
            return -1f;
            
        return Vector3.Distance(transformComponent.position, target.transform.position);
    }
    
    /// <summary>
    /// Check if the enemy is currently chasing the player.
    /// </summary>
    /// <returns>True if chasing, false otherwise.</returns>
    public bool IsChasing()
    {
        return isChasing;
    }
    
    /// <summary>
    /// Check if the target is within chase range.
    /// </summary>
    /// <returns>True if target is in range, false otherwise.</returns>
    public bool IsTargetInRange()
    {
        if (!target || transformComponent == null)
            return false;
            
        if (maxChaseDistance <= 0)
            return true; // Unlimited range
            
        float distance = GetDistanceToTarget();
        return distance <= maxChaseDistance;
    }
    
    /// <summary>
    /// Stop chasing and remain stationary.
    /// </summary>
    public void StopChasing()
    {
        isChasing = false;
        
        // Stop physics movement if using physics
        // Note: Physics velocity will be stopped in OnFixedUpdate when isChasing = false
        
        if (debugMovement)
        {
            Log.Info($"Enemy {owner.name}: Stopped chasing");
        }
    }
    
    /// <summary>
    /// Resume chasing the target.
    /// </summary>
    public void ResumeChasing()
    {
        if (target)
        {
            isChasing = true;
            
            if (debugMovement)
            {
                Log.Info($"Enemy {owner.name}: Resumed chasing");
            }
        }
    }
}
