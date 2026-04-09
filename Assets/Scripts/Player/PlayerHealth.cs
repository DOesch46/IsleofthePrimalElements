using UnityEngine;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Health")]
    [SerializeField] private float maxHealth       = 100f;
    [SerializeField] private float invincibleTime  = 0.8f;

    [Header("Death Flow")]
    [SerializeField] private bool autoRespawnIfAvailable = true;
    [SerializeField] private float respawnDelay = 1.25f;

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
    private bool isRespawning = false;

    private Rigidbody2D rb2d;
    private MovementSystem movementSystem;
    private PlayerController playerController;
    private PlayerCombat playerCombat;
    private InteractionSystem interactionSystem;
    private DashSystem dashSystem;
    private PlayerRespawn playerRespawn;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        currentHealth = maxHealth;
        rb2d = GetComponent<Rigidbody2D>();
        movementSystem = GetComponent<MovementSystem>();
        playerController = GetComponent<PlayerController>();
        playerCombat = GetComponent<PlayerCombat>();
        interactionSystem = GetComponent<InteractionSystem>();
        dashSystem = GetComponent<DashSystem>();
        playerRespawn = GetComponent<PlayerRespawn>();
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
        if (amount <= 0f)
        {
            Debug.LogWarning($"{name}: TakeDamage ignored because amount was {amount}.");
            return;
        }

        if (isDead)
        {
            Debug.Log($"{name}: TakeDamage ignored because the player is already dead.");
            return;
        }

        if (invincibleTimer > 0f)
        {
            Debug.Log($"{name}: TakeDamage ignored because invincibility is active for {invincibleTimer:F2}s.");
            return;
        }

        Debug.Log($"{name}: Taking {amount} damage. Current health before hit: {currentHealth}/{maxHealth}.");

        currentHealth  -= amount;
        currentHealth   = Mathf.Max(currentHealth, 0f);
        invincibleTimer = invincibleTime;

        Debug.Log($"{name}: Health after hit: {currentHealth}/{maxHealth}.");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        RefreshUI();

        if (currentHealth <= 0f)
        {
            Debug.Log($"{name}: Health reached 0. Triggering death.");
            Die();
        }
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
        if (isDead)
            return;

        isDead = true;
        isRespawning = false;

        Debug.Log($"{name}: Player death triggered.");

        if (movementSystem != null)
            movementSystem.SetMovementEnabled(false);

        if (rb2d != null)
            rb2d.linearVelocity = Vector2.zero;

        if (playerCombat != null)
            playerCombat.CancelWaveCharge();

        SetGameplayEnabled(false);
        OnDied?.Invoke();

        if (autoRespawnIfAvailable && playerRespawn != null)
        {
            Debug.Log($"{name}: Respawn handler found. Starting respawn flow.");
            StartCoroutine(RespawnRoutine());
        }
        else
        {
            Debug.LogWarning($"{name}: No respawn handler available. Player remains dead until another system handles recovery.");
        }
    }

    private IEnumerator RespawnRoutine()
    {
        if (isRespawning)
            yield break;

        isRespawning = true;
        yield return new WaitForSeconds(respawnDelay);

        if (playerRespawn != null)
            playerRespawn.Respawn();

        currentHealth = maxHealth;
        invincibleTimer = invincibleTime;
        isDead = false;
        isRespawning = false;

        if (movementSystem != null)
            movementSystem.SetMovementEnabled(true);

        SetGameplayEnabled(true);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        RefreshUI();

        Debug.Log($"{name}: Player respawned at full health {currentHealth}/{maxHealth}.");
    }

    private void SetGameplayEnabled(bool enabled)
    {
        if (playerController != null)
            playerController.enabled = enabled;

        if (playerCombat != null)
            playerCombat.enabled = enabled;

        if (interactionSystem != null)
            interactionSystem.enabled = enabled;

        if (dashSystem != null)
            dashSystem.enabled = enabled;
    }

    private void RefreshUI()
    {
        // Hook your HealthBarUI here once you've built it,
        // or subscribe to the OnHealthChanged event from a separate UI script.
    }
}
