using System;
using System.Collections.Generic;
using Unravel.Core;

/// <summary>
/// Static class for generating balanced upgrade cards with proper rarity distribution.
/// Uses range-based generation for better control over upgrade values.
/// </summary>
public static class UpgradeCardGenerator
{
    /// <summary>
    /// Generate a random upgrade card of the specified rarity.
    /// Automatically filters out cards with 0 remaining picks.
    /// </summary>
    /// <param name="rarity">The rarity level of the card to generate.</param>
    /// <returns>A randomly selected upgrade card of the specified rarity.</returns>
    public static UpgradeCard GenerateRandomCard(UpgradeRarity rarity)
    {
        var availableCards = GetAvailableCards(rarity);
        
        // Filter out cards with 0 remaining picks
        var validCards = new List<System.Func<UpgradeCard>>();
        foreach (var cardGenerator in availableCards)
        {
            UpgradeCard testCard = cardGenerator();
            if (testCard.GetRemainingPicks() != 0) // -1 (unlimited) or >0 (has picks left)
            {
                validCards.Add(cardGenerator);
            }
        }
        
        // If no valid cards, fall back to all available cards
        if (validCards.Count == 0)
        {
            validCards = availableCards;
        }
        
        int randomIndex = Random.Range(0, validCards.Count);
        return validCards[randomIndex]();
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
    /// <param name="playerLuck">Player's current luck value (default 0 for no luck bonus).</param>
    /// <returns>List of upgrade cards with varied rarities.</returns>
    public static List<UpgradeCard> GenerateCardSelection(int cardCount = 3, float playerLuck = 0.0f)
    {
        var cards = new List<UpgradeCard>();
        
        for (int i = 0; i < cardCount; i++)
        {
            // Weight rarity distribution based on player luck
            UpgradeRarity rarity = GetRandomWeightedRarity(playerLuck);
            cards.Add(GenerateRandomCard(rarity));
        }
   
        return cards;
    }
   
    
    /// <summary>
    /// Generate upgrade card selection for level up, handling ability-only levels automatically.
    /// This method encapsulates all the logic for determining whether to show ability-only cards
    /// or regular upgrade cards based on level and current abilities.
    /// </summary>
    /// <param name="level">Current player level</param>
    /// <param name="currentAbilities">Array of current ability components the player has</param>
    /// <param name="playerLuck">Player's luck value for rarity weighting</param>
    /// <param name="maxAbilitySlots">Maximum number of ability slots (default 4)</param>
    /// <param name="cardCount">Number of cards to generate (default 3)</param>
    /// <returns>List of upgrade cards (always returns exactly cardCount cards, padded with regular upgrades if needed)</returns>
    public static List<UpgradeCard> GenerateLevelUpSelection(int level, Ability[] currentAbilities, float playerLuck = 0.0f, int maxAbilitySlots = 4, int cardCount = 3)
    {
        // Check if this is an ability-only level (every 5 levels) and if ability slots are not full
        bool isAbilityOnlyLevel = (level % 5 == 0);
        int currentAbilityCount = currentAbilities != null ? currentAbilities.Length : 0;
        bool hasAvailableAbilitySlots = currentAbilityCount < maxAbilitySlots;
        
        List<UpgradeCard> cards = new List<UpgradeCard>();
        
        if (isAbilityOnlyLevel && hasAvailableAbilitySlots)
        {
            // Get owned ability types to exclude them
            var ownedAbilityTypes = new HashSet<Type>();
            if (currentAbilities != null)
            {
                foreach (var ability in currentAbilities)
                {
                    if (ability != null)
                    {
                        ownedAbilityTypes.Add(ability.GetType());
                    }
                }
            }
            
            // Try to generate ability-only selection excluding owned abilities
            cards = GenerateAbilityOnlySelectionExcludingOwned(cardCount, ownedAbilityTypes);
            
            // If no abilities available, fall back to regular upgrade cards
            if (cards == null || cards.Count == 0)
            {
                Log.Info($"UpgradeCardGenerator: No new abilities available at level {level}, showing regular upgrade cards");
                cards = GenerateCardSelection(cardCount, playerLuck);
            }
            else
            {
                Log.Info($"UpgradeCardGenerator: Showing ability-only selection at level {level} (owned: {currentAbilityCount}/{maxAbilitySlots})");
            }
        }
        else
        {
            // Generate upgrade card options using the new system with player luck
            cards = GenerateCardSelection(cardCount, playerLuck);
        }
        
        // Ensure we have exactly cardCount cards (pad with regular upgrades if needed)
        while (cards.Count < cardCount)
        {
            var fallbackCards = GenerateCardSelection(1, playerLuck);
            if (fallbackCards.Count > 0)
            {
                cards.Add(fallbackCards[0]);
            }
            else
            {
                break; // Can't generate more cards
            }
        }
        
        return cards;
    }
    
    /// <summary>
    /// Generate a selection of ability cards only, excluding abilities the player already has.
    /// </summary>
    /// <param name="cardCount">Number of cards to generate (default 3).</param>
    /// <param name="ownedAbilityTypes">Set of ability types the player already owns.</param>
    /// <returns>List of ability upgrade cards (or empty if no abilities available).</returns>
    private static List<UpgradeCard> GenerateAbilityOnlySelectionExcludingOwned(int cardCount = 3, HashSet<Type> ownedAbilityTypes = null)
    {
        var allAbilityCards = GetAvailableAbilityCards();
        var availableAbilityCards = new List<Func<UpgradeCard>>();
        
        // Filter out abilities the player already owns
        foreach (var abilityCardGenerator in allAbilityCards)
        {
            // Generate a test card to check its ability type
            var testCard = abilityCardGenerator();
            if (testCard is AbilityCard abilityCard)
            {
                // Check if player owns this ability type
                if (ownedAbilityTypes == null || !ownedAbilityTypes.Contains(abilityCard.AbilityType))
                {
                    availableAbilityCards.Add(abilityCardGenerator);
                }
            }
        }
        
        // If no abilities available, return empty list (caller should fall back to regular upgrades)
        if (availableAbilityCards.Count == 0)
        {
            return new List<UpgradeCard>();
        }
        
        var cards = new List<UpgradeCard>();
        
        // Pick random ability cards without duplicates if possible
        var usedIndices = new HashSet<int>();
        for (int i = 0; i < cardCount && cards.Count < availableAbilityCards.Count; i++)
        {
            int randomIndex;
            int attempts = 0;
            
            // Try to avoid duplicates if we have enough unique abilities
            do
            {
                randomIndex = Random.Range(0, availableAbilityCards.Count);
                attempts++;
            } while (usedIndices.Contains(randomIndex) && attempts < 10 && availableAbilityCards.Count > cardCount);
            
            usedIndices.Add(randomIndex);
            cards.Add(availableAbilityCards[randomIndex]());
        }
        
        return cards;
    }
    
    /// <summary>
    /// Get list of all available ability card generators (basic versions without upgrades).
    /// </summary>
    /// <returns>List of functions that generate ability cards.</returns>
    private static List<System.Func<UpgradeCard>> GetAvailableAbilityCards()
    {
        return new List<System.Func<UpgradeCard>>
        {
            () => GenerateBasicLightningBoltAbilityCard(),
            () => GenerateBasicFireballAbilityCard(),
            () => GenerateBasicBoomerangBladeAbilityCard(),
            () => GenerateBasicMeteorShowerAbilityCard(),
            () => GenerateBasicBlackHoleAbilityCard()
        };
    }
    
    /// <summary>
    /// Get a weighted random rarity (more common cards, fewer legendary).
    /// </summary>
    /// <returns>Randomly selected rarity with weighted distribution.</returns>
    private static UpgradeRarity GetRandomWeightedRarity()
    {
        return GetRandomWeightedRarity(0.0f);
    }
    
    /// <summary>
    /// Get a weighted random rarity affected by player luck.
    /// Higher luck increases chances of better rarities.
    /// </summary>
    /// <param name="playerLuck">Player's current luck value.</param>
    /// <returns>Randomly selected rarity with luck-modified distribution.</returns>
    private static UpgradeRarity GetRandomWeightedRarity(float playerLuck)
    {
        // Apply luck upgrades to the base luck
        float finalLuck = UpgradeSystem.ApplyLuckUpgrade(playerLuck);
        
        // Calculate luck bonus (each point of luck shifts probabilities by 0.5%)
        float luckBonus = finalLuck * 0.5f;
        
        // Base probabilities
        float normalChance = 50.0f;
        float commonChance = 30.0f;
        float epicChance = 15.0f;
        float legendaryChance = 5.0f;
        
        // Shift probabilities based on luck (higher rarities get more benefit)
        normalChance = Mathf.Max(10.0f, normalChance - luckBonus * 1.5f);        // Reduce normal chance more
        commonChance = Mathf.Max(10.0f, commonChance - luckBonus * 0.8f);        // Reduce common chance less
        epicChance = Mathf.Min(40.0f, epicChance + luckBonus * 1.2f);            // Increase epic chance
        legendaryChance = Mathf.Min(25.0f, legendaryChance + luckBonus * 1.1f);  // Increase legendary chance
        
        // Normalize to ensure total is 100%
        float total = normalChance + commonChance + epicChance + legendaryChance;
        normalChance = (normalChance / total) * 100.0f;
        commonChance = (commonChance / total) * 100.0f;
        epicChance = (epicChance / total) * 100.0f;
        legendaryChance = (legendaryChance / total) * 100.0f;
        
        // Roll for rarity
        float roll = Random.Range(0f, 100f);
        
        if (roll < normalChance) return UpgradeRarity.Normal;
        if (roll < normalChance + commonChance) return UpgradeRarity.Common;
        if (roll < normalChance + commonChance + epicChance) return UpgradeRarity.Epic;
        return UpgradeRarity.Legendary;
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
    // Basic upgrades with small bonuses
    private static List<System.Func<UpgradeCard>> GetNormalCards()
    {
        return new List<System.Func<UpgradeCard>>
        {
            // Basic damage boost
            () => new UpgradeCard("Power Up", UpgradeRarity.Normal, 
                DamageUpgrade.Generate(5.0f, 10.0f)),
                
            // Basic health boost
            () => new UpgradeCard("Vitality", UpgradeRarity.Normal, 
                MaxHealthUpgrade.Generate(15, 25)),
                
            // Basic speed boost
            () => new UpgradeCard("Swift Feet", UpgradeRarity.Normal, 
                MovementSpeedUpgrade.Generate(8.0f, 15.0f)),
                
            // Basic cooldown reduction
            () => new UpgradeCard("Quick Hands", UpgradeRarity.Normal, 
                CooldownReductionUpgrade.Generate(5.0f, 10.0f)),
                
            // Basic pickup radius
            () => new UpgradeCard("Magnetism", UpgradeRarity.Normal, 
                PickupRadiusUpgrade.Generate(45.0f, 65.0f)),
                
            // Basic luck
            () => new UpgradeCard("Fortune", UpgradeRarity.Normal, 
                LuckUpgrade.Generate(10.0f, 20.0f)),
                
            // Basic critical chance
            () => new UpgradeCard("Lucky Strike", UpgradeRarity.Normal, 
                CriticalChanceUpgrade.Generate(2.0f, 5.0f)),
                
            // Basic critical damage
            () => new UpgradeCard("Sharp Edge", UpgradeRarity.Normal, 
                CriticalDamageUpgrade.Generate(15.0f, 25.0f)),
                
            // Basic area of effect
            () => new UpgradeCard("Wider Impact", UpgradeRarity.Normal, 
                AreaOfEffectUpgrade.Generate(10.0f, 20.0f))
        };
    }
    
    // ===== COMMON RARITY CARDS =====
    // Better single upgrades and some dual combinations
    private static List<System.Func<UpgradeCard>> GetCommonCards()
    {
        return new List<System.Func<UpgradeCard>>
        {
            // Better single upgrades
            () => new UpgradeCard("Power Strike", UpgradeRarity.Common, 
                DamageUpgrade.Generate(12.0f, 18.0f)),
                
            () => new UpgradeCard("Robust Health", UpgradeRarity.Common, 
                MaxHealthUpgrade.Generate(30, 45)),
                
            () => new UpgradeCard("Fleet Footed", UpgradeRarity.Common, 
                MovementSpeedUpgrade.Generate(18.0f, 25.0f)),
                
            () => new UpgradeCard("Dexterity", UpgradeRarity.Common, 
                CooldownReductionUpgrade.Generate(12.0f, 18.0f)),
                
            () => new UpgradeCard("Item Magnet", UpgradeRarity.Common, 
                PickupRadiusUpgrade.Generate(90.0f, 100.0f)),
                
            () => new UpgradeCard("Good Fortune", UpgradeRarity.Common, 
                LuckUpgrade.Generate(25.0f, 35.0f)),
                
            () => new UpgradeCard("Critical Focus", UpgradeRarity.Common, 
                CriticalChanceUpgrade.Generate(6.0f, 10.0f)),
                
            () => new UpgradeCard("Devastating Strike", UpgradeRarity.Common, 
                CriticalDamageUpgrade.Generate(30.0f, 45.0f)),
                
            () => new UpgradeCard("Lasting Power", UpgradeRarity.Common, 
                DurationUpgrade.Generate(15.0f, 25.0f)),
                
            // Dual upgrade combinations
            () => new UpgradeCard("Combat Training", UpgradeRarity.Common, 
                new List<Upgrade>
                {
                    DamageUpgrade.Generate(8.0f, 12.0f),
                    CooldownReductionUpgrade.Generate(8.0f, 12.0f)
                }),
                
            () => new UpgradeCard("Agile Fighter", UpgradeRarity.Common, 
                new List<Upgrade>
                {
                    MovementSpeedUpgrade.Generate(12.0f, 18.0f),
                    MaxHealthUpgrade.Generate(20, 30)
                }),
                
            () => new UpgradeCard("Critical Mastery", UpgradeRarity.Common, 
                new List<Upgrade>
                {
                    CriticalChanceUpgrade.Generate(4.0f, 6.0f),
                    CriticalDamageUpgrade.Generate(20.0f, 30.0f)
                }),
                
            () => new UpgradeCard("Treasure Hunter", UpgradeRarity.Common, 
                new List<Upgrade>
                {
                    PickupRadiusUpgrade.Generate(120.0f, 150.0f),
                    LuckUpgrade.Generate(15.0f, 25.0f)
                }),
                
            // Better area of effect
            () => new UpgradeCard("Expanding Radius", UpgradeRarity.Common, 
                AreaOfEffectUpgrade.Generate(25.0f, 40.0f)),
                
            // AOE combo with damage
            () => new UpgradeCard("Explosive Power", UpgradeRarity.Common, 
                new List<Upgrade>
                {
                    AreaOfEffectUpgrade.Generate(20.0f, 30.0f),
                    DamageUpgrade.Generate(10.0f, 15.0f)
                }),
                
            // Boomerang Blade upgrades (Common) - Limited to 1 pick each
            () => new UpgradeCard("Multiple Blades", UpgradeRarity.Common, 
                MultipleBladesUpgrade.Generate(1, 2), 1),
                
            () => new UpgradeCard("Faster Rotation", UpgradeRarity.Common, 
                FasterRotationUpgrade.Generate(50.0f, 75.0f), 1),
                
            // Black Hole upgrades (Common)
            () => new UpgradeCard("Cursed Vortex", UpgradeRarity.Common, 
                IncreaseDoomStacksUpgrade.Generate(1, 2)),
                
            () => new UpgradeCard("Gravitational Pull", UpgradeRarity.Common, 
                IncreasePullStrengthUpgrade.Generate(30.0f, 50.0f)),
                
            () => new UpgradeCard("Amplified Doom", UpgradeRarity.Common, 
                IncreaseDoomDamagePerStackUpgrade.Generate(20.0f, 35.0f)),
                
            // Fireball upgrades (Common)
            () => new UpgradeCard("Igniting Strike", UpgradeRarity.Common, 
                BurnOnHitUpgrade.Generate(30.0f, 50.0f, 1, 1))
        };
    }
    
    // ===== EPIC RARITY CARDS =====
    // Powerful upgrades including projectile mechanics and first abilities
    private static List<System.Func<UpgradeCard>> GetEpicCards()
    {
        return new List<System.Func<UpgradeCard>>
        {
            // Powerful single upgrades
            () => new UpgradeCard("Devastating Power", UpgradeRarity.Epic, 
                DamageUpgrade.Generate(25.0f, 35.0f)),
                
            () => new UpgradeCard("Iron Constitution", UpgradeRarity.Epic, 
                MaxHealthUpgrade.Generate(60, 80)),
                
            () => new UpgradeCard("Lightning Speed", UpgradeRarity.Epic, 
                MovementSpeedUpgrade.Generate(35.0f, 50.0f)),
                
            () => new UpgradeCard("Master's Reflexes", UpgradeRarity.Epic, 
                CooldownReductionUpgrade.Generate(25.0f, 35.0f)),
                
            // First appearance of multicast upgrades
            () => new UpgradeCard("Multi-Cast", UpgradeRarity.Epic, 
                MulticastUpgrade.Generate(50.0f, 100.0f)),
                
            () => new UpgradeCard("Piercing Shot", UpgradeRarity.Epic, 
                PierceUpgrade.Generate(1, 2)),
                
            () => new UpgradeCard("Chain Lightning", UpgradeRarity.Epic, 
                ChainUpgrade.Generate(1, 3)),
                
            () => new UpgradeCard("Enduring Power", UpgradeRarity.Epic, 
                DurationUpgrade.Generate(30.0f, 50.0f)),
                
            // Powerful dual combinations
            () => new UpgradeCard("Berserker's Fury", UpgradeRarity.Epic, 
                new List<Upgrade>
                {
                    DamageUpgrade.Generate(20.0f, 30.0f),
                    MovementSpeedUpgrade.Generate(25.0f, 35.0f)
                }),
                
            () => new UpgradeCard("Assassin's Edge", UpgradeRarity.Epic, 
                new List<Upgrade>
                {
                    CriticalChanceUpgrade.Generate(8.0f, 12.0f),
                    CriticalDamageUpgrade.Generate(40.0f, 60.0f)
                }),
                
            () => new UpgradeCard("Battle Veteran", UpgradeRarity.Epic, 
                new List<Upgrade>
                {
                    MaxHealthUpgrade.Generate(40, 60),
                    CooldownReductionUpgrade.Generate(15.0f, 25.0f)
                }),
                
            () => new UpgradeCard("Arcane Barrage", UpgradeRarity.Epic, 
                new List<Upgrade>
                {
                    MulticastUpgrade.Generate(75.0f, 125.0f),
                    DamageUpgrade.Generate(15.0f, 25.0f)
                }),
                
            // Powerful area of effect
            () => new UpgradeCard("Massive Blast", UpgradeRarity.Epic, 
                AreaOfEffectUpgrade.Generate(50.0f, 75.0f)),
                
            // AOE combo with multicast
            () => new UpgradeCard("Cataclysm", UpgradeRarity.Epic, 
                new List<Upgrade>
                {
                    AreaOfEffectUpgrade.Generate(40.0f, 60.0f),
                    MulticastUpgrade.Generate(50.0f, 100.0f)
                }),
                
            // Boomerang Blade upgrades (Epic) - Limited to 1 pick each
            () => new UpgradeCard("Ping-Pong Orbit", UpgradeRarity.Epic, 
                PingPongOrbitUpgrade.Generate(50.0f, 100.0f, 50.0f, 100.0f), 1),
                
            () => new UpgradeCard("Dual Orbit", UpgradeRarity.Epic, 
                new DualOrbitUpgrade(), 1),
                
            () => new UpgradeCard("Returning Blade", UpgradeRarity.Epic, 
                ReturningBladeUpgrade.Generate(), 1),
                
            () => new UpgradeCard("Spinning Slash", UpgradeRarity.Epic, 
                SpinningSlashUpgrade.Generate(75.0f, 125.0f, 20.0f, 35.0f), 1),
                
            () => new UpgradeCard("Multiple Blades+", UpgradeRarity.Epic, 
                MultipleBladesUpgrade.Generate(2, 4), 1),
                
            () => new UpgradeCard("Faster Rotation+", UpgradeRarity.Epic, 
                FasterRotationUpgrade.Generate(75.0f, 125.0f), 1),
                
            // Black Hole upgrades (Epic)
            () => new UpgradeCard("Cursed Vortex+", UpgradeRarity.Epic, 
                IncreaseDoomStacksUpgrade.Generate(2, 4)),
                
            () => new UpgradeCard("Gravitational Pull+", UpgradeRarity.Epic, 
                IncreasePullStrengthUpgrade.Generate(60.0f, 90.0f)),
                
            () => new UpgradeCard("Amplified Doom+", UpgradeRarity.Epic, 
                IncreaseDoomDamagePerStackUpgrade.Generate(40.0f, 60.0f)),
                
            // Fireball upgrades (Epic)
            () => new UpgradeCard("Burning Impact", UpgradeRarity.Epic, 
                BurnOnHitUpgrade.Generate(60.0f, 80.0f, 1, 2)),
                
            () => new UpgradeCard("Inferno Strike", UpgradeRarity.Epic, 
                BurnOnHitUpgrade.Generate(70.0f, 90.0f, 2, 3))
        };
    }
    
    // ===== LEGENDARY RARITY CARDS =====
    // Game-changing combinations
    private static List<System.Func<UpgradeCard>> GetLegendaryCards()
    {
        return new List<System.Func<UpgradeCard>>
        {
            () => new UpgradeCard("God of War", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    DamageUpgrade.Generate(40.0f, 60.0f),
                    CooldownReductionUpgrade.Generate(30.0f, 45.0f)
                }),
                
            () => new UpgradeCard("Lightning Emperor", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    ChainUpgrade.Generate(3, 6),
                    DamageUpgrade.Generate(30.0f, 45.0f)
                }),
                
            () => new UpgradeCard("Immortal Guardian", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    MaxHealthUpgrade.Generate(100, 150),
                    MovementSpeedUpgrade.Generate(40.0f, 60.0f)
                }),
                
            () => new UpgradeCard("Piercing Storm", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    PierceUpgrade.Generate(3, 5),
                    ChainUpgrade.Generate(2, 4)
                }),
                
            () => new UpgradeCard("Perfect Warrior", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    DamageUpgrade.Generate(35.0f, 50.0f),
                    MaxHealthUpgrade.Generate(80, 120)
                }),
                
            () => new UpgradeCard("Omnislash Master", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    MulticastUpgrade.Generate(150.0f, 250.0f),
                    PierceUpgrade.Generate(2, 4)
                }),
                
            () => new UpgradeCard("Divine Arsenal", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    MulticastUpgrade.Generate(200.0f, 300.0f),
                    ChainUpgrade.Generate(2, 4)
                }),
                
            () => new UpgradeCard("Godslayer", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    CriticalChanceUpgrade.Generate(15.0f, 25.0f),
                    CriticalDamageUpgrade.Generate(80.0f, 120.0f)
                }),
                
            // Legendary AOE combinations
            () => new UpgradeCard("World Ender", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    AreaOfEffectUpgrade.Generate(80.0f, 120.0f),
                    DamageUpgrade.Generate(40.0f, 60.0f)
                }),
                
            () => new UpgradeCard("Nuclear Blast", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    AreaOfEffectUpgrade.Generate(100.0f, 150.0f),
                    MulticastUpgrade.Generate(150.0f, 250.0f)
                }),
            
            // Boomerang Blade upgrades (Legendary) - Limited to 1 pick each
            () => new UpgradeCard("Master Bladesman", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    MultipleBladesUpgrade.Generate(3, 5),
                    FasterRotationUpgrade.Generate(100.0f, 150.0f),
                    SpinningSlashUpgrade.Generate(100.0f, 150.0f, 30.0f, 50.0f)
                }, 1),
                
            () => new UpgradeCard("Perfect Orbit", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    PingPongOrbitUpgrade.Generate(100.0f, 150.0f, 100.0f, 150.0f),
                    new DualOrbitUpgrade(),
                    ReturningBladeUpgrade.Generate()
                }, 1),
                
            // Black Hole upgrades (Legendary)
            () => new UpgradeCard("Singularity", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    IncreaseDoomStacksUpgrade.Generate(4, 6),
                    IncreasePullStrengthUpgrade.Generate(100.0f, 150.0f),
                    IncreaseDoomDamagePerStackUpgrade.Generate(60.0f, 100.0f)
                }),
            
            // // Lightning Bolt upgrades (Legendary) - Limited to 1 pick
            // () => new UpgradeCard("Forked Lightning", UpgradeRarity.Legendary, 
            //     new List<Upgrade>
            //     {
            //         LightningSplitUpgrade.Generate(2, 3, 8.0f, 12.0f)
            //     }, 1),
                
            () => new UpgradeCard("Eternal Void", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    IncreasePullStrengthUpgrade.Generate(80.0f, 120.0f)
                })
        };
    }
    
    // ===== ABILITY CARD GENERATORS =====
    
    /// <summary>
    /// Generate a basic lightning bolt ability card without additional upgrades (for initial selection).
    /// </summary>
    /// <returns>AbilityCard with basic LightningBoltAbility</returns>
    private static AbilityCard GenerateBasicLightningBoltAbilityCard()
    {
        return new AbilityCard(
            "Lightning Bolt", 
            LightningBoltAbility.GetDescription(),
            UpgradeRarity.Common,
            new List<Upgrade>(),
            typeof(LightningBoltAbility),
            (ability) => {
                var lightningBoltAbility = ability as LightningBoltAbility;
                if (lightningBoltAbility != null)
                {
                    LightningBoltAbility.ConfigureAbility(lightningBoltAbility);
                }
            }
        );
    }
    
    /// <summary>
    /// Generate a basic fireball ability card without additional upgrades (for initial selection).
    /// </summary>
    /// <returns>AbilityCard with basic FireballAbility</returns>
    private static AbilityCard GenerateBasicFireballAbilityCard()
    {
        return new AbilityCard(
            "Fireball", 
            FireballAbility.GetDescription(),
            UpgradeRarity.Common,
            new List<Upgrade>(),
            typeof(FireballAbility),
            (ability) => {
                var fireballAbility = ability as FireballAbility;
                if (fireballAbility != null)
                {
                    FireballAbility.ConfigureAbility(fireballAbility);
                }
            }
        );
    }
    
    /// <summary>
    /// Generate a basic boomerang blade ability card without additional upgrades (for initial selection).
    /// </summary>
    /// <returns>AbilityCard with basic BoomerangBladeAbility</returns>
    private static AbilityCard GenerateBasicBoomerangBladeAbilityCard()
    {
        return new AbilityCard(
            "Boomerang Blade", 
            BoomerangBladeAbility.GetDescription(),
            UpgradeRarity.Common,
            new List<Upgrade>(),
            typeof(BoomerangBladeAbility),
            (ability) => {
                var boomerangAbility = ability as BoomerangBladeAbility;
                if (boomerangAbility != null)
                {
                    BoomerangBladeAbility.ConfigureAbility(boomerangAbility);
                }
            }
        );
    }
    
    /// <summary>
    /// Generate a basic meteor shower ability card without additional upgrades (for initial selection).
    /// </summary>
    /// <returns>AbilityCard with basic MeteorShowerAbility</returns>
    private static AbilityCard GenerateBasicMeteorShowerAbilityCard()
    {
        return new AbilityCard(
            "Meteor Shower", 
            MeteorShowerAbility.GetDescription(),
            UpgradeRarity.Common,
            new List<Upgrade>(),
            typeof(MeteorShowerAbility),
            (ability) => {
                var meteorAbility = ability as MeteorShowerAbility;
                if (meteorAbility != null)
                {
                    MeteorShowerAbility.ConfigureAbility(meteorAbility);
                }
            }
        );
    }
    
    /// <summary>
    /// Generate a basic black hole ability card without additional upgrades (for initial selection).
    /// </summary>
    /// <returns>AbilityCard with basic BlackHoleAbility</returns>
    private static AbilityCard GenerateBasicBlackHoleAbilityCard()
    {
        return new AbilityCard(
            "Black Hole", 
            BlackHoleAbility.GetDescription(),
            UpgradeRarity.Common,
            new List<Upgrade>(),
            typeof(BlackHoleAbility),
            (ability) => {
                var blackHoleAbility = ability as BlackHoleAbility;
                if (blackHoleAbility != null)
                {
                    BlackHoleAbility.ConfigureAbility(blackHoleAbility);
                }
            }
        );
    }
    
}