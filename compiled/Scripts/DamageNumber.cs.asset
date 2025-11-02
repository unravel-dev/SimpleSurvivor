using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// DamageNumber component that displays 3D damage text above entities.
/// Handles billboard behavior, floating animation, and automatic destruction.
/// </summary>
[ScriptSourceFile]
public class DamageNumber : ScriptComponent
{
    //[Header("Display Settings")]
    [Tooltip("Lifetime of the damage number in seconds")]
    public float lifetime = 1.5f;
    [Tooltip("Speed at which the number floats upward")]
    public float floatSpeed = 2.0f;
    [Tooltip("Horizontal drift speed (random direction)")]
    public float driftSpeed = 0.5f;
    [Tooltip("Scale animation speed")]
    public float scaleAnimationSpeed = 3.0f;
    [Tooltip("Initial scale multiplier")]
    public float initialScale = 1.5f;
    [Tooltip("Final scale multiplier")]
    public float finalScale = 0.2f;
    
    //[Header("Billboard Settings")]
    [Tooltip("Enable billboard behavior (always face camera)")]
    public bool enableBillboard = true;
    [Tooltip("Billboard update frequency (0 = every frame)")]
    public float billboardUpdateInterval = 0.1f;
    
    //[Header("Debug")]
    [Tooltip("Enable debug logging")]
    public bool debugDamageNumber = false;
    
    // Component references
    private TransformComponent transformComponent;
    private TextComponent textComponent;
    
    // State
    private float timeAlive = 0.0f;
    private Vector3 initialPosition;
    private Vector3 driftDirection;
    private Vector3 initialScale3D;
    private float lastBillboardUpdate = 0.0f;
    private Entity cameraEntity;
    
    /// <summary>
    /// Called when the script is created.
    /// </summary>
    public override void OnCreate()
    {
        transformComponent = owner.GetComponent<TransformComponent>();
        textComponent = owner.GetComponent<TextComponent>();
        
        if (transformComponent == null)
        {
            Log.Error($"DamageNumber on {owner.name}: TransformComponent not found!");
        }
        
        if (textComponent == null)
        {
            Log.Error($"DamageNumber on {owner.name}: TextComponent not found!");
        }
        
        if (debugDamageNumber)
        {
            Log.Info($"DamageNumber created on {owner.name}");
        }
    }
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        timeAlive = 0.0f;
        initialPosition = transformComponent.position;
        initialScale3D = transformComponent.scale;
        
        // Generate random drift direction
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);
        driftDirection = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle)).normalized;
        
        // Find camera for billboard behavior
        FindCamera();
        
        // Set initial scale
        transformComponent.scale = initialScale3D * initialScale;
        
        if (debugDamageNumber)
        {
            Log.Info($"DamageNumber started on {owner.name} - Lifetime: {lifetime}s");
        }
    }
    
    /// <summary>
    /// Called every frame to update damage number behavior.
    /// </summary>
    public override void OnUpdate()
    {
        if (transformComponent == null)
            return;
            
        // Update lifetime
        timeAlive += Time.deltaTime;
        
        // Check if should be destroyed
        if (timeAlive >= lifetime)
        {
            if (debugDamageNumber)
            {
                Log.Info($"DamageNumber {owner.name} expired after {timeAlive:F2} seconds");
            }
            
            Scene.DestroyEntity(owner);
            return;
        }
        
        // Update position (floating and drifting)
        UpdateMovement();
        
        // Update scale animation
        UpdateScale();
        
        // Update billboard rotation
        if (enableBillboard && Time.time - lastBillboardUpdate >= billboardUpdateInterval)
        {
            UpdateBillboard();
            lastBillboardUpdate = Time.time;
        }
        
        // Update text opacity based on lifetime
        UpdateOpacity();
    }
    
    /// <summary>
    /// Update floating and drifting movement.
    /// </summary>
    private void UpdateMovement()
    {
        float progress = timeAlive / lifetime;
        
        // Float upward
        float verticalOffset = floatSpeed * timeAlive;
        
        // Drift horizontally
        Vector3 horizontalOffset = driftDirection * driftSpeed * timeAlive;
        
        // Apply movement
        Vector3 newPosition = initialPosition + Vector3.up * verticalOffset + horizontalOffset;
        transformComponent.position = newPosition;
    }
    
    /// <summary>
    /// Update scale animation.
    /// </summary>
    private void UpdateScale()
    {
        float progress = timeAlive / lifetime;
        
        // Scale animation: start big, shrink to normal, then shrink more
        float scaleMultiplier;
        if (progress < 0.2f)
        {
            // Initial pop-in effect
            float popProgress = progress / 0.2f;
            scaleMultiplier = Mathf.Lerp(initialScale, 1.0f, popProgress);
        }
        else
        {
            // Gradual shrink
            float shrinkProgress = (progress - 0.2f) / 0.8f;
            scaleMultiplier = Mathf.Lerp(1.0f, finalScale, shrinkProgress);
        }
        
        transformComponent.scale = initialScale3D * scaleMultiplier;
    }
    
    /// <summary>
    /// Update billboard rotation to face camera (top-down optimized).
    /// </summary>
    private void UpdateBillboard()
    {
        if (!cameraEntity || transformComponent == null)
            return;
            
        Vector3 damageNumberPosition = transformComponent.position;
        Vector3 cameraPosition = cameraEntity.transform.position;
        
        // For top-down camera, we only need to rotate around Y axis
        // Calculate direction on XZ plane only
        Vector3 directionToCamera = cameraPosition - damageNumberPosition;
        directionToCamera.x = 0; // Flatten to XZ plane

        directionToCamera = directionToCamera.normalized;
        
        // Create rotation that faces the camera on Y axis only
        if (directionToCamera.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera, Vector3.up);
            transformComponent.rotation = targetRotation;
        }
    }

    /// <summary>
    /// Update text opacity based on lifetime progress.
    /// </summary>
    private void UpdateOpacity()
    {
        if (textComponent == null)
            return;

        float progress = timeAlive / lifetime;

        // Fade out
        float alpha = 1.0f - progress;

        // Apply alpha to text color
        textComponent.opacity = alpha;
        
        textComponent.outlineWidth = Mathf.Lerp(1.5f, 0.0f, progress);

    }
    
    /// <summary>
    /// Find the main camera entity for billboard behavior.
    /// </summary>
    private void FindCamera()
    {
        // Try to find camera by name first
        cameraEntity = Scene.FindEntityByName("Camera");
        
        if (!cameraEntity)
        {
            // Try to find entity with Camera component
            var cameraEntities = Scene.FindEntitiesWithComponent<CameraComponent>();
            if (cameraEntities != null && cameraEntities.Length > 0)
            {
                cameraEntity = cameraEntities[0];
            }
        }
        
        if (debugDamageNumber)
        {
            if (cameraEntity)
            {
                Log.Info($"DamageNumber: Found camera entity {cameraEntity.name}");
            }
            else
            {
                Log.Warning("DamageNumber: No camera entity found for billboard behavior");
            }
        }
    }
    
    /// <summary>
    /// Set the damage text to display.
    /// </summary>
    /// <param name="damageAmount">Damage amount to display.</param>
    /// <param name="damageType">Type of damage for color coding (optional).</param>
    public void SetDamageText(float damageAmount, string damageType = "normal")
    {
        if (textComponent == null)
            return;
            
        // Format damage number
        string damageText = Mathf.RoundToInt(damageAmount).ToString();
        textComponent.text = damageText;
        
        textComponent.alignment = Alignment.Center | Alignment.Middle;
        textComponent.fontSize = 6;
        // Set color based on damage type
        SetDamageColor(damageType);
        
        if (debugDamageNumber)
        {
            Log.Info($"DamageNumber {owner.name}: Set damage text to '{damageText}' (type: {damageType})");
        }
    }
    
    /// <summary>
    /// Set the color of the damage text based on damage type.
    /// </summary>
    /// <param name="damageType">Type of damage.</param>
    private void SetDamageColor(string damageType)
    {
        if (textComponent == null)
            return;
            
        // Color coding for different damage types
        switch (damageType.ToLower())
        {
            case "critical":
                // Red for critical damage
                textComponent.color = Color.red;
                textComponent.outlineColor = Color.red;
                initialScale = 3.5f;
                break;
            case "heal":
                // Green for healing
                textComponent.color = Color.green;
                break;
            case "normal":
            default:
                // White/yellow for normal damage
                textComponent.color = Color.red;
                textComponent.outlineColor = Color.red;
                break;
        }
    }
    
    /// <summary>
    /// Set custom lifetime for this damage number.
    /// </summary>
    /// <param name="newLifetime">New lifetime in seconds.</param>
    public void SetLifetime(float newLifetime)
    {
        lifetime = Mathf.Max(0.1f, newLifetime);
        
        if (debugDamageNumber)
        {
            Log.Info($"DamageNumber {owner.name}: Lifetime set to {lifetime:F2} seconds");
        }
    }
    
    /// <summary>
    /// Set the floating speed for this damage number.
    /// </summary>
    /// <param name="speed">New floating speed.</param>
    public void SetFloatSpeed(float speed)
    {
        floatSpeed = Mathf.Max(0f, speed);
        
        if (debugDamageNumber)
        {
            Log.Info($"DamageNumber {owner.name}: Float speed set to {floatSpeed:F2}");
        }
    }
    
    /// <summary>
    /// Get the remaining lifetime of this damage number.
    /// </summary>
    /// <returns>Remaining lifetime in seconds.</returns>
    public float GetRemainingLifetime()
    {
        return Mathf.Max(0f, lifetime - timeAlive);
    }
    
    /// <summary>
    /// Check if this damage number is about to expire.
    /// </summary>
    /// <returns>True if in the last 20% of lifetime.</returns>
    public bool IsAboutToExpire()
    {
        return (timeAlive / lifetime) > 0.8f;
    }
}
