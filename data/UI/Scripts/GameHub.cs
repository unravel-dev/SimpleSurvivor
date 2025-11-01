using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Game Hub HUD controller that displays player health and experience bars during gameplay.
/// Updates the progress bars via event subscriptions for optimal performance.
/// </summary>
[ScriptSourceFile]
public class GameHub : ScriptComponent
{
    // UI Elements
    private UIDocument document;
    private UIElement healthBar;
    private UIElement experienceBar;
    private UIElement gameTimer;
    private UIElement healthValue;
    private UIElement levelValue;
    
    // Player and component references
    private Player player;
    private Health playerHealth;
    private Experience playerExperience;
    
    // Timer tracking
    private float gameStartTime;
    private float timerUpdateInterval = 1.0f; // Update timer every second
    private float lastTimerUpdate = 0f;
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        // Get the UI document component
        var uiDoc = owner.GetComponent<UIDocumentComponent>();
        if (uiDoc == null)
        {
            Log.Error("GameHub: No UIDocumentComponent found on entity");
            return;
        }

        // Get the document wrapper
        document = uiDoc.GetDocument();
        if (document == null)
        {
            Log.Error("GameHub: Failed to get document wrapper - document may not be loaded");
            return;
        }

        Log.Info($"GameHub: Got document wrapper: {document.Title}");

        // Cache UI elements
        CacheUIElements();
        
        // Find player reference and subscribe to events
        FindPlayerAndSubscribeToEvents();
        
        // Initialize timer
        gameStartTime = Time.time;
        lastTimerUpdate = Time.time;
        
        // Initial update
        UpdateProgressBars();
        UpdateGameTimer();
    }
    
    /// <summary>
    /// Cache UI element references for fast access.
    /// </summary>
    private void CacheUIElements()
    {
        healthBar = document.GetElementById("health_bar");
        experienceBar = document.GetElementById("experience_bar");
        gameTimer = document.GetElementById("game_timer");
        healthValue = document.GetElementById("health_value");
        levelValue = document.GetElementById("level_value");
        
        if (healthBar?.IsValid() != true)
        {
            Log.Error("GameHub: Health bar element not found or invalid");
        }
        
        if (experienceBar?.IsValid() != true)
        {
            Log.Error("GameHub: Experience bar element not found or invalid");
        }
        
        if (gameTimer?.IsValid() != true)
        {
            Log.Error("GameHub: Game timer element not found or invalid");
        }
        
        if (healthValue?.IsValid() != true)
        {
            Log.Error("GameHub: Health value element not found or invalid");
        }
        
        if (levelValue?.IsValid() != true)
        {
            Log.Error("GameHub: Level value element not found or invalid");
        }
        
        Log.Info($"GameHub: Cached UI elements - Health: {healthBar?.IsValid()}, Experience: {experienceBar?.IsValid()}, Timer: {gameTimer?.IsValid()}, HealthValue: {healthValue?.IsValid()}, LevelValue: {levelValue?.IsValid()}");
    }
    
    /// <summary>
    /// Find the player entity in the scene and subscribe to health/experience events.
    /// </summary>
    private void FindPlayerAndSubscribeToEvents()
    {
        var playerEntity = Scene.FindEntityByName("Player");
        if (playerEntity)
        {
            player = playerEntity.GetComponent<Player>();
            if (player != null)
            {
                Log.Info("GameHub: Found player component");
                
                // Get health and experience components
                playerHealth = player.GetHealth();
                playerExperience = playerEntity.GetComponent<Experience>();
                
                // Subscribe to health events
                if (playerHealth != null)
                {
                    playerHealth.OnHealthChanged += OnPlayerHealthChanged;
                    Log.Info("GameHub: Subscribed to health events");
                }
                else
                {
                    Log.Warning("GameHub: Player health component not found");
                }
                
                // Subscribe to experience events
                if (playerExperience != null)
                {
                    playerExperience.OnExperienceChanged += OnPlayerExperienceChanged;
                    Log.Info("GameHub: Subscribed to experience events");
                }
                else
                {
                    Log.Warning("GameHub: Player experience component not found");
                }
            }
            else
            {
                Log.Warning("GameHub: Player entity found but no Player component");
            }
        }
        else
        {
            Log.Warning("GameHub: Player entity not found in scene");
        }
    }
    
    /// <summary>
    /// Event handler for when player health changes.
    /// </summary>
    /// <param name="currentHealth">Current health value</param>
    /// <param name="maxHealth">Maximum health value</param>
    private void OnPlayerHealthChanged(float currentHealth, float maxHealth)
    {
        UpdateHealthBar(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// Event handler for when player experience changes.
    /// </summary>
    /// <param name="currentExp">Current experience value</param>
    /// <param name="expToNextLevel">Experience required for next level</param>
    private void OnPlayerExperienceChanged(float currentExp, float expToNextLevel)
    {
        UpdateExperienceBar(currentExp, expToNextLevel);
    }
    
    /// <summary>
    /// Update method called every frame.
    /// </summary>
    public override void OnUpdate()
    {
        // Update timer at controlled intervals
        if (Time.time - lastTimerUpdate >= timerUpdateInterval)
        {
            UpdateGameTimer();
            lastTimerUpdate = Time.time;
        }
    }
    
    /// <summary>
    /// Update the health and experience progress bars with current player data (initial load).
    /// </summary>
    private void UpdateProgressBars()
    {
        if (player == null)
        {
            // Try to find player again if not found initially
            FindPlayerAndSubscribeToEvents();
            return;
        }
        
        // Initial health bar update
        if (playerHealth != null)
        {
            UpdateHealthBar(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
        }
        
        // Initial experience bar update
        if (playerExperience != null)
        {
            float currentExp = playerExperience.GetCurrentExperience();
            float expToNext = playerExperience.GetExperienceToNextLevel();
            UpdateExperienceBar(currentExp, expToNext);
        }
    }
    
    /// <summary>
    /// Update the health bar display.
    /// </summary>
    /// <param name="currentHealth">Current health value</param>
    /// <param name="maxHealth">Maximum health value</param>
    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBar?.IsValid() != true || maxHealth <= 0)
            return;
            
        float healthPercentage = currentHealth / maxHealth;
        healthBar.SetAttribute("value", healthPercentage.ToString("F3"));
        
        // Update health value text
        if (healthValue?.IsValid() == true)
        {
            string healthText = $"{(int)currentHealth}/{(int)maxHealth}";
            healthValue.InnerRml = healthText;
        }
        
        // Change visual state based on health level
        if (healthPercentage < 0.25f)
        {
            healthBar.SetClass("low-health", true);
        }
        else
        {
            healthBar.SetClass("low-health", false);
        }
        
        Log.Info($"GameHub: Health updated - {currentHealth}/{maxHealth} ({healthPercentage:P0})");
    }
    
    /// <summary>
    /// Update the experience bar display.
    /// </summary>
    /// <param name="currentExp">Current total experience value</param>
    /// <param name="expToNextLevel">Experience still needed for next level</param>
    private void UpdateExperienceBar(float currentExp, float expToNextLevel)
    {
        if (experienceBar?.IsValid() != true)
            return;
            
        // Use the Experience component's GetLevelProgress method for accurate calculation
        float expProgress = 0f;
        int currentLevel = 1;
        
        if (playerExperience != null)
        {
            expProgress = playerExperience.GetLevelProgress();
            currentLevel = playerExperience.GetCurrentLevel();
        }
        else
        {
            // Fallback calculation if we don't have direct access to Experience component
            // Get current level's experience requirement
            currentLevel = playerExperience?.GetCurrentLevel() ?? 1;
            float expForCurrentLevel = playerExperience?.GetExperienceRequiredForLevel(currentLevel) ?? 0f;
            float expForNextLevel = playerExperience?.GetExperienceRequiredForLevel(currentLevel + 1) ?? 100f;
            
            // Calculate experience within current level
            float expInCurrentLevel = currentExp - expForCurrentLevel;
            float expNeededForLevel = expForNextLevel - expForCurrentLevel;
            
            if (expNeededForLevel > 0)
            {
                expProgress = Mathf.Clamp01(expInCurrentLevel / expNeededForLevel);
            }
        }
        
        experienceBar.SetAttribute("value", expProgress.ToString("F3"));
        
        // Update level value text
        if (levelValue?.IsValid() == true)
        {
            string levelText = $"Level {currentLevel}";
            levelValue.InnerRml = levelText;
        }
        
        Log.Info($"GameHub: Experience updated - Level {currentLevel}, Progress: {expProgress:P0} (Need {expToNextLevel} more XP for next level)");
    }
    
    /// <summary>
    /// Update the game timer display.
    /// </summary>
    private void UpdateGameTimer()
    {
        if (gameTimer?.IsValid() != true)
            return;
            
        float elapsedTime = Time.time - gameStartTime;
        
        // Format time as MM:SS
        int minutes = (int)(elapsedTime / 60f);
        int seconds = (int)(elapsedTime % 60f);
        
        string timeString = $"{minutes:D2}:{seconds:D2}";
        gameTimer.InnerRml = timeString;
    }
    
    /// <summary>
    /// Called when the component is enabled.
    /// </summary>
    public override void OnEnable()
    {
        // Ensure we have fresh references when enabled
        if (document == null)
        {
            OnStart();
        }
        else
        {
            UpdateProgressBars();
            UpdateGameTimer();
        }
    }
    
    /// <summary>
    /// Called when the component is disabled.
    /// </summary>
    public override void OnDisable()
    {
        UnsubscribeFromEvents();
    }
    
    /// <summary>
    /// Called when the component is destroyed.
    /// </summary>
    public override void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    /// <summary>
    /// Unsubscribe from all player events to prevent memory leaks.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= OnPlayerHealthChanged;
            Log.Info("GameHub: Unsubscribed from health events");
        }
        
        if (playerExperience != null)
        {
            playerExperience.OnExperienceChanged -= OnPlayerExperienceChanged;
            Log.Info("GameHub: Unsubscribed from experience events");
        }
    }
    
    /// <summary>
    /// Get the current health percentage for external access.
    /// </summary>
    /// <returns>Health percentage (0.0 to 1.0), or 0 if no player found.</returns>
    public float GetHealthPercentage()
    {
        return player?.GetHealthPercentage() ?? 0f;
    }
    
    /// <summary>
    /// Get the current experience progress for external access.
    /// </summary>
    /// <returns>Experience progress (0.0 to 1.0), or 0 if no player found.</returns>
    public float GetExperienceProgress()
    {
        return player?.GetLevelProgress() ?? 0f;
    }
    
    /// <summary>
    /// Manually refresh the progress bars (useful for testing or external calls).
    /// </summary>
    public void RefreshProgressBars()
    {
        UpdateProgressBars();
    }
}
