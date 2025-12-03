using System;
using Unravel.Core;

/// <summary>
/// Game Over Menu script that handles game over screen navigation buttons.
/// Similar to GameMenu but only has Restart and Quit options.
/// Cannot be closed except by selecting one of these options.
/// </summary>
[ScriptSourceFile]
public class GameOverMenu : BaseMenu
{
	// Menu button elements
	private UIElement restartButton;
	private UIElement quitButton;

	protected override string GetTitleElementId()
	{
		return "gameover_title";
	}

	protected override void CacheUIElements()
	{
		// Call base implementation to cache title element
		base.CacheUIElements();

		// Cache menu-specific button elements
		restartButton = document.GetElementById("restart_btn");
		quitButton = document.GetElementById("quit_btn");
	}

	protected override int CountValidElements()
	{
		int count = base.CountValidElements();
		var buttonElements = new UIElement[] { restartButton, quitButton };

		foreach (var element in buttonElements)
		{
			if (element?.IsValid() == true) count++;
		}

		return count;
	}

	protected override void RegisterEventHandlers()
	{
		// Register Restart button event handlers
		RegisterButtonEvents(restartButton, "Restart",
			(ev) => OnButtonDown(restartButton, ev, "Restart"),
			OnRestartButtonClick,
			(ev) => OnButtonHover(restartButton, ev, "Restart"),
			(ev) => OnButtonLeave(restartButton, ev, "Restart"),
			(ev) => OnButtonRelease(restartButton, ev, "Restart"));

		// Register Quit button event handlers
		RegisterButtonEvents(quitButton, "Quit",
			(ev) => OnButtonDown(quitButton, ev, "Quit"),
			OnQuitButtonClick,
			(ev) => OnButtonHover(quitButton, ev, "Quit"),
			(ev) => OnButtonLeave(quitButton, ev, "Quit"),
			(ev) => OnButtonRelease(quitButton, ev, "Quit"));

		Log.Info("GameOverMenu event handlers registered successfully");
	}

	protected override void UnregisterEventHandlers()
	{
		restartButton.UnsubscribeAll();
		quitButton.UnsubscribeAll();
		
		base.UnregisterEventHandlers();
		Log.Info("GameOverMenu event handlers unregistered successfully");
	}

	protected override void SetupInitialUI()
	{
		// Call base implementation
		base.SetupInitialUI();
	}

	// ========== BUTTON CLICK HANDLERS ==========
	// All navigation goes through GameUI for consistency

	/// <summary>
	/// Handles the Restart button click - restarts the game via GameUI.
	/// </summary>
	private void OnRestartButtonClick(UIPointerEvent ev)
	{
		Log.Info($"Restart button clicked");
		MenuStackUI.FindInScene<GameUI>()?.RestartGame();
	}

	/// <summary>
	/// Handles the Quit button click - returns to main menu via GameUI.
	/// </summary>
	private void OnQuitButtonClick(UIPointerEvent ev)
	{
		Log.Info($"Quit button clicked");
		MenuStackUI.FindInScene<GameUI>()?.GoToMainMenu();
	}
}

