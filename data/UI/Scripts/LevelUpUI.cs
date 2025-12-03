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
    private LevelUpMenu levelUpMenuScript;

    /// <summary>
    /// Called when the script is created.
    /// </summary>
    public override void OnCreate()
    {
        // Find menu references if not assigned
        FindMenuReferences();
        
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
    }
    
    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {

        
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
    /// Show the level-up menu with the given upgrade cards.
    /// This pauses the game and displays the card selection interface.
    /// </summary>
    /// <param name="card1">Upgrade card for option 1</param>
    /// <param name="card2">Upgrade card for option 2</param>
    /// <param name="card3">Upgrade card for option 3</param>
    public void ShowLevelUpMenu(UpgradeCard card1, UpgradeCard card2, UpgradeCard card3)
    {
        if (!LevelUpMenu)
        {
            Log.Warning("LevelUpUI: Level up menu is already active or menu not found");
            return;
        }
        
        Log.Info($"LevelUpUI: Showing level up menu with cards: [{card1?.Name}], [{card2?.Name}], [{card3?.Name}]");
        
        // Push level up menu onto the menu stack (handles pause/audio automatically)
        var gameUI = MenuStackUI.FindInScene();
        gameUI.PushMenu(LevelUpMenu);

        
        // Now set the upgrade cards after the menu is active and elements are cached
        if (levelUpMenuScript != null)
        {
            levelUpMenuScript.SetUpgradeCards(card1, card2, card3);
        }
        
    }
    
    /// <summary>
    /// Hide the level-up menu and resume the game.
    /// </summary>
    public void HideLevelUpMenu()
    {
        Log.Info("LevelUpUI: Hiding level up menu");
        
        // Pop menu from stack (handles resume/audio automatically)
        var gameUI = MenuStackUI.FindInScene();
        gameUI.PopMenu();
        
    }
    
    /// <summary>
    /// Handle card selection from the level-up menu.
    /// </summary>
    /// <param name="selectedCard">The selected upgrade card</param>
    private void OnCardSelected(UpgradeCard selectedCard)
    {
        Log.Info($"LevelUpUI: Card '{selectedCard?.Name}' selected - hiding menu and resuming game");
        
        // Hide the menu and resume the game
        // The Player will handle the actual upgrade application via the static event
        HideLevelUpMenu();
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

    
    public override void OnDisable()
    {
        // Unsubscribe from events when component is disabled (e.g., scene unload)
        UnsubscribeFromEvents();
        base.OnDisable();
    }

    public override void OnDestroy()
    {
        // Ensure events are unsubscribed
        UnsubscribeFromEvents();
        Log.Info("LevelUpUI controller destroyed");
    }

    /// <summary>
    /// Unsubscribe from all events to prevent memory leaks.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (levelUpMenuScript != null)
        {
            levelUpMenuScript.OnCardSelected -= OnCardSelected;
            Log.Info("LevelUpUI: Unsubscribed from card selection events");
        }
    }
}
