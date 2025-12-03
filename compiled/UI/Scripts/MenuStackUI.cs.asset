using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Base component for UI controllers that use a menu stack system.
/// Provides common menu stack functionality that can be shared between GameUI and LobbyUI.
/// </summary>
[ScriptSourceFile]
public class MenuStackUI : ScriptComponent
{
    /// <summary>
    /// The menu stack instance for managing menu navigation.
    /// </summary>
    protected MenuStack menuStack = new MenuStack();

    /// <summary>
    /// Push a menu onto the stack.
    /// </summary>
    /// <param name="menu">The menu entity to push.</param>
    public virtual void PushMenu(Entity menu)
    {
        if (!menu)
        {
            Log.Warning($"{GetType().Name}: Attempted to push null menu");
            return;
        }

        menuStack.PushMenu(menu, hidePreviousMenu: true, hideBaseMenu: true);
    }

    /// <summary>
    /// Pop a menu from the stack.
    /// </summary>
    public virtual void PopMenu()
    {
        if (menuStack.IsEmpty())
        {
            return;
        }

        menuStack.PopMenu(showBaseMenu: true);
    }

    /// <summary>
    /// Get the menu stack instance (for advanced operations).
    /// </summary>
    /// <returns>The MenuStack instance.</returns>
    public MenuStack GetMenuStack()
    {
        return menuStack;
    }

    /// <summary>
    /// Static helper method to find a MenuStackUI component in the current scene.
    /// Can be used by menus to find the menu stack controller.
    /// </summary>
    /// <typeparam name="T">The type of MenuStackUI to find.</typeparam>
    /// <param name="entityName">The name of the entity to search for.</param>
    /// <returns>The found component, or null if not found.</returns>
    public static T FindInScene<T>() where T : MenuStackUI
    {
        var entity = Scene.FindEntityByName("UI");
        if (entity)
        {
            return entity.GetComponent<T>();
        }
        return null;
    }

    public static MenuStackUI FindInScene()
    {
        return FindInScene<MenuStackUI>();
    }

}

