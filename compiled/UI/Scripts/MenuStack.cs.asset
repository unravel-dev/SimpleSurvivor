using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Reusable menu stack system for managing menu navigation.
/// Handles pushing menus onto a stack, popping them off, and managing visibility.
/// Can be used by both GameUI and LobbyUI.
/// </summary>
[ScriptSourceFile]
public class MenuStack
{
    private Stack<Entity> menuStack = new Stack<Entity>();
    private Entity baseMenu; // The base menu that's always visible (e.g., GameHub)
    private bool isPaused = false;
    
    /// <summary>
    /// Initialize the menu stack with a base menu.
    /// </summary>
    /// <param name="baseMenuEntity">The base menu entity (e.g., GameHub) that should be visible when stack is empty.</param>
    public void Initialize(Entity baseMenuEntity)
    {
        baseMenu = baseMenuEntity;
        menuStack.Clear();
        
        // Show base menu initially
        if (baseMenu)
        {
            baseMenu.SetActive(true);
        }
    }
    
    /// <summary>
    /// Push a menu onto the stack and show it.
    /// </summary>
    /// <param name="menu">The menu entity to push.</param>
    /// <param name="hideBaseMenu">Whether to hide the base menu when pushing.</param>
    public void PushMenu(Entity menu, bool hidePreviousMenu = true, bool hideBaseMenu = true)
    {
        if (!menu)
        {
            Log.Warning("MenuStack: Attempted to push null menu");
            return;
        }
        
        // Hide the current top menu if stack is not empty
        if (hidePreviousMenu && menuStack.Count > 0)
        {
            var currentTop = menuStack.Peek();
            if (currentTop)
            {
                currentTop.SetActive(false);
            }
        }
        
        if (hideBaseMenu && baseMenu)
        {
            // Hide base menu if stack is empty
            baseMenu.SetActive(false);
        }
        
        // Push and show the new menu
        menuStack.Push(menu);
        menu.SetActive(true);
        
        Log.Info($"MenuStack: Pushed menu '{menu.name}' (stack size: {menuStack.Count})");
    }
    
    /// <summary>
    /// Pop the top menu from the stack and show the previous menu.
    /// </summary>
    /// <param name="showBaseMenu">Whether to show the base menu when stack becomes empty.</param>
    /// <returns>The menu that was popped, or Entity.Invalid if stack was empty.</returns>
    public Entity PopMenu(bool showBaseMenu = true)
    {
        if (menuStack.Count == 0)
        {
            Log.Warning("MenuStack: Attempted to pop from empty stack");
            return Entity.Invalid;
        }
        
        // Hide and pop the current top menu
        var topMenu = menuStack.Pop();
        if (topMenu)
        {
            topMenu.SetActive(false);
        }
        
        // Show the previous menu or base menu
        if (menuStack.Count > 0)
        {
            var previousMenu = menuStack.Peek();
            if (previousMenu)
            {
                previousMenu.SetActive(true);
            }
        }
        else if (showBaseMenu && baseMenu)
        {
            baseMenu.SetActive(true);
        }
        
        Log.Info($"MenuStack: Popped menu '{topMenu.name}' (stack size: {menuStack.Count})");
        return topMenu;
    }
    
    /// <summary>
    /// Clear the entire stack and show the base menu.
    /// </summary>
    public void ClearStack(bool showBaseMenu = true)
    {
        // Hide all menus in the stack
        while (menuStack.Count > 0)
        {
            var menu = menuStack.Pop();
            if (menu)
            {
                menu.SetActive(false);
            }
        }
        
        // Show base menu
        if (showBaseMenu && baseMenu)
        {
            baseMenu.SetActive(true);
        }
        
        Log.Info("MenuStack: Cleared stack");
    }
    
    /// <summary>
    /// Get the current top menu without removing it.
    /// </summary>
    /// <returns>The top menu entity, or Entity.Invalid if stack is empty.</returns>
    public Entity PeekMenu()
    {
        if (menuStack.Count == 0)
        {
            return Entity.Invalid;
        }
        return menuStack.Peek();
    }
    
    /// <summary>
    /// Check if the stack is empty.
    /// </summary>
    /// <returns>True if stack is empty, false otherwise.</returns>
    public bool IsEmpty()
    {
        return menuStack.Count == 0;
    }
    
    /// <summary>
    /// Get the number of menus in the stack.
    /// </summary>
    /// <returns>The stack size.</returns>
    public int Count()
    {
        return menuStack.Count;
    }
    
    /// <summary>
    /// Check if a specific menu is currently in the stack.
    /// </summary>
    /// <param name="menu">The menu entity to check for.</param>
    /// <returns>True if the menu is in the stack, false otherwise.</returns>
    public bool HasMenuInStack(Entity menu)
    {
        if (!menu)
        {
            return false;
        }
        
        return menuStack.Contains(menu);
    }
    
    /// <summary>
    /// Pause the game (sets time scale to 0).
    /// </summary>
    public void PauseGame()
    {
        if (!isPaused)
        {
            Time.timeScale = 0f;
            isPaused = true;
            Log.Info("MenuStack: Game paused");
        }
    }
    
    /// <summary>
    /// Resume the game (sets time scale to 1).
    /// </summary>
    public void ResumeGame()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
            isPaused = false;
            Log.Info("MenuStack: Game resumed");
        }
    }
    
    /// <summary>
    /// Pause all audio sources in the game audio entity.
    /// </summary>
    public void PauseAudio()
    {
        var gameAudio = Scene.FindEntityByName("GameAudio");
        if (gameAudio)
        {
            var sourceComponents = gameAudio.GetComponentsInChildren<AudioSourceComponent>();
            foreach (var sourceComponent in sourceComponents)
            {
                sourceComponent.Pause();
            }
        }
    }
    
    /// <summary>
    /// Resume all audio sources in the game audio entity.
    /// </summary>
    public void ResumeAudio()
    {
        var gameAudio = Scene.FindEntityByName("GameAudio");
        if (gameAudio)
        {
            var sourceComponents = gameAudio.GetComponentsInChildren<AudioSourceComponent>();
            foreach (var sourceComponent in sourceComponents)
            {
                sourceComponent.Resume();
            }
        }
    }
}

