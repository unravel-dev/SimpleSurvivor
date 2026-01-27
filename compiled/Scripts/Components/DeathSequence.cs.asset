using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Handles death visual sequence for entities (sinking, fading, scaling).
/// Listens to EnemyStateMachine state changes and handles death visuals.
/// Separates death visual effects from animation (which is handled by EnemyAnimationController).
/// </summary>
[ScriptSourceFile]
public class DeathSequence : ScriptComponent
{
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
    
    [Tooltip("Delay before starting sink effect (allows death animation to play first)")]
    public float sinkDelay = 0.0f;
    
    // Component references
    private Health healthComponent;
    private ModelComponent meshRenderer;
    private EnemyStateMachine stateMachine;
    private PhysicsComponent physicsComponent;
    
    // Death sequence state
    private bool isPlayingDeathSequence = false;
    private float deathTimer = 0.0f;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 startScale;
    private Vector3 targetScale;
    /// <summary>
    /// Called when the script is created.
    /// </summary>
    public override void OnCreate()
    {
        healthComponent = owner.GetComponent<Health>();
        meshRenderer = owner.GetComponent<ModelComponent>();
        stateMachine = owner.GetComponent<EnemyStateMachine>();
        physicsComponent = owner.GetComponent<PhysicsComponent>();

        if (stateMachine == null)
        {
            Log.Warning($"DeathSequence on {owner.name}: EnemyStateMachine component not found! Death sequence will not work.");
            return;
        }
    }
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        startScale = transform.localScale;
        
        // Subscribe to state machine changes
        if (stateMachine != null)
        {
            stateMachine.OnStateChanged += OnStateChanged;
        }
        
        // Also subscribe to health death event as a fallback (in case state machine isn't used)
        if (healthComponent != null)
        {
            healthComponent.OnDeath += OnDeathFallback;
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
        
        // Wait for sink delay before starting sink effect
        if (deathTimer < sinkDelay)
            return;
        
        float sinkStartTime = sinkDelay;
        float sinkEndTime = sinkStartTime + sinkDuration;
        
        // Sink phase
        if (deathTimer <= sinkEndTime)
        {
            float sinkProgress = (deathTimer - sinkStartTime) / sinkDuration;
            sinkProgress = Mathf.Clamp01(sinkProgress);

            // Ease out cubic for smooth sinking
            float easedProgress = 1.0f - Mathf.Pow(1.0f - sinkProgress, 3.0f);

            // Sink into ground
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, easedProgress);
            transform.position = newPosition;

            // Scale down if enabled
            if (scaleDown)
            {
                Vector3 newScale = Vector3.Lerp(startScale, targetScale, easedProgress);
                transform.localScale = newScale;
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
        // Sequence complete - set to Dead state and destroy entity
        else
        {
            // Set state to Dead before destroying
            if (stateMachine != null)
            {
                stateMachine.SetState(EnemyState.Dead);
            }
            
            Scene.DestroyEntity(owner);
        }
    }
    
    /// <summary>
    /// Handle state machine state changes.
    /// </summary>
    /// <param name="oldState">The previous state.</param>
    /// <param name="newState">The new state.</param>
    private void OnStateChanged(EnemyState oldState, EnemyState newState)
    {
        // Start death sequence when state changes to Dying
        if (newState == EnemyState.Dying && !isPlayingDeathSequence)
        {
            StartDeathSequence();
        }
    }
    
    /// <summary>
    /// Fallback: Called when health component fires death event (if state machine isn't used).
    /// </summary>
    private void OnDeathFallback()
    {
        // Only use this if state machine isn't available or didn't trigger
        if (stateMachine == null || !stateMachine.IsInState(EnemyState.Dying))
        {
            StartDeathSequence();
        }
    }
    
    /// <summary>
    /// Start the death visual sequence.
    /// </summary>
    private void StartDeathSequence()
    {
        if (isPlayingDeathSequence)
            return;

        // Disable physics collisions
        if (physicsComponent != null)
        {
            physicsComponent.useGravity = false;
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
        startPosition = transform.position;
        targetPosition = startPosition + new Vector3(0, -sinkDepth, 0);
            
        if (scaleDown)
        {
            startScale = transform.localScale;
            targetScale = startScale * finalScale;
        }
    }
    
    /// <summary>
    /// Clean up event subscriptions.
    /// </summary>
    public override void OnDestroy()
    {
        if (stateMachine != null)
        {
            stateMachine.OnStateChanged -= OnStateChanged;
        }
        
        if (healthComponent != null)
        {
            healthComponent.OnDeath -= OnDeathFallback;
        }
    }
}

