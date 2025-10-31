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
    [Tooltip("Speed at which orb moves towards player when attracted")]
    public float attractionSpeed = 15.0f;
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
    
    //[Header("Debug")]
    [Tooltip("Enable debug logging for experience orb events")]
    public bool debugOrb = false;
    
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
        
        if (debugOrb)
        {
            Log.Info($"ExperienceOrb created on {owner.name} with value {experienceValue}");
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
        
        if (debugOrb)
        {
            Log.Info($"ExperienceOrb started on {owner.name}");
        }
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
            if (debugOrb)
            {
                Log.Info($"ExperienceOrb {owner.name} expired after {timeAlive:F2} seconds");
            }
            
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
    /// Update attraction movement towards the player - simple MoveTowards.
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
        
        // Move towards player using MoveTowards
        float moveDistance = attractionSpeed * Time.deltaTime;
        transformComponent.position = Vector3.MoveTowards(orbPosition, playerPosition, moveDistance);
        
        if (debugOrb && Time.time % 1.0f < Time.deltaTime)
        {
            Log.Info($"ExperienceOrb {owner.name}: Moving towards player - Distance: {distance:F2}");
        }
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
        
        if (debugOrb)
        {
            Log.Info($"ExperienceOrb {owner.name}: Started attraction to {player.name}");
        }
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
        
        if (debugOrb)
        {
            Log.Info($"ExperienceOrb {owner.name}: Stopped attraction");
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
            
            if (debugOrb)
            {
                Log.Info($"ExperienceOrb {owner.name}: Collected by {targetPlayer.name} for {experienceValue} XP");
            }
        }
        else
        {
            if (debugOrb)
            {
                Log.Warning($"ExperienceOrb {owner.name}: Player {targetPlayer.name} has no Experience component");
            }
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
        
        if (debugOrb)
        {
            Log.Info($"ExperienceOrb {owner.name}: Experience value set to {experienceValue}");
        }
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
}
