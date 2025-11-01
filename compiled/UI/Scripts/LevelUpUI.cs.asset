using System;
using Unravel.Core;

/// <summary>
/// Level Up UI controller that manages the level-up card selection flow.
/// Handles game pausing, showing the menu, and resuming after selection.
/// Simple implementation focused on core functionality.
/// </summary>
[ScriptSourceFile]
public class LevelUpUI : ScriptComponent
{
    public Entity LevelUpMenu;
    
    // State
    private bool isLevelUpActive = false;
    private LevelUpMenu levelUpMenuScript;
    
    /// <summary>
    /// Called when the script is created.
    /// </summary>
    public override void OnCreate()
    {
        // Find menu references if not assigned
        FindMenuReferences();
    }
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        // Get the LevelUpMenu script component
        if (LevelUpMenu)
        {
            levelUpMenuScript = LevelUpMenu.GetComponent<LevelUpMenu>();
            if (levelUpMenuScript != null)
            {
                // Subscribe to card selection events
                levelUpMenuScript.OnCardSelected += OnCardSelected;
                Log.Info("LevelUpUI: Subscribed to card selection events");
            }
            else
            {
                Log.Warning("LevelUpUI: LevelUpMenu script component not found");
            }
        }
        
        // Initially hide the level up menu
        if (LevelUpMenu)
        {
            LevelUpMenu.SetActive(false);
        }
    }
    
    /// <summary>
    /// Automatically find menu entity references if not manually assigned.
    /// </summary>
    private void FindMenuReferences()
    {
        if (!LevelUpMenu)
        {
            var levelUpMenuEntity = owner.transform.FindChild("LevelUpMenu", true);
            if (levelUpMenuEntity) 
            {
                LevelUpMenu = levelUpMenuEntity;
                Log.Info("LevelUpUI: Found LevelUpMenu entity");
            }
            else
            {
                Log.Warning("LevelUpUI: LevelUpMenu entity not found");
            }
        }
    }
    
    /// <summary>
    /// Show the level-up menu with the given upgrade options.
    /// This pauses the game and displays the card selection interface.
    /// </summary>
    /// <param name="option1">Text for upgrade option 1</param>
    /// <param name="option2">Text for upgrade option 2</param>
    /// <param name="option3">Text for upgrade option 3</param>
    public void ShowLevelUpMenu(string option1, string option2, string option3)
    {
        if (isLevelUpActive)
        {
            Log.Warning("LevelUpUI: Level up menu is already active");
            return;
        }
        
        Log.Info($"LevelUpUI: Showing level up menu with options: [{option1}], [{option2}], [{option3}]");
        
        // Pause the game first
        PauseGame();
        
        // Show the menu (this triggers OnStart and caches UI elements)
        if (LevelUpMenu)
        {
            LevelUpMenu.SetActive(true);
        }
        
        // Now set the upgrade options after the menu is active and elements are cached
        if (levelUpMenuScript != null)
        {
            levelUpMenuScript.SetUpgradeOptions(option1, option2, option3);
        }
        
        isLevelUpActive = true;
    }
    
    /// <summary>
    /// Hide the level-up menu and resume the game.
    /// </summary>
    public void HideLevelUpMenu()
    {
        if (!isLevelUpActive)
        {
            return;
        }
        
        Log.Info("LevelUpUI: Hiding level up menu");
        
        // Hide the menu
        if (LevelUpMenu)
        {
            LevelUpMenu.SetActive(false);
        }
        
        // Resume the game
        ResumeGame();
        
        isLevelUpActive = false;
    }
    
    /// <summary>
    /// Handle card selection from the level-up menu.
    /// </summary>
    /// <param name="cardIndex">Index of the selected card (0, 1, or 2)</param>
    private void OnCardSelected(int cardIndex)
    {
        Log.Info($"LevelUpUI: Card {cardIndex + 1} selected - hiding menu and resuming game");
        
        // Hide the menu and resume the game
        // The Player will handle the actual upgrade application via the static event
        HideLevelUpMenu();
    }
    
    /// <summary>
    /// Pause the game when showing the level-up menu.
    /// </summary>
    private void PauseGame()
    {
        // Pause game time
        Time.timeScale = 0f;
        
        // Pause game audio (similar to GameUI implementation)
        var gameAudio = Scene.FindEntityByName("GameAudio");
        if (gameAudio)
        {
            var sourceComponents = gameAudio.GetComponentsInChildren<AudioSourceComponent>();
            foreach (var sourceComponent in sourceComponents)
            {
                sourceComponent.Pause();
            }
        }
        
        Log.Info("LevelUpUI: Game paused for level up");
    }
    
    /// <summary>
    /// Resume the game after level-up selection.
    /// </summary>
    private void ResumeGame()
    {
        // Resume game time
        Time.timeScale = 1f;
        
        // Resume game audio
        var gameAudio = Scene.FindEntityByName("GameAudio");
        if (gameAudio)
        {
            var sourceComponents = gameAudio.GetComponentsInChildren<AudioSourceComponent>();
            foreach (var sourceComponent in sourceComponents)
            {
                sourceComponent.Resume();
            }
        }
        
        Log.Info("LevelUpUI: Game resumed after level up");
    }
    
    /// <summary>
    /// Static helper method to find the LevelUpUI controller in the current scene.
    /// Can be used by other systems to trigger level-up menus.
    /// </summary>
    public static LevelUpUI FindInScene()
    {
        var levelUpUIEntity = Scene.FindEntityByName("LevelUpUI");
        if (levelUpUIEntity)
        {
            return levelUpUIEntity.GetComponent<LevelUpUI>();
        }
        return null;
    }
    
    /// <summary>
    /// Check if the level-up menu is currently active.
    /// </summary>
    /// <returns>True if the level-up menu is active</returns>
    public bool IsLevelUpActive()
    {
        return isLevelUpActive;
    }
    
    public override void OnDestroy()
    {
        // Unsubscribe from events
        if (levelUpMenuScript != null)
        {
            levelUpMenuScript.OnCardSelected -= OnCardSelected;
        }
        
        Log.Info("LevelUpUI controller destroyed");
    }
}
