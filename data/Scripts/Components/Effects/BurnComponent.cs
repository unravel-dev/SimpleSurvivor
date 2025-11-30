using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Burn damage over time effect.
/// Burn deals moderate damage over time and stacks increase damage.
/// Represents fire/heat damage.
/// </summary>
[ScriptSourceFile]
public class BurnComponent : DamageOverTimeComponent
{
    public override void Initialize(Entity source, float dps, float effectDuration, int maxStackCount = 3)
    {
        // Burn can stack up to 3 times by default
        base.Initialize(source, dps, effectDuration, maxStackCount);
        stacksDamage = true;
        stacksDuration = false;
    }
    
    public override string GetEffectName()
    {
        return "Burn";
    }
    
    public override string GetEffectColor()
    {
        return "rgba(255, 150, 0, 255)"; // Orange-red
    }
    
    public override Color GetDamageColor()
    {
        return new Color(255, 150, 0, 255);
    }

    public override void OnDamageApplied(int damageAmount)
    {
        base.OnDamageApplied(damageAmount);
        // Could add burn particles or visual effects here
    }
}

