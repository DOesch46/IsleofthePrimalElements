using UnityEngine;
using System;

public class EarthBossHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 300f;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDied;

    private float currentHealth;
    private bool isDead = false;

    public float HealthFraction => currentHealth / maxHealth;
    public bool IsDead => isDead;

    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"Earth Boss took {amount} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Earth Boss defeated!");
        OnDied?.Invoke();
    }
}