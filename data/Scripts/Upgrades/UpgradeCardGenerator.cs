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
    /// If we run out of ability cards, fills remaining slots with regular upgrade cards.
    /// </summary>
    /// <param name="cardCount">Number of cards to generate (default 3).</param>
    /// <param name="ownedAbilityTypes">Set of ability types the player already owns.</param>
    /// <returns>List of upgrade cards (ability cards + regular upgrades if needed).</returns>
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
        
        var cards = new List<UpgradeCard>();
        
        // Pick random ability cards without duplicates by removing from the pool
        for (int i = 0; i < cardCount; i++)
        {
            // If we still have ability cards available, pick one
            if (availableAbilityCards.Count > 0)
            {
                int randomIndex = Random.Range(0, availableAbilityCards.Count);
                cards.Add(availableAbilityCards[randomIndex]());
                
                // Remove the chosen card from the pool to prevent duplicates
                availableAbilityCards.RemoveAt(randomIndex);
            }

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
            () => GenerateBasicBlackHoleAbilityCard(),
            () => GenerateBasicPlagueAbilityCard()
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
    /// Check if the player has a specific ability type.
    /// </summary>
    /// <param name="abilityType">The ability type to check for.</param>
    /// <returns>True if the player has the ability, false otherwise.</returns>
    private static bool PlayerHasAbility(Type abilityType)
    {
        var playerEntity = Scene.FindEntityByName("Player");
        if (!playerEntity)
            return false;
        
        return playerEntity.GetComponentInChildren(abilityType) != null;
    }

    /// <summary>
    /// Get list of available card generators for the specified rarity.
    /// Filters out ability-specific upgrades if the player doesn't have the corresponding ability.
    /// </summary>
    /// <param name="rarity">The rarity level.</param>
    /// <returns>List of functions that generate cards of the specified rarity.</returns>
    private static List<System.Func<UpgradeCard>> GetAvailableCards(UpgradeRarity rarity)
    {
        List<System.Func<UpgradeCard>> allCards;
        
        switch (rarity)
        {
            case UpgradeRarity.Normal:
                allCards = GetNormalCards();
                break;
            case UpgradeRarity.Common:
                allCards = GetCommonCards();
                break;
            case UpgradeRarity.Epic:
                allCards = GetEpicCards();
                break;
            case UpgradeRarity.Legendary:
                allCards = GetLegendaryCards();
                break;
            default:
                allCards = GetNormalCards();
                break;
        }
        
        // Filter out ability-specific upgrades if player doesn't have the corresponding ability
        var filteredCards = new List<System.Func<UpgradeCard>>();
        foreach (var cardGenerator in allCards)
        {
            // Generate a test card to check its required ability type
            UpgradeCard testCard = cardGenerator();
            
            // If card has a required ability type, check if player has it
            if (testCard.RequiredAbilityType != null)
            {
                if (!PlayerHasAbility(testCard.RequiredAbilityType))
                {
                    continue; // Skip this card if player doesn't have the required ability
                }
            }
            
            filteredCards.Add(cardGenerator);
        }
        
        return filteredCards;
    }
    
    // ===== NORMAL RARITY CARDS =====
    // Basic upgrades with small bonuses
    private static List<System.Func<UpgradeCard>> GetNormalCards()
    {
        return new List<System.Func<UpgradeCard>>
        {
            // Basic damage boost - reduced from 5-10% to 4-7%
            () => new UpgradeCard("Power Up", UpgradeRarity.Normal, 
                DamageUpgrade.Generate(4.0f, 7.0f)),
                
            // Basic health boost - reduced from 15-25 to 10-20
            () => new UpgradeCard("Vitality", UpgradeRarity.Normal, 
                MaxHealthUpgrade.Generate(10, 20)),
                
            // Basic speed boost - reduced from 8-15% to 5-10%
            () => new UpgradeCard("Swift Feet", UpgradeRarity.Normal, 
                MovementSpeedUpgrade.Generate(5.0f, 10.0f)),
                
            // Basic cooldown reduction - reduced from 5-10% to 3-6%
            () => new UpgradeCard("Quick Hands", UpgradeRarity.Normal, 
                CooldownReductionUpgrade.Generate(3.0f, 6.0f)),
                
            // Basic pickup radius - kept similar for QoL
            () => new UpgradeCard("Magnetism", UpgradeRarity.Normal, 
                PickupRadiusUpgrade.Generate(40.0f, 60.0f)),
                
            // Basic luck - reduced from 10-20% to 8-15%
            () => new UpgradeCard("Fortune", UpgradeRarity.Normal, 
                LuckUpgrade.Generate(8.0f, 15.0f)),
                
            // Basic critical chance - kept low for balance
            () => new UpgradeCard("Lucky Strike", UpgradeRarity.Normal, 
                CriticalChanceUpgrade.Generate(2.0f, 4.0f)),
                
            // Basic critical damage - reduced from 15-25% to 10-20%
            () => new UpgradeCard("Sharp Edge", UpgradeRarity.Normal, 
                CriticalDamageUpgrade.Generate(10.0f, 20.0f)),
                
            // Basic area of effect - reduced from 10-20% to 8-15%
            () => new UpgradeCard("Wider Impact", UpgradeRarity.Normal, 
                AreaOfEffectUpgrade.Generate(8.0f, 15.0f))
        };
    }
    
    // ===== COMMON RARITY CARDS =====
    // Better single upgrades and some dual combinations
    private static List<System.Func<UpgradeCard>> GetCommonCards()
    {
        return new List<System.Func<UpgradeCard>>
        {
            // Better single upgrades - reduced for better scaling
            () => new UpgradeCard("Power Strike", UpgradeRarity.Common, 
                DamageUpgrade.Generate(8.0f, 12.0f)),
                
            () => new UpgradeCard("Robust Health", UpgradeRarity.Common, 
                MaxHealthUpgrade.Generate(25, 35)),
                
            () => new UpgradeCard("Fleet Footed", UpgradeRarity.Common, 
                MovementSpeedUpgrade.Generate(12.0f, 18.0f)),
                
            () => new UpgradeCard("Dexterity", UpgradeRarity.Common, 
                CooldownReductionUpgrade.Generate(8.0f, 12.0f)),
                
            () => new UpgradeCard("Item Magnet", UpgradeRarity.Common, 
                PickupRadiusUpgrade.Generate(80.0f, 100.0f)),
                
            () => new UpgradeCard("Good Fortune", UpgradeRarity.Common, 
                LuckUpgrade.Generate(18.0f, 28.0f)),
                
            () => new UpgradeCard("Critical Focus", UpgradeRarity.Common, 
                CriticalChanceUpgrade.Generate(4.0f, 7.0f)),
                
            () => new UpgradeCard("Devastating Strike", UpgradeRarity.Common, 
                CriticalDamageUpgrade.Generate(25.0f, 35.0f)),
                
            () => new UpgradeCard("Lasting Power", UpgradeRarity.Common, 
                DurationUpgrade.Generate(12.0f, 20.0f)),
                
            // Dual upgrade combinations - reduced to avoid power creep
            () => new UpgradeCard("Combat Training", UpgradeRarity.Common, 
                new List<Upgrade>
                {
                    DamageUpgrade.Generate(5.0f, 8.0f),
                    CooldownReductionUpgrade.Generate(5.0f, 8.0f)
                }),
                
            () => new UpgradeCard("Agile Fighter", UpgradeRarity.Common, 
                new List<Upgrade>
                {
                    MovementSpeedUpgrade.Generate(8.0f, 12.0f),
                    MaxHealthUpgrade.Generate(15, 25)
                }),
                
            () => new UpgradeCard("Critical Mastery", UpgradeRarity.Common, 
                new List<Upgrade>
                {
                    CriticalChanceUpgrade.Generate(3.0f, 5.0f),
                    CriticalDamageUpgrade.Generate(15.0f, 25.0f)
                }),
                
            () => new UpgradeCard("Treasure Hunter", UpgradeRarity.Common, 
                new List<Upgrade>
                {
                    PickupRadiusUpgrade.Generate(100.0f, 130.0f),
                    LuckUpgrade.Generate(12.0f, 20.0f)
                }),
                
            // Better area of effect - reduced from 25-40% to 18-30%
            () => new UpgradeCard("Expanding Radius", UpgradeRarity.Common, 
                AreaOfEffectUpgrade.Generate(18.0f, 30.0f)),
                
            // AOE combo with damage - reduced
            () => new UpgradeCard("Explosive Power", UpgradeRarity.Common, 
                new List<Upgrade>
                {
                    AreaOfEffectUpgrade.Generate(15.0f, 22.0f),
                    DamageUpgrade.Generate(6.0f, 10.0f)
                }),
                
   
            () => new UpgradeCard("Faster Rotation", UpgradeRarity.Common, 
                FasterRotationUpgrade.Generate(20.0f, 30.0f), 1, typeof(BoomerangBladeAbility)),
                
            // Lightning Bolt upgrades (Common)
            () => new UpgradeCard("Stun Chain", UpgradeRarity.Common, 
                LightningStunUpgrade.Generate(0.5f, 1.0f), -1, typeof(LightningBoltAbility)),
                
            // Black Hole upgrades (Common)
            () => new UpgradeCard("Cursed Vortex", UpgradeRarity.Common, 
                IncreaseDoomStacksUpgrade.Generate(1, 2), -1, typeof(BlackHoleAbility)),
            
            () => new UpgradeCard("Gravitational Pull", UpgradeRarity.Common, 
                IncreasePullStrengthUpgrade.Generate(30.0f, 50.0f), -1, typeof(BlackHoleAbility)),
            
            () => new UpgradeCard("Amplified Doom", UpgradeRarity.Common, 
                IncreaseDoomDamagePerStackUpgrade.Generate(20.0f, 35.0f), -1, typeof(BlackHoleAbility)),
                
            // Fireball upgrades (Common)
            () => new UpgradeCard("Igniting Strike", UpgradeRarity.Common, 
                BurnOnHitUpgrade.Generate(30.0f, 50.0f, 1, 1), -1, typeof(FireballAbility)),
            
            // Plague upgrades (Common)
            () => new UpgradeCard("Rapid Decay", UpgradeRarity.Common, 
                FasterPlagueTickUpgrade.Generate(25.0f, 40.0f), -1, typeof(PlagueAbility)),
            
            () => new UpgradeCard("Toxic Strike", UpgradeRarity.Common, 
                PlaguePoisonUpgrade.Generate(25.0f, 40.0f, 1, 1), -1, typeof(PlagueAbility)),
            
            () => new UpgradeCard("Enduring Plague", UpgradeRarity.Common, 
                ExtendedPlagueDurationUpgrade.Generate(40.0f, 60.0f), -1, typeof(PlagueAbility)),
            
            () => new UpgradeCard("Life Drain", UpgradeRarity.Common, 
                PlagueLifeDrainUpgrade.Generate(15.0f, 25.0f, 1, 2), -1, typeof(PlagueAbility))
        };
    }
    
    // ===== EPIC RARITY CARDS =====
    // Powerful upgrades including projectile mechanics - balanced for long runs
    private static List<System.Func<UpgradeCard>> GetEpicCards()
    {
        return new List<System.Func<UpgradeCard>>
        {
            // Powerful single upgrades - significantly reduced
            () => new UpgradeCard("Devastating Power", UpgradeRarity.Epic, 
                DamageUpgrade.Generate(15.0f, 22.0f)),
                
            () => new UpgradeCard("Iron Constitution", UpgradeRarity.Epic, 
                MaxHealthUpgrade.Generate(45, 60)),
                
            () => new UpgradeCard("Lightning Speed", UpgradeRarity.Epic, 
                MovementSpeedUpgrade.Generate(22.0f, 32.0f)),
                
            () => new UpgradeCard("Master's Reflexes", UpgradeRarity.Epic, 
                CooldownReductionUpgrade.Generate(15.0f, 22.0f)),
                
            // Multicast - heavily reduced from 50-100% to 25-40%
            () => new UpgradeCard("Multi-Cast", UpgradeRarity.Epic, 
                MulticastUpgrade.Generate(25.0f, 40.0f)),
                
            // Pierce - kept at 1-2 (good value)
            () => new UpgradeCard("Piercing Shot", UpgradeRarity.Epic, 
                PierceUpgrade.Generate(1, 2)),
                
            // Chain - reduced from 1-3 to 1-2
            () => new UpgradeCard("Chain Lightning", UpgradeRarity.Epic, 
                ChainUpgrade.Generate(1, 2), -1, typeof(LightningBoltAbility)),
                
            () => new UpgradeCard("Enduring Power", UpgradeRarity.Epic, 
                DurationUpgrade.Generate(22.0f, 35.0f)),
                
            // Powerful dual combinations - significantly reduced
            () => new UpgradeCard("Berserker's Fury", UpgradeRarity.Epic, 
                new List<Upgrade>
                {
                    DamageUpgrade.Generate(12.0f, 18.0f),
                    MovementSpeedUpgrade.Generate(15.0f, 22.0f)
                }),
                
            () => new UpgradeCard("Assassin's Edge", UpgradeRarity.Epic, 
                new List<Upgrade>
                {
                    CriticalChanceUpgrade.Generate(6.0f, 10.0f),
                    CriticalDamageUpgrade.Generate(30.0f, 45.0f)
                }),
                
            () => new UpgradeCard("Battle Veteran", UpgradeRarity.Epic, 
                new List<Upgrade>
                {
                    MaxHealthUpgrade.Generate(30, 45),
                    CooldownReductionUpgrade.Generate(10.0f, 16.0f)
                }),
                
            // Multicast combo - heavily reduced
            () => new UpgradeCard("Arcane Barrage", UpgradeRarity.Epic, 
                new List<Upgrade>
                {
                    MulticastUpgrade.Generate(30.0f, 50.0f),
                    DamageUpgrade.Generate(8.0f, 14.0f)
                }),
                
            // Area of effect - reduced from 50-75% to 35-50%
            () => new UpgradeCard("Massive Blast", UpgradeRarity.Epic, 
                AreaOfEffectUpgrade.Generate(35.0f, 50.0f)),
                
            // AOE combo - reduced
            () => new UpgradeCard("Cataclysm", UpgradeRarity.Epic, 
                new List<Upgrade>
                {
                    AreaOfEffectUpgrade.Generate(25.0f, 38.0f),
                    MulticastUpgrade.Generate(20.0f, 35.0f)
                }),
                
            // Boomerang Blade upgrades (Epic) - Limited to 1 pick each
            () => new UpgradeCard("Ping-Pong Orbit", UpgradeRarity.Epic, 
                PingPongOrbitUpgrade.Generate(50.0f, 100.0f, 50.0f, 100.0f), 1, typeof(BoomerangBladeAbility)),
            
            () => new UpgradeCard("Dual Orbit", UpgradeRarity.Epic, 
                new DualOrbitUpgrade(), 1, typeof(BoomerangBladeAbility)),
            
            () => new UpgradeCard("Returning Blade", UpgradeRarity.Epic, 
                ReturningBladeUpgrade.Generate(), 1, typeof(BoomerangBladeAbility)),
            
            () => new UpgradeCard("Spinning Slash", UpgradeRarity.Epic, 
                SpinningSlashUpgrade.Generate(75.0f, 125.0f, 20.0f, 35.0f), 1, typeof(BoomerangBladeAbility)),
            
            () => new UpgradeCard("Multiple Blades+", UpgradeRarity.Epic, 
                MultipleBladesUpgrade.Generate(1, 2), 2, typeof(BoomerangBladeAbility)),
            
            () => new UpgradeCard("Faster Rotation+", UpgradeRarity.Epic, 
                FasterRotationUpgrade.Generate(75.0f, 125.0f), 1, typeof(BoomerangBladeAbility)),
                
            // Lightning Bolt upgrades (Epic)
            () => new UpgradeCard("Chain Explosion", UpgradeRarity.Epic, 
                LightningChainExplosionUpgrade.Generate(2.0f, 3.0f, 40.0f, 60.0f), -1, typeof(LightningBoltAbility)),
                
            () => new UpgradeCard("Stun Chain+", UpgradeRarity.Epic, 
                LightningStunUpgrade.Generate(1.0f, 1.5f), -1, typeof(LightningBoltAbility)),
                
            () => new UpgradeCard("Bouncing Lightning", UpgradeRarity.Epic, 
                LightningBouncingUpgrade.Generate(), -1, typeof(LightningBoltAbility)),
                
            // Black Hole upgrades (Epic)
            () => new UpgradeCard("Cursed Vortex+", UpgradeRarity.Epic, 
                IncreaseDoomStacksUpgrade.Generate(2, 4), -1, typeof(BlackHoleAbility)),
            
            () => new UpgradeCard("Gravitational Pull+", UpgradeRarity.Epic, 
                IncreasePullStrengthUpgrade.Generate(60.0f, 90.0f), -1, typeof(BlackHoleAbility)),
            
            () => new UpgradeCard("Amplified Doom+", UpgradeRarity.Epic, 
                IncreaseDoomDamagePerStackUpgrade.Generate(40.0f, 60.0f), -1, typeof(BlackHoleAbility)),
            
            // Fireball upgrades (Epic)
            () => new UpgradeCard("Burning Impact", UpgradeRarity.Epic, 
                BurnOnHitUpgrade.Generate(60.0f, 80.0f, 1, 2), -1, typeof(FireballAbility)),
            
            () => new UpgradeCard("Inferno Strike", UpgradeRarity.Epic, 
                BurnOnHitUpgrade.Generate(70.0f, 90.0f, 2, 3), -1, typeof(FireballAbility)),
            
            // Plague upgrades (Epic)
            () => new UpgradeCard("Rapid Decay+", UpgradeRarity.Epic, 
                FasterPlagueTickUpgrade.Generate(50.0f, 70.0f), -1, typeof(PlagueAbility)),
            
            () => new UpgradeCard("Toxic Strike+", UpgradeRarity.Epic, 
                PlaguePoisonUpgrade.Generate(50.0f, 70.0f, 1, 2), -1, typeof(PlagueAbility)),
            
            () => new UpgradeCard("Enduring Plague+", UpgradeRarity.Epic, 
                ExtendedPlagueDurationUpgrade.Generate(75.0f, 100.0f), -1, typeof(PlagueAbility)),
            
            () => new UpgradeCard("Life Drain+", UpgradeRarity.Epic, 
                PlagueLifeDrainUpgrade.Generate(30.0f, 45.0f, 2, 4), -1, typeof(PlagueAbility))
        };
    }
    
    // ===== LEGENDARY RARITY CARDS =====
    // Game-changing but balanced combinations
    private static List<System.Func<UpgradeCard>> GetLegendaryCards()
    {
        return new List<System.Func<UpgradeCard>>
        {
            () => new UpgradeCard("God of War", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    DamageUpgrade.Generate(25.0f, 35.0f),
                    CooldownReductionUpgrade.Generate(20.0f, 28.0f)
                }),
                
            // Chain - reduced from 3-6 to 2-3
            () => new UpgradeCard("Lightning Emperor", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    ChainUpgrade.Generate(2, 3),
                    DamageUpgrade.Generate(18.0f, 28.0f)
                }, 1, typeof(LightningBoltAbility)),
                
            () => new UpgradeCard("Immortal Guardian", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    MaxHealthUpgrade.Generate(70, 100),
                    MovementSpeedUpgrade.Generate(28.0f, 40.0f)
                }),
                
            // Pierce + Chain combo - reduced from 3-5 pierce to 2-3
            () => new UpgradeCard("Piercing Storm", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    PierceUpgrade.Generate(1, 2),
                    ChainUpgrade.Generate(2, 4)
                }),
                
            () => new UpgradeCard("Perfect Warrior", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    DamageUpgrade.Generate(22.0f, 32.0f),
                    MaxHealthUpgrade.Generate(55, 80)
                }),
                
            // Multicast - HEAVILY reduced from 150-250% to 50-75%
            () => new UpgradeCard("Omnislash Master", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    MulticastUpgrade.Generate(50.0f, 75.0f),
                    PierceUpgrade.Generate(1, 2)
                }),
                
            // Multicast - HEAVILY reduced from 200-300% to 60-90%
            () => new UpgradeCard("Divine Arsenal", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    MulticastUpgrade.Generate(60.0f, 90.0f),
                    ChainUpgrade.Generate(1, 2)
                }),
                
            // Crit combo - reduced from 15-25% chance to 10-16%
            () => new UpgradeCard("Godslayer", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    CriticalChanceUpgrade.Generate(10.0f, 16.0f),
                    CriticalDamageUpgrade.Generate(55.0f, 80.0f)
                }),
                
            // AOE - reduced from 80-120% to 50-70%
            () => new UpgradeCard("World Ender", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    AreaOfEffectUpgrade.Generate(50.0f, 70.0f),
                    DamageUpgrade.Generate(25.0f, 38.0f)
                }),
                
            // AOE + Multicast - heavily reduced
            () => new UpgradeCard("Nuclear Blast", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    AreaOfEffectUpgrade.Generate(60.0f, 85.0f),
                    MulticastUpgrade.Generate(45.0f, 70.0f)
                }),
            
            // Boomerang Blade upgrades (Legendary) - Limited to 1 pick each
            () => new UpgradeCard("Master Bladesman", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    MultipleBladesUpgrade.Generate(3, 5),
                    FasterRotationUpgrade.Generate(100.0f, 150.0f),
                    SpinningSlashUpgrade.Generate(100.0f, 150.0f, 30.0f, 50.0f)
                }, 1, typeof(BoomerangBladeAbility)),
                
            () => new UpgradeCard("Perfect Orbit", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    PingPongOrbitUpgrade.Generate(100.0f, 150.0f, 100.0f, 150.0f),
                    new DualOrbitUpgrade(),
                    ReturningBladeUpgrade.Generate()
                }, 1, typeof(BoomerangBladeAbility)),
            
            // Black Hole upgrades (Legendary)
            () => new UpgradeCard("Singularity", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    IncreaseDoomStacksUpgrade.Generate(4, 6),
                    IncreasePullStrengthUpgrade.Generate(100.0f, 150.0f),
                    IncreaseDoomDamagePerStackUpgrade.Generate(60.0f, 100.0f)
                }, -1, typeof(BlackHoleAbility)),
            
            // Lightning Bolt upgrades (Legendary)
            () => new UpgradeCard("Forked Lightning", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    LightningSplitUpgrade.Generate(2, 3)
                }, 1, typeof(LightningBoltAbility)),
                
            () => new UpgradeCard("Storm Master", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    LightningChainExplosionUpgrade.Generate(3.0f, 4.0f, 60.0f, 80.0f),
                    LightningStunUpgrade.Generate(1.5f, 2.0f),
                    LightningBouncingUpgrade.Generate()
                }, -1, typeof(LightningBoltAbility)),
                
            () => new UpgradeCard("Eternal Void", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    IncreasePullStrengthUpgrade.Generate(80.0f, 120.0f)
                }, -1, typeof(BlackHoleAbility)),
            
            // Plague upgrades (Legendary)
            () => new UpgradeCard("Pestilence Master", UpgradeRarity.Legendary, 
                new List<Upgrade>
                {
                    FasterPlagueTickUpgrade.Generate(70.0f, 90.0f),
                    PlaguePoisonUpgrade.Generate(60.0f, 80.0f, 2, 3),
                    ExtendedPlagueDurationUpgrade.Generate(100.0f, 150.0f),
                    PlagueLifeDrainUpgrade.Generate(40.0f, 60.0f, 3, 5)
                }, -1, typeof(PlagueAbility))
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
            LightningBoltAbility.GetDisplayInfo().name, 
            UpgradeRarity.Legendary,
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
            FireballAbility.GetDisplayInfo().name, 
            UpgradeRarity.Legendary,
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
            BoomerangBladeAbility.GetDisplayInfo().name, 
            UpgradeRarity.Legendary,
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
            MeteorShowerAbility.GetDisplayInfo().name, 
            UpgradeRarity.Legendary,
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
            BlackHoleAbility.GetDisplayInfo().name, 
            UpgradeRarity.Legendary,
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
    
    /// <summary>
    /// Generate a basic plague ability card without additional upgrades (for initial selection).
    /// </summary>
    /// <returns>AbilityCard with basic PlagueAbility</returns>
    private static AbilityCard GenerateBasicPlagueAbilityCard()
    {
        return new AbilityCard(
            PlagueAbility.GetDisplayInfo().name, 
            UpgradeRarity.Legendary,
            new List<Upgrade>(),
            typeof(PlagueAbility),
            (ability) => {
                var plagueAbility = ability as PlagueAbility;
                if (plagueAbility != null)
                {
                    PlagueAbility.ConfigureAbility(plagueAbility);
                }
            }
        );
    }
    
    /// <summary>
    /// Generate a basic dash ability card without additional upgrades (for initial selection).
    /// </summary>
    /// <returns>AbilityCard with basic DashAbility</returns>
    public static AbilityCard GenerateBasicDashAbilityCard()
    {
        return new AbilityCard(
            DashAbility.GetDisplayInfo().name, 
            UpgradeRarity.Legendary,
            new List<Upgrade>(),
            typeof(DashAbility),
            (ability) => {
                var dashAbility = ability as DashAbility;
                if (dashAbility != null)
                {
                    DashAbility.ConfigureAbility(dashAbility);
                }
            }
        );
    }
    
}