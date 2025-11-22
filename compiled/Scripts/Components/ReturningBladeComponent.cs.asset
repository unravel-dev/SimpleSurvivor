using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Component that makes a blade return to its source after its orbit duration.
/// The blade moves directly toward the source entity and can pierce through enemies on the return path.
/// </summary>
[ScriptSourceFile]
public class ReturningBladeComponent : ScriptComponent
{
    [Tooltip("Entity to return to (usually the player)")]
    public Entity targetEntity;

    [Tooltip("Speed at which blade returns")]
    public float returnSpeed = 15.0f;

    [Tooltip("Distance threshold to consider blade returned")]
    public float returnDistanceThreshold = 1.0f;

    [Tooltip("Pierce count for the return journey (high value to pierce through many enemies)")]
    public int returnPierceCount = 999;

    private TransformComponent transformComponent;
    private TransformComponent targetTransform;
    private Projectile projectileComponent;
    private bool isReturning = false;

    public override void OnStart()
    {
        transformComponent = owner.GetComponent<TransformComponent>();
        projectileComponent = owner.GetComponent<Projectile>();

        if (transformComponent == null)
        {
            Log.Warning($"ReturningBladeComponent on {owner.name}: No TransformComponent found!");
            return;
        }

        if (!targetEntity)
        {
            // Try to get target from DamageSourceComponent
            var damageSource = owner.GetComponent<DamageSourceComponent>();
            if (damageSource != null)
            {
                targetEntity = damageSource.GetAbilityEntity();
            }
            
            if (!targetEntity)
            {
                Log.Warning($"ReturningBladeComponent on {owner.name}: No target entity assigned!");
                return;
            }
        }

        targetTransform = targetEntity.GetComponent<TransformComponent>();
        if (targetTransform == null)
        {
            Log.Warning($"ReturningBladeComponent on {owner.name}: Target entity has no TransformComponent!");
            return;
        }
    }

    public override void OnUpdate()
    {
        if (transformComponent == null || targetTransform == null || !targetEntity)
            return;

        // Check if projectile lifetime is almost up (start returning)
        if (!isReturning && projectileComponent != null)
        {
            float remainingLifetime = projectileComponent.GetRemainingLifetime();
            if (remainingLifetime <= 0.5f) // Start returning when 0.5s left
            {
                StartReturning();
            }
        }

        if (isReturning)
        {
            // Move toward target
            Vector3 direction = (targetTransform.position - transformComponent.position).normalized;
            transformComponent.position += direction * returnSpeed * Time.deltaTime;

            // Check if we've reached the target
            float distance = Vector3.Distance(transformComponent.position, targetTransform.position);
            if (distance <= returnDistanceThreshold)
            {
                // Blade has returned - destroy it
                Scene.DestroyEntity(owner);
            }
        }
    }

    private void StartReturning()
    {
        if (isReturning)
            return;

        isReturning = true;

        // Disable orbital movement component (no longer orbiting)
        // We'll handle movement manually in OnUpdate
        owner.RemoveComponent<OrbitalMovementComponent>();

        // Add or update pierce component for return journey
        owner.RemoveComponent<PierceComponent>();
        var pierceComponent = owner.AddComponent<PierceComponent>();
        pierceComponent.pierceCount = returnPierceCount;

        // Extend lifetime to allow return journey
        if (projectileComponent != null)
        {
            projectileComponent.lifetime = 10.0f; // Give plenty of time to return
        }
    }
}

