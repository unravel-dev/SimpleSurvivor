using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Maps enemy states to animation clips and handles animation playback.
/// Listens to EnemyStateMachine state changes and plays appropriate animations.
/// Separates animation logic from game logic.
/// </summary>
[ScriptSourceFile]
public class EnemyAnimationController : ScriptComponent
{
    [System.Serializable]
    public class StateAnimationMapping
    {
        [Tooltip("The state this animation is for")]
        public EnemyState state;
        
        [Tooltip("Animation clip to play for this state")]
        public AnimationClip animation;
        
        [Tooltip("Blend time when transitioning to this animation (seconds)")]
        public float blendTime = 0.2f;
        
        [Tooltip("Whether this animation should loop")]
        public bool loop = true;
        
        [Tooltip("Whether this animation can be interrupted")]
        public bool canInterrupt = true;
    }
    
    [Tooltip("Mappings from enemy states to animation clips")]
    public List<StateAnimationMapping> stateAnimations = new List<StateAnimationMapping>();
    
    [Tooltip("Default blend time for animations that don't specify one")]
    public float defaultBlendTime = 0.2f;
    
    // Component references
    private EnemyStateMachine stateMachine;
    private AnimationComponent animationComponent;
    
    // Current animation state
    private EnemyState currentAnimationState = EnemyState.Idle;
    
    /// <summary>
    /// Called when the script is created.
    /// </summary>
    public override void OnCreate()
    {
        stateMachine = owner.GetComponent<EnemyStateMachine>();
        animationComponent = owner.GetComponent<AnimationComponent>();
        
        if (stateMachine == null)
        {
            Log.Warning($"EnemyAnimationController on {owner.name}: EnemyStateMachine component not found! Animations will not work.");
            return;
        }
        
        if (animationComponent == null)
        {
            Log.Warning($"EnemyAnimationController on {owner.name}: AnimationComponent not found! Animations will not play.");
            return;
        }
    }
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        if (stateMachine == null)
            return;
        
        // Subscribe to state changes
        stateMachine.OnStateChanged += OnStateChanged;
        
        // Play initial animation
        PlayAnimationForState(stateMachine.CurrentState);
    }
    
    /// <summary>
    /// Called when the script is destroyed.
    /// </summary>
    public override void OnDestroy()
    {
        if (stateMachine != null)
        {
            stateMachine.OnStateChanged -= OnStateChanged;
        }
    }
    
    /// <summary>
    /// Handle state change events from the state machine.
    /// </summary>
    /// <param name="oldState">The previous state.</param>
    /// <param name="newState">The new state.</param>
    private void OnStateChanged(EnemyState oldState, EnemyState newState)
    {
        PlayAnimationForState(newState);
    }
    
    /// <summary>
    /// Play the animation for the given state.
    /// </summary>
    /// <param name="state">The state to play animation for.</param>
    private void PlayAnimationForState(EnemyState state)
    {
        if (animationComponent == null)
            return;
        
        // Find animation mapping for this state
        StateAnimationMapping mapping = FindMappingForState(state);
        
        if (mapping == null || mapping.animation == null)
        {
            // No animation defined for this state - that's okay, just skip it
            return;
        }
        
        // Check if we can interrupt current animation
        StateAnimationMapping currentMapping = FindMappingForState(currentAnimationState);
        if (currentMapping != null && !currentMapping.canInterrupt && state != currentAnimationState)
        {
            // Can't interrupt, skip this animation
            return;
        }
        
        // Play the animation
        animationComponent.Blend(mapping.animation, mapping.blendTime, mapping.loop, false);
        
        // Update current animation state
        currentAnimationState = state;
    }
    
    /// <summary>
    /// Find the animation mapping for a given state.
    /// </summary>
    /// <param name="state">The state to find mapping for.</param>
    /// <returns>The mapping, or null if not found.</returns>
    private StateAnimationMapping FindMappingForState(EnemyState state)
    {
        foreach (var mapping in stateAnimations)
        {
            if (mapping.state == state)
            {
                return mapping;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Manually play an animation for a state (useful for one-off animations).
    /// </summary>
    /// <param name="state">The state to play animation for.</param>
    /// <param name="force">If true, plays even if already playing this state's animation.</param>
    public void PlayStateAnimation(EnemyState state, bool force = false)
    {
        if (!force && currentAnimationState == state)
            return;
        
        PlayAnimationForState(state);
    }
}
