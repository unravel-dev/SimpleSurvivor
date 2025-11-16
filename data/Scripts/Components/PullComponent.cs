using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Component that pulls enemies toward itself over a period of time.
/// Applies physics forces to pull enemies within range toward the component's position.
/// </summary>
[ScriptSourceFile]
public class PullComponent : ScriptComponent
{
    [Tooltip("Radius within which enemies will be pulled")]
    public float pullRadius = 5.0f;

    [Tooltip("Strength of the pull force (higher = stronger pull)")]
    public float pullStrength = 10.0f;

    [Tooltip("How long the pull effect lasts (in seconds). Set to 0 for infinite duration.")]
    public float duration = 3.0f;

    [Tooltip("Layer mask for entities that can be pulled")]
    public LayerMask pullLayerMask = LayerMask.GetMask("Enemy");

    [Tooltip("Whether to increase pull strength based on distance (closer = stronger pull)")]
    public bool distanceBasedStrength = true;

    [Tooltip("Minimum pull strength multiplier when using distance-based strength")]
    public float minStrengthMultiplier = 0.3f;

    [Tooltip("Interval (in seconds) at which the callback is triggered with affected entities")]
    public float callbackInterval = 1.0f;

    /// <summary>
    /// Callback that is invoked periodically with the list of entities currently affected by the pull.
    /// Use this to apply effects like DoT, debuffs, etc. to pulled enemies.
    /// </summary>
    public Action<Entity[]> onAffectedEntities;

    private float elapsedTime = 0.0f;
    private float callbackTimer = 0.0f;

    public override void OnCreate()
    {

    }

    public override void OnStart()
    {
        // If duration is 0, set to a very large value for "infinite" duration
        if (duration <= 0.0f)
        {
            duration = float.MaxValue;
        }
    }

    public override void OnFixedUpdate()
    {

        UpdateImpl(Time.fixedDeltaTime);
    }

    public void UpdateImpl(float deltaTime)
    {


        // Update elapsed time
        elapsedTime += deltaTime;

        // Check if duration has expired
        if (elapsedTime >= duration)
        {
            var particleEmitter = owner.GetComponent<ParticleEmitterComponent>();
            if (particleEmitter != null)
            {
                particleEmitter.Stop();
            }
            // Destroy this component or entity when duration expires
            Scene.DestroyEntity(owner, 1.5f);
            return;
        }

        // Find all enemies within pull radius
        Vector3 pullPosition = owner.transform.position;
        var nearbyEnemies = Physics.SphereOverlap(pullPosition, pullRadius, pullLayerMask);

        // Update callback timer and trigger callback if interval has passed
        callbackTimer += deltaTime;
        if (callbackTimer >= callbackInterval && onAffectedEntities != null && nearbyEnemies.Length > 0)
        {
            onAffectedEntities.Invoke(nearbyEnemies);
            callbackTimer = 0.0f;
        }

        // Apply pull force to each enemy
        foreach (var enemyEntity in nearbyEnemies)
        {
            if (!enemyEntity)
            {
                continue;
            }

            // Get enemy transform and physics components
            var enemyPhysics = enemyEntity.GetComponent<PhysicsComponent>();

            if (enemyPhysics == null)
            {
                continue;
            }

            // Calculate direction from enemy to pull center
            Vector3 enemyPosition = enemyEntity.transform.position;
            Vector3 toCenter = pullPosition - enemyPosition;
            toCenter.y = 0.0f; // Flatten to XZ plane for top-down gameplay

            float distance = toCenter.magnitude;

            // Skip if enemy is at the center (avoid division by zero)
            if (distance < 0.01f)
            {
                continue;
            }

            // Calculate pull force
            Vector3 pullDirection = toCenter.normalized;
            float currentPullStrength = pullStrength;

            // Apply distance-based strength if enabled
            if (distanceBasedStrength)
            {
                // Closer enemies get stronger pull
                // Strength increases as distance decreases
                float normalizedDistance = Mathf.Clamp01(distance / pullRadius);
                float strengthMultiplier = Mathf.Lerp(1.0f, minStrengthMultiplier, normalizedDistance);
                currentPullStrength *= strengthMultiplier;
            }

            // Apply pull force to enemy
            Vector3 pullForce = pullDirection * currentPullStrength;
            enemyPhysics.ApplyForce(pullForce, ForceMode.Force);
        }
    }

    /// <summary>
    /// Get the current pull radius.
    /// </summary>
    /// <returns>Current pull radius</returns>
    public float GetPullRadius()
    {
        return pullRadius;
    }

    /// <summary>
    /// Set the pull radius.
    /// </summary>
    /// <param name="newRadius">New pull radius</param>
    public void SetPullRadius(float newRadius)
    {
        pullRadius = Mathf.Max(0.1f, newRadius);
    }

    /// <summary>
    /// Get the current pull strength.
    /// </summary>
    /// <returns>Current pull strength</returns>
    public float GetPullStrength()
    {
        return pullStrength;
    }

    /// <summary>
    /// Set the pull strength.
    /// </summary>
    /// <param name="newStrength">New pull strength</param>
    public void SetPullStrength(float newStrength)
    {
        pullStrength = Mathf.Max(0.0f, newStrength);
    }

}

