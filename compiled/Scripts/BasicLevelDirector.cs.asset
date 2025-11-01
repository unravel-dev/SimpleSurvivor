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
    [Tooltip("Base number of spawns per second")]
    public float baseSpawnsPerSecond = 1.0f;
    [Tooltip("Base minimum number of enemies to maintain")]
    public int baseMinEnemies = 20;
    [Tooltip("Additional enemies per player level")]
    public int enemiesPerLevel = 3;
    [Tooltip("Maximum enemies allowed at once")]
    public int maxEnemies = 100;
    [Tooltip("Enable automatic enemy spawning")]
    public bool enableAutoSpawning = true;
    
    //[Header("Adaptive Spawning")]
    [Tooltip("How quickly to adapt spawn rate based on kill rate (0-1)")]
    public float adaptationRate = 0.1f;
    [Tooltip("Maximum multiplier for spawn rate adaptation")]
    public float maxSpawnRateMultiplier = 3.0f;
    [Tooltip("Time window for measuring kill rate (seconds)")]
    public float killRateWindow = 10.0f;
    
    //[Header("Enemy Scaling")]
    [Tooltip("Enable enemy scaling based on time and player level")]
    public bool enableEnemyScaling = true;
    [Tooltip("Health scaling per minute of game time (multiplier)")]
    public float healthScalingPerMinute = 0.1f;
    [Tooltip("Speed scaling per minute of game time (multiplier)")]
    public float speedScalingPerMinute = 0.05f;
    [Tooltip("Health scaling per player level (multiplier)")]
    public float healthScalingPerLevel = 0.15f;
    [Tooltip("Speed scaling per player level (multiplier)")]
    public float speedScalingPerLevel = 0.08f;
    [Tooltip("Maximum health scaling multiplier")]
    public float maxHealthScaling = 5.0f;
    [Tooltip("Maximum speed scaling multiplier")]
    public float maxSpeedScaling = 3.0f;
    
    // Spawn timing and tracking
    private float timeSinceLastSpawn = 0.0f;
    private int currentEnemyCount = 0;
    private float[] recentKillTimes = new float[50]; // Circular buffer for recent kills
    private int killTimeIndex = 0;
    private float currentSpawnRateMultiplier = 1.0f;
    
    // Scaling tracking
    private float gameStartTime = 0.0f;
    private Player playerComponent;
    private Experience playerExperience;
    
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
        // Initialize scaling system
        gameStartTime = Time.time;
        
        // Auto-find player
        if (!player)
        {
            FindPlayer();
        }
        
        // Get player components for scaling
        if (player)
        {
            playerComponent = player.GetComponent<Player>();
            playerExperience = player.GetComponent<Experience>();
        }
        
        // Initialize kill tracking
        InitializeKillTracking();
        
        // Subscribe to enemy death events to track kills
        SubscribeToEnemyDeaths();
        
        if (debugSpawning)
        {
            Log.Info($"BasicLevelDirector: Started - Auto-spawning: {enableAutoSpawning}, Enemy prefab: {(enemy != null ? "Set" : "None")}, Scaling: {enableEnemyScaling}");
        }
    }
    
    /// <summary>
    /// Called every frame to handle intelligent enemy spawning.
    /// </summary>
    public override void OnUpdate()
    {
        // Only spawn if auto-spawning is enabled and we have an enemy prefab
        if (!enableAutoSpawning || enemy == null)
            return;
            
        // Update enemy count
        UpdateEnemyCount();
        
        // Update adaptive spawn rate based on recent kill rate
        UpdateAdaptiveSpawnRate();
        
        // Calculate target enemy count based on player level
        int targetEnemyCount = GetTargetEnemyCount();
        
        // Update spawn timer
        timeSinceLastSpawn += Time.deltaTime;
        
        // Calculate current spawn interval (1 / spawns per second)
        float currentSpawnsPerSecond = baseSpawnsPerSecond * currentSpawnRateMultiplier;
        float spawnInterval = 1.0f / currentSpawnsPerSecond;
        
        // Determine if we should spawn based on enemy count and timing
        bool shouldSpawn = ShouldSpawnEnemy(targetEnemyCount, spawnInterval);
        
        if (shouldSpawn)
        {
            // Try to spawn an enemy
            Entity spawnedEnemy = SpawnAroundPlayer(enemy);
            
            if (spawnedEnemy != Entity.Invalid)
            {
                // Apply scaling to the spawned enemy
                if (enableEnemyScaling)
                {
                    ApplyEnemyScaling(spawnedEnemy);
                }
                
                // Reset timer and increment count
                timeSinceLastSpawn = 0.0f;
                currentEnemyCount++;
                
                if (debugSpawning)
                {
                    Log.Info($"BasicLevelDirector: Spawned enemy - Count: {currentEnemyCount}/{targetEnemyCount}, Rate: {currentSpawnsPerSecond:F2}/sec");
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
    /// Set the base spawn rate (spawns per second).
    /// </summary>
    /// <param name="newSpawnsPerSecond">Number of spawns per second.</param>
    public void SetSpawnRate(float newSpawnsPerSecond)
    {
        baseSpawnsPerSecond = Mathf.Max(0.1f, newSpawnsPerSecond); // Minimum 0.1 spawns per second
        
        if (debugSpawning)
        {
            Log.Info($"BasicLevelDirector: Base spawn rate set to {baseSpawnsPerSecond:F2} spawns/sec");
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
        timeSinceLastSpawn = 1.0f / baseSpawnsPerSecond; // Set to spawn interval to trigger immediate spawn
        
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
            
        float currentSpawnsPerSecond = baseSpawnsPerSecond * currentSpawnRateMultiplier;
        float spawnInterval = 1.0f / currentSpawnsPerSecond;
        float timeRemaining = spawnInterval - timeSinceLastSpawn;
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
    
    // ========== ENEMY SCALING METHODS ==========
    
    /// <summary>
    /// Apply scaling to a newly spawned enemy based on game time and player level.
    /// </summary>
    /// <param name="enemy">The enemy entity to scale.</param>
    private void ApplyEnemyScaling(Entity enemy)
    {
        if (!enemy || !enableEnemyScaling)
            return;
            
        // Calculate scaling multipliers
        float healthMultiplier = CalculateHealthScaling();
        float speedMultiplier = CalculateSpeedScaling();
        
        // Apply health scaling
        var healthComponent = enemy.GetComponent<Health>();
        if (healthComponent != null)
        {
            float originalMaxHealth = healthComponent.GetMaxHealth();
            float scaledMaxHealth = originalMaxHealth * healthMultiplier;
            healthComponent.SetMaxHealth(scaledMaxHealth, true); // Adjust current health proportionally
            
            if (debugSpawning)
            {
                Log.Info($"BasicLevelDirector: Scaled enemy health from {originalMaxHealth:F0} to {scaledMaxHealth:F0} (x{healthMultiplier:F2})");
            }
        }
        
        // Apply speed scaling
        var enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            float originalMaxSpeed = enemyComponent.maxSpeed;
            float scaledMaxSpeed = originalMaxSpeed * speedMultiplier;
            enemyComponent.SetMaxSpeed(scaledMaxSpeed);
            
            // Also scale acceleration to maintain responsive movement
            float originalAcceleration = enemyComponent.maxAcceleration;
            float scaledAcceleration = originalAcceleration * speedMultiplier;
            enemyComponent.SetMaxAcceleration(scaledAcceleration);
            
            if (debugSpawning)
            {
                Log.Info($"BasicLevelDirector: Scaled enemy speed from {originalMaxSpeed:F1} to {scaledMaxSpeed:F1} (x{speedMultiplier:F2})");
            }
        }
    }
    
    /// <summary>
    /// Calculate the health scaling multiplier based on game time and player level.
    /// </summary>
    /// <returns>Health scaling multiplier.</returns>
    private float CalculateHealthScaling()
    {
        float timeMultiplier = 1.0f;
        float levelMultiplier = 1.0f;
        
        // Time-based scaling
        float gameTimeMinutes = (Time.time - gameStartTime) / 60.0f;
        timeMultiplier = 1.0f + (gameTimeMinutes * healthScalingPerMinute);
        
        // Level-based scaling
        if (playerExperience != null)
        {
            int playerLevel = playerExperience.GetCurrentLevel();
            levelMultiplier = 1.0f + ((playerLevel - 1) * healthScalingPerLevel);
        }
        
        // Combine multipliers and clamp to maximum
        float totalMultiplier = timeMultiplier * levelMultiplier;
        return Mathf.Min(totalMultiplier, maxHealthScaling);
    }
    
    /// <summary>
    /// Calculate the speed scaling multiplier based on game time and player level.
    /// </summary>
    /// <returns>Speed scaling multiplier.</returns>
    private float CalculateSpeedScaling()
    {
        float timeMultiplier = 1.0f;
        float levelMultiplier = 1.0f;
        
        // Time-based scaling
        float gameTimeMinutes = (Time.time - gameStartTime) / 60.0f;
        timeMultiplier = 1.0f + (gameTimeMinutes * speedScalingPerMinute);
        
        // Level-based scaling
        if (playerExperience != null)
        {
            int playerLevel = playerExperience.GetCurrentLevel();
            levelMultiplier = 1.0f + ((playerLevel - 1) * speedScalingPerLevel);
        }
        
        // Combine multipliers and clamp to maximum
        float totalMultiplier = timeMultiplier * levelMultiplier;
        return Mathf.Min(totalMultiplier, maxSpeedScaling);
    }
    
    /// <summary>
    /// Get the current enemy scaling information for debugging or UI display.
    /// </summary>
    /// <returns>A formatted string with current scaling values.</returns>
    public string GetScalingInfo()
    {
        if (!enableEnemyScaling)
            return "Enemy scaling disabled";
            
        float gameTimeMinutes = (Time.time - gameStartTime) / 60.0f;
        int playerLevel = playerExperience?.GetCurrentLevel() ?? 1;
        float healthMultiplier = CalculateHealthScaling();
        float speedMultiplier = CalculateSpeedScaling();
        
        return $"Time: {gameTimeMinutes:F1}min, Level: {playerLevel}, Health: x{healthMultiplier:F2}, Speed: x{speedMultiplier:F2}";
    }
    
    /// <summary>
    /// Enable or disable enemy scaling.
    /// </summary>
    /// <param name="enabled">Whether to enable enemy scaling.</param>
    public void SetEnemyScaling(bool enabled)
    {
        enableEnemyScaling = enabled;
        
        if (debugSpawning)
        {
            Log.Info($"BasicLevelDirector: Enemy scaling {(enabled ? "enabled" : "disabled")}");
        }
    }
    
    /// <summary>
    /// Get the current health scaling multiplier.
    /// </summary>
    /// <returns>Current health scaling multiplier.</returns>
    public float GetCurrentHealthScaling()
    {
        return enableEnemyScaling ? CalculateHealthScaling() : 1.0f;
    }
    
    /// <summary>
    /// Get the current speed scaling multiplier.
    /// </summary>
    /// <returns>Current speed scaling multiplier.</returns>
    public float GetCurrentSpeedScaling()
    {
        return enableEnemyScaling ? CalculateSpeedScaling() : 1.0f;
    }
    
    // ========== NEW INTELLIGENT SPAWNING METHODS ==========
    
    /// <summary>
    /// Initialize the kill tracking system.
    /// </summary>
    private void InitializeKillTracking()
    {
        // Initialize kill times array with old timestamps
        float oldTime = Time.time - killRateWindow - 1.0f;
        for (int i = 0; i < recentKillTimes.Length; i++)
        {
            recentKillTimes[i] = oldTime;
        }
        killTimeIndex = 0;
        currentSpawnRateMultiplier = 1.0f;
        
        if (debugSpawning)
        {
            Log.Info("BasicLevelDirector: Kill tracking initialized");
        }
    }
    
    /// <summary>
    /// Subscribe to the DamageSystem's OnEntityDied event to track enemy kills.
    /// </summary>
    private void SubscribeToEnemyDeaths()
    {
        // Subscribe to the centralized death event system
        DamageSystem.OnEntityDied += HandleEntityDeath;
        
        if (debugSpawning)
        {
            Log.Info("BasicLevelDirector: Subscribed to DamageSystem OnEntityDied event");
        }
    }
    
    /// <summary>
    /// Update the current enemy count by scanning for active enemies.
    /// </summary>
    private void UpdateEnemyCount()
    {
        // This is a simple approach - count all entities with Enemy components
        // In a more optimized system, you might maintain this count through events
        var enemies = Scene.FindEntitiesWithComponent<Enemy>();
        currentEnemyCount = enemies != null ? enemies.Length : 0;
    }
    
    /// <summary>
    /// Get the target number of enemies based on player level.
    /// </summary>
    /// <returns>Target enemy count</returns>
    private int GetTargetEnemyCount()
    {
        int playerLevel = GetPlayerLevel();
        int targetCount = baseMinEnemies + (playerLevel - 1) * enemiesPerLevel;
        return Mathf.Min(targetCount, maxEnemies);
    }
    
    /// <summary>
    /// Get the player's current level.
    /// </summary>
    /// <returns>Player level, or 1 if not found</returns>
    private int GetPlayerLevel()
    {
        if (!player)
            return 1;
            
        var playerScript = player.GetComponent<Player>();
        if (playerScript != null)
        {
            return playerScript.GetLevel();
        }
        
        return 1; // Default level
    }
    
    /// <summary>
    /// Update the adaptive spawn rate based on recent kill rate.
    /// </summary>
    private void UpdateAdaptiveSpawnRate()
    {
        float currentKillRate = CalculateRecentKillRate();
        float targetKillRate = baseSpawnsPerSecond; // Ideally, kills per second should match spawns per second
        
        // Calculate desired spawn rate multiplier
        float desiredMultiplier = 1.0f;
        if (currentKillRate > targetKillRate * 1.2f) // Player is killing 20% faster than spawn rate
        {
            // Increase spawn rate to maintain challenge
            desiredMultiplier = Mathf.Min(currentKillRate / targetKillRate, maxSpawnRateMultiplier);
        }
        else if (currentKillRate < targetKillRate * 0.8f) // Player is killing 20% slower than spawn rate
        {
            // Decrease spawn rate to avoid overwhelming player
            desiredMultiplier = Mathf.Max(currentKillRate / targetKillRate, 0.5f);
        }
        
        // Smoothly adapt to the desired multiplier
        currentSpawnRateMultiplier = Mathf.Lerp(currentSpawnRateMultiplier, desiredMultiplier, adaptationRate * Time.deltaTime);
        
        if (debugSpawning && Time.time % 2.0f < Time.deltaTime) // Log every 2 seconds
        {
            Log.Info($"BasicLevelDirector: Kill rate: {currentKillRate:F2}/sec, Spawn multiplier: {currentSpawnRateMultiplier:F2}");
        }
    }
    
    /// <summary>
    /// Calculate the recent kill rate (kills per second).
    /// </summary>
    /// <returns>Recent kill rate</returns>
    private float CalculateRecentKillRate()
    {
        float currentTime = Time.time;
        float cutoffTime = currentTime - killRateWindow;
        int recentKills = 0;
        
        // Count kills within the time window
        for (int i = 0; i < recentKillTimes.Length; i++)
        {
            if (recentKillTimes[i] > cutoffTime)
            {
                recentKills++;
            }
        }
        
        return recentKills / killRateWindow;
    }
    
    /// <summary>
    /// Handle entity death from the DamageSystem event.
    /// Only tracks enemy deaths for adaptive spawning.
    /// </summary>
    /// <param name="deadEntity">The entity that died</param>
    /// <param name="killer">The entity that killed it</param>
    private void HandleEntityDeath(Entity deadEntity, Entity killer)
    {
        if (!deadEntity)
            return;
            
        // Only track enemy deaths
        var enemyComponent = deadEntity.GetComponent<Enemy>();
        if (enemyComponent == null)
            return;
            
        // Record the kill for adaptive spawning
        RecordEnemyKill(deadEntity, killer);
    }
    
    /// <summary>
    /// Record an enemy kill for adaptive spawning.
    /// </summary>
    /// <param name="deadEnemy">The enemy that died</param>
    /// <param name="killer">The entity that killed it</param>
    private void RecordEnemyKill(Entity deadEnemy, Entity killer)
    {
        recentKillTimes[killTimeIndex] = Time.time;
        killTimeIndex = (killTimeIndex + 1) % recentKillTimes.Length;
        currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);
        
        if (debugSpawning)
        {
            string killerName = killer ? killer.name : "Unknown";
            Log.Info($"BasicLevelDirector: Enemy {deadEnemy.name} killed by {killerName} - Count now: {currentEnemyCount}");
        }
    }
    
    /// <summary>
    /// Record an enemy kill for adaptive spawning (legacy method for backward compatibility).
    /// </summary>
    public void RecordEnemyKill()
    {
        recentKillTimes[killTimeIndex] = Time.time;
        killTimeIndex = (killTimeIndex + 1) % recentKillTimes.Length;
        currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);
        
        if (debugSpawning)
        {
            Log.Info($"BasicLevelDirector: Enemy killed (legacy call) - Count now: {currentEnemyCount}");
        }
    }
    
    /// <summary>
    /// Determine if we should spawn an enemy based on current conditions.
    /// </summary>
    /// <param name="targetCount">Target enemy count</param>
    /// <param name="spawnInterval">Time between spawns</param>
    /// <returns>True if we should spawn</returns>
    private bool ShouldSpawnEnemy(int targetCount, float spawnInterval)
    {
        // Always respect the maximum enemy limit
        if (currentEnemyCount >= maxEnemies)
            return false;
            
        // If we're below the minimum, spawn more aggressively
        if (currentEnemyCount < targetCount)
        {
            // If we're significantly below target, spawn faster
            if (currentEnemyCount < targetCount * 0.7f)
            {
                return timeSinceLastSpawn >= spawnInterval * 0.5f; // Spawn twice as fast
            }
            else
            {
                return timeSinceLastSpawn >= spawnInterval;
            }
        }
        
        // If we're at or above target, spawn more conservatively
        return timeSinceLastSpawn >= spawnInterval * 1.5f;
    }
    
    // ========== PUBLIC API FOR MONITORING ==========
    
    /// <summary>
    /// Get current spawning statistics for debugging/UI.
    /// </summary>
    /// <returns>Spawning stats</returns>
    public (int currentEnemies, int targetEnemies, float spawnRate, float killRate) GetSpawningStats()
    {
        return (
            currentEnemyCount,
            GetTargetEnemyCount(),
            baseSpawnsPerSecond * currentSpawnRateMultiplier,
            CalculateRecentKillRate()
        );
    }
    
    /// <summary>
    /// Force spawn enemies to reach target count immediately.
    /// </summary>
    public void ForceSpawnToTarget()
    {
        int targetCount = GetTargetEnemyCount();
        int enemiesToSpawn = targetCount - currentEnemyCount;
        
        if (enemiesToSpawn > 0)
        {
            var spawned = SpawnMultipleAroundPlayer(enemy, enemiesToSpawn);
            currentEnemyCount += spawned.Length;
            
            if (debugSpawning)
            {
                Log.Info($"BasicLevelDirector: Force spawned {spawned.Length} enemies to reach target");
            }
        }
    }
    
    /// <summary>
    /// Called when the script is destroyed. Clean up event subscriptions.
    /// </summary>
    public override void OnDestroy()
    {
        // Unsubscribe from the DamageSystem event to prevent memory leaks
        DamageSystem.OnEntityDied -= HandleEntityDeath;
        
        if (debugSpawning)
        {
            Log.Info("BasicLevelDirector: Unsubscribed from DamageSystem OnEntityDied event");
        }
    }

}
