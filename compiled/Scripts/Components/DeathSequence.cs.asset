using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Handles death animation sequence for entities.
/// Plays animation, sinks into ground, and destroys the entity.
/// Requires a Health component to subscribe to death events.
/// </summary>
[ScriptSourceFile]
public class DeathSequence : ScriptComponent
{
    [Tooltip("Animation clip to play when the entity dies")]
    public AnimationClip deathAnimation;
    
    [Tooltip("Duration of the sink effect in seconds")]
    public float sinkDuration = 0.8f;
    
    [Tooltip("How far to sink into the ground (negative Y offset)")]
    public float sinkDepth = 2.0f;
    
    [Tooltip("Fade out the entity during sink")]
    public bool fadeOut = true;
    
    [Tooltip("Scale down the entity during death")]
    public bool scaleDown = false;
    
    [Tooltip("Final scale multiplier if scaleDown is enabled")]
    public float finalScale = 0.5f;
    
    // Component references
    private Health healthComponent;
    private TransformComponent transformComponent;
    private ModelComponent meshRenderer;

    private AnimationComponent animationComponent;

    private PhysicsComponent physicsComponent;
    
    // Death sequence state
    private bool isPlayingDeathSequence = false;
    private float deathTimer = 0.0f;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 startScale;
    private Vector3 targetScale;
    
    private float animationDuration = 0.0f;
    /// <summary>
    /// Called when the script is created.
    /// </summary>
    public override void OnCreate()
    {
        healthComponent = owner.GetComponent<Health>();
        transformComponent = owner.GetComponent<TransformComponent>();
        meshRenderer = owner.GetComponent<ModelComponent>();
        animationComponent = owner.GetComponent<AnimationComponent>();
        physicsComponent = owner.GetComponent<PhysicsComponent>();

        if (healthComponent == null)
        {
            Log.Error($"DeathSequence on {owner.name}: Health component not found! This component requires a Health component.");
            return;
        }

        if (transformComponent == null)
        {
            Log.Error($"DeathSequence on {owner.name}: TransformComponent not found!");
            return;
        }

        if(deathAnimation != null)
        {
            animationDuration = deathAnimation.length;
        }

        // Subscribe to death event
        healthComponent.OnDeath += OnDeath;
    }
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        if (transformComponent != null)
        {
            startScale = transformComponent.localScale;
        }
    }
    
    /// <summary>
    /// Called every frame to update the death sequence.
    /// </summary>
    public override void OnUpdate()
    {
        if (!isPlayingDeathSequence)
            return;
        
        deathTimer += Time.deltaTime;
        
        float totalDuration = animationDuration + sinkDuration;

        // Animation phase (0 to animationDuration)
        if (deathTimer <= animationDuration)
        {
            // Play death animation here
            // For now, just wait for animation to complete
            // You can trigger animation state here if you have an AnimationComponent
            animationComponent.Blend(deathAnimation, 0.2f, false, true);
        }
        // Sink phase (animationDuration to totalDuration)
        else if (deathTimer <= totalDuration)
        {
            float sinkProgress = (deathTimer - animationDuration) / sinkDuration;
            sinkProgress = Mathf.Clamp01(sinkProgress);

            // Ease out cubic for smooth sinking
            float easedProgress = 1.0f - Mathf.Pow(1.0f - sinkProgress, 3.0f);

            // Sink into ground
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, easedProgress);
            transformComponent.position = newPosition;

            // Scale down if enabled
            if (scaleDown)
            {
                Vector3 newScale = Vector3.Lerp(startScale, targetScale, easedProgress);
                transformComponent.localScale = newScale;
            }

            // Fade out if enabled (requires material property manipulation)
            if (fadeOut && meshRenderer != null)
            {
                // Note: Fading requires material alpha manipulation
                // This is a placeholder - you may need to implement this based on your engine's API
                float alpha = 1.0f - easedProgress;
                var color = meshRenderer.GetColor();
                color.a = alpha;
                meshRenderer.SetColor(color);
            }
        }
        // Sequence complete - destroy entity
        else
        {
            Scene.DestroyEntity(owner);
        }
    }
    
    /// <summary>
    /// Called when the entity dies.
    /// </summary>
    private void OnDeath()
    {
        if (isPlayingDeathSequence)
            return;
        if(physicsComponent != null)
        {
            physicsComponent.excludeLayers = -1;
        }
        // Disable Health's auto-destroy since we're handling it
        if (healthComponent != null)
        {
            healthComponent.destroyOnDeath = false;
        }
        
        // Start death sequence
        isPlayingDeathSequence = true;
        deathTimer = 0.0f;
        
        // Store starting position and calculate target
        if (transformComponent != null)
        {
            startPosition = transformComponent.position;
            targetPosition = startPosition + new Vector3(0, -sinkDepth, 0);
            
            if (scaleDown)
            {
                startScale = transformComponent.localScale;
                targetScale = startScale * finalScale;
            }
        }
        
        // Log.Info($"DeathSequence: Starting death sequence for {owner.name}");
    }
    
    /// <summary>
    /// Clean up event subscriptions.
    /// </summary>
    public override void OnDestroy()
    {
        if (healthComponent != null)
        {
            healthComponent.OnDeath -= OnDeath;
        }
    }
}

