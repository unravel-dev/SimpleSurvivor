using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// MagnetLoot component that represents a collectible magnet item.
/// When collected, temporarily increases the player's pickup range significantly.
/// </summary>
[ScriptSourceFile]
public class MagnetLoot : ScriptComponent
{
    
    [Tooltip("Duration of the magnet effect in seconds")]
    public float duration = 0.1f;
    
    [Tooltip("Base speed at which magnet moves towards player when attracted")]
    public float baseAttractionSpeed = 15.0f;
    
    [Tooltip("Speed multiplier relative to player speed")]
    public float playerSpeedMultiplier = 1.5f;
    
    [Tooltip("Minimum attraction speed regardless of player speed")]
    public float minimumAttractionSpeed = 12.0f;
    
    [Tooltip("Maximum attraction speed cap")]
    public float maximumAttractionSpeed = 50.0f;
    
    [Tooltip("Distance at which magnet is considered collected")]
    public float collectDistance = 1.0f;
    
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
            Log.Error($"MagnetLoot on {owner.name}: TransformComponent not found!");
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
    /// Called every frame to update magnet behavior.
    /// </summary>
    public override void OnUpdate()
    {
        if (transformComponent == null)
            return;
        
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
    /// Update attraction movement towards the player.
    /// </summary>
    private void UpdateAttraction()
    {
        if (!targetPlayer || transformComponent == null)
        {
            StopAttraction();
            return;
        }
        
        Vector3 magnetPosition = transformComponent.position;
        Vector3 playerPosition = targetPlayer.transform.position + Vector3.up * 1.0f;
        
        // Check if we're close enough to collect
        float distance = Vector3.Distance(magnetPosition, playerPosition);
        if (distance <= collectDistance)
        {
            CollectMagnet();
            return;
        }
        
        // Calculate dynamic attraction speed based on player speed
        float dynamicSpeed = CalculateDynamicAttractionSpeed();
        
        // Move towards player using MoveTowards
        float moveDistance = dynamicSpeed * Time.deltaTime;
        transformComponent.position = Vector3.MoveTowards(magnetPosition, playerPosition, moveDistance);
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
        float playerSpeed = new Vector3(playerVelocity.x, 0, playerVelocity.z).magnitude;
        
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
    /// Start attracting this magnet to the specified player.
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
    /// Stop attracting this magnet to the player.
    /// </summary>
    public void StopAttraction()
    {
        isBeingAttracted = false;
        targetPlayer = Entity.Invalid;
        
        // Reset to floating position
        if (enableFloating && transformComponent != null)
        {
            initialPosition = transformComponent.position;
        }
    }
    
    /// <summary>
    /// Collect this magnet and apply the pickup range boost to the player.
    /// </summary>
    private void CollectMagnet()
    {
        if (!targetPlayer)
            return;
            

        UpgradeSystem.ApplyMagnetEffect(99999, duration);


        StopAttraction();
        // Destroy the magnet
        Scene.DestroyEntity(owner);
    }
    
    /// <summary>
    /// Check if this magnet is currently being attracted to a player.
    /// </summary>
    /// <returns>True if being attracted.</returns>
    public bool IsBeingAttracted()
    {
        return isBeingAttracted;
    }
}

