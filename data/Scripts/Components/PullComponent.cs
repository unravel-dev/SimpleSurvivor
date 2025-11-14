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

    private TransformComponent transformComponent;
    private float elapsedTime = 0.0f;

    public override void OnCreate()
    {
        transformComponent = owner.GetComponent<TransformComponent>();
        if (transformComponent == null)
        {
            Log.Error($"PullComponent on {owner.name}: No TransformComponent found!");
        }
    }

    public override void OnStart()
    {
        if (transformComponent == null)
        {
            Log.Error($"PullComponent on {owner.name}: Missing TransformComponent. Disabling pull effect.");
            return;
        }

        // If duration is 0, set to a very large value for "infinite" duration
        if (duration <= 0.0f)
        {
            duration = float.MaxValue;
        }
    }

    public override void OnUpdate()
    {
        if (transformComponent == null)
        {
            return;
        }

        // Update elapsed time
        elapsedTime += Time.deltaTime;

        // Check if duration has expired
        if (elapsedTime >= duration)
        {
            // Destroy this component or entity when duration expires
            Scene.DestroyEntity(owner);
            return;
        }

        // Find all enemies within pull radius
        Vector3 pullPosition = transformComponent.position;
        var nearbyEnemies = Physics.SphereOverlap(pullPosition, pullRadius, pullLayerMask);

        // Apply pull force to each enemy
        foreach (var enemyEntity in nearbyEnemies)
        {
            if (!enemyEntity)
            {
                continue;
            }

            // Get enemy transform and physics components
            var enemyTransform = enemyEntity.GetComponent<TransformComponent>();
            var enemyPhysics = enemyEntity.GetComponent<PhysicsComponent>();

            if (enemyTransform == null || enemyPhysics == null)
            {
                continue;
            }

            // Calculate direction from enemy to pull center
            Vector3 enemyPosition = enemyTransform.position;
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

    /// <summary>
    /// Get the remaining duration.
    /// </summary>
    /// <returns>Remaining duration in seconds</returns>
    public float GetRemainingDuration()
    {
        return Mathf.Max(0.0f, duration - elapsedTime);
    }

    /// <summary>
    /// Get the number of enemies currently in pull range.
    /// </summary>
    /// <returns>Number of enemies in pull range</returns>
    public int GetPulledEnemyCount()
    {
        if (transformComponent == null)
        {
            return 0;
        }

        Vector3 pullPosition = transformComponent.position;
        var nearbyEnemies = Physics.SphereOverlap(pullPosition, pullRadius, pullLayerMask);
        return nearbyEnemies.Length;
    }
}

