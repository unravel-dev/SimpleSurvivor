using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// ChestLoot component that represents a collectible chest item.
/// When collected, prompts an upgrade selection similar to leveling up.
/// </summary>
[ScriptSourceFile]
public class ChestLoot : ScriptComponent
{
    protected Entity targetPlayer;
    public override void OnStart()
    {
        targetPlayer = Scene.FindEntityByName("Player");
    }
    public override void OnSensorEnter(Collision collision)
    {
        if(collision.entity != targetPlayer)
            return;

        base.OnSensorEnter(collision);
        CollectLoot();
    }
    /// <summary>
    /// Collect this chest and trigger the upgrade selection menu.
    /// </summary>
    protected void CollectLoot()
    {
        // Find the Player component to get level and abilities
        var playerComponent = targetPlayer.GetComponent<Player>();
        if (playerComponent == null)
        {
            Log.Warning("ChestLoot: Player component not found on target player");
            return;
        }

        playerComponent.ShowUpgradeSelectionMenu();
       
        // Destroy the chest (don't call base.CollectLoot() as it also destroys, and we want to control when)
        Scene.DestroyEntity(owner);
    }
}

