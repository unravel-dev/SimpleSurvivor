using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Level director that provides spawning functionality for a top-down game.
/// Spawns entities around the player at random positions within configurable ranges.
/// </summary>
[ScriptSourceFile]
public class BasicLevelDirector : ScriptComponent
{
    //[Header("Player Settings")]
    [Tooltip("The player entity to spawn around (auto-found if not set)")]
    public Entity player;
    
    //[Header("Spawn Settings")]
    [Tooltip("Minimum distance from player to spawn entities")]
    public float minSpawnDistance = 5.0f;
    [Tooltip("Maximum distance from player to spawn entities")]
    public float maxSpawnDistance = 15.0f;
    [Tooltip("Y position offset for spawned entities (relative to player Y)")]
    public float spawnYOffset = 0.0f;
    [Tooltip("Enable debug logging for spawn operations")]
    public bool debugSpawning = false;
    
    //[Header("Enemy Spawning")]
    [Tooltip("Enemy prefab to spawn automatically")]
    public Prefab enemy;
    [Tooltip("Time in seconds between enemy spawns")]
    public float spawnRate = 2.0f;
    [Tooltip("Enable automatic enemy spawning")]
    public bool enableAutoSpawning = true;
    
    // Spawn timing
    private float timeSinceLastSpawn = 0.0f;
    
    /// <summary>
    /// Called when the script is created.
    /// </summary>
    public override void OnCreate()
    {
    }
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        // Auto-find player
        if (!player)
        {
            FindPlayer();
        }
    }
    
    /// <summary>
    /// Called every frame to handle automatic enemy spawning.
    /// </summary>
    public override void OnUpdate()
    {
        // Only spawn if auto-spawning is enabled and we have an enemy prefab
        if (!enableAutoSpawning || enemy == null)
            return;
            
        // Update timer
        timeSinceLastSpawn += Time.deltaTime;
            
        // Check if enough time has passed since last spawn
        if (timeSinceLastSpawn >= spawnRate)
        {
            // Try to spawn an enemy
            Entity spawnedEnemy = SpawnAroundPlayer(enemy);
            
            if (spawnedEnemy != Entity.Invalid)
            {
                // Reset timer only if spawn was successful
                timeSinceLastSpawn = 0.0f;
                
                if (debugSpawning)
                {
                    Log.Info($"BasicLevelDirector: Auto-spawned enemy (spawn rate: {spawnRate:F2}s)");
                }
            }
        }
    }
    
    /// <summary>
    /// Spawn a prefab around the player at a random position on the X,Z plane.
    /// The Y position is maintained from the player with an optional offset.
    /// </summary>
    /// <param name="prefab">The prefab to spawn.</param>
    /// <returns>The spawned entity, or invalid entity if spawn failed.</returns>
    public Entity SpawnAroundPlayer(Prefab prefab)
    {
        // Validate inputs
        if (prefab == null)
        {
            Log.Error("BasicLevelDirector: Cannot spawn - prefab is null");
            return Entity.Invalid;
        }
        
        if (!IsPlayerValid())
        {
            Log.Error("BasicLevelDirector: Cannot spawn - player is not valid");
            return Entity.Invalid;
        }
        
        // Get player position
        Vector3 playerPosition = GetPlayerPosition();
        
        // Generate random spawn position around player
        Vector3 spawnPosition = GenerateRandomSpawnPosition(playerPosition);
        
        // Spawn the entity
        Entity spawnedEntity = Scene.Instantiate(prefab);
        
        if (!spawnedEntity)
        {
            Log.Error("BasicLevelDirector: Failed to instantiate prefab");
            return Entity.Invalid;
        }
        
        // Set the spawned entity's position
        spawnedEntity.transform.position = spawnPosition;
        
        if (debugSpawning)
        {
            Log.Info($"BasicLevelDirector: Spawned entity at position {spawnPosition} (distance from player: {Vector3.Distance(playerPosition, spawnPosition):F2})");
        }
        
        return spawnedEntity;
    }
    
    /// <summary>
    /// Generate a random spawn position around the player on the X,Z plane.
    /// </summary>
    /// <param name="playerPosition">The player's current position.</param>
    /// <returns>A random position around the player.</returns>
    private Vector3 GenerateRandomSpawnPosition(Vector3 playerPosition)
    {
        // Generate random distance within the specified range
        float spawnDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
        
        // Generate random angle (0 to 2π radians)
        float spawnAngle = Random.Range(0.0f, 2.0f * Mathf.PI);
        
        // Calculate X and Z offsets using polar coordinates
        float xOffset = spawnDistance * Mathf.Cos(spawnAngle);
        float zOffset = spawnDistance * Mathf.Sin(spawnAngle);
        
        // Create spawn position (maintain player's Y + offset, vary X and Z)
        Vector3 spawnPosition = new Vector3(
            playerPosition.x + xOffset,
            playerPosition.y + spawnYOffset,
            playerPosition.z + zOffset
        );
        
        return spawnPosition;
    }
    
    /// <summary>
    /// Spawn multiple entities around the player.
    /// </summary>
    /// <param name="prefab">The prefab to spawn.</param>
    /// <param name="count">Number of entities to spawn.</param>
    /// <returns>Array of spawned entities.</returns>
    public Entity[] SpawnMultipleAroundPlayer(Prefab prefab, int count)
    {
        if (count <= 0)
        {
            Log.Warning("BasicLevelDirector: SpawnMultipleAroundPlayer called with count <= 0");
            return new Entity[0];
        }
        
        Entity[] spawnedEntities = new Entity[count];
        int successfulSpawns = 0;
        
        for (int i = 0; i < count; i++)
        {
            Entity spawned = SpawnAroundPlayer(prefab);
            if (spawned)
            {
                spawnedEntities[successfulSpawns] = spawned;
                successfulSpawns++;
            }
        }
        
        if (debugSpawning)
        {
            Log.Info($"BasicLevelDirector: Successfully spawned {successfulSpawns}/{count} entities");
        }
        
        // Resize array to only include successful spawns
        if (successfulSpawns < count)
        {
            Entity[] result = new Entity[successfulSpawns];
            for (int i = 0; i < successfulSpawns; i++)
            {
                result[i] = spawnedEntities[i];
            }
            return result;
        }
        
        return spawnedEntities;
    }
    
    /// <summary>
    /// Set the spawn distance range.
    /// </summary>
    /// <param name="minDistance">Minimum spawn distance.</param>
    /// <param name="maxDistance">Maximum spawn distance.</param>
    public void SetSpawnDistanceRange(float minDistance, float maxDistance)
    {
        if (minDistance < 0)
        {
            Log.Warning("BasicLevelDirector: minDistance cannot be negative, clamping to 0");
            minDistance = 0;
        }
        
        if (maxDistance < minDistance)
        {
            Log.Warning("BasicLevelDirector: maxDistance cannot be less than minDistance, swapping values");
            float temp = minDistance;
            minDistance = maxDistance;
            maxDistance = temp;
        }
        
        minSpawnDistance = minDistance;
        maxSpawnDistance = maxDistance;
        
        if (debugSpawning)
        {
            Log.Info($"BasicLevelDirector: Spawn distance range set to {minSpawnDistance} - {maxSpawnDistance}");
        }
    }
    
    /// <summary>
    /// Get the current spawn distance range.
    /// </summary>
    /// <returns>A Vector2 with x = minDistance, y = maxDistance.</returns>
    public Vector2 GetSpawnDistanceRange()
    {
        return new Vector2(minSpawnDistance, maxSpawnDistance);
    }
    
    /// <summary>
    /// Check if a position is within the spawn range of the player.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>True if the position is within spawn range, false otherwise.</returns>
    public bool IsPositionInSpawnRange(Vector3 position)
    {
        if (!IsPlayerValid())
            return false;
            
        Vector3 playerPosition = GetPlayerPosition();
        float distance = Vector3.Distance(playerPosition, position);
        
        return distance >= minSpawnDistance && distance <= maxSpawnDistance;
    }
    
    /// <summary>
    /// Set the enemy spawn rate.
    /// </summary>
    /// <param name="newSpawnRate">Time in seconds between spawns.</param>
    public void SetSpawnRate(float newSpawnRate)
    {
        spawnRate = Mathf.Max(0.1f, newSpawnRate); // Minimum 0.1 seconds
        
        if (debugSpawning)
        {
            Log.Info($"BasicLevelDirector: Spawn rate set to {spawnRate:F2} seconds");
        }
    }
    
    /// <summary>
    /// Enable or disable automatic enemy spawning.
    /// </summary>
    /// <param name="enabled">Whether to enable auto-spawning.</param>
    public void SetAutoSpawning(bool enabled)
    {
        enableAutoSpawning = enabled;
        
        if (debugSpawning)
        {
            Log.Info($"BasicLevelDirector: Auto-spawning {(enabled ? "enabled" : "disabled")}");
        }
    }
    
    /// <summary>
    /// Reset the spawn timer to force immediate spawn on next update.
    /// </summary>
    public void ResetSpawnTimer()
    {
        timeSinceLastSpawn = spawnRate; // Set to spawn rate to trigger immediate spawn
        
        if (debugSpawning)
        {
            Log.Info("BasicLevelDirector: Spawn timer reset - will spawn immediately");
        }
    }
    
    /// <summary>
    /// Get the time remaining until next spawn.
    /// </summary>
    /// <returns>Time in seconds until next spawn, or 0 if ready to spawn.</returns>
    public float GetTimeUntilNextSpawn()
    {
        if (!enableAutoSpawning)
            return -1f; // Auto-spawning disabled
            
        float timeRemaining = spawnRate - timeSinceLastSpawn;
        return Mathf.Max(0f, timeRemaining);
    }
    
    /// <summary>
    /// Automatically find the player entity.
    /// </summary>
    private void FindPlayer()
    {
        // Try to find by name first
        var playerEntity = Scene.FindEntityByName("Player");
        if (playerEntity)
        {
            player = playerEntity;
            return;
        }
        
        // Try to find entity with Player component
        // This is a simplified approach - in a real implementation you might want
        // to use a more sophisticated entity finding system or tags
        Log.Warning($"BasicLevelDirector: Could not auto-find player. Please assign player manually.");
    }
    
    /// <summary>
    /// Get the player's current position.
    /// </summary>
    /// <returns>The player's position, or Vector3.zero if no player is set.</returns>
    private Vector3 GetPlayerPosition()
    {
        if (!player)
        {
            Log.Warning("BasicLevelDirector: No player set, returning Vector3.zero");
            return Vector3.zero;
        }
        
        return player.transform.position;
    }
    
    /// <summary>
    /// Check if the player is valid and available.
    /// </summary>
    /// <returns>True if player is valid, false otherwise.</returns>
    private bool IsPlayerValid()
    {
        return player && Scene.IsEntityValid(player);
    }
    
    /// <summary>
    /// Set the player entity manually.
    /// </summary>
    /// <param name="newPlayer">The new player entity.</param>
    public void SetPlayer(Entity newPlayer)
    {
        player = newPlayer;
        if (player)
        {
            Log.Info($"BasicLevelDirector: Player set to {player.name}");
        }
    }

}
