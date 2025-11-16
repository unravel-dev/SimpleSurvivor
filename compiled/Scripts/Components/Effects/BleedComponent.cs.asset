using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Bleed damage over time effect.
/// Bleed deals moderate damage over medium duration.
/// Both stacks and damage increase with stacking.
/// Represents physical bleeding/hemorrhage damage.
/// </summary>
[ScriptSourceFile]
public class BleedComponent : DamageOverTimeComponent
{
    public override void Initialize(Entity source, float dps, float effectDuration, int maxStackCount = 10)
    {
        // Bleed can stack many times
        base.Initialize(source, dps, effectDuration, maxStackCount);
        stacksDamage = true;
        stacksDuration = false;
    }
    
    public override string GetEffectName()
    {
        return "Bleed";
    }
    
    public override string GetEffectColor()
    {
        return "rgba(220, 20, 60, 255)"; // Crimson red
    }
    
    public override Color GetDamageColor()
    {
        return new Color(220, 20, 60, 255);
    }

    public override int GetDamagePerTick(float tickInterval)
    {
        // Bleed has slightly increasing damage per stack (1.1x per stack instead of linear)
        float baseDamage = damagePerSecond * tickInterval;

        if (stacksDamage && currentStacks > 1)
        {
            // Each stack adds 110% of base damage
            float stackMultiplier = 1.0f + ((currentStacks - 1) * 1.1f);
            baseDamage *= stackMultiplier;
        }

        return Mathf.RoundToInt(baseDamage);
    }
    
    public override void OnDamageApplied(int damageAmount)
    {
        base.OnDamageApplied(damageAmount);
        // Could add blood drip particles here
    }
}

