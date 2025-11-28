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
    
    // Ability slot elements (5 slots: 0-4, where 0 is dash)
    private UIElement[] abilitySlots = new UIElement[5];
    private UIElement[] abilityIcons = new UIElement[5];
    private UIElement[] abilityCooldowns = new UIElement[5];
    
    // Player and component references
    private Player player;
    private Health playerHealth;
    private Experience playerExperience;
    
    // Timer tracking
    private float gameStartTime;
    private float timerUpdateInterval = 1.0f; // Update timer every second
    private float lastTimerUpdate = 0f;
    
    /// <summary>
    /// Called when the script is created. Cache all references.
    /// </summary>
    public override void OnCreate()
    {
        // Cache UI document and elements
        CacheUIElements();
        
        // Cache ability slot elements
        CacheAbilitySlotElements();
        
        // Find player references
        FindPlayerReferences();
    }
    
    /// <summary>
    /// Called when the script starts execution. Initialize state.
    /// </summary>
    public override void OnStart()
    {
        // Initialize timer
        gameStartTime = Time.time;
        lastTimerUpdate = Time.time;
        
        // Initialize displays with current values
        UpdateProgressBars();
        UpdateGameTimer();
        
        // Initialize ability slots (will be empty initially, updated when player gets abilities)
        InitializeAbilitySlots();
    }
    
    /// <summary>
    /// Cache UI element references for fast access.
    /// </summary>
    private void CacheUIElements()
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
    }
    
    /// <summary>
    /// Cache ability slot UI element references.
    /// </summary>
    private void CacheAbilitySlotElements()
    {
        // Cache all ability slots (0-4)
        for (int i = 0; i < 5; i++)
        {
            abilitySlots[i] = document.GetElementById($"ability_slot_{i}");
            abilityIcons[i] = document.GetElementById($"ability_icon_{i}");
            abilityCooldowns[i] = document.GetElementById($"ability_cooldown_{i}");
            
            if (abilitySlots[i]?.IsValid() != true)
            {
                Log.Warning($"GameHub: Ability slot {i} element not found or invalid");
            }
            
            if (abilityIcons[i]?.IsValid() != true)
            {
                Log.Warning($"GameHub: Ability icon {i} element not found or invalid");
            }
            
            if (abilityCooldowns[i]?.IsValid() != true)
            {
                Log.Warning($"GameHub: Ability cooldown {i} element not found or invalid");
            }
        }
        
        Log.Info("GameHub: Cached ability slot elements");
    }
    
    /// <summary>
    /// Find the player entity in the scene and cache component references.
    /// </summary>
    private void FindPlayerReferences()
    {
        var playerEntity = Scene.FindEntityByName("Player");
        if (playerEntity)
        {
            player = playerEntity.GetComponent<Player>();
            if (player != null)
            {
                Log.Info("GameHub: Found player component");
                
                // Get health and experience components
                playerHealth = playerEntity.GetComponent<Health>();
                playerExperience = playerEntity.GetComponent<Experience>();
                
                if (playerHealth == null)
                {
                    Log.Warning("GameHub: Player health component not found");
                }
                
                if (playerExperience == null)
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
    /// Subscribe to player health and experience events.
    /// </summary>
    private void SubscribeToPlayerEvents()
    {
        // Subscribe to health events
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += OnPlayerHealthChanged;
            Log.Info("GameHub: Subscribed to health events");
        }
        
        // Subscribe to experience events
        if (playerExperience != null)
        {
            playerExperience.OnExperienceChanged += OnPlayerExperienceChanged;
            Log.Info("GameHub: Subscribed to experience events");
        }
    }
    
    /// <summary>
    /// Event handler for when player health changes.
    /// </summary>
    /// <param name="currentHealth">Current health value</param>
    /// <param name="maxHealth">Maximum health value</param>
    private void OnPlayerHealthChanged(int currentHealth, int maxHealth)
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
        
        // Update ability cooldowns
        UpdateAbilityCooldowns();
    }
    
    /// <summary>
    /// Update the health and experience progress bars with current player data.
    /// </summary>
    private void UpdateProgressBars()
    {
        if (player == null)
            return;
        
        // Update health bar
        if (playerHealth != null)
        {
            UpdateHealthBar(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
        }
        
        // Update experience bar
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
    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthBar?.IsValid() != true || maxHealth <= 0)
            return;
            
        float healthPercentage = (float)currentHealth / (float)maxHealth;
        healthBar.SetAttribute("value", healthPercentage.ToString("F3"));
        
        // Update health value text
        if (healthValue?.IsValid() == true)
        {
            string healthText = $"{currentHealth}/{maxHealth}";
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
        
        // Log.Info($"GameHub: Health updated - {currentHealth}/{maxHealth} ({healthPercentage:P0})");
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
            string levelText = $"{currentLevel}";
            levelValue.InnerRml = levelText;
        }
        
        // Log.Info($"GameHub: Experience updated - Level {currentLevel}, Progress: {expProgress:P0} (Need {expToNextLevel} more XP for next level)");
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
    /// Called when the component is enabled. Subscribe to events.
    /// </summary>
    public override void OnEnable()
    {     
        // Subscribe to player events
        SubscribeToPlayerEvents();
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
    
    // ========== ABILITY SLOT MANAGEMENT ==========
    
    /// <summary>
    /// Initialize ability slots with default empty state.
    /// </summary>
    public void InitializeAbilitySlots()
    {
        // Clear all slots first
        for (int i = 0; i < abilitySlots.Length; i++)
        {
            SetAbilitySlotEmpty(i);
        }
        
        if (player == null)
        {
            Log.Warning("GameHub: Player not found, cannot initialize ability slots");
            return;
        }
        

        int slotIndex = 0;

        // Then add regular abilities starting from slot 1 (or 0 if no dash)
        var abilities = player.owner.GetComponentsInChildren<Ability>();
        foreach (var ability in abilities)
        {
            if (ability != null)
            {
                var abilityInfo = ability.GetDisplayInfo();
                SetAbilitySlot(slotIndex, abilityInfo.iconType, abilityInfo.name, abilityInfo.icon);
                slotIndex++;
            }
        }
        
        Log.Info($"GameHub: Initialized {slotIndex} ability slots");
    }
    
    /// <summary>
    /// Set an ability slot to display a specific ability.
    /// </summary>
    /// <param name="slotIndex">Slot index (0-4)</param>
    /// <param name="abilityType">Type of ability (dash, fireball, spark, cube, etc.)</param>
    /// <param name="abilityName">Display name of the ability</param>
    /// <param name="iconText">Text to display in the icon (optional)</param>
    public void SetAbilitySlot(int slotIndex, string abilityType, string abilityName, string iconText = "")
    {
        if (slotIndex < 0 || slotIndex >= abilitySlots.Length)
        {
            Log.Warning($"GameHub: Invalid ability slot index: {slotIndex}");
            return;
        }
        
        var slot = abilitySlots[slotIndex];
        var icon = abilityIcons[slotIndex];
        
        if (slot?.IsValid() != true || icon?.IsValid() != true)
        {
            Log.Warning($"GameHub: Ability slot {slotIndex} elements not valid");
            return;
        }
        
        // Remove empty class from slot
        slot.SetClass("empty", false);
        
        // Remove all ability type classes from icon
        RemoveAbilityClassesFromIcon(icon);
        
        // Add the specific ability type class to the icon (shared styles in CommonMenu.rcss)
        icon.SetClass(abilityType, true);
        icon.SetClass("has-ability", true);
        
        // Set icon text content
        if (!string.IsNullOrEmpty(iconText))
        {
            icon.InnerRml = iconText;
        }
        
        // Set tooltip or title attribute for accessibility
        slot.SetAttribute("title", abilityName);
        
        Log.Info($"GameHub: Set ability slot {slotIndex} to {abilityType} ({abilityName})");
    }
    
    /// <summary>
    /// Set an ability slot to empty state.
    /// </summary>
    /// <param name="slotIndex">Slot index (0-4)</param>
    public void SetAbilitySlotEmpty(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= abilitySlots.Length)
        {
            Log.Warning($"GameHub: Invalid ability slot index: {slotIndex}");
            return;
        }
        
        var slot = abilitySlots[slotIndex];
        var icon = abilityIcons[slotIndex];
        
        if (slot?.IsValid() != true || icon?.IsValid() != true)
        {
            Log.Warning($"GameHub: Ability slot {slotIndex} elements not valid");
            return;
        }
        
        // Remove all ability type classes and set to empty
        RemoveAbilityClassesFromIcon(icon);
        slot.SetClass("empty", true);
        icon.SetClass("has-ability", false);
        
        // Clear icon text and set placeholder
        icon.InnerRml = "?";
        
        // Clear tooltip
        slot.SetAttribute("title", "Empty Slot");
    }
    
    /// <summary>
    /// Remove all ability type classes from an icon element.
    /// </summary>
    /// <param name="icon">The icon element to clean</param>
    private void RemoveAbilityClassesFromIcon(UIElement icon)
    {
        if (icon == null)
            return;
        
        // Remove ability-specific classes
        icon.SetClass("fireball", false);
        icon.SetClass("dash", false);
        icon.SetClass("spark", false);
        icon.SetClass("cube", false);
        icon.SetClass("boomerang", false);
        icon.SetClass("meteorshower", false);
        icon.SetClass("blackhole", false);
        icon.SetClass("plague", false);
        
        // Remove upgrade rarity classes
        icon.SetClass("upgrade-normal", false);
        icon.SetClass("upgrade-common", false);
        icon.SetClass("upgrade-epic", false);
        icon.SetClass("upgrade-legendary", false);
    }
    
    /// <summary>
    /// Set cooldown display for an ability slot.
    /// </summary>
    /// <param name="slotIndex">Slot index (0-4)</param>
    /// <param name="cooldownTime">Remaining cooldown time in seconds (0 to hide cooldown)</param>
    /// <param name="maxCooldown">Maximum cooldown time for calculating progress</param>
    public void SetAbilityCooldown(int slotIndex, float cooldownTime, float maxCooldown = 1.0f)
    {
        if (slotIndex < 0 || slotIndex >= abilitySlots.Length)
        {
            Log.Warning($"GameHub: Invalid ability slot index: {slotIndex}");
            return;
        }
        
        var slot = abilitySlots[slotIndex];
        var cooldownElement = abilityCooldowns[slotIndex];
        
        if (slot?.IsValid() != true || cooldownElement?.IsValid() != true)
        {
            Log.Warning($"GameHub: Ability slot {slotIndex} cooldown elements not valid");
            return;
        }
        
        bool onCooldown = cooldownTime > 0;
        slot.SetClass("on-cooldown", onCooldown);
        
        // Calculate cooldown progress (0 = ready, 1 = just used)
        float progress = 0.0f;
        if (maxCooldown > 0 && cooldownTime > 0)
        {
            progress = cooldownTime / maxCooldown;
        }
        
        // Update the circular progress gauge
        cooldownElement.SetAttribute("value", progress.ToString("F3"));
    }
    
    /// <summary>
    /// Set active state for an ability slot (when ability is being used).
    /// </summary>
    /// <param name="slotIndex">Slot index (0-4)</param>
    /// <param name="isActive">Whether the ability is currently active</param>
    public void SetAbilityActive(int slotIndex, bool isActive)
    {
        if (slotIndex < 0 || slotIndex >= abilitySlots.Length)
        {
            Log.Warning($"GameHub: Invalid ability slot index: {slotIndex}");
            return;
        }
        
        var slot = abilitySlots[slotIndex];
        
        if (slot?.IsValid() != true)
        {
            Log.Warning($"GameHub: Ability slot {slotIndex} element not valid");
            return;
        }
        
        slot.SetClass("active", isActive);
    }
    
    /// <summary>
    /// Update ability cooldown displays based on current ability states.
    /// </summary>
    private void UpdateAbilityCooldowns()
    {
        if (player?.owner == null)
            return;
        
        int slotIndex = 0;
        

        // Update all abilities (including DashAbility)
        var abilities = player.owner.GetComponentsInChildren<Ability>();
        foreach (var ability in abilities)
        {
            if (ability != null)
            {
                float remainingCooldown = ability.GetRemainingCooldown();
                float maxCooldown = ability.GetCooldown();
                SetAbilityCooldown(slotIndex, remainingCooldown, maxCooldown);
                slotIndex++;
            }
        }
    }
}
