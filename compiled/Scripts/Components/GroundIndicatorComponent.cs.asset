using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Component that creates a ground indicator (warning marker) at a specific position.
/// Useful for telegraphing incoming attacks like meteor strikes.
/// </summary>
[ScriptSourceFile]
public class GroundIndicatorComponent : ScriptComponent
{

    [Tooltip("How long the indicator lasts (in seconds)")]
    public float lifetime = 1.0f;

    [Tooltip("Radius of the indicator circle")]
    public float radius = 5.0f;

    [Tooltip("Should the indicator pulse/scale?")]
    public bool enablePulse = true;

    [Tooltip("Pulse speed (cycles per second)")]
    public float pulseSpeed = 2.0f;

    [Tooltip("Minimum scale multiplier for pulse")]
    public float minScale = 0.8f;

    [Tooltip("Maximum scale multiplier for pulse")]
    public float maxScale = 1.2f;

    private TransformComponent transformComponent;
    private float elapsedTime = 0.0f;
    private Vector3 baseScale;

    public override void OnCreate()
    {
        transformComponent = owner.GetComponent<TransformComponent>();
    }

    public override void OnStart()
    {
        if (transformComponent == null)
        {
            Log.Error($"GroundIndicatorComponent on {owner.name}: No TransformComponent found!");
            return;
        }

        // Store base scale
        baseScale = transformComponent.scale;
    }

    public override void OnUpdate()
    {
        if (transformComponent == null)
        {
            return;
        }

        // Update lifetime
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= lifetime)
        {
            Scene.DestroyEntity(owner);
            return;
        }

        // Pulse effect
        if (enablePulse)
        {
            float pulseTime = elapsedTime * pulseSpeed;
            float pulseValue = Mathf.PingPong(pulseTime, 1.0f);
            float scaleMultiplier = Mathf.Lerp(minScale, maxScale, pulseValue);
            
            transformComponent.scale = baseScale * scaleMultiplier;
        }

        // Optional: Fade out near end of lifetime
        // Could add material opacity changes here if needed
    }

    /// <summary>
    /// Get the progress of the indicator (0-1, where 1 = about to disappear).
    /// </summary>
    public float GetLifetimeProgress()
    {
        return Mathf.Clamp01(elapsedTime / lifetime);
    }

    /// <summary>
    /// Get remaining time until indicator disappears.
    /// </summary>
    public float GetRemainingTime()
    {
        return Mathf.Max(0.0f, lifetime - elapsedTime);
    }
}

