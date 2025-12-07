using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Centralized static cache for all container entities in the game.
/// Provides static getters that find or create containers as needed.
/// Containers should be cleared when loading/reloading/quitting the game.
/// </summary>
[ScriptSourceFile]
public static class ContainerCache
{
    // Cached container entities
    private static Entity enemyContainer = Entity.Invalid;
    private static Entity effectsContainer = Entity.Invalid;
    private static Entity lootContainer = Entity.Invalid;
    private static Entity damageContainer = Entity.Invalid;
    
    /// <summary>
    /// Get the EnemyContainer entity, creating it if it doesn't exist.
    /// </summary>
    public static Entity EnemyContainer
    {
        get
        {
            if (!enemyContainer)
            {
                enemyContainer = Scene.FindEntityByName("EnemyContainer");
                
                if (!enemyContainer)
                {
                    enemyContainer = Scene.CreateEntity("EnemyContainer");

                }
            }
            
            return enemyContainer;
        }
    }
    
    /// <summary>
    /// Get the EffectsContainer entity, creating it if it doesn't exist.
    /// Used for ability spawn effects (projectiles, black holes, meteors, etc.).
    /// </summary>
    public static Entity EffectsContainer
    {
        get
        {
            if (!effectsContainer)
            {
                effectsContainer = Scene.FindEntityByName("EffectsContainer");
                
                if (!effectsContainer)
                {
                    effectsContainer = Scene.CreateEntity("EffectsContainer");
                }
            }
            
            return effectsContainer;
        }
    }
    
    /// <summary>
    /// Get the LootContainer entity, creating it if it doesn't exist.
    /// </summary>
    public static Entity LootContainer
    {
        get
        {
            if (!lootContainer)
            {
                lootContainer = Scene.FindEntityByName("LootContainer");
                
                if (!lootContainer)
                {
                    lootContainer = Scene.CreateEntity("LootContainer");
                }
            }
            
            return lootContainer;
        }
    }
    
    /// <summary>
    /// Get the DamageContainer entity, creating it if it doesn't exist.
    /// </summary>
    public static Entity DamageContainer
    {
        get
        {
            if (!damageContainer)
            {
                damageContainer = Scene.FindEntityByName("DamageContainer");
                
                if (!damageContainer)
                {
                    damageContainer = Scene.CreateEntity("DamageContainer");
                }
            }
            
            return damageContainer;
        }
    }
    
    /// <summary>
    /// Clear all cached container references.
    /// Should be called when loading, reloading, or quitting the game.
    /// </summary>
    public static void Clear()
    {
        enemyContainer = Entity.Invalid;
        effectsContainer = Entity.Invalid;
        lootContainer = Entity.Invalid;
        damageContainer = Entity.Invalid;
    }
}

