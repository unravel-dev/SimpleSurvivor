using System.Collections.Generic;
using Unravel.Core;

/// <summary>
/// Static class for generating predefined upgrade cards with thematic combinations.
/// Provides various card types with 1-2 upgrades each based on rarity.
/// </summary>
public static class UpgradeCardGenerator
{
    /// <summary>
    /// Generate a random upgrade card of the specified rarity.
    /// </summary>
    /// <param name="rarity">The rarity level of the card to generate.</param>
    /// <returns>A randomly selected upgrade card of the specified rarity.</returns>
    public static UpgradeCard GenerateRandomCard(UpgradeRarity rarity)
    {
        var availableCards = GetAvailableCards(rarity);
        int randomIndex = Random.Range(0, availableCards.Count);
        return availableCards[randomIndex]();
    }
    
    /// <summary>
    /// Generate multiple random upgrade cards.
    /// </summary>
    /// <param name="count">Number of cards to generate.</param>
    /// <param name="rarity">Rarity level for all cards.</param>
    /// <returns>List of randomly generated upgrade cards.</returns>
    public static List<UpgradeCard> GenerateRandomCards(int count, UpgradeRarity rarity)
    {
        var cards = new List<UpgradeCard>();
        for (int i = 0; i < count; i++)
        {
            cards.Add(GenerateRandomCard(rarity));
        }
        return cards;
    }
    
    /// <summary>
    /// Generate a selection of cards with mixed rarities for player choice.
    /// </summary>
    /// <param name="cardCount">Number of cards to generate (default 3).</param>
    /// <returns>List of upgrade cards with varied rarities.</returns>
    public static List<UpgradeCard> GenerateCardSelection(int cardCount = 3)
    {
        var cards = new List<UpgradeCard>();
        
        for (int i = 0; i < cardCount; i++)
        {
            // Weight rarity distribution: more common cards, fewer legendary
            UpgradeRarity rarity = GetRandomWeightedRarity();
            cards.Add(GenerateRandomCard(rarity));
        }
        
        return cards;
    }
    
    /// <summary>
    /// Get a weighted random rarity (more common cards, fewer legendary).
    /// </summary>
    /// <returns>Randomly selected rarity with weighted distribution.</returns>
    private static UpgradeRarity GetRandomWeightedRarity()
    {
        float roll = Random.Range(0f, 100f);
        
        if (roll < 50f) return UpgradeRarity.Normal;      // 50% chance
        if (roll < 80f) return UpgradeRarity.Common;      // 30% chance
        if (roll < 95f) return UpgradeRarity.Epic;        // 15% chance
        return UpgradeRarity.Legendary;                   // 5% chance
    }
    
    /// <summary>
    /// Get list of available card generators for the specified rarity.
    /// </summary>
    /// <param name="rarity">The rarity level.</param>
    /// <returns>List of functions that generate cards of the specified rarity.</returns>
    private static List<System.Func<UpgradeCard>> GetAvailableCards(UpgradeRarity rarity)
    {
        switch (rarity)
        {
            case UpgradeRarity.Normal:
                return GetNormalCards();
            case UpgradeRarity.Common:
                return GetCommonCards();
            case UpgradeRarity.Epic:
                return GetEpicCards();
            case UpgradeRarity.Legendary:
                return GetLegendaryCards();
            default:
                return GetNormalCards();
        }
    }
    
    // ===== NORMAL RARITY CARDS =====
    private static List<System.Func<UpgradeCard>> GetNormalCards()
    {
        return new List<System.Func<UpgradeCard>>
        {
            () => new UpgradeCard("Basic Power", UpgradeRarity.Normal, 
                DamageUpgrade.Generate(UpgradeRarity.Normal)),
                
            () => new UpgradeCard("Sharp Edge", UpgradeRarity.Normal, 
                PierceUpgrade.Generate(UpgradeRarity.Normal)),
                
            () => new UpgradeCard("Quick Step", UpgradeRarity.Normal, 
                MovementSpeedUpgrade.Generate(UpgradeRarity.Normal)),
                
            () => new UpgradeCard("Sturdy Build", UpgradeRarity.Normal, 
                MaxHealthUpgrade.Generate(UpgradeRarity.Normal)),
                
            () => new UpgradeCard("Fast Hands", UpgradeRarity.Normal, 
                CooldownReductionUpgrade.Generate(UpgradeRarity.Normal)),
                
            () => new UpgradeCard("Spark", UpgradeRarity.Normal, 
                ChainUpgrade.Generate(UpgradeRarity.Normal)),
                
            () => new UpgradeCard("Double Shot", UpgradeRarity.Normal, 
                ProjectileCountUpgrade.Generate(UpgradeRarity.Normal))
        };
    }
    
    // ===== COMMON RARITY CARDS =====
    private static List<System.Func<UpgradeCard>> GetCommonCards()
    {
        return new List<System.Func<UpgradeCard>>
        {
            // Single upgrade cards
            () => new UpgradeCard("Power Strike", UpgradeRarity.Common, 
                DamageUpgrade.Generate(UpgradeRarity.Common)),
                
            () => new UpgradeCard("Piercing Shot", UpgradeRarity.Common, 
                PierceUpgrade.Generate(UpgradeRarity.Common)),
                
            () => new UpgradeCard("Lightning Arc", UpgradeRarity.Common, 
                ChainUpgrade.Generate(UpgradeRarity.Common)),
                
            () => new UpgradeCard("Multi-Shot", UpgradeRarity.Common, 
                ProjectileCountUpgrade.Generate(UpgradeRarity.Common)),
                
            // Dual upgrade cards
            () => new UpgradeCard("Combat Training", UpgradeRarity.Common, 
                new List<Upgrade>
                {
                    DamageUpgrade.Generate(UpgradeRarity.Common),
                    CooldownReductionUpgrade.Generate(UpgradeRarity.Common)
                }),
                
            () => new UpgradeCard("Agile Fighter", UpgradeRarity.Common, 
                new List<Upgrade>
                {
                    MovementSpeedUpgrade.Generate(UpgradeRarity.Common),
                    MaxHealthUpgrade.Generate(UpgradeRarity.Common)
                }),
                
            () => new UpgradeCard("Precision Focus", UpgradeRarity.Common, 
                new List<Upgrade>
                {
                    PierceUpgrade.Generate(UpgradeRarity.Common),
                    DamageUpgrade.Generate(UpgradeRarity.Common)
                }),
                
            () => new UpgradeCard("Barrage Master", UpgradeRarity.Common, 
                new List<Upgrade>
                {
                    ProjectileCountUpgrade.Generate(UpgradeRarity.Common),
                    CooldownReductionUpgrade.Generate(UpgradeRarity.Common)
                })
        };
    }
    
    // ===== EPIC RARITY CARDS =====
    private static List<System.Func<UpgradeCard>> GetEpicCards()
    {
        return new List<System.Func<UpgradeCard>>
        {
            // Single powerful upgrades
            () => new UpgradeCard("Devastating Blow", UpgradeRarity.Epic, 
                DamageUpgrade.Generate(UpgradeRarity.Epic)),
                
            () => new UpgradeCard("Chain Lightning", UpgradeRarity.Epic, 
                ChainUpgrade.Generate(UpgradeRarity.Epic)),
                
            // Powerful dual combinations
            () => new UpgradeCard("Berserker's Fury", UpgradeRarity.Epic, 
                new List<Upgrade>
                {
                    DamageUpgrade.Generate(UpgradeRarity.Epic),
                    MovementSpeedUpgrade.Generate(UpgradeRarity.Epic)
                }),
                
            () => new UpgradeCard("Master Archer", UpgradeRarity.Epic, 
                new List<Upgrade>
                {
                    PierceUpgrade.Generate(UpgradeRarity.Epic),
                    ChainUpgrade.Generate(UpgradeRarity.Epic)
                }),
                
            () => new UpgradeCard("Battle Veteran", UpgradeRarity.Epic, 
                new List<Upgrade>
                {
                    MaxHealthUpgrade.Generate(UpgradeRarity.Epic),
                    CooldownReductionUpgrade.Generate(UpgradeRarity.Epic)
                }),
                
            () => new UpgradeCard("Storm Caller", UpgradeRarity.Epic, 
                new List<Upgrade>
                {
                    ChainUpgrade.Generate(UpgradeRarity.Epic),
                    CooldownReductionUpgrade.Generate(UpgradeRarity.Epic)
                }),
                
            () => new UpgradeCard("Projectile Storm", UpgradeRarity.Epic, 
                ProjectileCountUpgrade.Generate(UpgradeRarity.Epic)),
                
            () => new UpgradeCard("Artillery Barrage", UpgradeRarity.Epic, 
                new List<Upgrade>
                {
                    ProjectileCountUpgrade.Generate(UpgradeRarity.Epic),
                    DamageUpgrade.Generate(UpgradeRarity.Epic)
                })
        };
    }
    
    // ===== LEGENDARY RARITY CARDS =====
    private static List<System.Func<UpgradeCard>> GetLegendaryCards()
    {
        return new List<System.Func<UpgradeCard>>
        {
            () => new UpgradeCard("God of War", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    DamageUpgrade.Generate(UpgradeRarity.Legendary),
                    CooldownReductionUpgrade.Generate(UpgradeRarity.Legendary)
                }),
                
            () => new UpgradeCard("Lightning Emperor", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    ChainUpgrade.Generate(UpgradeRarity.Legendary),
                    DamageUpgrade.Generate(UpgradeRarity.Legendary)
                }),
                
            () => new UpgradeCard("Immortal Guardian", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    MaxHealthUpgrade.Generate(UpgradeRarity.Legendary),
                    MovementSpeedUpgrade.Generate(UpgradeRarity.Legendary)
                }),
                
            () => new UpgradeCard("Piercing Storm", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    PierceUpgrade.Generate(UpgradeRarity.Legendary),
                    ChainUpgrade.Generate(UpgradeRarity.Legendary)
                }),
                
            () => new UpgradeCard("Perfect Warrior", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    DamageUpgrade.Generate(UpgradeRarity.Legendary),
                    MaxHealthUpgrade.Generate(UpgradeRarity.Legendary)
                }),
                
            () => new UpgradeCard("Omnislash Master", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    ProjectileCountUpgrade.Generate(UpgradeRarity.Legendary),
                    PierceUpgrade.Generate(UpgradeRarity.Legendary)
                }),
                
            () => new UpgradeCard("Divine Arsenal", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    ProjectileCountUpgrade.Generate(UpgradeRarity.Legendary),
                    ChainUpgrade.Generate(UpgradeRarity.Legendary)
                })
        };
    }
}
