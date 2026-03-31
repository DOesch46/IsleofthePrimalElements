using UnityEngine;
using System;

/// <summary>
/// Simple player health component.
///
/// SETUP:
/// - Attach to the same Player GameObject as PlayerController.
/// - Wire up the HealthBarUI in the Inspector if you have one.
/// - Bosses and hazards call TakeDamage(amount) directly.
///
/// NOTE: If you already have a PlayerHealth.cs in your project, skip this file.
/// Just make sure your existing version has a public TakeDamage(float) method
/// so FallingRock and TerradonController can call it.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Health")]
    [SerializeField] private float maxHealth       = 100f;
    [SerializeField] private float invincibleTime  = 0.8f;   // brief invincibility after a hit

    // HealthBarUI reference removed — wire up via the OnHealthChanged event instead

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    /// <summary>Fired when health changes. Passes (current, max).</summary>
    public event Action<float, float> OnHealthChanged;

    /// <summary>Fired when the player reaches 0 HP.</summary>
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

    /// <summary>
    /// Deal damage to the player.
    /// Called by FallingRock, TerradonController shockwave, hazard zones, etc.
    /// </summary>
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

    /// <summary>Heals the player up to maxHealth.</summary>
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

    // -------------------------------------------------------------------------
    // Private Helpers
    // -------------------------------------------------------------------------

    private void Die()
    {
        isDead = true;
        OnDied?.Invoke();
        Debug.Log("Player died!");
        // Hook GameManager.Instance.OnPlayerDeath() here if you have one
    }

    private void RefreshUI()
    {
        // Hook your HealthBarUI here once you've built it,
        // or subscribe to the OnHealthChanged event from a separate UI script.
    }
}
