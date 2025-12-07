using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Player component that implements basic rigidbody movement for a top-down game.
/// Handles player input, movement, and physics interactions.
/// Requires a PhysicsComponent with a Capsule collider attached to the same entity.
/// </summary>
[ScriptSourceFile]
public class Player : ScriptComponent
{
    //[Header("Movement Settings")]
    [Tooltip("Base maximum movement speed in units per second (before upgrades)")]
    public float baseMaxSpeed = 10.0f;
    [Tooltip("Maximum acceleration in units per second squared")]
    public float maxAcceleration = 50.0f;
    [Tooltip("Maximum deceleration when no input (higher = stops faster)")]
    public float maxDeceleration = 30.0f;
    
    //[Header("Physics Settings")]
    [Tooltip("Use physics-based movement (recommended) vs direct transform movement")]
    public bool usePhysicsMovement = true;
    
    //[Header("Player Stats")]
    [Tooltip("Base maximum health (before upgrades)")]
    public int baseMaxHealth = 100;
    [Tooltip("Base pickup range for experience and items (before upgrades)")]
    public float basePickupRange = 5.0f;
    [Tooltip("Base luck value for better upgrade card rarities")]
    public float baseLuck = 0.0f;
    
    // Component references
    private PhysicsComponent physicsComponent;
    private Health Health;
    private Experience Experience;
    private TopDownCamera cachedCamera; // Cached camera reference for shake
    
    // Movement state
    private Vector3 inputDirection;
    private Vector3 currentVelocity;
    private Vector3 targetVelocity;
    
    // Level up upgrade cards
    private List<UpgradeCard> currentUpgradeOptions;

    private bool initialUpdate = true;
    
    /// <summary>
    /// Called when the script is created. Cache component references.
    /// </summary>
    public override void OnCreate()
    {
        // Cache required components
        physicsComponent = owner.GetComponent<PhysicsComponent>();
        Health = owner.GetComponent<Health>();
        Experience = owner.GetComponent<Experience>();

        if (physicsComponent == null)
        {
            Log.Error($"Player on {owner.name}: PhysicsComponent not found! Please attach a PhysicsComponent with a Capsule collider.");
        }

        if (Health == null)
        {
            Log.Warning($"Player on {owner.name}: Health not found! Player will not be able to take damage or heal.");
        }

        if (Experience == null)
        {
            Log.Warning($"Player on {owner.name}: Experience not found! Player will not be able to collect experience.");
        }
    }

    /// <summary>
    /// Called when the script starts execution. Initialize state and values.
    /// </summary>
    public override void OnStart()
    {
        // Initialize movement state
        currentVelocity = Vector3.zero;
        targetVelocity = Vector3.zero;

        // Validate components
        if (physicsComponent == null)
        {
            Log.Error($"Player on {owner.name}: Missing required components. Disabling script.");
            return;
        }

        // Initialize health with base max health + upgrades
        InitializeHealth();

        // Initialize pickup range with base pickup range + upgrades
        InitializePickupRange();

        // Note: Don't call UpdateAbilities() here - player has no abilities yet
        // Abilities will be updated after initial ability selection in OnUpgradeSelected
        
    }
    
    /// <summary>
    /// Called when the script is enabled. Subscribe to events.
    /// </summary>
    public override void OnEnable()
    {
        // Subscribe to health events
        if (Health != null)
        {
            Health.OnDeath += OnPlayerDeath;
            Health.OnDamageTaken += OnPlayerDamageTaken;
            Health.OnHealed += OnPlayerHealed;
        }

        // Subscribe to experience events
        if (Experience != null)
        {
            Experience.OnExperienceGained += OnExperienceGained;
            Experience.OnLevelUp += OnLevelUp;
            Experience.OnExperienceChanged += OnExperienceChanged;
        }

        // Subscribe to level-up upgrade selection events
        LevelUpMenu.OnUpgradeSelected += OnUpgradeSelected;
    }
    
    /// <summary>
    /// Called when the script is disabled. Unsubscribe from events.
    /// </summary>
    public override void OnDisable()
    {
        // Unsubscribe from health events
        if (Health != null)
        {
            Health.OnDeath -= OnPlayerDeath;
            Health.OnDamageTaken -= OnPlayerDamageTaken;
            Health.OnHealed -= OnPlayerHealed;
        }
        
        // Unsubscribe from experience events
        if (Experience != null)
        {
            Experience.OnExperienceGained -= OnExperienceGained;
            Experience.OnLevelUp -= OnLevelUp;
            Experience.OnExperienceChanged -= OnExperienceChanged;
        }
        
        // Unsubscribe from level-up upgrade selection events
        LevelUpMenu.OnUpgradeSelected -= OnUpgradeSelected;
    }
    
    /// <summary>
    /// Called every frame to handle input and movement.
    /// </summary>
    public override void OnUpdate()
    {
        // Update magnet effect duration and pickup range continuously
        if (UpgradeSystem.GetMagnetMultiplier() > 1.0f)
        {
            UpgradeSystem.UpdateMagnetEffect(Time.deltaTime);
            UpdatePickupRange();
        }
        
        if (initialUpdate)
        {
            ShowInitialAbilitySelection();
            initialUpdate = false;
            return;
        }

        if (physicsComponent == null)
            return;
            
        // Don't process input/movement if player is dead
        if (Health != null && Health.IsDead())
            return;
            
        // Handle input
        HandleInput();
        
        // Update movement
        if (usePhysicsMovement)
        {
            // HandlePhysicsMovement();
        }
        else
        {
            HandleDirectMovement();
        }
    }
    
    public override void OnFixedUpdate()
    {
        if (physicsComponent == null)
            return;
            
        // Don't process physics movement if player is dead
        if (Health != null && Health.IsDead())
        {
            // Stop horizontal movement when dead
            if (physicsComponent != null)
            {
                Vector3 currentVelocity = physicsComponent.velocity;
                physicsComponent.velocity = new Vector3(0, currentVelocity.y, 0);
            }
            return;
        }
            
        // Handle input
        // HandleInput();
        
        // Update movement
        if (usePhysicsMovement)
        {
            HandlePhysicsMovement();
        }
        else
        {
            // HandleDirectMovement();
        }
    }

    /// <summary>
    /// Handle player input for movement.
    /// </summary>
    private void HandleInput()
    {
        // Get input axes
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Create input direction vector (top-down, so Y is forward/backward)
        inputDirection = new Vector3(horizontal, 0, vertical);

        // Normalize input to prevent faster diagonal movement
        if (inputDirection.magnitude > 1.0f)
        {
            inputDirection = inputDirection.normalized;
        }

        // Calculate target velocity using upgraded max speed
        float currentMaxSpeed = UpgradeSystem.ApplyMovementSpeedUpgrade(baseMaxSpeed);
        targetVelocity = inputDirection * currentMaxSpeed;
        
        
        var cameraEntity = Scene.FindEntityByName("Main Camera");
        if (cameraEntity)
        {
            var camera = cameraEntity.GetComponent<CameraComponent>();
            if (camera != null)
            {
                Vector2 mousePosition = Input.mousePosition;
                var playerPosition = transform.position + Vector3.up;   
                // float distance = Vector3.Distance(camera.transform.position, playerPosition);
                //Vector3 worldPosition = camera.ScreenPointToWorld(new Vector3(mousePosition.x, mousePosition.y, distance));
                if (camera.ScreenPointToRay(mousePosition, out var ray))
                {
                    var hit = Physics.Raycast(ray, 1000.0f, LayerMask.Everything);
                    if (hit.HasValue)
                    {
                        var worldPosition = hit.Value.point;
                        worldPosition.y = playerPosition.y;
                        transform.forward = (worldPosition - playerPosition).normalized;
                    }
                }
               
            }
        }
    }
    
    /// <summary>
    /// Handle movement using physics forces with steering behavior (recommended for realistic physics).
    /// </summary>
    private void HandlePhysicsMovement()
    {
        // Get current velocity (flattened to XZ plane for top-down movement)
        Vector3 currentVelocity = physicsComponent.velocity;
        Vector3 currentPlanarVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        
        // Calculate desired velocity based on input
        Vector3 desiredVelocity = targetVelocity;
        
        // Steering = how much we want to change our current velocity
        Vector3 steering = desiredVelocity - currentPlanarVelocity;
        
        // Choose acceleration or deceleration based on input
        float maxAccelerationRate = inputDirection.magnitude > 0.1f ? maxAcceleration : maxDeceleration;
        
        // Cap to what we can actually change this frame with our max acceleration
        float maxVelocityChange = maxAccelerationRate * Time.fixedDeltaTime;
        if (steering.sqrMagnitude > maxVelocityChange * maxVelocityChange)
        {
            steering = steering.normalized * maxVelocityChange;
        }
        
        // Apply as acceleration so it's mass-independent
        // Convert velocity change back to acceleration by dividing by deltaTime
        Vector3 accelerationForce = steering / Time.fixedDeltaTime;
        physicsComponent.ApplyForce(accelerationForce, ForceMode.Acceleration);
    }
    
    /// <summary>
    /// Handle movement by directly modifying transform (alternative method).
    /// </summary>
    private void HandleDirectMovement()
    {
        // Smoothly interpolate current velocity towards target
        float accelerationRate = inputDirection.magnitude > 0.1f ? maxAcceleration : maxDeceleration;
        currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, accelerationRate * Time.deltaTime);
        
        // Apply movement to transform
        if (currentVelocity.magnitude > 0.01f)
        {
            Vector3 movement = currentVelocity * Time.deltaTime;
            transform.position += movement;
        }
    }
    
    /// <summary>
    /// Get the current movement input direction.
    /// </summary>
    /// <returns>The normalized input direction vector.</returns>
    public Vector3 GetInputDirection()
    {
        return inputDirection;
    }
    
    /// <summary>
    /// Get the current movement velocity.
    /// </summary>
    /// <returns>The current velocity vector.</returns>
    public Vector3 GetVelocity()
    {
        if (usePhysicsMovement && physicsComponent != null)
        {
            return physicsComponent.velocity;
        }
        return currentVelocity;
    }
    
    /// <summary>
    /// Check if the player is currently moving.
    /// </summary>
    /// <returns>True if the player is moving, false otherwise.</returns>
    public bool IsMoving()
    {
        return GetVelocity().magnitude > 0.1f;
    }
    
    /// <summary>
    /// Add an external force to the player (useful for knockback, wind, etc.).
    /// </summary>
    /// <param name="force">The force to apply.</param>
    /// <param name="mode">The force mode to use.</param>
    public void AddExternalForce(Vector3 force, ForceMode mode = ForceMode.Force)
    {
        if (physicsComponent != null)
        {
            physicsComponent.ApplyForce(force, mode);
        }
    }
    
    /// <summary>
    /// Called when the player takes damage.
    /// </summary>
    /// <param name="damageAmount">Amount of damage taken.</param>
    private void OnPlayerDamageTaken(int damageAmount)
    {
        Log.Info($"Player took {damageAmount} damage - Health: {Health.GetCurrentHealth()}/{Health.GetMaxHealth()}");
        
        // Trigger camera shake
        TriggerCameraShake(damageAmount);
        
        // Could add other damage reaction behaviors here, like:
        // - Screen flash
        // - Play damage sound
        // - Show damage UI
        // - Brief invincibility frames
        // - Knockback effect
    }
    
    /// <summary>
    /// Trigger camera shake when player takes damage.
    /// </summary>
    /// <param name="damageAmount">Amount of damage taken (used to scale shake intensity).</param>
    private void TriggerCameraShake(int damageAmount)
    {
        // Cache camera reference if not already cached
        if (cachedCamera == null)
        {
            // Find the camera entity in the scene
            var cameraEntity = Scene.FindEntityByName("Main Camera");
            
            if (!cameraEntity)
            {
                // Camera not found, skip shake
                return;
            }
            
            // Get the TopDownCamera component
            cachedCamera = cameraEntity.GetComponent<TopDownCamera>();
        }
        
        // Trigger shake if camera is available
        if (cachedCamera != null)
        {
            cachedCamera.TriggerShake(damageAmount);
        }
    }
    
    /// <summary>
    /// Called when the player is healed.
    /// </summary>
    /// <param name="healAmount">Amount healed.</param>
    private void OnPlayerHealed(int healAmount)
    {
        Log.Info($"Player healed for {healAmount} - Health: {Health.GetCurrentHealth()}/{Health.GetMaxHealth()}");
        
        // Could add healing reaction behaviors here, like:
        // - Play healing sound/effect
        // - Show healing UI
        // - Particle effects
    }
    
    /// <summary>
    /// Called when the player dies.
    /// </summary>
    private void OnPlayerDeath()
    {
        Log.Info($"Player has died");
        
        // Stop all movement
        currentVelocity = Vector3.zero;
        targetVelocity = Vector3.zero;
        inputDirection = Vector3.zero;
        
        var gameOverSound = Assets.GetAsset<AudioClip>("app:/data/Sounds/game-over.mp3");
        var source = AudioSourceComponent.PlayClipAtPoint(gameOverSound, transform.position, 1.0f);
        source.maxDistance = 100.0f;

        // Show game over menu
        var gameUI = MenuStackUI.FindInScene<GameUI>();
        if (gameUI != null)
        {
            gameUI.OpenGameOverMenu();
        }
        else
        {
            Log.Warning("GameUI not found - cannot show game over menu");
        }
    }
    
    /// <summary>
    /// Get the player's health component.
    /// </summary>
    /// <returns>Health if available, null otherwise.</returns>
    public Health GetHealth()
    {
        return Health;
    }
    
    /// <summary>
    /// Check if the player is alive.
    /// </summary>
    /// <returns>True if alive (not dead).</returns>
    public bool IsAlive()
    {
        return Health == null || !Health.IsDead();
    }
    
    /// <summary>
    /// Get the player's current health percentage.
    /// </summary>
    /// <returns>Health percentage (0.0 to 1.0), or 1.0 if no health component.</returns>
    public float GetHealthPercentage()
    {
        if (Health == null)
            return 1.0f;
            
        return Health.GetHealthPercentage();
    }
    
    /// <summary>
    /// Heal the player by a specific amount.
    /// </summary>
    /// <param name="healAmount">Amount to heal.</param>
    /// <returns>Actual amount healed.</returns>
    public int HealPlayer(int healAmount)
    {
        if (Health == null)
            return 0;
            
        return Health.Heal(healAmount, owner);
    }
    
    /// <summary>
    /// Deal damage to the player.
    /// </summary>
    /// <param name="damage">Amount of damage to deal.</param>
    /// <param name="source">Source of the damage.</param>
    /// <returns>True if the player died from this damage.</returns>
    public bool DamagePlayer(int damage, Entity source)
    {
        if (Health == null)
            return false;
            
        return Health.TakeDamage(damage, source);
    }
    
    /// <summary>
    /// Called when the player gains experience.
    /// </summary>
    /// <param name="experienceAmount">Amount of experience gained.</param>
    private void OnExperienceGained(float experienceAmount)
    {
        // Log.Info($"Player gained {experienceAmount} experience!");
        
        // Could add experience gain behaviors here, like:
        // - Play experience gain sound/effect
        // - Show floating text
        // - Update UI
        // - Particle effects
    }
    
    /// <summary>
    /// Called when the player levels up.
    /// </summary>
    /// <param name="newLevel">New level.</param>
    /// <param name="oldLevel">Previous level.</param>
    private void OnLevelUp(int newLevel, int oldLevel)
    {
        Log.Info($"Player LEVEL UP! {oldLevel} -> {newLevel}");
        
        // Restore health on level up
        if (Health != null)
        {
            Health.RestoreToFullHealth();
            Log.Info("Player health restored on level up!");
        }
        
        // Show the level-up card selection menu
        ShowLevelUpMenu(newLevel);
    }
    
    public void ShowUpgradeSelectionMenu()
    {
        ShowLevelUpMenu(GetLevel());
    }
    
    /// <summary>
    /// Show the initial ability selection menu at game start.
    /// </summary>
    private void ShowInitialAbilitySelection()
    {

        var dashAbilityCard = UpgradeCardGenerator.GenerateBasicDashAbilityCard();
        dashAbilityCard.ApplyUpgrades();

        ShowLevelUpMenu(0);
    }
    
    /// <summary>
    /// Show the level-up menu with upgrade options.
    /// </summary>
    /// <param name="level">The new level the player reached</param>
    private void ShowLevelUpMenu(int level)
    {
        // Find the GameUI entity in the scene
        var gameUIEntity = MenuStackUI.FindInScene<GameUI>().owner;
        if (!gameUIEntity)
        {
            Log.Warning("Player: GameUI entity not found in scene - cannot show level up menu");
            return;
        }
        
        // Get the LevelUpUI script component
        var levelUpUIScript = gameUIEntity.GetComponent<LevelUpUI>();
        
        if (levelUpUIScript == null)
        {
            Log.Warning("Player: LevelUpUI script component not found - cannot show level up menu");
            return;
        }

        var levelUpSound = Assets.GetAsset<AudioClip>("app:/data/Sounds/level-up.mp3");
        var source = AudioSourceComponent.PlayClipAtPoint(levelUpSound, transform.position, 0.4f);
        source.maxDistance = 100.0f;
        
        // Generate upgrade card options (handles ability-only levels automatically)
        var currentAbilities = owner.GetComponentsInChildren<Ability>();
        currentUpgradeOptions = UpgradeCardGenerator.GenerateLevelUpSelection(level, currentAbilities, baseLuck);
        
        // Pass cards directly to the UI
        if (currentUpgradeOptions != null && currentUpgradeOptions.Count >= 3)
        {
            levelUpUIScript.ShowLevelUpMenu(currentUpgradeOptions[0], currentUpgradeOptions[1], currentUpgradeOptions[2]);
        }
        else
        {
            Log.Error($"Player: Not enough upgrade cards generated ({currentUpgradeOptions?.Count ?? 0}/3)");
        }
    }
    
    /// <summary>
    /// Get the color scheme for a given upgrade rarity (for UI reference).
    /// </summary>
    /// <param name="rarity">The upgrade rarity</param>
    /// <returns>Color description string</returns>
    public static string GetRarityColorScheme(UpgradeRarity rarity)
    {
        switch (rarity)
        {
            case UpgradeRarity.Normal:
                return "White-grayish";
            case UpgradeRarity.Common:
                return "Blue-ish";
            case UpgradeRarity.Epic:
                return "Purple-ish";
            case UpgradeRarity.Legendary:
                return "Gold-orange";
            default:
                return "White-grayish";
        }
    }
    
    /// <summary>
    /// Called when the player selects an upgrade from the level-up menu.
    /// </summary>
    /// <param name="selectedCard">The selected upgrade card</param>
    private void OnUpgradeSelected(UpgradeCard selectedCard)
    {
        if (selectedCard == null)
        {
            Log.Error("Player: Selected card is null");
            return;
        }
        
        Log.Info($"Player: Selected card '{selectedCard.Name}' with {selectedCard.Upgrades.Count} upgrades");
        
        // Apply all upgrades from the selected card
        selectedCard.ApplyUpgrades();
        
        // Update max health based on new upgrades
        UpdateMaxHealth();
        
        // Update pickup range based on new upgrades
        UpdatePickupRange();
        
        // Update abilities display
        UpdateAbilities();
        
        // Clear the current options
        currentUpgradeOptions = null;
        
        Log.Info($"Player: Successfully applied upgrades from card '{selectedCard.Name}'");
    }
    

    /// <summary>
    /// Called when the player's experience changes.
    /// </summary>
    /// <param name="currentExp">Current experience amount.</param>
    /// <param name="expToNextLevel">Experience needed for next level.</param>
    private void OnExperienceChanged(float currentExp, float expToNextLevel)
    {
        // This is called frequently, so avoid heavy logging
        // Could update UI elements here, like:
        // - Experience bar
        // - Level display
        // - Progress indicators
    }
    
    /// <summary>
    /// Get the player's current level.
    /// </summary>
    /// <returns>Current level, or 1 if no experience pickup component.</returns>
    public int GetLevel()
    {
        if (Experience == null)
            return 1;
            
        return Experience.GetCurrentLevel();
    }
    
    /// <summary>
    /// Get the player's current experience.
    /// </summary>
    /// <returns>Current experience, or 0 if no experience pickup component.</returns>
    public float GetExperience()
    {
        if (Experience == null)
            return 0;
            
        return Experience.GetCurrentExperience();
    }
    
    /// <summary>
    /// Get the player's current luck (base luck + upgrades).
    /// </summary>
    /// <returns>Current total luck value.</returns>
    public float GetCurrentLuck()
    {
        return UpgradeSystem.ApplyLuckUpgrade(baseLuck);
    }
    
    /// <summary>
    /// Get the player's current pickup range (base pickup range + upgrades).
    /// </summary>
    /// <returns>Current total pickup range value.</returns>
    public float GetCurrentPickupRange()
    {
        return UpgradeSystem.ApplyPickupRadiusUpgrade(basePickupRange);
    }
    
    /// <summary>
    /// Initialize the Health component with base max health + upgrades.
    /// </summary>
    private void InitializeHealth()
    {
        if (Health == null)
            return;
            
        int upgradedMaxHealth = UpgradeSystem.ApplyMaxHealthUpgrade(baseMaxHealth);
        Health.SetMaxHealth(upgradedMaxHealth);
        
        // If current health is 0 or less, set it to max health
        if (Health.GetCurrentHealth() <= 0)
        {
            Health.SetHealth(upgradedMaxHealth);
        }
        
        Log.Info($"Player health initialized: {Health.GetCurrentHealth()}/{Health.GetMaxHealth()}");
    }
    
    /// <summary>
    /// Update the Health component's max health based on current upgrades.
    /// </summary>
    private void UpdateMaxHealth()
    {
        if (Health == null)
            return;
            
        int upgradedMaxHealth = UpgradeSystem.ApplyMaxHealthUpgrade(baseMaxHealth);
        int oldMaxHealth = Health.GetMaxHealth();
        int oldCurrentHealth = Health.GetCurrentHealth();
        
        // Calculate health percentage before change
        float healthPercentage = (float)oldCurrentHealth / (float)oldMaxHealth;
        
        // Update max health
        Health.SetMaxHealth(upgradedMaxHealth, true);
    }
    
    /// <summary>
    /// Initialize the Experience component with base pickup range + upgrades.
    /// </summary>
    private void InitializePickupRange()
    {
        if (Experience == null)
            return;
            
        float upgradedPickupRange = UpgradeSystem.ApplyPickupRadiusUpgrade(basePickupRange);
        Experience.SetPickupRange(upgradedPickupRange);
        
        Log.Info($"Player pickup range initialized: {upgradedPickupRange}");
    }
    
    /// <summary>
    /// Update the Experience component's pickup range based on current upgrades.
    /// </summary>
    private void UpdatePickupRange()
    {
        if (Experience == null)
            return;
        
        // ApplyPickupRadiusUpgrade already includes magnet multiplier
        float upgradedPickupRange = UpgradeSystem.ApplyPickupRadiusUpgrade(basePickupRange);
        Experience.SetPickupRange(upgradedPickupRange);
    }


    /// <summary>
    /// Update the GameHub ability slots based on current player abilities.
    /// </summary>
    private void UpdateAbilities()
    {
        // Find the GameHub entity in the scene
        var gameHubEntity = Scene.FindEntityByName("GameHub");
        if (!gameHubEntity)
        {
            Log.Warning("Player: GameUI entity not found in scene - cannot update abilities display");
            return;
        }

        // Get the GameHub script component
        var gameHubScript = gameHubEntity.GetComponent<GameHub>();

        if (gameHubScript == null)
        {
            Log.Warning("Player: GameHub script component not found - cannot update abilities display");
            return;
        }

        // Call GameHub to initialize ability slots with current player abilities
        gameHubScript.InitializeAbilitySlots();

        Log.Info("Player: Updated abilities display in GameHub");

    }
    
    /// <summary>
    /// Get the player's experience progress towards next level.
    /// </summary>
    /// <returns>Progress percentage (0.0 to 1.0).</returns>
    public float GetLevelProgress()
    {
        if (Experience == null)
            return 0;
            
        return Experience.GetLevelProgress();
    }
    
    /// <summary>
    /// Add experience directly to the player (for testing purposes).
    /// </summary>
    /// <param name="amount">Amount of experience to add</param>
    public void AddExperienceForTesting(float amount)
    {
        if (Experience != null)
        {
            Experience.AddExperience(amount);
            Log.Info($"Player: Added {amount} experience for testing");
        }
        else
        {
            Log.Warning("Player: Cannot add experience - Experience component not found");
        }
    }
    
    /// <summary>
    /// Called when the script is destroyed. Final cleanup.
    /// </summary>
    public override void OnDestroy()
    {
        // OnDisable already handles event unsubscription
        Log.Info("Player script destroyed");
    }
}
