using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Setup script for the DamageNumberSystem.
/// Add this to an entity in your scene to automatically initialize the damage number system.
/// </summary>
[ScriptSourceFile]
public class DamageNumberSystemSetup : ScriptComponent
{
    //[Header("System Configuration")]
    [Tooltip("Default lifetime for damage numbers in seconds")]
    public float damageNumberLifetime = 1.5f;
    [Tooltip("Default floating speed for damage numbers")]
    public float damageNumberFloatSpeed = 2.0f;
    [Tooltip("Height offset above damaged entities")]
    public float spawnHeightOffset = 2.0f;
    [Tooltip("Random position spread radius")]
    public float spawnSpreadRadius = 0.5f;
    [Tooltip("Maximum number of active damage numbers")]
    public int maxActiveDamageNumbers = 50;
    [Tooltip("Enable debug logging")]
    public bool enableDebugLogging = false;
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        // Create damage number system entity
        Entity damageSystemEntity = Scene.CreateEntity("DamageNumberSystem");
        
        if (damageSystemEntity)
        {
            // Add the DamageNumberSystem component
            var damageSystem = damageSystemEntity.AddComponent<DamageNumberSystem>();
            
            if (damageSystem != null)
            {
                // Configure the system with our settings
                damageSystem.defaultLifetime = damageNumberLifetime;
                damageSystem.defaultFloatSpeed = damageNumberFloatSpeed;
                damageSystem.spawnHeightOffset = spawnHeightOffset;
                damageSystem.spawnSpreadRadius = spawnSpreadRadius;
                damageSystem.maxActiveDamageNumbers = maxActiveDamageNumbers;
                damageSystem.debugDamageSystem = enableDebugLogging;
                
                Log.Info("DamageNumberSystemSetup: Successfully created and configured DamageNumberSystem");
            }
            else
            {
                Log.Error("DamageNumberSystemSetup: Failed to add DamageNumberSystem component");
            }
        }
        else
        {
            Log.Error("DamageNumberSystemSetup: Failed to create DamageNumberSystem entity");
        }
        
        // Destroy this setup entity as it's no longer needed
        Scene.DestroyEntity(owner);
    }
}
