using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Example weapon configurations showing how to set up different weapon effects.
/// This is just for reference - you would configure these in the editor on actual weapon prefabs.
/// </summary>
public static class ExampleWeaponConfigs
{
    /// <summary>
    /// Flamethrower configuration - high damage with burn effect.
    /// </summary>
    public static WeaponEffectData[] FlamethrowerEffects = new WeaponEffectData[]
    {
        new WeaponEffectData(WeaponEffectType.Burn, 0.8f, 3.0f, 15.0f) // 80% chance, 3 seconds, 15 DPS
    };
    
    /// <summary>
    /// Ice gun configuration - moderate damage with slow effect.
    /// </summary>
    public static WeaponEffectData[] IceGunEffects = new WeaponEffectData[]
    {
        new WeaponEffectData(WeaponEffectType.Freeze, 1.0f, 2.0f, 0.3f) // 100% chance, 2 seconds, 30% speed
    };
    
    /// <summary>
    /// Poison dart configuration - low damage with strong poison.
    /// </summary>
    public static WeaponEffectData[] PoisonDartEffects = new WeaponEffectData[]
    {
        new WeaponEffectData(WeaponEffectType.Poison, 0.6f, 5.0f, 8.0f) // 60% chance, 5 seconds, 8 DPS
    };
    
    /// <summary>
    /// Stun baton configuration - moderate damage with stun.
    /// </summary>
    public static WeaponEffectData[] StunBatonEffects = new WeaponEffectData[]
    {
        new WeaponEffectData(WeaponEffectType.Stun, 0.3f, 1.5f, 0.0f) // 30% chance, 1.5 seconds stun
    };
    
    /// <summary>
    /// Multi-effect weapon - combines multiple effects.
    /// </summary>
    public static WeaponEffectData[] MultiEffectWeaponEffects = new WeaponEffectData[]
    {
        new WeaponEffectData(WeaponEffectType.Burn, 0.4f, 2.0f, 10.0f),  // 40% burn
        new WeaponEffectData(WeaponEffectType.Slow, 0.6f, 3.0f, 0.5f),   // 60% slow
        new WeaponEffectData(WeaponEffectType.Stun, 0.1f, 1.0f, 0.0f)    // 10% stun
    };
    
    /// <summary>
    /// Example of how to configure a weapon programmatically.
    /// </summary>
    /// <param name="weapon">Weapon component to configure.</param>
    /// <param name="weaponType">Type of weapon to configure.</param>
    public static void ConfigureWeapon(Weapon weapon, string weaponType)
    {
        switch (weaponType.ToLower())
        {
            case "flamethrower":
                weapon.weaponEffects = FlamethrowerEffects;
                weapon.damage = 30.0f;
                weapon.fireRate = 5.0f;
                weapon.range = 8.0f;
                break;
                
            case "icegun":
                weapon.weaponEffects = IceGunEffects;
                weapon.damage = 20.0f;
                weapon.fireRate = 3.0f;
                weapon.range = 12.0f;
                break;
                
            case "poisondart":
                weapon.weaponEffects = PoisonDartEffects;
                weapon.damage = 15.0f;
                weapon.fireRate = 2.0f;
                weapon.range = 15.0f;
                break;
                
            case "stunbaton":
                weapon.weaponEffects = StunBatonEffects;
                weapon.damage = 25.0f;
                weapon.fireRate = 1.5f;
                weapon.range = 6.0f;
                break;
                
            case "multieffect":
                weapon.weaponEffects = MultiEffectWeaponEffects;
                weapon.damage = 18.0f;
                weapon.fireRate = 2.5f;
                weapon.range = 10.0f;
                break;
                
            default:
                // No effects - standard weapon
                weapon.weaponEffects = new WeaponEffectData[0];
                break;
        }
    }
}
