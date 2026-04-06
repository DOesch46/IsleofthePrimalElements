using UnityEngine;
using System;

/// <summary>
/// Enemy health component that handles taking damage, dying,
/// dropping coins, and notifying the level objective system.
///
/// SETUP:
/// 1. Attach to the enemy GameObject (alongside EnemyAI and EnemyDamage).
/// 2. Assign a coin prefab in the Inspector for coin drops.
/// 3. Adjust maxHealth and coinDropCount as needed.
/// </summary>
public class EnemyHealth : MonoBehaviour, IDamageable
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Health")]
    [SerializeField] private float maxHealth = 50f;

    [Header("Coin Drop")]
    [Tooltip("Coin prefab to spawn on death. Must have CoinPickup component.")]
    [SerializeField] private GameObject coinPrefab;

    [Tooltip("Number of coins to drop on death.")]
    [SerializeField] private int coinDropCount = 3;

    [Tooltip("How far coins scatter from the death position.")]
    [SerializeField] private float coinScatterRadius = 0.5f;

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    /// <summary>Fired when this enemy dies. Passes this enemy's GameObject.</summary>
    public static event Action<GameObject> OnEnemyDied;

    /// <summary>Fired when health changes. Passes (current, max).</summary>
    public event Action<float, float> OnHealthChanged;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private float currentHealth;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // -------------------------------------------------------------------------
    // IDamageable Implementation
    // -------------------------------------------------------------------------

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    // -------------------------------------------------------------------------
    // Death
    // -------------------------------------------------------------------------

    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");

        // Drop coins
        DropCoins();

        // Notify the level objective system
        if (LevelObjective.Instance != null)
        {
            LevelObjective.Instance.EnemyDefeated();
        }

        // Fire event for any listeners
        OnEnemyDied?.Invoke(gameObject);

        Destroy(gameObject);
    }

    private void DropCoins()
    {
        if (coinPrefab == null) return;

        for (int i = 0; i < coinDropCount; i++)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * coinScatterRadius;
            Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y, 0f);
            Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        }
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public float GetHealthFraction() => currentHealth / maxHealth;
}
