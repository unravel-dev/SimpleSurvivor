using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Component that spawns a prefab after a delay.
/// Useful for delayed meteor strikes, delayed explosions, etc.
/// </summary>
[ScriptSourceFile]
public class DelayedSpawnComponent : ScriptComponent
{
    [Tooltip("Prefab to spawn after delay")]
    public Prefab prefabToSpawn;

    [Tooltip("Delay before spawning (in seconds)")]
    public float spawnDelay = 1.0f;

    [Tooltip("Position where to spawn the prefab")]
    public Vector3 spawnPosition;

    [Tooltip("Direction the spawned entity should face")]
    public Vector3 spawnDirection = Vector3.down;

    [Tooltip("Should this component self-destruct after spawning?")]
    public bool destroyAfterSpawn = true;

    // Callback for configuring the spawned entity
    public System.Action<Entity> onSpawnCallback;

    private float elapsedTime = 0.0f;
    private bool hasSpawned = false;

    public override void OnUpdate()
    {
        if (hasSpawned)
        {
            return;
        }

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= spawnDelay)
        {
            SpawnPrefab();
        }
    }

    private void SpawnPrefab()
    {
        if (hasSpawned)
        {
            return;
        }

        hasSpawned = true;

        if (prefabToSpawn == null)
        {
            Log.Warning("DelayedSpawnComponent: No prefab assigned to spawn!");
            
            if (destroyAfterSpawn)
            {
                Scene.DestroyEntity(owner);
            }
            return;
        }

        // Instantiate the prefab
        Entity spawnedEntity = Scene.Instantiate(prefabToSpawn);
        if (!spawnedEntity)
        {
            Log.Error("DelayedSpawnComponent: Failed to instantiate prefab!");
            
            if (destroyAfterSpawn)
            {
                Scene.DestroyEntity(owner);
            }
            return;
        }

        // Set position and direction
        spawnedEntity.transform.position = spawnPosition;
        spawnedEntity.transform.forward = spawnDirection;

        // Call the callback if provided
        if (onSpawnCallback != null)
        {
            onSpawnCallback(spawnedEntity);
        }

        // Destroy this entity if requested
        if (destroyAfterSpawn)
        {
            Scene.DestroyEntity(owner);
        }
    }

    /// <summary>
    /// Get the progress of the delay (0-1, where 1 = about to spawn).
    /// </summary>
    public float GetDelayProgress()
    {
        return Mathf.Clamp01(elapsedTime / spawnDelay);
    }

    /// <summary>
    /// Get remaining time until spawn.
    /// </summary>
    public float GetRemainingTime()
    {
        return Mathf.Max(0.0f, spawnDelay - elapsedTime);
    }
}

