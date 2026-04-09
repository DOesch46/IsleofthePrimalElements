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

    [SerializeField] private bool isBoss = false;
    [SerializeField] private ElementType elementReward;
    
    [Header("Health")]
    [SerializeField] private float maxHealth = 50f;

    [Header("Coin Drop")]
    [Tooltip("Coin prefab to spawn on death. Must have CoinPickup component.")]
    [SerializeField] private GameObject coinPrefab;

    [Tooltip("Number of coins to drop on death.")]
    [SerializeField] private int coinDropCount = 3;

    [Tooltip("How far coins scatter from the death position.")]
    [SerializeField] private float coinScatterRadius = 0.5f;

    [SerializeField] private GameObject tridentDrop;
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
    private bool isInvulnerable = false;
    private Animator animator;
    private bool isDead = false;

    // Golem controller parameter hashes
    private static readonly int AnimHit   = Animator.StringToHash("Hit");
    private static readonly int AnimDeath = Animator.StringToHash("Death");

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------
    public float GetMaxHealth()
{
    return maxHealth;
}
    public float GetCurrentHealth()
{
    return currentHealth;
}

public void SetCurrentHealth(float value)
{
    currentHealth = value;
    OnHealthChanged?.Invoke(currentHealth, maxHealth);
}
    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    // -------------------------------------------------------------------------
    // IDamageable Implementation
    // -------------------------------------------------------------------------

    public void TakeDamage(float damage)
    {
        if (isInvulnerable || isDead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
        else
        {
            // Play hit animation
            if (animator != null)
                animator.SetTrigger(AnimHit);
        }
    }

    // -------------------------------------------------------------------------
    // Invulnerability — used by BossCutsceneTrigger to protect boss
    // -------------------------------------------------------------------------

    /// <summary>
    /// Makes this enemy immune to all damage.
    /// </summary>
    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }

    // -------------------------------------------------------------------------
    // Death
    // -------------------------------------------------------------------------

    private void Die()
    {

        isDead = true;
        Debug.Log($"{gameObject.name} died!");

        // Stop enemy from moving/attacking
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        EnemyDamage dmg = GetComponent<EnemyDamage>();
        if (dmg != null) dmg.enabled = false;

        // Play death animation
        if (animator != null)
            animator.SetTrigger(AnimDeath);

       // Drop coins
DropCoins();

// Spawn trident if this is the boss
if (isBoss && tridentDrop != null)
{
    Instantiate(tridentDrop, transform.position, Quaternion.identity);
}

// ADD THIS (ABILITY UNLOCK)
Debug.Log("isBoss = " + isBoss);

if (isBoss)
{
    Debug.Log("UNLOCKING ABILITY");
    if (isBoss)
{
    GameProgressManager.Instance.CollectElement(elementReward);
}
}
// Notify the level objective system
if (LevelObjective.Instance != null)
{
    LevelObjective.Instance.EnemyDefeated();
}

        // Fire event for any listeners
        OnEnemyDied?.Invoke(gameObject);

        // Wait for death animation then destroy
        StartCoroutine(DestroyAfterDeathAnim());
    }

    private System.Collections.IEnumerator DestroyAfterDeathAnim()
    {
        // Wait for death animation to play
        yield return new WaitForSeconds(1f);
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
