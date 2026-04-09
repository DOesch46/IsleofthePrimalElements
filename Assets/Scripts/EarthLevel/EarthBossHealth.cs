using UnityEngine;
using System;

/// <summary>
/// Earth Boss Health — fixes:
///   1. Implements IDamageable so PlayerCombat.OnHitboxTrigger can damage it
///   2. Requires a Collider2D on the boss for the hitbox to detect it
///   3. Fires events for the health bar UI
/// </summary>
[RequireComponent(typeof(Collider2D))]   // Boss MUST have a collider for hits to register
public class EarthBossHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 300f;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDied;

    private float currentHealth;
    private bool isDead = false;

    public float HealthFraction => currentHealth / maxHealth;
    public bool  IsDead         => isDead;
    
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Called by PlayerCombat when the attack hitbox overlaps this boss.
    /// Requires IDamageable interface — already implemented here.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth  = Mathf.Max(currentHealth, 0f);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"Earth Boss took {amount} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
            Die();
        
        HitFlash flash = GetComponent<HitFlash>();
        if (flash != null)
            flash.Flash();
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Earth Boss defeated!");
        OnDied?.Invoke();
    }
}