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
    
    // Component references
    protected PhysicsComponent physicsComponent;
    
    // State
    protected float timeAlive = 0.0f;
    protected bool isBeingAttracted = false;
    protected Entity targetPlayer;
    protected Vector3 initialPosition;
    protected float floatOffset;
    
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
        initialPosition = transform.position;
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
    }
    
    /// <summary>
    /// Update attraction movement towards the player - dynamic speed based on player speed.
    /// </summary>
    protected void UpdateAttraction()
    {
        if (!targetPlayer)
        {
            StopAttraction();
            return;
        }
        
        Vector3 lootPosition = transform.position;
        Vector3 playerPosition = targetPlayer.transform.position + Vector3.up * 1.0f;
        
        // Check if we're close enough to collect
        float distance = Vector3.Distance(lootPosition, playerPosition);
        if (distance <= collectDistance)
        {
            CollectLoot();
            return;
        }
        
        // Calculate dynamic attraction speed based on player speed
        float dynamicSpeed = CalculateDynamicAttractionSpeed();
        
        // Move towards player using MoveTowards
        float moveDistance = dynamicSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(lootPosition, playerPosition, moveDistance);
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
    /// Start attracting this loot to the specified player.
    /// </summary>
    /// <param name="player">Player entity to attract to.</param>
    public void StartAttraction(Entity player)
    {
        if (!player)
            return;
            
        targetPlayer = player;
        isBeingAttracted = true;
    }
    
    /// <summary>
    /// Stop attracting this loot to the player.
    /// </summary>
    public void StopAttraction()
    {
        isBeingAttracted = false;
        targetPlayer = Entity.Invalid;
        
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

