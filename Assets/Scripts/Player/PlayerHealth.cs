using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Health")]
    [SerializeField] private float maxHealth       = 100f;
    [SerializeField] private float invincibleTime  = 0.8f;

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    public event Action<float, float> OnHealthChanged;
    public event Action OnDied;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private float currentHealth;
    private float invincibleTimer = 0f;
    private bool  isDead          = false;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        RefreshUI();
    }

    private void Update()
    {
        if (invincibleTimer > 0f)
            invincibleTimer -= Time.deltaTime;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void TakeDamage(float amount)
    {
        if (isDead || invincibleTimer > 0f) return;

        currentHealth  -= amount;
        currentHealth   = Mathf.Max(currentHealth, 0f);
        invincibleTimer = invincibleTime;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        RefreshUI();

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth  += amount;
        currentHealth   = Mathf.Min(currentHealth, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        RefreshUI();
    }

    public float GetCurrentHealth()  => currentHealth;
    public float GetMaxHealth()      => maxHealth;
    public float GetHealthFraction() => currentHealth / maxHealth;
    public bool  IsDead()            => isDead;

    /// <summary>
    /// Called by PlayerStats to apply max health bonus from shop items.
    /// Increases max health and heals to the new max.
    /// </summary>
    public void SetMaxHealth(float newMax)
    {
        float oldMax = maxHealth;
        maxHealth = newMax;

        // Heal by the amount max health increased
        float increase = newMax - oldMax;
        if (increase > 0f)
            currentHealth += increase;

        currentHealth = Mathf.Min(currentHealth, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        RefreshUI();
    }

    // -------------------------------------------------------------------------
    // Private Helpers
    // -------------------------------------------------------------------------

    private void Die()
    {
        isDead = true;
        OnDied?.Invoke();
        Debug.Log("Player died!");
    }

    private void RefreshUI()
    {
        // Hook your HealthBarUI here once you've built it,
        // or subscribe to the OnHealthChanged event from a separate UI script.
    }
}