using System;
using System.Collections.Generic;
using Unravel.Core;

/// <summary>
/// Special upgrade card that can grant new abilities to the player.
/// Extends UpgradeCard to also add ability components when applied.
/// </summary>
public class AbilityCard : UpgradeCard
{
    /// <summary>
    /// Type of ability component to add to the player.
    /// </summary>
    public Type AbilityType { get; private set; }
    
    /// <summary>
    /// Optional configuration action to set up the ability after it's added.
    /// </summary>
    public System.Action<ScriptComponent> ConfigureAbility { get; private set; }
    
    /// <summary>
    /// Create a new ability card with a single upgrade and an ability.
    /// </summary>
    /// <param name="name">Name of the card</param>
    /// <param name="rarity">Rarity of the card</param>
    /// <param name="upgrade">Single upgrade to apply</param>
    /// <param name="abilityType">Type of ability component to add</param>
    /// <param name="configureAbility">Optional configuration action for the ability</param>
    /// <param name="maxPicks">Maximum number of times this card can be picked. -1 = unlimited (default).</param>
    public AbilityCard(string name, UpgradeRarity rarity, Upgrade upgrade, Type abilityType, System.Action<ScriptComponent> configureAbility = null, int maxPicks = -1)
        : base(name, rarity, upgrade, maxPicks)
    {
        ValidateAbilityType(abilityType);
        AbilityType = abilityType;
        ConfigureAbility = configureAbility;
    }
    
    /// <summary>
    /// Create a new ability card with multiple upgrades and an ability.
    /// </summary>
    /// <param name="name">Name of the card</param>
    /// <param name="rarity">Rarity of the card</param>
    /// <param name="upgrades">List of upgrades to apply</param>
    /// <param name="abilityType">Type of ability component to add</param>
    /// <param name="configureAbility">Optional configuration action for the ability</param>
    /// <param name="maxPicks">Maximum number of times this card can be picked. -1 = unlimited (default).</param>
    public AbilityCard(string name, UpgradeRarity rarity, List<Upgrade> upgrades, Type abilityType, System.Action<ScriptComponent> configureAbility = null, int maxPicks = -1)
        : base(name, rarity, upgrades, maxPicks)
    {
        ValidateAbilityType(abilityType);
        AbilityType = abilityType;
        ConfigureAbility = configureAbility;
    }
    
    /// <summary>
    /// Validate that the ability type is a valid ScriptComponent.
    /// </summary>
    /// <param name="abilityType">Type to validate</param>
    private void ValidateAbilityType(Type abilityType)
    {
        if (abilityType == null)
        {
            throw new ArgumentNullException(nameof(abilityType), "Ability type cannot be null");
        }
        
        if (!typeof(ScriptComponent).IsAssignableFrom(abilityType))
        {
            throw new ArgumentException($"Ability type {abilityType.Name} must inherit from ScriptComponent", nameof(abilityType));
        }
    }
    
    /// <summary>
    /// Apply all upgrades and add the new ability to the player.
    /// </summary>
    public override void ApplyUpgrades()
    {
        // Apply base upgrades first
        base.ApplyUpgrades();
        
        // Add the new ability to the player
        AddAbilityToPlayer();
    }
    
    /// <summary>
    /// Add the ability component to the player entity.
    /// </summary>
    private void AddAbilityToPlayer()
    {
        // Find the player entity
        var playerEntity = Scene.FindEntityByName("Player");
        if (!playerEntity)
        {
            Log.Error($"AbilityCard: Player entity not found - cannot add ability {AbilityType.Name}");
            return;
        }
        
        // Check if the player already has this ability type
        var existingAbility = playerEntity.GetComponentInChildren(AbilityType);
        if (existingAbility != null)
        {
            Log.Warning($"AbilityCard: Player already has ability {AbilityType.Name} - skipping addition");
            return;
        }
        
        try
        {
            var abilityEntity = Scene.CreateEntity(AbilityType.Name);
            abilityEntity.transform.SetParent(playerEntity, false);
            // Add the ability component to the ability entity
            var abilityComponent = abilityEntity.AddComponent(AbilityType) as ScriptComponent;
            
            if (abilityComponent == null)
            {
                Log.Error($"AbilityCard: Failed to add ability component {AbilityType.Name} to player");
                return;
            }
            
            // Configure the ability if a configuration action was provided
            ConfigureAbility?.Invoke(abilityComponent);
            
            Log.Info($"AbilityCard: Successfully added ability {AbilityType.Name} to player");
        }
        catch (Exception ex)
        {
            Log.Error($"AbilityCard: Exception while adding ability {AbilityType.Name}: {ex.Message}");
        }
    }
    
    
    /// <summary>
    /// Check if the player already has this ability type.
    /// </summary>
    /// <returns>True if the player already has this ability</returns>
    public bool PlayerHasAbility()
    {
        var playerEntity = Scene.FindEntityByName("Player");
        if (!playerEntity)
            return false;
            
        return playerEntity.GetComponentInChildren(AbilityType) != null;
    }
    
    /// <summary>
    /// Get display information for the Ability Card.
    /// Overrides base implementation to include ability icon type.
    /// </summary>
    /// <returns>Display information for the upgrade card.</returns>
    public override UpgradeDisplayInfo GetDisplayInfo()
    {
        // Set icon type from the ability type
        if (AbilityType != null)
        {
            return Ability.GetDisplayInfo(AbilityType);
        }
        
        return base.GetDisplayInfo();
    }
}
