using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Component that updates the EffectsSystem every frame.
/// Add this to a persistent game object (like GameManager or Level) to enable damage over time effects.
/// </summary>
[ScriptSourceFile]
public class EffectsSystemUpdater : ScriptComponent
{
    
    public override void OnStart()
    {
        Log.Info("EffectsSystemUpdater: Effects system initialized");
    }
    
    public override void OnUpdate()
    {
        // Update the effects system
        EffectsSystem.Tick(Time.deltaTime);
        
    }
    
}

