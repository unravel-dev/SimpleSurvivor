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
    
    [Header("Obstacle Avoidance")]
    [Tooltip("Distance ahead to check for obstacles (lookahead distance)")]
    public float obstacleLookaheadDistance = 1.5f;
    [Tooltip("Radius for obstacle detection (capsule/sphere cast)")]
    public float obstacleDetectionRadius = 0.4f;
    [Tooltip("Distance to maintain from walls when wall-following")]
    public float wallFollowDistance = 0.8f;
    [Tooltip("How often to check line of sight to player (seconds)")]
    public float losCheckInterval = 0.2f;
    [Tooltip("Enable line-of-sight based pathfinding with wall-following")]
    public bool useLOSPathfinding = true;
    [Tooltip("Enable local obstacle avoidance/sliding")]
    public bool useObstacleAvoidance = true;
    
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
    
    // Obstacle avoidance state
    private bool isWallFollowing = false;
    private bool wasWallFollowing = false; // Track previous state to detect when entering wall-following
    private Vector3 wallFollowDirection = Vector3.zero; // Tangential direction along wall
    private Vector3 lastWallNormal = Vector3.zero;
    private float losCheckTimer = 0.0f;
    private int wallFollowSide = 1; // 1 for right, -1 for left (chosen once when entering wall-follow)
    private Vector3 lastKnownPlayerPosition = Vector3.zero; // Last position where player was seen
    private bool hasLastKnownPosition = false; // Whether we have a valid last known position
    
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
    
    public LayerMask GetObstacleLayerMask()
    {
        return LayerMask.GetMask("Environment");
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
    /// Update physics-based movement using steering forces with obstacle avoidance (called at fixed intervals).
    /// </summary>
    private void UpdatePhysicsMovement()
    {
        Vector3 playerPosition = target.transform.position;
        Vector3 enemyPosition = transform.position;
        
        // Direction to target (flattened to XZ plane for top-down)
        Vector3 toTarget = playerPosition - enemyPosition;
        
        float distance = toTarget.magnitude;
        if (distance < 0.0001f) return;
        
        Vector3 direction = toTarget / distance;
        
        // Check line of sight and update wall-following state
        // When wall-following, check LOS every frame to quickly detect when path is clear
        // When not wall-following, check periodically to save performance
        if (useLOSPathfinding)
        {
            bool shouldCheckLOS = false;
            if (isWallFollowing)
            {
                // Check every frame when wall-following to exit as soon as LOS clears
                shouldCheckLOS = true;
            }
            else
            {
                // Check periodically when not wall-following
                losCheckTimer += Time.fixedDeltaTime;
                if (losCheckTimer >= losCheckInterval)
                {
                    losCheckTimer = 0.0f;
                    shouldCheckLOS = true;
                }
            }
            
            if (shouldCheckLOS)
            {
                CheckLineOfSight(playerPosition, enemyPosition);
            }
        }
        
        // Calculate desired movement direction with obstacle avoidance
        Vector3 desiredDirection = direction;
        
        if (isWallFollowing)
        {
            // Wall-following mode: move tangentially along the wall
            desiredDirection = GetWallFollowDirection(enemyPosition, direction);
        }
        else if (useObstacleAvoidance)
        {
            // Not wall-following - use local obstacle avoidance for small obstacles
            // This handles obstacles that are close but don't require full wall-following
            Vector3 obstacleNormal = GetObstacleNormal(enemyPosition, direction);
            if (obstacleNormal.sqrMagnitude > 0.01f)
            {
                // Small obstacle detected - slide around it
                obstacleNormal = obstacleNormal.normalized;
                float dotIntoObstacle = Vector3.Dot(direction, obstacleNormal);
                if (dotIntoObstacle < 0) // Moving into obstacle
                {
                    // Slide along obstacle
                    desiredDirection = direction - obstacleNormal * dotIntoObstacle;
                    desiredDirection = desiredDirection.normalized;
                }
            }
        }
        
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
        
        Vector3 desiredVelocity = desiredDirection * targetSpeed;
        
        // Get current velocity (flattened to XZ plane)
        Vector3 currentVelocity = physicsComponent.velocity;
        Vector3 currentPlanarVelocity = currentVelocity; //new Vector3(currentVelocity.x, 0, currentVelocity.z);
        
        // Apply sliding if we're hitting an obstacle
        if (useObstacleAvoidance)
        {
            Vector3 obstacleNormal = GetObstacleNormal(enemyPosition, desiredDirection);
            if (obstacleNormal.sqrMagnitude > 0.01f)
            {
                // Slide along the obstacle: remove component of velocity into the obstacle
                float dot = Vector3.Dot(currentPlanarVelocity, obstacleNormal);
                if (dot < 0) // Only slide if moving into obstacle
                {
                    currentPlanarVelocity = currentPlanarVelocity - obstacleNormal * dot;
                }
            }
        }
        
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
    /// Move the enemy towards the player using direct transform movement with obstacle avoidance.
    /// Includes arrive behavior for smooth stopping.
    /// </summary>
    /// <param name="direction">Normalized direction to move.</param>
    /// <param name="distance">Distance to the target.</param>
    private void MoveTowardsPlayer(Vector3 direction, float distance)
    {
        Vector3 enemyPosition = transform.position;
        Vector3 playerPosition = target.transform.position;
        
        // Check line of sight and update wall-following state
        if (useLOSPathfinding)
        {
            losCheckTimer += Time.deltaTime;
            if (losCheckTimer >= losCheckInterval)
            {
                losCheckTimer = 0.0f;
                CheckLineOfSight(playerPosition, enemyPosition);
            }
        }
        
        // Calculate desired movement direction with obstacle avoidance
        Vector3 desiredDirection = direction;
        
        if (isWallFollowing)
        {
            // Wall-following mode: move tangentially along the wall
            desiredDirection = GetWallFollowDirection(enemyPosition, direction);
        }
        else if (useObstacleAvoidance)
        {
            // Not wall-following - use local obstacle avoidance for small obstacles
            // This handles obstacles that are close but don't require full wall-following
            Vector3 obstacleNormal = GetObstacleNormal(enemyPosition, direction);
            if (obstacleNormal.sqrMagnitude > 0.01f)
            {
                // Small obstacle detected - slide around it
                obstacleNormal = obstacleNormal.normalized;
                float dotIntoObstacle = Vector3.Dot(direction, obstacleNormal);
                if (dotIntoObstacle < 0) // Moving into obstacle
                {
                    // Slide along obstacle
                    desiredDirection = direction - obstacleNormal * dotIntoObstacle;
                    desiredDirection = desiredDirection.normalized;
                }
            }
        }
        
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
        Vector3 movement = desiredDirection * targetSpeed * Time.deltaTime;
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
    /// Check line of sight to player and update wall-following state using a single sphere cast.
    /// This is the ONLY sphere cast needed for obstacle avoidance.
    /// </summary>
    /// <param name="playerPosition">Player's position.</param>
    /// <param name="enemyPosition">Enemy's position.</param>
    private void CheckLineOfSight(Vector3 playerPosition, Vector3 enemyPosition)
    {
        // Create ray from enemy to player (on XZ plane)
        Vector3 rayDirection = playerPosition - enemyPosition;
        
        float rayDistance = rayDirection.magnitude;
        if (rayDistance < 0.01f)
        {
            isWallFollowing = false;
            return;
        }
        
        rayDirection = rayDirection.normalized;
        
        // SINGLE sphere cast: enemy to player (accounts for enemy's collision radius)
        Vector3 castOrigin = enemyPosition + Vector3.up;
        Ray ray;
        ray.origin = castOrigin;
        ray.direction = rayDirection;
        var obstacleLayerMask = GetObstacleLayerMask();
        var hit = Physics.SphereCast(ray, obstacleDetectionRadius, rayDistance, obstacleLayerMask);

        if (!hit.HasValue)
        {
            // Clear line of sight - update last known position and exit wall-following mode
            lastKnownPlayerPosition = playerPosition;
            hasLastKnownPosition = true;
            isWallFollowing = false;
            wallFollowDirection = Vector3.zero;
            wasWallFollowing = false;
            
        }
        else
        {
            // LOS blocked - check if we're close enough to the wall to start following
            float distanceToWall = hit.Value.distance;
            float wallFollowThreshold = obstacleLookaheadDistance * 1.5f; // Start following when wall is within this distance

            if (distanceToWall <= wallFollowThreshold)
            {
                // Close to wall - enter or continue wall-following mode
                bool justEnteredWallFollowing = !wasWallFollowing;
                isWallFollowing = true;
                wasWallFollowing = true;

                // Store the wall normal from the hit
                Vector3 currentWallNormal = hit.Value.normal;

                // Validate normal before using
                if (currentWallNormal.sqrMagnitude > 0.01f)
                {
                    currentWallNormal = currentWallNormal.normalized;
                    lastWallNormal = currentWallNormal;

                    // Only recalculate side when first entering wall-following mode
                    // This keeps the behavior stable and prevents constant direction changes
                    Vector3 up = Vector3.up;
                    if (justEnteredWallFollowing)
                    {
                        // Store last known position when entering wall-following
                        if (!hasLastKnownPosition)
                        {
                            lastKnownPlayerPosition = playerPosition;
                            hasLastKnownPosition = true;
                        }

                        // Calculate tangent directions (left and right along the wall)
                        Vector3 tangentRight = Vector3.Cross(up, currentWallNormal).normalized;
                        Vector3 tangentLeft = -tangentRight;

                        // Determine which direction gets closer to the last known player position
                        float lookaheadDistance = obstacleLookaheadDistance * 2.0f;
                        Vector3 rightAhead = enemyPosition + tangentRight * lookaheadDistance;
                        Vector3 leftAhead = enemyPosition + tangentLeft * lookaheadDistance;

                        float rightDistanceToTarget = Vector3.Distance(rightAhead, lastKnownPlayerPosition);
                        float leftDistanceToTarget = Vector3.Distance(leftAhead, lastKnownPlayerPosition);

                        // Also check free space as secondary factor
                        float rightSpace = GetFreeSpaceInDirection(enemyPosition, tangentRight);
                        float leftSpace = GetFreeSpaceInDirection(enemyPosition, tangentLeft);

                        // Combine: prefer direction that gets closer to last known position (70%) with more space (30%)
                        float rightScore = (rayDistance - rightDistanceToTarget) * 0.7f + rightSpace * 0.3f;
                        float leftScore = (rayDistance - leftDistanceToTarget) * 0.7f + leftSpace * 0.3f;

                        wallFollowSide = (rightScore > leftScore) ? 1 : -1;
                    }

                    // Calculate direction using the stable side choice
                    Vector3 tangent = Vector3.Cross(up, currentWallNormal).normalized * wallFollowSide;

                    // Move tangentially along the wall - this is the primary direction
                    // Only add small correction if we're too close to the wall
                    Vector3 toWall = -currentWallNormal; // Direction toward wall (opposite of normal)
                    float currentDistanceToWall = distanceToWall;

                    // If too close to wall, add small push away; otherwise just move tangentially
                    if (currentDistanceToWall < wallFollowDistance)
                    {
                        // Too close - add small correction to maintain distance
                        float correctionStrength = (wallFollowDistance - currentDistanceToWall) / wallFollowDistance;
                        Vector3 pushAway = currentWallNormal * correctionStrength * 0.3f;
                        wallFollowDirection = (tangent + pushAway).normalized;
                    }
                    else
                    {
                        // At good distance - move purely tangentially
                        wallFollowDirection = tangent;
                    }
                }
                else
                {
                    // Invalid normal, fall back to moving perpendicular to wall
                    Vector3 up = Vector3.up;
                    Vector3 tangent = Vector3.Cross(up, rayDirection).normalized;
                    wallFollowDirection = tangent;
                    if (justEnteredWallFollowing)
                    {
                        wallFollowSide = 1;
                    }
                }
            }
            else
            {
                // Wall is too far away - move directly toward player, let local avoidance handle it when closer
                isWallFollowing = false;
                wallFollowDirection = Vector3.zero;
                wasWallFollowing = false;
            }
        }
    }
    
    /// <summary>
    /// Get the direction to move when wall-following.
    /// Moves tangentially along the wall, maintaining a small distance from it.
    /// </summary>
    /// <param name="enemyPosition">Enemy's current position.</param>
    /// <param name="desiredDirection">Desired direction (toward player).</param>
    /// <returns>Wall-follow direction (tangential to wall).</returns>
    private Vector3 GetWallFollowDirection(Vector3 enemyPosition, Vector3 desiredDirection)
    {
        // Reuse the wall normal and direction from CheckLineOfSight
        // No additional sphere cast needed - the LOS check already provided all the info
        if (wallFollowDirection.sqrMagnitude > 0.01f)
        {
            return wallFollowDirection;
        }
        
        // Fallback: if wallFollowDirection is invalid, recalculate from stored normal
        if (lastWallNormal.sqrMagnitude > 0.01f)
        {
            Vector3 upVec = Vector3.up;
            Vector3 tangentDir = Vector3.Cross(upVec, lastWallNormal).normalized * wallFollowSide;
            
            // Check current distance to wall
            Vector3 castOrigin = enemyPosition + Vector3.up * 0.5f;
            Ray ray;
            ray.origin = castOrigin;
            ray.direction = -lastWallNormal; // Cast toward wall to measure distance
            var obstacleLayerMask = GetObstacleLayerMask();
            var hit = Physics.SphereCast(ray, obstacleDetectionRadius, wallFollowDistance * 2.0f, obstacleLayerMask);
            
            if (hit.HasValue)
            {
                float currentDistanceToWall = hit.Value.distance;
                // If too close to wall, add small correction; otherwise move purely tangentially
                if (currentDistanceToWall < wallFollowDistance)
                {
                    float correctionStrength = (wallFollowDistance - currentDistanceToWall) / wallFollowDistance;
                    Vector3 pushAway = lastWallNormal * correctionStrength * 0.3f;
                    return (tangentDir + pushAway).normalized;
                }
            }
            
            // At good distance or no hit - move purely tangentially
            return tangentDir;
        }
        
        // Last resort: move perpendicular to desired direction
        Vector3 upVec2 = Vector3.up;
        Vector3 tangentDir2 = Vector3.Cross(upVec2, desiredDirection).normalized;
        return tangentDir2;
    }
    
    /// <summary>
    /// Get obstacle normal if there's an obstacle ahead using sphere cast.
    /// Simplified to use only a single forward cast - LOS check already determines if we need avoidance.
    /// </summary>
    /// <param name="enemyPosition">Enemy's current position.</param>
    /// <param name="direction">Direction to check.</param>
    /// <returns>Obstacle normal if hit, zero vector otherwise.</returns>
    private Vector3 GetObstacleNormal(Vector3 enemyPosition, Vector3 direction)
    {
        Vector3 castOrigin = enemyPosition + Vector3.up * 0.5f;
        float checkDistance = obstacleLookaheadDistance;
        
        // Single sphere cast forward - sufficient since LOS check already handles pathfinding
        Ray ray;
        ray.origin = castOrigin;
        ray.direction = direction;
        var obstacleLayerMask = GetObstacleLayerMask();
        var hit = Physics.SphereCast(ray, obstacleDetectionRadius, checkDistance, obstacleLayerMask);
        if (hit.HasValue)
        {
            Vector3 normal = hit.Value.normal;
            // Validate that normal is not zero before normalizing (prevents NaN)
            if (normal.sqrMagnitude > 0.01f)
            {
                return normal.normalized;
            }
        }
        
        return Vector3.zero;
    }
    
    /// <summary>
    /// Get direction with obstacle avoidance applied (steering around obstacles).
    /// </summary>
    /// <param name="enemyPosition">Enemy's current position.</param>
    /// <param name="desiredDirection">Desired direction (toward player).</param>
    /// <returns>Adjusted direction with obstacle avoidance.</returns>
    private Vector3 GetObstacleAvoidedDirection(Vector3 enemyPosition, Vector3 desiredDirection)
    {
        Vector3 obstacleNormal = GetObstacleNormal(enemyPosition, desiredDirection);
        
        // Early return if no obstacle detected (use stricter threshold)
        // Also check for NaN explicitly (NaN comparisons always return false)
        if (obstacleNormal.sqrMagnitude < 0.01f || 
            float.IsNaN(obstacleNormal.x) || float.IsNaN(obstacleNormal.y) || float.IsNaN(obstacleNormal.z))
        {
            return desiredDirection;
        }
        
        // Normalize obstacle normal
        obstacleNormal = obstacleNormal.normalized;
        
        // Validate normalized normal is not NaN
        if (float.IsNaN(obstacleNormal.x) || float.IsNaN(obstacleNormal.y) || float.IsNaN(obstacleNormal.z))
        {
            return desiredDirection;
        }
        
        // Slide along obstacle: remove component into obstacle
        Vector3 slideDirection = desiredDirection - Vector3.Dot(desiredDirection, obstacleNormal) * obstacleNormal;
        
        // Validate slide direction
        if (slideDirection.sqrMagnitude < 0.01f || 
            float.IsNaN(slideDirection.x) || float.IsNaN(slideDirection.y) || float.IsNaN(slideDirection.z))
        {
            // Desired direction is parallel to obstacle normal, can't slide
            return desiredDirection;
        }
        
        // Also add lateral steering: check left and right for best path
        Vector3 up = Vector3.up;
        Vector3 right = Vector3.Cross(up, obstacleNormal);
        
        // Validate cross product (could be zero if obstacleNormal is parallel to up, or NaN)
        if (right.sqrMagnitude < 0.01f || 
            float.IsNaN(right.x) || float.IsNaN(right.y) || float.IsNaN(right.z))
        {
            // Obstacle normal is parallel to up, or cross product failed, just use slide direction
            return slideDirection.normalized;
        }
        
        right = right.normalized;
        
        // Validate normalized right is not NaN
        if (float.IsNaN(right.x) || float.IsNaN(right.y) || float.IsNaN(right.z))
        {
            return slideDirection.normalized;
        }
        
        Vector3 left = -right;
        
        float rightSpace = GetFreeSpaceInDirection(enemyPosition, right);
        float leftSpace = GetFreeSpaceInDirection(enemyPosition, left);
        
        // Validate space values are not NaN
        if (float.IsNaN(rightSpace) || float.IsNaN(leftSpace))
        {
            return slideDirection.normalized;
        }
        
        // Steer toward the side with more free space
        Vector3 avoidDirection = (rightSpace > leftSpace) ? right : left;
        
        // Blend slide direction with avoid direction
        float avoidWeight = 0.4f; // How much to favor avoiding vs sliding
        Vector3 finalDirection = Vector3.Slerp(slideDirection.normalized, avoidDirection, avoidWeight);
        
        // Final validation - ensure we don't return zero or NaN
        if (finalDirection.sqrMagnitude < 0.01f || 
            float.IsNaN(finalDirection.x) || float.IsNaN(finalDirection.y) || float.IsNaN(finalDirection.z))
        {
            return desiredDirection;
        }
        
        return finalDirection.normalized;
    }
    
    /// <summary>
    /// Get the amount of free space in a given direction using sphere cast.
    /// </summary>
    /// <param name="position">Position to check from.</param>
    /// <param name="direction">Direction to check.</param>
    /// <returns>Distance to nearest obstacle in that direction.</returns>
    private float GetFreeSpaceInDirection(Vector3 position, Vector3 direction)
    {
        Vector3 castOrigin = position + Vector3.up * 0.5f;
        float maxDistance = obstacleLookaheadDistance * 1.5f;
        
        Ray ray;
        ray.origin = castOrigin;
        ray.direction = direction;
        var obstacleLayerMask = GetObstacleLayerMask();
        var hit = Physics.SphereCast(ray, obstacleDetectionRadius, maxDistance, obstacleLayerMask);
        if (hit.HasValue)
        {
            return hit.Value.distance;
        }
        
        return maxDistance;
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
