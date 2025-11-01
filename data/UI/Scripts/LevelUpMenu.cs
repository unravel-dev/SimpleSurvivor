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
    private UIElement card2Title;
    private UIElement card2Description;
    private UIElement card3Title;
    private UIElement card3Description;
    
    // Current upgrade options
    private string[] upgradeOptions = new string[3];
    
    // Events
    public System.Action<int> OnCardSelected; // (cardIndex) - 0, 1, or 2
    
    // Static event for global subscription (Player can subscribe to this)
    public static System.Action<int, string> OnUpgradeSelected; // (cardIndex, upgradeText)
    
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
        card2Title = document.GetElementById("card2_title");
        card2Description = document.GetElementById("card2_description");
        card3Title = document.GetElementById("card3_title");
        card3Description = document.GetElementById("card3_description");
    }
    
    protected override int CountValidElements()
    {
        int count = base.CountValidElements();
        var elements = new UIElement[] { 
            card1, card2, card3,
            card1Title, card1Description,
            card2Title, card2Description,
            card3Title, card3Description
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
    
    /// <summary>
    /// Set the upgrade options for the three cards.
    /// </summary>
    /// <param name="option1">Text for card 1</param>
    /// <param name="option2">Text for card 2</param>
    /// <param name="option3">Text for card 3</param>
    public void SetUpgradeOptions(string option1, string option2, string option3)
    {
        upgradeOptions[0] = option1;
        upgradeOptions[1] = option2;
        upgradeOptions[2] = option3;
        
        // Ensure UI elements are cached before trying to update them
        EnsureInitialized();
        
        // Update the UI text
        UpdateCardText();
        
        Log.Info($"LevelUpMenu: Set upgrade options - [{option1}], [{option2}], [{option3}]");
    }
    
    /// <summary>
    /// Ensure that UI elements are cached and ready to use.
    /// This handles the case where the menu was inactive and OnStart() wasn't called.
    /// </summary>
    private void EnsureInitialized()
    {
        // If document is null, we haven't been initialized yet
        if (document == null)
        {
            Log.Info("LevelUpMenu: UI not initialized yet, initializing now...");
            
            // Get the UI document component
            var uiDoc = owner.GetComponent<UIDocumentComponent>();
            if (uiDoc == null)
            {
                Log.Error("LevelUpMenu: No UIDocumentComponent found on entity");
                return;
            }

            // Get the document wrapper
            document = uiDoc.GetDocument();
            if (document == null)
            {
                Log.Error("LevelUpMenu: Failed to get document wrapper - document may not be loaded");
                return;
            }

            Log.Info($"LevelUpMenu: Got document wrapper: {document.Title}");

            // Cache element wrappers
            CacheUIElements();
            
            // Set up initial UI state
            SetupInitialUI();
            
            // Register event handlers
            RegisterEventHandlers();
            
            Log.Info("LevelUpMenu: Manual initialization completed");
        }
    }
    
    /// <summary>
    /// Update the card text elements with the current upgrade options.
    /// </summary>
    private void UpdateCardText()
    {
        // Safety check: ensure we have valid elements before updating
        if (document == null)
        {
            Log.Warning("LevelUpMenu: Cannot update card text - document not initialized");
            return;
        }
        
        // Parse and set card 1
        if (card1Title != null && card1Description != null && !string.IsNullOrEmpty(upgradeOptions[0]))
        {
            var parts = ParseUpgradeText(upgradeOptions[0]);
            card1Title.InnerRml = (parts.title);
            card1Description.InnerRml = (parts.description);
        }
        else if (!string.IsNullOrEmpty(upgradeOptions[0]))
        {
            Log.Warning("LevelUpMenu: Card 1 elements not found, cannot set text");
        }
        
        // Parse and set card 2
        if (card2Title != null && card2Description != null && !string.IsNullOrEmpty(upgradeOptions[1]))
        {
            var parts = ParseUpgradeText(upgradeOptions[1]);
            card2Title.InnerRml = (parts.title);
            card2Description.InnerRml = (parts.description);
        }
        else if (!string.IsNullOrEmpty(upgradeOptions[1]))
        {
            Log.Warning("LevelUpMenu: Card 2 elements not found, cannot set text");
        }
        
        // Parse and set card 3
        if (card3Title != null && card3Description != null && !string.IsNullOrEmpty(upgradeOptions[2]))
        {
            var parts = ParseUpgradeText(upgradeOptions[2]);
            card3Title.InnerRml = (parts.title);
            card3Description.InnerRml = (parts.description);
        }
        else if (!string.IsNullOrEmpty(upgradeOptions[2]))
        {
            Log.Warning("LevelUpMenu: Card 3 elements not found, cannot set text");
        }
    }
    
    /// <summary>
    /// Parse upgrade text into title and description.
    /// Expected format: "Title|Description" or just "Title" if no separator.
    /// </summary>
    /// <param name="upgradeText">The upgrade text to parse</param>
    /// <returns>Parsed title and description</returns>
    private (string title, string description) ParseUpgradeText(string upgradeText)
    {
        if (string.IsNullOrEmpty(upgradeText))
            return ("Unknown", "No description");
        
        var parts = upgradeText.Split('|');
        if (parts.Length >= 2)
        {
            return (parts[0].Trim(), parts[1].Trim());
        }
        else
        {
            return (upgradeText.Trim(), "Select this upgrade");
        }
    }
    
    // ========== CARD CLICK HANDLERS ==========
    
    /// <summary>
    /// Handle Card 1 click.
    /// </summary>
    private void OnCard1Click(UIPointerEvent ev)
    {
        Log.Info("Card 1 selected");
        SelectCard(0);
    }
    
    /// <summary>
    /// Handle Card 2 click.
    /// </summary>
    private void OnCard2Click(UIPointerEvent ev)
    {
        Log.Info("Card 2 selected");
        SelectCard(1);
    }
    
    /// <summary>
    /// Handle Card 3 click.
    /// </summary>
    private void OnCard3Click(UIPointerEvent ev)
    {
        Log.Info("Card 3 selected");
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
        
        string selectedUpgrade = upgradeOptions[cardIndex];
        Log.Info($"LevelUpMenu: Selected upgrade {cardIndex + 1}: {selectedUpgrade}");
        
        // Trigger the local selection event (for LevelUpUI)
        OnCardSelected?.Invoke(cardIndex);
        
        // Trigger the global selection event (for Player to subscribe to)
        OnUpgradeSelected?.Invoke(cardIndex, selectedUpgrade);
    }
    
    // ========== CARD EVENT HANDLERS ==========
    // Custom handlers for card-specific behavior
    
    /// <summary>
    /// Handle card mouse down with visual feedback.
    /// </summary>
    private void OnCardDown(UIElement card, UIPointerEvent ev, string cardName)
    {
        Log.Info($"{cardName} pressed down");
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
        Log.Info($"{cardName} hovered");
        PlayButtonHoverSound(card, ev);
    }
    
    /// <summary>
    /// Handle card leave.
    /// </summary>
    private void OnCardLeave(UIElement card, UIPointerEvent ev, string cardName)
    {
        Log.Info($"{cardName} hover ended");
    }
    
    /// <summary>
    /// Handle card mouse release.
    /// </summary>
    private void OnCardRelease(UIElement card, UIPointerEvent ev, string cardName)
    {
        Log.Info($"{cardName} released");
        
        // Remove selected class
        if (card != null)
        {
            card.SetClass("selected", false);
        }
    }
}
