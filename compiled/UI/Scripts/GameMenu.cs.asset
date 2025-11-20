using System;
using Unravel.Core;

/// <summary>
/// Game Menu script that handles in-game navigation buttons.
/// Uses GameUI for centralized navigation instead of direct menu manipulation.
/// Now inherits from BaseMenuController to eliminate code duplication.
/// </summary>
[ScriptSourceFile]
public class GameMenu : BaseMenu
{

	// Menu button elements
	private UIElement resumeButton;
	private UIElement restartButton;
	private UIElement settingsButton;
	private UIElement quitButton;

	// Upgrade display elements
	private UIElement upgradeListContainer;
	private UIElement damageValueElement;
	private UIElement multicastValueElement;
	private UIElement pierceValueElement;
	private UIElement chainValueElement;
	private UIElement cooldownValueElement;
	private UIElement speedValueElement;
	private UIElement healthValueElement;
	private UIElement critChanceValueElement;
	private UIElement critDamageValueElement;
	private UIElement pickupRadiusValueElement;
	private UIElement luckValueElement;
	private UIElement aoeValueElement;

	protected override string GetTitleElementId()
	{
		return "game_title";
	}

	protected override void CacheUIElements()
	{
		// Call base implementation to cache title element
		base.CacheUIElements();

		// Cache menu-specific button elements
		resumeButton = document.GetElementById("resume_btn");
		restartButton = document.GetElementById("restart_btn");
		settingsButton = document.GetElementById("settings_btn");
		quitButton = document.GetElementById("quit_btn");

		// Cache upgrade display elements
		upgradeListContainer = document.GetElementById("upgrade_list_container");
		damageValueElement = document.GetElementById("damage_value");
		multicastValueElement = document.GetElementById("multicast_value");
		pierceValueElement = document.GetElementById("pierce_value");
		chainValueElement = document.GetElementById("chain_value");
		cooldownValueElement = document.GetElementById("cooldown_value");
		speedValueElement = document.GetElementById("speed_value");
		healthValueElement = document.GetElementById("health_value");
		critChanceValueElement = document.GetElementById("crit_chance_value");
		critDamageValueElement = document.GetElementById("crit_damage_value");
		pickupRadiusValueElement = document.GetElementById("pickup_radius_value");
		luckValueElement = document.GetElementById("luck_value");
		aoeValueElement = document.GetElementById("aoe_value");
	}

	protected override int CountValidElements()
	{
		int count = base.CountValidElements();
		var buttonElements = new UIElement[] { resumeButton, restartButton, settingsButton, quitButton };
		var upgradeElements = new UIElement[] { 
			upgradeListContainer, damageValueElement, multicastValueElement, pierceValueElement,
			chainValueElement, cooldownValueElement, speedValueElement, healthValueElement,
			critChanceValueElement, critDamageValueElement, pickupRadiusValueElement, luckValueElement,
			aoeValueElement
		};

		foreach (var element in buttonElements)
		{
			if (element?.IsValid() == true) count++;
		}

		foreach (var element in upgradeElements)
		{
			if (element?.IsValid() == true) count++;
		}

		return count;
	}

	protected override void RegisterEventHandlers()
	{
		// Register Resume button event handlers
		RegisterButtonEvents(resumeButton, "Resume",
			(ev) => OnButtonDown(resumeButton, ev, "Resume"),
			OnResumeButtonClick,
			(ev) => OnButtonHover(resumeButton, ev, "Resume"),
			(ev) => OnButtonLeave(resumeButton, ev, "Resume"),
			(ev) => OnButtonRelease(resumeButton, ev, "Resume"));

		// Register Restart button event handlers
		RegisterButtonEvents(restartButton, "Restart",
			(ev) => OnButtonDown(restartButton, ev, "Restart"),
			OnRestartButtonClick,
			(ev) => OnButtonHover(restartButton, ev, "Restart"),
			(ev) => OnButtonLeave(restartButton, ev, "Restart"),
			(ev) => OnButtonRelease(restartButton, ev, "Restart"));

		// Register Settings button event handlers
		RegisterButtonEvents(settingsButton, "Settings",
			(ev) => OnButtonDown(settingsButton, ev, "Settings"),
			OnSettingsButtonClick,
			(ev) => OnButtonHover(settingsButton, ev, "Settings"),
			(ev) => OnButtonLeave(settingsButton, ev, "Settings"),
			(ev) => OnButtonRelease(settingsButton, ev, "Settings"));

		// Register Quit button event handlers
		RegisterButtonEvents(quitButton, "Quit",
			(ev) => OnButtonDown(quitButton, ev, "Quit"),
			OnQuitButtonClick,
			(ev) => OnButtonHover(quitButton, ev, "Quit"),
			(ev) => OnButtonLeave(quitButton, ev, "Quit"),
			(ev) => OnButtonRelease(quitButton, ev, "Quit"));

		Log.Info("GameMenu event handlers registered successfully");
	}

	protected override void UnregisterEventHandlers()
	{
		resumeButton.UnsubscribeAll();
		restartButton.UnsubscribeAll();
		settingsButton.UnsubscribeAll();
		quitButton.UnsubscribeAll();
		

		base.UnregisterEventHandlers();
	}

	protected override void SetupInitialUI()
	{
		// Call base implementation
		base.SetupInitialUI();

		// Initialize upgrade display
		UpdateUpgradeDisplay();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		UpdateUpgradeDisplay();
	}

	/// <summary>
	/// Updates the upgrade display with current accumulated values from UpgradeSystem.
	/// </summary>
	private void UpdateUpgradeDisplay()
	{
		// Update damage value
		if (damageValueElement?.IsValid() == true)
		{
			float damagePercent = UpgradeSystem.GetAccumulatedDamagePercent();
			damageValueElement.InnerRml = $"+{damagePercent:F1}%";
		}

		// Update multicast value
		if (multicastValueElement?.IsValid() == true)
		{
			float multicastPercent = UpgradeSystem.GetAccumulatedMulticastPercent();
			multicastValueElement.InnerRml = $"+{multicastPercent:F1}%";
		}

		// Update pierce value
		if (pierceValueElement?.IsValid() == true)
		{
			int pierceCount = UpgradeSystem.GetAccumulatedPierceCount();
			pierceValueElement.InnerRml = $"+{pierceCount}";
		}

		// Update chain value
		if (chainValueElement?.IsValid() == true)
		{
			int chainCount = UpgradeSystem.GetAccumulatedChainCount();
			chainValueElement.InnerRml = $"+{chainCount}";
		}

		// Update cooldown reduction value
		if (cooldownValueElement?.IsValid() == true)
		{
			float cooldownPercent = UpgradeSystem.GetAccumulatedCooldownReductionPercent();
			cooldownValueElement.InnerRml = $"-{cooldownPercent:F1}%";
		}

		// Update movement speed value
		if (speedValueElement?.IsValid() == true)
		{
			float speedPercent = UpgradeSystem.GetAccumulatedMovementSpeedPercent();
			speedValueElement.InnerRml = $"+{speedPercent:F1}%";
		}

		// Update max health value
		if (healthValueElement?.IsValid() == true)
		{
			int healthIncrease = UpgradeSystem.GetAccumulatedMaxHealthIncrease();
			healthValueElement.InnerRml = $"+{healthIncrease}";
		}

		// Update critical chance value
		if (critChanceValueElement?.IsValid() == true)
		{
			float critChancePercent = UpgradeSystem.GetAccumulatedCriticalChancePercent();
			critChanceValueElement.InnerRml = $"+{critChancePercent:F1}%";
		}

		// Update critical damage value
		if (critDamageValueElement?.IsValid() == true)
		{
			float critDamagePercent = UpgradeSystem.GetAccumulatedCriticalDamagePercent();
			critDamageValueElement.InnerRml = $"+{critDamagePercent:F1}%";
		}

		// Update pickup radius value
		if (pickupRadiusValueElement?.IsValid() == true)
		{
			float pickupRadiusPercent = UpgradeSystem.GetAccumulatedPickupRadiusPercent();
			pickupRadiusValueElement.InnerRml = $"+{pickupRadiusPercent:F1}%";
		}

		// Update luck value
		if (luckValueElement?.IsValid() == true)
		{
			float luckPercent = UpgradeSystem.GetAccumulatedLuckPercent();
			luckValueElement.InnerRml = $"+{luckPercent:F1}%";
		}

		// Update area of effect value
		if (aoeValueElement?.IsValid() == true)
		{
			float aoePercent = UpgradeSystem.GetAccumulatedAreaOfEffectPercent();
			aoeValueElement.InnerRml = $"+{aoePercent:F1}%";
		}
	}

	public override void OnUpdate()
	{
		// Call base implementation to update time display
		base.OnUpdate();

		// Update upgrade display values
		// UpdateUpgradeDisplay();
	}

	// ========== BUTTON CLICK HANDLERS ==========
	// All navigation goes through GameUI for consistency

	/// <summary>
	/// Handles the Resume button click - resumes the game via GameUI.
	/// </summary>
	private void OnResumeButtonClick(UIPointerEvent ev)
	{
		Log.Info($"Resume button clicked");
		GameUI.FindInScene()?.ResumeGame();
	}

	/// <summary>
	/// Handles the Restart button click - restarts the game via GameUI.
	/// </summary>
	private void OnRestartButtonClick(UIPointerEvent ev)
	{
		Log.Info($"Restart button clicked");
		GameUI.FindInScene()?.RestartGame();
	}

	/// <summary>
	/// Handles the Settings button click - opens settings menu via GameUI.
	/// </summary>
	private void OnSettingsButtonClick(UIPointerEvent ev)
	{
		Log.Info($"Settings button clicked");
		GameUI.FindInScene()?.OpenSettings();
	}

	/// <summary>
	/// Handles the Quit button click - returns to main menu via GameUI.
	/// </summary>
	private void OnQuitButtonClick(UIPointerEvent ev)
	{
		Log.Info($"Quit button clicked");
		GameUI.FindInScene()?.GoToMainMenu();
	}
}
