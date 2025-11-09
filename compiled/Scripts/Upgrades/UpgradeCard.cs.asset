using System.Collections.Generic;
using System.Text;

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
    /// Description of the upgrade card.
    /// </summary>
    public string Description { get; protected set; }
    
    /// <summary>
    /// Rarity level of this upgrade card.
    /// </summary>
    public UpgradeRarity Rarity { get; protected set; }
    
    /// <summary>
    /// List of upgrades that this card provides when selected.
    /// </summary>
    public List<Upgrade> Upgrades { get; protected set; }
    
    /// <summary>
    /// Create a new upgrade card with specified upgrades and rarity.
    /// </summary>
    /// <param name="name">Display name of the card.</param>
    /// <param name="rarity">Rarity level of the card.</param>
    /// <param name="upgrades">List of upgrades this card provides.</param>
    public UpgradeCard(string name, UpgradeRarity rarity, List<Upgrade> upgrades)
    {
        Name = name;
        Rarity = rarity;
        Upgrades = upgrades ?? new List<Upgrade>();
        Description = GetDescription();
    }
    
    /// <summary>
    /// Create a new upgrade card with a single upgrade.
    /// </summary>
    /// <param name="name">Display name of the card.</param>
    /// <param name="rarity">Rarity level of the card.</param>
    /// <param name="upgrade">Single upgrade this card provides.</param>
    public UpgradeCard(string name, UpgradeRarity rarity, Upgrade upgrade)
    {
        Name = name;
        Rarity = rarity;
        Upgrades = new List<Upgrade> { upgrade };
        Description = GetDescription();
    }
    
    /// <summary>
    /// Get the combined description of all upgrades in this card.
    /// Each upgrade description is on a new line.
    /// </summary>
    /// <returns>Combined description string.</returns>
    private string GetDescription()
    {
        if (Upgrades == null || Upgrades.Count == 0)
        {
            return "";
        }
        
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
        
        return description.ToString();
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
    }
    
    /// <summary>
    /// Get the number of upgrades in this card.
    /// </summary>
    /// <returns>Number of upgrades.</returns>
    public int GetUpgradeCount()
    {
        return Upgrades?.Count ?? 0;
    }
}
