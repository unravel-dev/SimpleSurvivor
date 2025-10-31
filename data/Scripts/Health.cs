using System;
using System.Runtime.CompilerServices;
using Unravel.Core;

/// <summary>
/// Health that tracks entity health and handles damage/healing.
/// Provides simple methods like TakeDamage and Heal, and kills the entity when health drops below 0.
/// </summary>
[ScriptSourceFile]
public class Health : ScriptComponent
{
    //[Header("Health Settings")]
    [Tooltip("Maximum health value")]
    public float maxHealth = 100.0f;
    [Tooltip("Current health value")]
    public float currentHealth = 100.0f;
    [Tooltip("Whether the entity can be healed above max health")]
    public bool allowOverheal = false;
    [Tooltip("Destroy the entity when health reaches 0")]
    public bool destroyOnDeath = true;
    [Tooltip("Delay before destroying entity after death (in seconds)")]
    public float destroyDelay = 0.0f;

    //[Header("Events")]
    [Tooltip("Enable debug logging for health events")]
    public bool debugHealth = false;

    // Health state
    private bool isDead = false;

    // Events (using System.Action for simplicity)
    public System.Action<float, float> OnHealthChanged; // (currentHealth, maxHealth)
    public System.Action<float> OnDamageTaken; // (damageAmount)
    public System.Action<float> OnHealed; // (healAmount)
    public System.Action OnDeath;

    /// <summary>
    /// Called when the script is created.
    /// </summary>
    public override void OnCreate()
    {
        // Initialize current health to max health if not set
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
        }

        // Clamp current health to max health
        if (currentHealth > maxHealth && !allowOverheal)
        {
            currentHealth = maxHealth;
        }

        if (debugHealth)
        {
            Log.Info($"Health initialized on {owner.name} - Health: {currentHealth}/{maxHealth}");
        }
    }

    /// <summary>
    /// Called when the script starts execution.
    /// </summary>
    public override void OnStart()
    {
        if (debugHealth)
        {
            Log.Info($"Health started on {owner.name}");
        }
    }

    /// <summary>
    /// Deal damage to this entity.
    /// </summary>
    /// <param name="damage">Amount of damage to deal (positive value).</param>
    /// <param name="source">Optional source entity that dealt the damage.</param>
    /// <returns>True if the entity died from this damage.</returns>
    public bool TakeDamage(float damage, Entity source)
    {
        if (isDead || damage <= 0)
            return false;

        float previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (debugHealth)
        {
            string sourceInfo = source ? $" from {source.name}" : "";
            Log.Info($"{owner.name} took {damage} damage{sourceInfo} - Health: {currentHealth}/{maxHealth}");
        }

        // Trigger damage event
        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Check if entity died
        if (currentHealth <= 0 && !isDead)
        {
            Die();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Heal this entity.
    /// </summary>
    /// <param name="healAmount">Amount of health to restore (positive value).</param>
    /// <param name="source">Optional source entity that provided the healing.</param>
    /// <returns>The actual amount healed (may be less if at max health).</returns>
    public float Heal(float healAmount, Entity source)
    {
        if (isDead || healAmount <= 0)
            return 0;

        float previousHealth = currentHealth;
        float targetHealth = currentHealth + healAmount;

        // Clamp to max health unless overheal is allowed
        if (!allowOverheal)
        {
            targetHealth = Mathf.Min(targetHealth, maxHealth);
        }

        currentHealth = targetHealth;
        float actualHealAmount = currentHealth - previousHealth;

        if (actualHealAmount > 0)
        {
            if (debugHealth)
            {
                string sourceInfo = source ? $" from {source.name}" : "";
                Log.Info($"{owner.name} healed for {actualHealAmount}{sourceInfo} - Health: {currentHealth}/{maxHealth}");
            }

            // Trigger heal event
            OnHealed?.Invoke(actualHealAmount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        return actualHealAmount;
    }

    /// <summary>
    /// Set the entity's health to a specific value.
    /// </summary>
    /// <param name="newHealth">The new health value.</param>
    public void SetHealth(float newHealth)
    {
        if (isDead)
            return;

        float previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, newHealth);

        // Clamp to max health unless overheal is allowed
        if (!allowOverheal && currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (debugHealth)
        {
            Log.Info($"{owner.name} health set to {currentHealth}/{maxHealth}");
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Check if entity died
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    /// <summary>
    /// Set the maximum health and optionally adjust current health.
    /// </summary>
    /// <param name="newMaxHealth">The new maximum health value.</param>
    /// <param name="adjustCurrentHealth">Whether to scale current health proportionally.</param>
    public void SetMaxHealth(float newMaxHealth, bool adjustCurrentHealth = false)
    {
        if (newMaxHealth <= 0)
        {
            Log.Warning($"Health on {owner.name}: Attempted to set max health to {newMaxHealth}. Must be positive.");
            return;
        }

        float healthRatio = currentHealth / maxHealth;
        maxHealth = newMaxHealth;

        if (adjustCurrentHealth)
        {
            currentHealth = maxHealth * healthRatio;
        }
        else if (!allowOverheal && currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (debugHealth)
        {
            Log.Info($"{owner.name} max health set to {maxHealth} - Current health: {currentHealth}");
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Kill the entity immediately.
    /// </summary>
    public void Kill()
    {
        if (isDead)
            return;

        currentHealth = 0;
        Die();
    }

    /// <summary>
    /// Handle entity death.
    /// </summary>
    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (debugHealth)
        {
            Log.Info($"{owner.name} has died");
        }

        // Trigger death event
        OnDeath?.Invoke();

        // Destroy entity if configured to do so
        if (destroyOnDeath)
        {
            if (destroyDelay > 0)
            {
                Scene.DestroyEntity(owner, destroyDelay);
            }
            else
            {
                Scene.DestroyEntity(owner);
            }
        }
    }

    /// <summary>
    /// Get the current health value.
    /// </summary>
    /// <returns>Current health.</returns>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Get the maximum health value.
    /// </summary>
    /// <returns>Maximum health.</returns>
    public float GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// Get the health as a percentage (0.0 to 1.0).
    /// </summary>
    /// <returns>Health percentage.</returns>
    public float GetHealthPercentage()
    {
        if (maxHealth <= 0)
            return 0;

        return currentHealth / maxHealth;
    }

    /// <summary>
    /// Check if the entity is dead.
    /// </summary>
    /// <returns>True if dead.</returns>
    public bool IsDead()
    {
        return isDead;
    }

    /// <summary>
    /// Check if the entity is at full health.
    /// </summary>
    /// <returns>True if at full health.</returns>
    public bool IsAtFullHealth()
    {
        return currentHealth >= maxHealth;
    }

    /// <summary>
    /// Check if the entity is critically injured (below a certain threshold).
    /// </summary>
    /// <param name="threshold">Health percentage threshold (0.0 to 1.0).</param>
    /// <returns>True if health is below the threshold.</returns>
    public bool IsCriticallyInjured(float threshold = 0.25f)
    {
        return GetHealthPercentage() < threshold;
    }

    /// <summary>
    /// Restore the entity to full health.
    /// </summary>
    public void RestoreToFullHealth()
    {
        if (isDead)
            return;

        float healAmount = maxHealth - currentHealth;
        if (healAmount > 0)
        {
            Heal(healAmount, owner);
        }
    }

    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        SetMaxHealth(maxHealth, true);
    }
}

