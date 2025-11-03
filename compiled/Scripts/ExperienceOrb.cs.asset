using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// ExperienceOrb component that represents a collectible experience point.
/// Handles attraction to player when in pickup range and provides experience value.
/// </summary>
[ScriptSourceFile]
public class ExperienceOrb : ScriptComponent
{
    //[Header("Experience Settings")]
    [Tooltip("Experience value this orb provides when collected")]
    public float experienceValue = 10.0f;
    [Tooltip("Lifetime of the orb in seconds (0 = infinite)")]
    public float lifetime = 30.0f;
    [Tooltip("Base speed at which orb moves towards player when attracted")]
    public float baseAttractionSpeed = 15.0f;
    [Tooltip("Speed multiplier relative to player speed (orb speed = player speed * multiplier)")]
    public float playerSpeedMultiplier = 1.5f;
    [Tooltip("Minimum attraction speed regardless of player speed")]
    public float minimumAttractionSpeed = 12.0f;
    [Tooltip("Maximum attraction speed cap")]
    public float maximumAttractionSpeed = 50.0f;
    [Tooltip("Acceleration when moving towards player")]
    public float attractionAcceleration = 30.0f;
    [Tooltip("Distance at which orb is considered collected")]
    public float collectDistance = 1.0f;
    
    //[Header("Visual Settings")]
    [Tooltip("Enable floating/bobbing animation")]
    public bool enableFloating = true;
    [Tooltip("Floating animation speed")]
    public float floatSpeed = 2.0f;
    [Tooltip("Floating animation amplitude")]
    public float floatAmplitude = 0.3f;
    
    // Component references
    private TransformComponent transformComponent;
    private PhysicsComponent physicsComponent;
    
    // State
    private float timeAlive = 0.0f;
    private bool isBeingAttracted = false;
    private Entity targetPlayer;
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 initialPosition;
    private float floatOffset;
    
    /// <summary>
    /// Called when the script is created.
    /// </summary>
    public override void OnCreate()
    {
        transformComponent = owner.GetComponent<TransformComponent>();
        physicsComponent = owner.GetComponent<PhysicsComponent>();
        
        if (transformComponent == null)
        {
            Log.Error($"ExperienceOrb on {owner.name}: TransformComponent not found!");
        }
    }
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        timeAlive = 0.0f;
        isBeingAttracted = false;
        initialPosition = transformComponent.position;
        floatOffset = Random.Range(0f, Mathf.PI * 2f); // Random phase for floating animation

    }
    
    /// <summary>
    /// Called every frame to update orb behavior.
    /// </summary>
    public override void OnUpdate()
    {
        if (transformComponent == null)
            return;
            
        // Update lifetime
        timeAlive += Time.deltaTime;
        
        // Check if orb should expire
        if (lifetime > 0 && timeAlive >= lifetime)
        {
            
            Scene.DestroyEntity(owner);
            return;
        }
        
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
    private void UpdateAttraction()
    {
        if (!targetPlayer || transformComponent == null)
        {
            StopAttraction();
            return;
        }
        
        Vector3 orbPosition = transformComponent.position;
        Vector3 playerPosition = targetPlayer.transform.position + Vector3.up * 1.0f;
        
        // Check if we're close enough to collect
        float distance = Vector3.Distance(orbPosition, playerPosition);
        if (distance <= collectDistance)
        {
            CollectOrb();
            return;
        }
        
        // Calculate dynamic attraction speed based on player speed
        float dynamicSpeed = CalculateDynamicAttractionSpeed();
        
        // Move towards player using MoveTowards
        float moveDistance = dynamicSpeed * Time.deltaTime;
        transformComponent.position = Vector3.MoveTowards(orbPosition, playerPosition, moveDistance);
    }
    
    /// <summary>
    /// Calculate the dynamic attraction speed based on player's current speed.
    /// </summary>
    /// <returns>The calculated attraction speed.</returns>
    private float CalculateDynamicAttractionSpeed()
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
    private void UpdateFloating()
    {
        if (transformComponent == null)
            return;
            
        // Calculate floating offset
        float floatY = Mathf.Sin((Time.time * floatSpeed) + floatOffset) * floatAmplitude;
        Vector3 targetPosition = initialPosition + Vector3.up * floatY;
        
        // Direct position update for floating
        transformComponent.position = targetPosition;
    }
    
    /// <summary>
    /// Start attracting this orb to the specified player.
    /// </summary>
    /// <param name="player">Player entity to attract to.</param>
    public void StartAttraction(Entity player)
    {
        if (!player)
            return;
            
        targetPlayer = player;
        isBeingAttracted = true;
        currentVelocity = Vector3.zero;
    }
    
    /// <summary>
    /// Stop attracting this orb to the player.
    /// </summary>
    public void StopAttraction()
    {
        isBeingAttracted = false;
        targetPlayer = Entity.Invalid;
        currentVelocity = Vector3.zero;
        
        // Reset to floating position
        if (enableFloating && transformComponent != null)
        {
            initialPosition = transformComponent.position;
        }
    }
    
    /// <summary>
    /// Collect this orb and give experience to the player.
    /// </summary>
    private void CollectOrb()
    {
        if (!targetPlayer)
            return;
            
        // Try to give experience to player
        var Experience = targetPlayer.GetComponent<Experience>();
        if (Experience != null)
        {
            Experience.CollectExperience(experienceValue, owner);
        }

        StopAttraction();
        // Destroy the orb
        Scene.DestroyEntity(owner);
    }
    
    /// <summary>
    /// Get the experience value of this orb.
    /// </summary>
    /// <returns>Experience value.</returns>
    public float GetExperienceValue()
    {
        return experienceValue;
    }
    
    /// <summary>
    /// Set the experience value of this orb.
    /// </summary>
    /// <param name="value">New experience value.</param>
    public void SetExperienceValue(float value)
    {
        experienceValue = Mathf.Max(0, value);
    }
    
    /// <summary>
    /// Check if this orb is currently being attracted to a player.
    /// </summary>
    /// <returns>True if being attracted.</returns>
    public bool IsBeingAttracted()
    {
        return isBeingAttracted;
    }
    
    /// <summary>
    /// Get the player this orb is being attracted to.
    /// </summary>
    /// <returns>Target player entity.</returns>
    public Entity GetTargetPlayer()
    {
        return targetPlayer;
    }
    
    /// <summary>
    /// Get how long this orb has been alive.
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
        playerSpeedMultiplier = Mathf.Max(1.0f, multiplier); // Ensure orbs are always at least as fast as player
    }
}
