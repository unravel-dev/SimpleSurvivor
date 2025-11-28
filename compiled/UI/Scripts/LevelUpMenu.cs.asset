using System;
using Unravel.Core;

/// <summary>
/// Level Up Menu script that handles the card selection interface when the player levels up.
/// Displays 3 upgrade options and handles the selection logic.
/// Simple implementation without over-engineering - just handles the core functionality.
/// </summary>
[ScriptSourceFile]
public class LevelUpMenu : BaseMenu
{
    // Card elements
    private UIElement card1;
    private UIElement card2;
    private UIElement card3;
    
    // Card text elements
    private UIElement card1Title;
    private UIElement card1Description;
    private UIElement card1Rarity;
    private UIElement card1Icon;
    private UIElement card2Title;
    private UIElement card2Description;
    private UIElement card2Rarity;
    private UIElement card2Icon;
    private UIElement card3Title;
    private UIElement card3Description;
    private UIElement card3Rarity;
    private UIElement card3Icon;
    
    // Current upgrade cards
    private UpgradeCard[] upgradeCards = new UpgradeCard[3];
    
    // Events
    public System.Action<UpgradeCard> OnCardSelected; // (selectedCard)
    
    // Static event for global subscription (Player can subscribe to this)
    public static System.Action<UpgradeCard> OnUpgradeSelected; // (selectedCard)
    
    protected override string GetTitleElementId()
    {
        return "levelup_title";
    }
    
    protected override void CacheUIElements()
    {
        // Call base implementation to cache title element
        base.CacheUIElements();
        
        // Cache card elements
        card1 = document.GetElementById("card1");
        card2 = document.GetElementById("card2");
        card3 = document.GetElementById("card3");
        
        // Cache text elements
        card1Title = document.GetElementById("card1_title");
        card1Description = document.GetElementById("card1_description");
        card1Rarity = document.GetElementById("card1_rarity");
        card1Icon = document.GetElementById("card1_icon");
        card2Title = document.GetElementById("card2_title");
        card2Description = document.GetElementById("card2_description");
        card2Rarity = document.GetElementById("card2_rarity");
        card2Icon = document.GetElementById("card2_icon");
        card3Title = document.GetElementById("card3_title");
        card3Description = document.GetElementById("card3_description");
        card3Rarity = document.GetElementById("card3_rarity");
        card3Icon = document.GetElementById("card3_icon");
    }
    
    protected override int CountValidElements()
    {
        int count = base.CountValidElements();
        var elements = new UIElement[] { 
            card1, card2, card3,
            card1Title, card1Description, card1Rarity, card1Icon,
            card2Title, card2Description, card2Rarity, card2Icon,
            card3Title, card3Description, card3Rarity, card3Icon
        };
        
        foreach (var element in elements)
        {
            if (element?.IsValid() == true) count++;
        }
        
        return count;
    }
    
    protected override void RegisterEventHandlers()
    {
        // Register Card 1 event handlers
        RegisterButtonEvents(card1, "Card1",
            (ev) => OnCardDown(card1, ev, "Card1"),
            (ev) => OnCard1Click(ev),
            (ev) => OnCardHover(card1, ev, "Card1"),
            (ev) => OnCardLeave(card1, ev, "Card1"),
            (ev) => OnCardRelease(card1, ev, "Card1"));
        
        // Register Card 2 event handlers
        RegisterButtonEvents(card2, "Card2",
            (ev) => OnCardDown(card2, ev, "Card2"),
            (ev) => OnCard2Click(ev),
            (ev) => OnCardHover(card2, ev, "Card2"),
            (ev) => OnCardLeave(card2, ev, "Card2"),
            (ev) => OnCardRelease(card2, ev, "Card2"));
        
        // Register Card 3 event handlers
        RegisterButtonEvents(card3, "Card3",
            (ev) => OnCardDown(card3, ev, "Card3"),
            (ev) => OnCard3Click(ev),
            (ev) => OnCardHover(card3, ev, "Card3"),
            (ev) => OnCardLeave(card3, ev, "Card3"),
            (ev) => OnCardRelease(card3, ev, "Card3"));
        
        Log.Info("LevelUpMenu event handlers registered successfully");
    }

    protected override void UnregisterEventHandlers()
    {
        card1.UnsubscribeAll();
        card2.UnsubscribeAll();
        card3.UnsubscribeAll();

        base.UnregisterEventHandlers();
		Log.Info("LevelUpMenu event handlers unregistered successfully");
    }
    
    /// <summary>
    /// Set the upgrade cards for the three card slots.
    /// </summary>
    /// <param name="card1">Upgrade card for slot 1</param>
    /// <param name="card2">Upgrade card for slot 2</param>
    /// <param name="card3">Upgrade card for slot 3</param>
    public void SetUpgradeCards(UpgradeCard card1, UpgradeCard card2, UpgradeCard card3)
    {
        upgradeCards[0] = card1;
        upgradeCards[1] = card2;
        upgradeCards[2] = card3;
        
        // Update the UI text and styling
        UpdateCardDisplay();
        
        Log.Info($"LevelUpMenu: Set upgrade cards - [{card1?.Name}], [{card2?.Name}], [{card3?.Name}]");
    }
    /// <summary>
    /// Update the card display with the current upgrade cards.
    /// </summary>
    private void UpdateCardDisplay()
    {
        // Safety check: ensure we have valid elements before updating
        if (document == null)
        {
            Log.Warning("LevelUpMenu: Cannot update card display - document not initialized");
            return;
        }
        
        // Update card 1
        UpdateSingleCard(0, card1, card1Title, card1Description, card1Rarity, card1Icon, upgradeCards[0]);
        
        // Update card 2
        UpdateSingleCard(1, card2, card2Title, card2Description, card2Rarity, card2Icon, upgradeCards[1]);
        
        // Update card 3
        UpdateSingleCard(2, card3, card3Title, card3Description, card3Rarity, card3Icon, upgradeCards[2]);
    }
    
    /// <summary>
    /// Update a single card's display with the given upgrade card data.
    /// </summary>
    /// <param name="cardIndex">Index of the card (0, 1, or 2)</param>
    /// <param name="cardElement">The card container element</param>
    /// <param name="titleElement">The card title element</param>
    /// <param name="descriptionElement">The card description element</param>
    /// <param name="rarityElement">The card rarity element</param>
    /// <param name="iconElement">The card icon element</param>
    /// <param name="upgradeCard">The upgrade card data</param>
    private void UpdateSingleCard(int cardIndex, UIElement cardElement, UIElement titleElement, UIElement descriptionElement, UIElement rarityElement, UIElement iconElement, UpgradeCard upgradeCard)
    {
        if (upgradeCard == null)
        {
            Log.Warning($"LevelUpMenu: Card {cardIndex + 1} is null, cannot update display");
            return;
        }
        
        // Get display information from the upgrade card
        var displayInfo = upgradeCard.GetDisplayInfo();
        
        // Update text content
        if (titleElement != null && descriptionElement != null && rarityElement != null)
        {
            titleElement.InnerRml = displayInfo.name;
            descriptionElement.InnerRml = displayInfo.description;
            rarityElement.InnerRml = upgradeCard.Rarity.ToString();
        }
        else
        {
            Log.Warning($"LevelUpMenu: Card {cardIndex + 1} elements not found, cannot set text");
        }
        
        // Apply rarity styling
        if (cardElement != null)
        {
            // Remove existing rarity classes
            cardElement.SetClass("rarity-normal", false);
            cardElement.SetClass("rarity-common", false);
            cardElement.SetClass("rarity-epic", false);
            cardElement.SetClass("rarity-legendary", false);
            
            // Add the appropriate rarity class
            string rarityClass = GetRarityClass(upgradeCard.Rarity);
            cardElement.SetClass(rarityClass, true);
            
            Log.Info($"LevelUpMenu: Applied rarity class '{rarityClass}' to card {cardIndex + 1}");
        }
        
        // Apply ability icon styling
        if (cardElement != null && iconElement != null)
        {
            // Remove all ability type classes from icon
            RemoveAbilityClassesFromIcon(iconElement);
            
            // Add the appropriate ability class if we have an icon type from display info
            if (!string.IsNullOrEmpty(displayInfo.iconType))
            {
                // Set ability class on icon element (shared styles in CommonMenu.rcss)
                iconElement.SetClass(displayInfo.iconType, true);
                iconElement.SetClass("has-ability", true);
                Log.Info($"LevelUpMenu: Applied ability class '{displayInfo.iconType}' to card {cardIndex + 1} icon");
            }
            else
            {
                // No ability - ensure icon shows empty state
                iconElement.SetClass("has-ability", false);
            }
        }
    }
    
    /// <summary>
    /// Get the CSS class name for a given rarity.
    /// </summary>
    /// <param name="rarity">The upgrade rarity</param>
    /// <returns>CSS class name for the rarity</returns>
    private string GetRarityClass(UpgradeRarity rarity)
    {
        switch (rarity)
        {
            case UpgradeRarity.Normal:
                return "rarity-normal";
            case UpgradeRarity.Common:
                return "rarity-common";
            case UpgradeRarity.Epic:
                return "rarity-epic";
            case UpgradeRarity.Legendary:
                return "rarity-legendary";
            default:
                return "rarity-normal";
        }
    }
    
    /// <summary>
    /// Remove all ability type classes from an icon element.
    /// </summary>
    /// <param name="iconElement">The icon element to clean</param>
    private void RemoveAbilityClassesFromIcon(UIElement iconElement)
    {
        if (iconElement == null)
            return;
        
        // Remove ability-specific classes
        iconElement.SetClass("fireball", false);
        iconElement.SetClass("dash", false);
        iconElement.SetClass("spark", false);
        iconElement.SetClass("cube", false);
        iconElement.SetClass("boomerang", false);
        iconElement.SetClass("meteorshower", false);
        iconElement.SetClass("blackhole", false);
        iconElement.SetClass("plague", false);
        
        // Remove upgrade rarity classes
        iconElement.SetClass("upgrade-normal", false);
        iconElement.SetClass("upgrade-common", false);
        iconElement.SetClass("upgrade-epic", false);
        iconElement.SetClass("upgrade-legendary", false);
    }
    
    
    // ========== CARD CLICK HANDLERS ==========
    
    /// <summary>
    /// Handle Card 1 click.
    /// </summary>
    private void OnCard1Click(UIPointerEvent ev)
    {
        SelectCard(0);
    }
    
    /// <summary>
    /// Handle Card 2 click.
    /// </summary>
    private void OnCard2Click(UIPointerEvent ev)
    {
        SelectCard(1);
    }
    
    /// <summary>
    /// Handle Card 3 click.
    /// </summary>
    private void OnCard3Click(UIPointerEvent ev)
    {
        SelectCard(2);
    }
    
    /// <summary>
    /// Handle card selection logic.
    /// </summary>
    /// <param name="cardIndex">Index of the selected card (0, 1, or 2)</param>
    private void SelectCard(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= 3)
        {
            Log.Warning($"Invalid card index: {cardIndex}");
            return;
        }
        
        UpgradeCard selectedCard = upgradeCards[cardIndex];
        if (selectedCard == null)
        {
            Log.Warning($"LevelUpMenu: No card available at index {cardIndex}");
            return;
        }
        
        Log.Info($"LevelUpMenu: Selected card {cardIndex + 1}: {selectedCard.Name}");
        
        // Trigger the local selection event (for LevelUpUI)
        OnCardSelected?.Invoke(selectedCard);
        
        // Trigger the global selection event (for Player to subscribe to)
        OnUpgradeSelected?.Invoke(selectedCard);
    }
    
    // ========== CARD EVENT HANDLERS ==========
    // Custom handlers for card-specific behavior
    
    /// <summary>
    /// Handle card mouse down with visual feedback.
    /// </summary>
    private void OnCardDown(UIElement card, UIPointerEvent ev, string cardName)
    {
        PlayButtonClickSound(card, ev);
        
        // Add selected class for visual feedback
        if (card != null)
        {
            card.SetClass("selected", true);
        }
    }
    
    /// <summary>
    /// Handle card hover with visual feedback.
    /// </summary>
    private void OnCardHover(UIElement card, UIPointerEvent ev, string cardName)
    {
        PlayButtonHoverSound(card, ev);
    }
    
    /// <summary>
    /// Handle card leave.
    /// </summary>
    private void OnCardLeave(UIElement card, UIPointerEvent ev, string cardName)
    {
    }
    
    /// <summary>
    /// Handle card mouse release.
    /// </summary>
    private void OnCardRelease(UIElement card, UIPointerEvent ev, string cardName)
    {
        
        // Remove selected class
        if (card != null)
        {
            card.SetClass("selected", false);
        }
    }
}
