using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Player component that implements basic rigidbody movement for a top-down game.
/// Handles player input, movement, and physics interactions.
/// Requires a PhysicsComponent with a Capsule collider attached to the same entity.
/// </summary>
[ScriptSourceFile]
public class Player : ScriptComponent
{
    //[Header("Movement Settings")]
    [Tooltip("Maximum movement speed in units per second")]
    public float maxSpeed = 10.0f;
    [Tooltip("Maximum acceleration in units per second squared")]
    public float maxAcceleration = 50.0f;
    [Tooltip("Maximum deceleration when no input (higher = stops faster)")]
    public float maxDeceleration = 30.0f;
    
    //[Header("Physics Settings")]
    [Tooltip("Use physics-based movement (recommended) vs direct transform movement")]
    public bool usePhysicsMovement = true;
    
    // Component references
    private PhysicsComponent physicsComponent;
    private TransformComponent transformComponent;
    private Health Health;
    private Experience Experience;
    
    // Movement state
    private Vector3 inputDirection;
    private Vector3 currentVelocity;
    private Vector3 targetVelocity;
    
    /// <summary>
    /// Called when the script is created. Initialize component references.
    /// </summary>
    public override void OnCreate()
    {
        // Get required components
        physicsComponent = owner.GetComponent<PhysicsComponent>();
        transformComponent = owner.GetComponent<TransformComponent>();
        Health = owner.GetComponent<Health>();
        Experience = owner.GetComponent<Experience>();
        
        if (physicsComponent == null)
        {
            Log.Error($"Player on {owner.name}: PhysicsComponent not found! Please attach a PhysicsComponent with a Capsule collider.");
        }
        
        if (transformComponent == null)
        {
            Log.Error($"Player on {owner.name}: TransformComponent not found!");
        }
        
        if (Health == null)
        {
            Log.Warning($"Player on {owner.name}: Health not found! Player will not be able to take damage or heal.");
        }
        else
        {
            // Subscribe to health events
            Health.OnDeath += OnPlayerDeath;
            Health.OnDamageTaken += OnPlayerDamageTaken;
            Health.OnHealed += OnPlayerHealed;
        }
        
        if (Experience == null)
        {
            Log.Warning($"Player on {owner.name}: Experience not found! Player will not be able to collect experience.");
        }
        else
        {
            // Subscribe to experience events
            Experience.OnExperienceGained += OnExperienceGained;
            Experience.OnLevelUp += OnLevelUp;
            Experience.OnExperienceChanged += OnExperienceChanged;
        }
    }
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        // Initialize movement state
        currentVelocity = Vector3.zero;
        targetVelocity = Vector3.zero;
        
        // Validate components
        if (physicsComponent == null || transformComponent == null)
        {
            Log.Error($"Player on {owner.name}: Missing required components. Disabling script.");
            return;
        }
    }
    
    /// <summary>
    /// Called every frame to handle input and movement.
    /// </summary>
    public override void OnUpdate()
    {
        if (physicsComponent == null || transformComponent == null)
            return;
            
        // Don't process input/movement if player is dead
        if (Health != null && Health.IsDead())
            return;
            
        // Handle input
        HandleInput();
        
        // Update movement
        if (usePhysicsMovement)
        {
            // HandlePhysicsMovement();
        }
        else
        {
            HandleDirectMovement();
        }
    }
    
public override void OnFixedUpdate()
    {
        if (physicsComponent == null || transformComponent == null)
            return;
            
        // Don't process physics movement if player is dead
        if (Health != null && Health.IsDead())
        {
            // Stop horizontal movement when dead
            if (physicsComponent != null)
            {
                Vector3 currentVelocity = physicsComponent.velocity;
                physicsComponent.velocity = new Vector3(0, currentVelocity.y, 0);
            }
            return;
        }
            
        // Handle input
        // HandleInput();
        
        // Update movement
        if (usePhysicsMovement)
        {
            HandlePhysicsMovement();
        }
        else
        {
            // HandleDirectMovement();
        }
    }

    /// <summary>
    /// Handle player input for movement.
    /// </summary>
    private void HandleInput()
    {
        // Get input axes
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Create input direction vector (top-down, so Y is forward/backward)
        inputDirection = new Vector3(horizontal, 0, vertical);

        // Normalize input to prevent faster diagonal movement
        if (inputDirection.magnitude > 1.0f)
        {
            inputDirection = inputDirection.normalized;
        }

        // Calculate target velocity
        targetVelocity = inputDirection * maxSpeed;
        

        if (Input.IsDown(KeyCode.Q))
        {
            // 2) raycast from camera → mouse
            var camE = Scene.FindEntityByName("Main Camera");
            if (!camE.IsValid()) return;
            var cam = camE.GetComponent<CameraComponent>();
            cam.ScreenPointToRay(Input.mousePosition, out Ray ray);

            var hit = Physics.Raycast(ray, 500f, -1, false);
            if (!hit.HasValue) return;

            Vector3 hitPoint = hit.Value.point;

            float radius = 15.0f;

            var hits = Physics.SphereOverlap(hitPoint, radius, LayerMask.GetMask("Enemy"));

            foreach (var e in hits)
            {
                var ph = e.GetComponent<PhysicsComponent>();
                if (ph != null)
                {
                    ph.ApplyExplosionForce(2.0f, hitPoint, radius, 0.0f, ForceMode.Impulse);
                }
            }
            // bool shootRight = righthandIKWeight >= lefthandIKWeight;
            // Vector3 source = (shootRight ? RightHand : LeftHand).transform.position;
            // Vector3 dir = (hitPoint - source).normalized;
            // Shoot(source, dir, 4.4f, 200);
        }
    }
    
    /// <summary>
    /// Handle movement using physics forces with steering behavior (recommended for realistic physics).
    /// </summary>
    private void HandlePhysicsMovement()
    {
        // Get current velocity (flattened to XZ plane for top-down movement)
        Vector3 currentVelocity = physicsComponent.velocity;
        Vector3 currentPlanarVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        
        // Calculate desired velocity based on input
        Vector3 desiredVelocity = targetVelocity;
        
        // Steering = how much we want to change our current velocity
        Vector3 steering = desiredVelocity - currentPlanarVelocity;
        
        // Choose acceleration or deceleration based on input
        float maxAccelerationRate = inputDirection.magnitude > 0.1f ? maxAcceleration : maxDeceleration;
        
        // Cap to what we can actually change this frame with our max acceleration
        float maxVelocityChange = maxAccelerationRate * Time.fixedDeltaTime;
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
    /// Handle movement by directly modifying transform (alternative method).
    /// </summary>
    private void HandleDirectMovement()
    {
        // Smoothly interpolate current velocity towards target
        float accelerationRate = inputDirection.magnitude > 0.1f ? maxAcceleration : maxDeceleration;
        currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, accelerationRate * Time.deltaTime);
        
        // Apply movement to transform
        if (currentVelocity.magnitude > 0.01f)
        {
            Vector3 movement = currentVelocity * Time.deltaTime;
            transformComponent.position += movement;
        }
    }
    
    /// <summary>
    /// Get the current movement input direction.
    /// </summary>
    /// <returns>The normalized input direction vector.</returns>
    public Vector3 GetInputDirection()
    {
        return inputDirection;
    }
    
    /// <summary>
    /// Get the current movement velocity.
    /// </summary>
    /// <returns>The current velocity vector.</returns>
    public Vector3 GetVelocity()
    {
        if (usePhysicsMovement && physicsComponent != null)
        {
            return physicsComponent.velocity;
        }
        return currentVelocity;
    }
    
    /// <summary>
    /// Check if the player is currently moving.
    /// </summary>
    /// <returns>True if the player is moving, false otherwise.</returns>
    public bool IsMoving()
    {
        return GetVelocity().magnitude > 0.1f;
    }
    
    /// <summary>
    /// Add an external force to the player (useful for knockback, wind, etc.).
    /// </summary>
    /// <param name="force">The force to apply.</param>
    /// <param name="mode">The force mode to use.</param>
    public void AddExternalForce(Vector3 force, ForceMode mode = ForceMode.Force)
    {
        if (physicsComponent != null)
        {
            physicsComponent.ApplyForce(force, mode);
        }
    }

    
    /// <summary>
    /// Called when this entity begins a collision with another entity.
    /// Override this method to handle collision events.
    /// </summary>
    /// <param name="collision">Details of the collision.</param>
    public override void OnCollisionEnter(Collision collision)
    {
        // Example: Log collision for debugging
        // Log.Info($"Player collided with {collision.entity.name}");
        
        // Add custom collision handling here
        // For example: damage from enemies, item pickup, etc.
    }
    
    /// <summary>
    /// Called when another entity enters a sensor attached to this entity.
    /// Useful for trigger zones, item pickup areas, etc.
    /// </summary>
    /// <param name="entity">The entity that entered the sensor.</param>
    public override void OnSensorEnter(Entity entity)
    {
        Log.Info($"Player sensor triggered by {entity.name}");
        
        // Add custom sensor handling here
        // For example: item pickup, area triggers, etc.
    }
    
    /// <summary>
    /// Called when the player takes damage.
    /// </summary>
    /// <param name="damageAmount">Amount of damage taken.</param>
    private void OnPlayerDamageTaken(float damageAmount)
    {
        Log.Info($"Player took {damageAmount} damage - Health: {Health.GetCurrentHealth()}/{Health.GetMaxHealth()}");
        
        // Could add damage reaction behaviors here, like:
        // - Screen shake/flash
        // - Play damage sound
        // - Show damage UI
        // - Brief invincibility frames
        // - Knockback effect
    }
    
    /// <summary>
    /// Called when the player is healed.
    /// </summary>
    /// <param name="healAmount">Amount healed.</param>
    private void OnPlayerHealed(float healAmount)
    {
        Log.Info($"Player healed for {healAmount} - Health: {Health.GetCurrentHealth()}/{Health.GetMaxHealth()}");
        
        // Could add healing reaction behaviors here, like:
        // - Play healing sound/effect
        // - Show healing UI
        // - Particle effects
    }
    
    /// <summary>
    /// Called when the player dies.
    /// </summary>
    private void OnPlayerDeath()
    {
        Log.Info($"Player has died");
        
        // Stop all movement
        currentVelocity = Vector3.zero;
        targetVelocity = Vector3.zero;
        inputDirection = Vector3.zero;
        
        // Could add death behaviors here, like:
        // - Show game over screen
        // - Play death sound/animation
        // - Disable input
        // - Restart level
        // - Show respawn options
    }
    
    /// <summary>
    /// Get the player's health component.
    /// </summary>
    /// <returns>Health if available, null otherwise.</returns>
    public Health GetHealth()
    {
        return Health;
    }
    
    /// <summary>
    /// Check if the player is alive.
    /// </summary>
    /// <returns>True if alive (not dead).</returns>
    public bool IsAlive()
    {
        return Health == null || !Health.IsDead();
    }
    
    /// <summary>
    /// Get the player's current health percentage.
    /// </summary>
    /// <returns>Health percentage (0.0 to 1.0), or 1.0 if no health component.</returns>
    public float GetHealthPercentage()
    {
        if (Health == null)
            return 1.0f;
            
        return Health.GetHealthPercentage();
    }
    
    /// <summary>
    /// Heal the player by a specific amount.
    /// </summary>
    /// <param name="healAmount">Amount to heal.</param>
    /// <returns>Actual amount healed.</returns>
    public float HealPlayer(float healAmount)
    {
        if (Health == null)
            return 0;
            
        return Health.Heal(healAmount, owner);
    }
    
    /// <summary>
    /// Deal damage to the player.
    /// </summary>
    /// <param name="damage">Amount of damage to deal.</param>
    /// <param name="source">Source of the damage.</param>
    /// <returns>True if the player died from this damage.</returns>
    public bool DamagePlayer(float damage, Entity source)
    {
        if (Health == null)
            return false;
            
        return Health.TakeDamage(damage, source);
    }
    
    /// <summary>
    /// Called when the player gains experience.
    /// </summary>
    /// <param name="experienceAmount">Amount of experience gained.</param>
    private void OnExperienceGained(float experienceAmount)
    {
        Log.Info($"Player gained {experienceAmount} experience!");
        
        // Could add experience gain behaviors here, like:
        // - Play experience gain sound/effect
        // - Show floating text
        // - Update UI
        // - Particle effects
    }
    
    /// <summary>
    /// Called when the player levels up.
    /// </summary>
    /// <param name="newLevel">New level.</param>
    /// <param name="oldLevel">Previous level.</param>
    private void OnLevelUp(int newLevel, int oldLevel)
    {
        Log.Info($"Player LEVEL UP! {oldLevel} -> {newLevel}");
        
        // Could add level up behaviors here, like:
        // - Play level up sound/fanfare
        // - Screen flash/effect
        // - Show level up UI
        // - Restore health
        // - Unlock new abilities
        // - Increase stats
        
        // Example: Restore health on level up
        if (Health != null)
        {
            Health.RestoreToFullHealth();
            Log.Info("Player health restored on level up!");
        }
    }
    
    /// <summary>
    /// Called when the player's experience changes.
    /// </summary>
    /// <param name="currentExp">Current experience amount.</param>
    /// <param name="expToNextLevel">Experience needed for next level.</param>
    private void OnExperienceChanged(float currentExp, float expToNextLevel)
    {
        // This is called frequently, so avoid heavy logging
        // Could update UI elements here, like:
        // - Experience bar
        // - Level display
        // - Progress indicators
    }
    
    /// <summary>
    /// Get the player's current level.
    /// </summary>
    /// <returns>Current level, or 1 if no experience pickup component.</returns>
    public int GetLevel()
    {
        if (Experience == null)
            return 1;
            
        return Experience.GetCurrentLevel();
    }
    
    /// <summary>
    /// Get the player's current experience.
    /// </summary>
    /// <returns>Current experience, or 0 if no experience pickup component.</returns>
    public float GetExperience()
    {
        if (Experience == null)
            return 0;
            
        return Experience.GetCurrentExperience();
    }
    
    /// <summary>
    /// Get the player's experience progress towards next level.
    /// </summary>
    /// <returns>Progress percentage (0.0 to 1.0).</returns>
    public float GetLevelProgress()
    {
        if (Experience == null)
            return 0;
            
        return Experience.GetLevelProgress();
    }
}
