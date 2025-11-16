using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Doom damage over time effect.
/// Doom deals high damage over a short duration and cannot be stacked.
/// Represents a curse or death mark effect.
/// </summary>
[ScriptSourceFile]
public class DoomComponent : DamageOverTimeComponent
{
    public override void Initialize(Entity source, float dps, float effectDuration, int maxStackCount = 1)
    {
        // Doom doesn't stack
        base.Initialize(source, dps, effectDuration, maxStackCount);
        stacksDamage = true;
        stacksDuration = false;
    }
    
    public override string GetEffectName()
    {
        return "Doom";
    }
    
    public override string GetEffectColor()
    {
        return "rgba(139, 0, 139, 255)"; // Dark magenta/purple
    }

    public override Color GetDamageColor()
    {
        return new Color(139, 0, 139, 255);
    }

    public override void OnExpired()
    {
        base.OnExpired();
        // Could add an explosion or additional effect here
        Log.Info($"Doom effect expired on {owner.name}");
    }
}

