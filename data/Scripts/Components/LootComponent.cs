using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Base component for collectible loot items.
/// Handles common attraction, floating animation, and collection logic.
/// </summary>
[ScriptSourceFile]
public abstract class LootComponent : ScriptComponent
{
    [Tooltip("Base speed at which loot moves towards player when attracted")]
    public float baseAttractionSpeed = 15.0f;
    
    [Tooltip("Speed multiplier relative to player speed (loot speed = player speed * multiplier)")]
    public float playerSpeedMultiplier = 1.5f;
    
    [Tooltip("Minimum attraction speed regardless of player speed")]
    public float minimumAttractionSpeed = 12.0f;
    
    [Tooltip("Maximum attraction speed cap")]
    public float maximumAttractionSpeed = 50.0f;
    
    [Tooltip("Distance at which loot is considered collected")]
    public float collectDistance = 1.0f;
    
    [Tooltip("Enable floating/bobbing animation")]
    public bool enableFloating = true;
    
    [Tooltip("Floating animation speed")]
    public float floatSpeed = 2.0f;
    
    [Tooltip("Floating animation amplitude")]
    public float floatAmplitude = 0.3f;
    
    [Tooltip("Position offset for the loot")]
    public Vector3 positionOffset = Vector3.zero;
    
    [Tooltip("Enable rotation around Y axis")]
    public bool enableRotation = false;
    
    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 90.0f;
    
    [Tooltip("Enable back easing effect when moving towards player")]
    public bool enableBackEasing = true;
    
    [Tooltip("Duration of the back easing animation (in seconds)")]
    public float backEaseDuration = 0.4f;
    
    [Tooltip("Overshoot amount for back easing (higher = more bounce back)")]
    public float backEaseOvershoot = 1.0f;
    
    [Tooltip("Absolute distance to bounce back (in world units)")]
    public float bounceBackDistance = 1.5f;
    
    [Tooltip("Upward height during bounce-back phase (in world units)")]
    public float bounceBackHeight = 0.5f;
    
    // Component references
    protected PhysicsComponent physicsComponent;
    
    // State
    protected float timeAlive = 0.0f;
    protected bool isBeingAttracted = false;
    protected Entity targetPlayer;
    protected Vector3 initialPosition;
    protected float floatOffset;
    protected float attractionStartTime = -1f;
    protected Vector3 attractionStartPosition;
    protected Vector3 initialDirectionToTarget;
    
    /// <summary>
    /// Called when the script is created.
    /// </summary>
    public override void OnCreate()
    {
        physicsComponent = owner.GetComponent<PhysicsComponent>();
        
    }
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        timeAlive = 0.0f;
        isBeingAttracted = false;
        initialPosition = transform.position + positionOffset;
        floatOffset = Random.Range(0f, Mathf.PI * 2f); // Random phase for floating animation
    }
    
    /// <summary>
    /// Called every frame to update loot behavior.
    /// </summary>
    public override void OnUpdate()
    {
        // Handle attraction to player
        if (isBeingAttracted && targetPlayer)
        {
            UpdateAttraction();
        }
        else if (enableFloating)
        {
            UpdateFloating();
        }
        
        // Update rotation if enabled
        if (enableRotation)
        {
            UpdateRotation();
        }
    }
    
    /// <summary>
    /// Update attraction movement towards the player - uses back easing for smooth bounce effect.
    /// </summary>
    protected void UpdateAttraction()
    {
        if (!targetPlayer)
        {
            StopAttraction();
            return;
        }
        
        Vector3 lootPosition = transform.position;
        Vector3 playerPosition = targetPlayer.transform.position + Vector3.up * 2.0f;
        
        // Check if we're close enough to collect
        float distance = Vector3.Distance(lootPosition, playerPosition);
        if (distance <= collectDistance)
        {
            CollectLoot();
            return;
        }
   
        // Calculate dynamic attraction speed based on player speed
        float dynamicSpeed = CalculateDynamicAttractionSpeed();
        
        // Apply back easing if enabled
        if (enableBackEasing && attractionStartTime > 0)
        {
            // Calculate time-based progress (0 to 1 over the easing duration)
            float elapsedTime = Time.time - attractionStartTime;
            float progress = Mathf.Clamp01(elapsedTime / backEaseDuration);
            
            // Apply EaseInBack - creates bounce-back effect at the start, then eases forward
            // The eased value goes from negative (bounce-back) to positive (forward)
            float easedValue = EaseInBack(progress, backEaseOvershoot);

            Vector3 targetPosition;
            if (easedValue < 0.5f)
            {
                // Bounce-back phase: interpolate from bounce-back position to start position
                float bounceProgress = easedValue * 2.0f; // 0 -> 0, 0.5 -> 1
                
                // Calculate horizontal position (backwards movement)
                Vector3 horizontalBounceBack = attractionStartPosition - initialDirectionToTarget * bounceBackDistance;
                Vector3 horizontalPosition = Vector3.Lerp(horizontalBounceBack, attractionStartPosition, bounceProgress);
                
                // Calculate vertical position (upward arc that peaks at maximum bounce-back)
                // Use a parabolic curve: height peaks at bounceProgress = 0, returns to 0 at bounceProgress = 1
                float verticalOffset = bounceBackHeight * (1.0f - bounceProgress) * (1.0f - bounceProgress);
                
                targetPosition = horizontalPosition + Vector3.up * verticalOffset;
            }
            else
            {
                // Forward phase: interpolate from start position to target position
                float forwardProgress = (easedValue - 0.5f) * 2.0f; // 0.5 -> 0, 1 -> 1
                targetPosition = Vector3.Lerp(attractionStartPosition, playerPosition, forwardProgress);
            }
            
            // Move towards the eased target position
            float moveDistance = dynamicSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(lootPosition, targetPosition, moveDistance);
        }
        else
        {
            // Normal movement without easing
            float moveDistance = dynamicSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(lootPosition, playerPosition, moveDistance);
        }
    }
    
    /// <summary>
    /// EaseInBack easing function - creates a bounce-back effect at the start, then eases forward.
    /// </summary>
    /// <param name="t">Normalized progress (0 to 1).</param>
    /// <param name="overshoot">Overshoot amount (higher = more bounce back).</param>
    /// <returns>Eased value (negative at start for bounce-back, then positive as it eases forward).</returns>
    private float EaseInBack(float t, float overshoot)
    {
        // Clamp t to valid range
        t = Mathf.Clamp01(t);
        
        // EaseInBack formula: bounces back at the start, then accelerates forward
        // The overshoot parameter controls how much it bounces back
        float c1 = 1.70158f + overshoot;
        float c3 = c1 + 1.0f;
        
        // At t=0, this returns 0, but we want it to start negative for bounce-back
        // So we adjust the formula to ensure bounce-back at the start
        float result = c3 * t * t * t - c1 * t * t;
        
        // Ensure we get a visible bounce-back at the start
        // When t is very small, result should be negative
        // The formula already does this, but we can enhance it
        return result;
    }
    
    /// <summary>
    /// Calculate the dynamic attraction speed based on player's current speed.
    /// </summary>
    /// <returns>The calculated attraction speed.</returns>
    protected float CalculateDynamicAttractionSpeed()
    {
        if (!targetPlayer)
            return baseAttractionSpeed;
            
        // Get player component to access speed information
        var playerComponent = targetPlayer.GetComponent<Player>();
        if (playerComponent == null)
            return baseAttractionSpeed;
            
        // Get player's current velocity magnitude
        Vector3 playerVelocity = playerComponent.GetVelocity();
        float playerSpeed = new Vector3(playerVelocity.x, 0, playerVelocity.z).magnitude; // Ignore Y component for top-down
        
        // Calculate speed based on player's current speed
        float targetSpeed = playerSpeed * playerSpeedMultiplier;
        
        // Use base speed if player is not moving or calculated speed is too low
        if (playerSpeed < 0.1f || targetSpeed < baseAttractionSpeed)
        {
            targetSpeed = baseAttractionSpeed;
        }
        
        // Clamp to min/max bounds
        targetSpeed = Mathf.Clamp(targetSpeed, minimumAttractionSpeed, maximumAttractionSpeed);
        
        return targetSpeed;
    }
    
    /// <summary>
    /// Update floating animation when not being attracted.
    /// </summary>
    protected void UpdateFloating()
    {   
        // Calculate floating offset
        float floatY = Mathf.Sin((Time.time * floatSpeed) + floatOffset) * floatAmplitude;
        Vector3 targetPosition = initialPosition + Vector3.up * floatY;
        
        // Direct position update for floating
        transform.position = targetPosition;
    }
    
    /// <summary>
    /// Update rotation around Y axis if enabled.
    /// </summary>
    protected void UpdateRotation()
    {
        if (rotationSpeed == 0.0f)
            return;
        
        transform.RotateByEulerLocal(Vector3.up * rotationSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// Start attracting this loot to the specified player.
    /// </summary>
    /// <param name="player">Player entity to attract to.</param>
    public void StartAttraction(Entity player)
    {
        if (!player)
            return;
            
        targetPlayer = player;
        isBeingAttracted = true;
        attractionStartTime = Time.time;
        attractionStartPosition = transform.position;
        
        // Calculate initial direction to target for bounce-back
        Vector3 targetPos = player.transform.position + Vector3.up * 1.0f;
        initialDirectionToTarget = (targetPos - attractionStartPosition).normalized;
        
        // If direction is zero (already at target), use a default direction
        if (initialDirectionToTarget.magnitude < 0.001f)
        {
            initialDirectionToTarget = Vector3.forward;
        }
    }
    
    /// <summary>
    /// Stop attracting this loot to the player.
    /// </summary>
    public void StopAttraction()
    {
        isBeingAttracted = false;
        targetPlayer = Entity.Invalid;
        attractionStartTime = -1f;
        
        // Reset to floating position
        if (enableFloating)
        {
            initialPosition = transform.position;
        }
    }
    
    /// <summary>
    /// Collect this loot item. Override in derived classes to implement specific collection behavior.
    /// </summary>
    protected virtual void CollectLoot()
    {
        if (!targetPlayer)
            return;
            
        StopAttraction();
        // Destroy the loot
        Scene.DestroyEntity(owner);
    }
    
    /// <summary>
    /// Check if this loot is currently being attracted to a player.
    /// </summary>
    /// <returns>True if being attracted.</returns>
    public bool IsBeingAttracted()
    {
        return isBeingAttracted;
    }
    
    /// <summary>
    /// Get the player this loot is being attracted to.
    /// </summary>
    /// <returns>Target player entity.</returns>
    public Entity GetTargetPlayer()
    {
        return targetPlayer;
    }
    
    /// <summary>
    /// Get how long this loot has been alive.
    /// </summary>
    /// <returns>Time alive in seconds.</returns>
    public float GetTimeAlive()
    {
        return timeAlive;
    }
    
    /// <summary>
    /// Get the current dynamic attraction speed.
    /// </summary>
    /// <returns>Current attraction speed being used.</returns>
    public float GetCurrentAttractionSpeed()
    {
        return isBeingAttracted ? CalculateDynamicAttractionSpeed() : 0f;
    }
    
    /// <summary>
    /// Set the player speed multiplier for dynamic speed calculation.
    /// </summary>
    /// <param name="multiplier">New speed multiplier.</param>
    public void SetPlayerSpeedMultiplier(float multiplier)
    {
        playerSpeedMultiplier = Mathf.Max(1.0f, multiplier); // Ensure loot is always at least as fast as player
    }
}

