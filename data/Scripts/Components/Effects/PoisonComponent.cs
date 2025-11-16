using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Poison damage over time effect.
/// Poison deals low-moderate damage over a long duration.
/// Stacks increase duration rather than damage.
/// Represents toxin/venom damage.
/// </summary>
[ScriptSourceFile]
public class PoisonComponent : DamageOverTimeComponent
{
    public override void Initialize(Entity source, float dps, float effectDuration, int maxStackCount = 5)
    {
        // Poison can stack up to 5 times by default
        base.Initialize(source, dps, effectDuration, maxStackCount);
        stacksDamage = false;
        stacksDuration = true; // Stacking poison extends duration
    }
    
    public override string GetEffectName()
    {
        return "Poison";
    }
    
    public override string GetEffectColor()
    {
        return "rgba(34, 139, 34, 255)"; // Forest green
    }

    public override Color GetDamageColor()
    {
        return new Color(34, 139, 34, 255);
    }

    public override void OnDamageApplied(int damageAmount)
    {
        base.OnDamageApplied(damageAmount);
        // Could add poison cloud or drip visual effects here
    }
}

