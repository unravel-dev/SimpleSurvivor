using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Component that makes an entity orbit around a center point (usually the player).
/// Handles only the orbital movement and visual spinning logic.
/// Lifetime management should be handled by other components (e.g., Projectile, AutoDestroyComponent).
/// </summary>
[ScriptSourceFile]
public class OrbitalMovementComponent : ScriptComponent
{
    [Tooltip("Entity to orbit around (usually the player)")]
    public Entity centerEntity;

    [Tooltip("Base radius of the orbit (used when ping-pong is disabled)")]
    public float orbitRadius = 4.0f;

    [Tooltip("Enable radius ping-pong effect (oscillate between min and max radius)")]
    public bool enableRadiusPingPong = true;

    [Tooltip("Minimum orbit radius in meters")]
    public float minRadius = 1.0f;

    [Tooltip("Maximum orbit radius in meters")]
    public float maxRadius = 6.0f;

    [Tooltip("Speed of radius ping-pong oscillation (cycles per second)")]
    public float radiusPingPongSpeed = 1.0f;

    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 360.0f;

    [Tooltip("Current angle in degrees")]
    public float currentAngle = 0.0f;

    [Tooltip("Height offset from center entity")]
    public float heightOffset = 1.0f;

    [Tooltip("Should the blade rotate/spin visually?")]
    public bool visualSpin = true;

    [Tooltip("Visual spin speed (degrees per second)")]
    public float visualSpinSpeed = 720.0f;

    private TransformComponent transformComponent;
    private TransformComponent centerTransform;
    private float visualRotation = 0.0f;
    private float radiusPingPongTime = 0.0f;

    public override void OnCreate()
    {
        transformComponent = owner.GetComponent<TransformComponent>();
    }

    public override void OnStart()
    {
        if (transformComponent == null)
        {
            Log.Error($"OrbitalProjectile on {owner.name}: No TransformComponent found!");
            return;
        }

        if (!centerEntity)
        {
            Log.Error($"OrbitalProjectile on {owner.name}: No center entity assigned!");
            return;
        }

        centerTransform = centerEntity.GetComponent<TransformComponent>();
        if (centerTransform == null)
        {
            Log.Error($"OrbitalProjectile on {owner.name}: Center entity has no TransformComponent!");
        }
    }

    public override void OnUpdate()
    {
        if (transformComponent == null || centerTransform == null || !centerEntity)
        {
            // Center entity destroyed or invalid, destroy this orbital
            Scene.DestroyEntity(owner);
            return;
        }

        // Update orbital angle
        currentAngle += rotationSpeed * Time.deltaTime;
        if (currentAngle >= 360.0f)
        {
            currentAngle -= 360.0f;
        }

        // Calculate current radius with ping-pong effect
        float currentRadius = orbitRadius;
        if (enableRadiusPingPong)
        {
            // Update ping-pong time
            radiusPingPongTime += Time.deltaTime * radiusPingPongSpeed;
            
            // Use PingPong to oscillate between 0 and 1
            float pingPongValue = Mathf.PingPong(radiusPingPongTime, 1.0f);
            
            // Lerp between min and max radius
            currentRadius = Mathf.Lerp(minRadius, maxRadius, pingPongValue);
        }

        // Calculate new position
        float angleRad = currentAngle * Mathf.Deg2Rad;
        Vector3 centerPosition = centerTransform.position;
        
        Vector3 offset = new Vector3(
            Mathf.Cos(angleRad) * currentRadius,
            heightOffset,
            Mathf.Sin(angleRad) * currentRadius
        );

        transformComponent.position = centerPosition + offset;

        // Visual spinning
        if (visualSpin)
        {
            visualRotation += visualSpinSpeed * Time.deltaTime;
            if (visualRotation >= 360.0f)
            {
                visualRotation -= 360.0f;
            }

            // Rotate the blade around its own axis for visual effect
            Quaternion spinRotation = Quaternion.AngleAxis(visualRotation, Vector3.up);
            
            // Also tilt it slightly for better visuals
            Quaternion tiltRotation = Quaternion.AngleAxis(45.0f, Vector3.right);
            
            transformComponent.rotation = spinRotation * tiltRotation;
        }
    }
}

