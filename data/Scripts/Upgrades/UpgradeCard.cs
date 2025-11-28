using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Structure to hold display information for UI (used for both abilities and upgrade cards).
/// </summary>
public struct UpgradeDisplayInfo
{
    public string name;
    public string description;
    public string iconType; // CSS class name for the ability icon (if applicable)
    public string icon; // Text icon/emoji (for abilities)
    public string color; // Color string (for abilities)
}

/// <summary>
/// Represents a card that contains one or more upgrades and has its own rarity.
/// Used for the upgrade selection system where players choose cards containing upgrades.
/// </summary>
public class UpgradeCard
{
    /// <summary>
    /// Display name of the upgrade card.
    /// </summary>
    public string Name { get; protected set; }

    
    /// <summary>
    /// Rarity level of this upgrade card.
    /// </summary>
    public UpgradeRarity Rarity { get; protected set; }
    
    /// <summary>
    /// List of upgrades that this card provides when selected.
    /// </summary>
    public List<Upgrade> Upgrades { get; protected set; }
    
    /// <summary>
    /// Optional ability type that this card is specific to. If set, the player must have this ability to see this card.
    /// </summary>
    public Type RequiredAbilityType { get; protected set; }
    
    /// <summary>
    /// Number of times this card can be picked. -1 = unlimited, 0 = cannot be picked, >0 = specific count.
    /// </summary>
    protected int RemainingPicks;
    
    /// <summary>
    /// Create a new upgrade card with specified upgrades and rarity.
    /// </summary>
    /// <param name="name">Display name of the card.</param>
    /// <param name="rarity">Rarity level of the card.</param>
    /// <param name="upgrades">List of upgrades this card provides.</param>
    /// <param name="maxPicks">Maximum number of times this card can be picked. -1 = unlimited (default).</param>
    /// <param name="requiredAbilityType">Optional ability type that this card is specific to. If set, player must have this ability to see this card.</param>
    public UpgradeCard(string name, UpgradeRarity rarity, List<Upgrade> upgrades, int maxPicks = -1, Type requiredAbilityType = null)
    {
        Name = name;
        Rarity = rarity;
        Upgrades = upgrades ?? new List<Upgrade>();
        RemainingPicks = maxPicks;
        RequiredAbilityType = requiredAbilityType;
    }
    
    /// <summary>
    /// Create a new upgrade card with a single upgrade.
    /// </summary>
    /// <param name="name">Display name of the card.</param>
    /// <param name="rarity">Rarity level of the card.</param>
    /// <param name="upgrade">Single upgrade this card provides.</param>
    /// <param name="maxPicks">Maximum number of times this card can be picked. -1 = unlimited (default).</param>
    /// <param name="requiredAbilityType">Optional ability type that this card is specific to. If set, player must have this ability to see this card.</param>
    public UpgradeCard(string name, UpgradeRarity rarity, Upgrade upgrade, int maxPicks = -1, Type requiredAbilityType = null)
    {
        Name = name;
        Rarity = rarity;
        Upgrades = new List<Upgrade> { upgrade };
        RemainingPicks = maxPicks;
        RequiredAbilityType = requiredAbilityType;
    }
    
    /// <summary>
    /// Virtual method for derived classes to provide display information for UI.
    /// Override this to customize how the upgrade card appears in the UI.
    /// </summary>
    /// <returns>Display information for the upgrade card.</returns>
    public virtual UpgradeDisplayInfo GetDisplayInfo()
    {


        UpgradeDisplayInfo info = new UpgradeDisplayInfo();
        info.name = Name;
        
        // Get the combined description of all upgrades in this card
        if (Upgrades != null && Upgrades.Count > 0)
        {
            var description = new StringBuilder();
            
            for (int i = 0; i < Upgrades.Count; i++)
            {
                description.Append(Upgrades[i].Description);
                
                // Add newline between upgrades (but not after the last one)
                if (i < Upgrades.Count - 1)
                {
                    description.AppendLine();
                }
            }
            
            info.description = description.ToString();
        }
        else
        {
            info.description = "";
        }
        
        // Set icon type based on rarity for non-ability-specific upgrades
        info.iconType = GetRarityIconType(Rarity);

        if (RequiredAbilityType != null)
        {
            info.iconType = Ability.GetDisplayInfo(RequiredAbilityType).iconType;
        }
        
        return info;
    }
    
    /// <summary>
    /// Get the CSS icon class name for a given rarity.
    /// </summary>
    /// <param name="rarity">The upgrade rarity</param>
    /// <returns>CSS class name for the rarity icon</returns>
    private string GetRarityIconType(UpgradeRarity rarity)
    {
        switch (rarity)
        {
            case UpgradeRarity.Normal:
                return "upgrade-normal";
            case UpgradeRarity.Common:
                return "upgrade-common";
            case UpgradeRarity.Epic:
                return "upgrade-epic";
            case UpgradeRarity.Legendary:
                return "upgrade-legendary";
            default:
                return "upgrade-normal";
        }
    }

    
    /// <summary>
    /// Apply all upgrades from this card to the upgrade system.
    /// </summary>
    public virtual void ApplyUpgrades()
    {
        if (Upgrades == null) return;
        
        foreach (var upgrade in Upgrades)
        {
            UpgradeSystem.AddUpgrade(upgrade);
        }

        // Notify the card that it was selected
        OnCardSelected();
    }
    
    /// <summary>
    /// Get the number of upgrades in this card.
    /// </summary>
    /// <returns>Number of upgrades.</returns>
    public int GetUpgradeCount()
    {
        return Upgrades?.Count ?? 0;
    }

    /// <summary>
    /// Get the remaining number of times this card can be picked.
    /// </summary>
    /// <returns>
    /// -1 = unlimited picks (default behavior)
    /// 0 = cannot be picked anymore
    /// >0 = specific number of remaining picks
    /// </returns>
    public virtual int GetRemainingPicks()
    {
        return RemainingPicks;
    }

    /// <summary>
    /// Called when this card is selected by the player.
    /// Decrements remaining picks if they are limited (>0).
    /// </summary>
    public virtual void OnCardSelected()
    {
        if (RemainingPicks > 0)
        {
            RemainingPicks--;
        }
    }
}
