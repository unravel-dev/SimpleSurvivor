using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// TopDownCamera component that implements smooth camera following for a top-down game.
/// Handles camera positioning, smooth following, and optional boundaries.
/// Requires a CameraComponent attached to the same entity.
/// </summary>
[ScriptSourceFile]
public class TopDownCamera : ScriptComponent
{
    //[Header("Target Settings")]
    [Tooltip("The entity (player) that the camera should follow")]
    public Entity target;
    [Tooltip("Camera position offset from the target (X, Y, Z)")]
    public Vector3 offset = new Vector3(0, 20, -10);
    [Tooltip("Automatically find and follow an entity named 'Player' on start")]
    public bool autoFindPlayer = true;
    
    //[Header("Follow Settings")]
    [Tooltip("Use physics-based camera movement in OnFixedUpdate vs frame-based in OnUpdate")]
    public bool usePhysicsMovement = true;
    [Tooltip("Use smooth camera following vs linear movement")]
    public bool smoothFollow = true;
    [Tooltip("Speed of linear camera following (when smooth follow is disabled)")]
    public float followSpeed = 5.0f;
    [Tooltip("Time for smooth camera following to reach target (lower = faster)")]
    public float smoothTime = 0.3f;
    [Tooltip("Maximum speed limit for smooth camera following")]
    public float maxFollowSpeed = 20.0f;
    
    //[Header("Camera Settings")]
    [Tooltip("Lock camera rotation to maintain consistent top-down view")]
    public bool lockRotation = true;
    [Tooltip("Camera rotation angles for top-down view (X=90 for looking down)")]
    public Vector3 cameraRotation = new Vector3(65, 0, 0); // Top-down view
    
    //[Header("Boundary Settings")]
    [Tooltip("Enable camera movement boundaries to constrain camera position")]
    public bool useBoundaries = false;
    [Tooltip("Minimum camera position boundaries (X, Y, Z)")]
    public Vector3 minBounds = new Vector3(-50, 0, -50);
    [Tooltip("Maximum camera position boundaries (X, Y, Z)")]
    public Vector3 maxBounds = new Vector3(50, 20, 50);
    
    //[Header("Look Ahead Settings")]
    [Tooltip("Enable predictive camera movement based on player input direction")]
    public bool useLookAhead = true;
    [Tooltip("Distance the camera looks ahead in the movement direction")]
    public float lookAheadDistance = 3.0f;
    [Tooltip("Speed of look-ahead offset transitions (higher = more responsive)")]
    public float lookAheadSpeed = 2.0f;
    
    //[Header("Camera Shake Settings")]
    [Tooltip("Enable camera shake when player takes damage")]
    public bool enableCameraShake = true;
    [Tooltip("Base shake intensity (multiplied by damage amount)")]
    public float shakeIntensity = 0.5f;
    [Tooltip("Base shake duration in seconds")]
    public float shakeDuration = 0.3f;
    [Tooltip("Shake decay curve (higher = faster decay)")]
    public float shakeDecay = 2.0f;
    [Tooltip("Maximum shake intensity (caps the shake strength)")]
    public float maxShakeIntensity = 2.0f;
    
    // Component references
    private CameraComponent cameraComponent;    
    // Follow state
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;
    private Vector3 lookAheadOffset = Vector3.zero;
    
    // Camera shake state
    private float shakeTimer = 0.0f;
    private float currentShakeIntensity = 0.0f;
    private Vector3 shakeOffset = Vector3.zero;
    private System.Random shakeRandom = new System.Random();
    
    /// <summary>
    /// Called when the script is created. Initialize component references.
    /// </summary>
    public override void OnCreate()
    {
        // Get required components
        cameraComponent = owner.GetComponent<CameraComponent>();
        
        if (cameraComponent == null)
        {
            Log.Error($"TopDownCamera on {owner.name}: CameraComponent not found! Please attach a CameraComponent.");
        }
     
    }
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        // Validate components
        if (cameraComponent == null)
        {
            Log.Error($"TopDownCamera on {owner.name}: Missing required components. Disabling script.");
            return;
        }
        
        // Auto-find player if enabled
        if (autoFindPlayer && !target)
        {
            FindPlayerTarget();
        }
        
        // Set initial camera rotation for top-down view
        if (lockRotation)
        {
            transform.eulerAngles = cameraRotation;
        }
        
        // Initialize target position
        if (target)
        {
            targetPosition = CalculateTargetPosition();
            transform.position = targetPosition;
        }
    }
    
    
    /// <summary>
    /// Called every frame to update camera position (when not using physics movement).
    /// </summary>
    public override void OnUpdate()
    {
        if (cameraComponent == null || !target)
            return;
            
        // Update camera shake
        if (enableCameraShake)
        {
            UpdateCameraShake();
        }
            
        // Update camera following (only if not using physics movement)
        if (!usePhysicsMovement)
        {
            UpdateCameraFollow();
        }
        
        // Update camera rotation if locked (always in OnUpdate for responsiveness)
        if (lockRotation)
        {
            transform.eulerAngles = cameraRotation;
        }
    }
    
    /// <summary>
    /// Called at fixed intervals for physics-based camera movement.
    /// </summary>
    public override void OnFixedUpdate()
    {
        if (cameraComponent == null || !target)
            return;
            
        // Update camera shake
        if (enableCameraShake)
        {
            UpdateCameraShake();
        }
            
        // Update camera following (only if using physics movement)
        if (usePhysicsMovement)
        {
            UpdateCameraFollowPhysics();
        }
    }
    
    /// <summary>
    /// Update camera following logic.
    /// </summary>
    private void UpdateCameraFollow()
    {
        // Calculate target position
        targetPosition = CalculateTargetPosition();
        
        // Apply look-ahead if enabled
        if (useLookAhead)
        {
            ApplyLookAhead();
        }
        
        // Apply boundaries if enabled
        if (useBoundaries)
        {
            targetPosition = ApplyBoundaries(targetPosition);
        }
        
        // Move camera towards target
        Vector3 finalPosition;
        if (smoothFollow)
        {
            // Use smooth damping for natural movement
            finalPosition = Vector3.SmoothDamp(
                transform.position, 
                targetPosition, 
                ref velocity, 
                smoothTime, 
                maxFollowSpeed, 
                Time.deltaTime
            );
        }
        else
        {
            // Use linear interpolation for consistent speed
            finalPosition = Vector3.MoveTowards(
                transform.position, 
                targetPosition, 
                followSpeed * Time.deltaTime
            );
        }
        
        // Apply camera shake offset to final position
        if (enableCameraShake)
        {
            finalPosition += shakeOffset;
        }
        
        transform.position = finalPosition;
    }
    
    /// <summary>
    /// Update camera following logic for physics-based movement (uses Time.fixedDeltaTime).
    /// </summary>
    private void UpdateCameraFollowPhysics()
    {
        // Calculate target position
        targetPosition = CalculateTargetPosition();
        
        // Apply look-ahead if enabled
        if (useLookAhead)
        {
            ApplyLookAhead();
        }
        
        // Apply boundaries if enabled
        if (useBoundaries)
        {
            targetPosition = ApplyBoundaries(targetPosition);
        }
        
        // Move camera towards target using fixed timestep
        Vector3 finalPosition;
        if (smoothFollow)
        {
            // Use smooth damping for natural movement with fixed timestep
            finalPosition = Vector3.SmoothDamp(
                transform.position, 
                targetPosition, 
                ref velocity, 
                smoothTime, 
                maxFollowSpeed, 
                Time.fixedDeltaTime
            );
        }
        else
        {
            // Use linear interpolation for consistent speed with fixed timestep
            finalPosition = Vector3.MoveTowards(
                transform.position, 
                targetPosition, 
                followSpeed * Time.fixedDeltaTime
            );
        }
        
        // Apply camera shake offset to final position
        if (enableCameraShake)
        {
            finalPosition += shakeOffset;
        }
        
        transform.position = finalPosition;
    }
    
    /// <summary>
    /// Calculate the target position based on the player's position and offset.
    /// </summary>
    /// <returns>The calculated target position.</returns>
    private Vector3 CalculateTargetPosition()
    {
        if (!target)
            return transform.position;
            
        Vector3 playerPosition = target.transform.position;
        return playerPosition + offset + lookAheadOffset;
    }
    
    /// <summary>
    /// Apply look-ahead offset based on player movement.
    /// </summary>
    private void ApplyLookAhead()
    {
        if (!target)
            return;
            
        // Get player controller to check movement
        var player = target.GetComponent<Player>();
        if (player != null)
        {
            Vector3 inputDirection = player.GetInputDirection();
            Vector3 desiredLookAhead = inputDirection * lookAheadDistance;
            
            // Smooth the look-ahead offset (use appropriate deltaTime based on physics movement)
            float deltaTime = usePhysicsMovement ? Time.fixedDeltaTime : Time.deltaTime;
            lookAheadOffset = Vector3.Lerp(lookAheadOffset, desiredLookAhead, lookAheadSpeed * deltaTime);
        }
    }
    
    /// <summary>
    /// Apply boundary constraints to the target position.
    /// </summary>
    /// <param name="position">The position to constrain.</param>
    /// <returns>The constrained position.</returns>
    private Vector3 ApplyBoundaries(Vector3 position)
    {
        return new Vector3(
            Mathf.Clamp(position.x, minBounds.x, maxBounds.x),
            Mathf.Clamp(position.y, minBounds.y, maxBounds.y),
            Mathf.Clamp(position.z, minBounds.z, maxBounds.z)
        );
    }
    
    /// <summary>
    /// Automatically find the player entity to follow.
    /// </summary>
    private void FindPlayerTarget()
    {
        // Try to find by name first
        var playerEntity = Scene.FindEntityByName("Player");
        if (playerEntity)
        {
            target = playerEntity;
            return;
        }
        
        // Try to find entity with Player component
        // Note: This is a simplified approach - in a real implementation you might want
        // to use a more sophisticated entity finding system
        Log.Warning($"TopDownCamera: Could not auto-find player. Please assign target manually.");
    }
    
    /// <summary>
    /// Set the target entity to follow.
    /// </summary>
    /// <param name="newTarget">The new target entity.</param>
    public void SetTarget(Entity newTarget)
    {
        target = newTarget;
        if (target)
        {
            // Reset velocity for smooth transition
            velocity = Vector3.zero;
        }
    }
    
    /// <summary>
    /// Instantly snap the camera to the target position (no smooth movement).
    /// </summary>
    public void SnapToTarget()
    {
        if (!target)
            return;
            
        targetPosition = CalculateTargetPosition();
        
        if (useBoundaries)
        {
            targetPosition = ApplyBoundaries(targetPosition);
        }
        
        transform.position = targetPosition;
        velocity = Vector3.zero;
        lookAheadOffset = Vector3.zero;
        shakeOffset = Vector3.zero;
        shakeTimer = 0.0f;
        currentShakeIntensity = 0.0f;
    }
    
    /// <summary>
    /// Trigger camera shake with specified intensity.
    /// </summary>
    /// <param name="intensityMultiplier">Multiplier for shake intensity (e.g., damage amount).</param>
    public void TriggerShake(float intensityMultiplier = 1.0f)
    {
        if (!enableCameraShake)
            return;
            
        // Scale damage to reasonable shake intensity (divide by 10 to normalize typical damage values)
        // This means damage of 10 = 1.0x base intensity, damage of 50 = 5.0x base intensity
        float scaledMultiplier = intensityMultiplier / 10.0f;
        
        // Calculate shake intensity based on damage (scaled by intensity multiplier)
        float calculatedIntensity = shakeIntensity * scaledMultiplier;
        
        // Cap the intensity to prevent excessive shaking
        calculatedIntensity = Mathf.Min(calculatedIntensity, maxShakeIntensity);
        
        // If already shaking, add to existing intensity (stacking)
        currentShakeIntensity = Mathf.Max(currentShakeIntensity, calculatedIntensity);
        
        // Reset timer to full duration
        shakeTimer = shakeDuration;
    }
    
    /// <summary>
    /// Update camera shake effect.
    /// </summary>
    private void UpdateCameraShake()
    {
        if (shakeTimer > 0.0f)
        {
            // Calculate shake decay (exponential decay)
            float decayFactor = Mathf.Pow(shakeTimer / shakeDuration, shakeDecay);
            float currentIntensity = currentShakeIntensity * decayFactor;
            
            // Generate random shake offset (perlin noise-like behavior using random)
            float deltaTime = usePhysicsMovement ? Time.fixedDeltaTime : Time.deltaTime;
            float randomX = (float)(shakeRandom.NextDouble() * 2.0 - 1.0) * currentIntensity;
            float randomY = (float)(shakeRandom.NextDouble() * 2.0 - 1.0) * currentIntensity;
            float randomZ = (float)(shakeRandom.NextDouble() * 2.0 - 1.0) * currentIntensity;
            
            // Smooth the shake offset for more natural movement
            Vector3 targetShakeOffset = new Vector3(randomX, randomY, randomZ);
            shakeOffset = Vector3.Lerp(shakeOffset, targetShakeOffset, 15.0f * deltaTime);
            
            // Update timer
            shakeTimer -= deltaTime;
            
            if (shakeTimer <= 0.0f)
            {
                // Shake finished, reset
                shakeTimer = 0.0f;
                currentShakeIntensity = 0.0f;
                shakeOffset = Vector3.zero;
            }
        }
        else
        {
            // No shake active, ensure offset is zero
            shakeOffset = Vector3.zero;
        }
    }
}
