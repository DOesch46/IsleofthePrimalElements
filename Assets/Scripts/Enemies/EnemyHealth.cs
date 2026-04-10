using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private bool isBoss = false;
    [SerializeField] private ElementType elementReward;

    [Header("Health")]
    [SerializeField] private float maxHealth = 50f;

    [Header("Coin Drop")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int coinDropCount = 3;
    [SerializeField] private float coinScatterRadius = 0.5f;

    [SerializeField] private GameObject tridentDrop;

    // Events
    public static event Action<GameObject> OnEnemyDied;
    public event Action<float, float> OnHealthChanged;
    public event Action OnDied;  // ✅ Added so death handler can subscribe

    // State
    private float currentHealth;
    private bool isInvulnerable = false;
    private Animator animator;
    private bool isDead = false;

    private static readonly int AnimHit = Animator.StringToHash("Hit");
    private static readonly int AnimDeath = Animator.StringToHash("Death");

    // -------------------------------------------------------------------------
    // Public Getters/Setters
    // -------------------------------------------------------------------------

    public float GetMaxHealth() => maxHealth;
    public float GetCurrentHealth() => currentHealth;
    public bool IsDead => isDead;
    public float HealthFraction => currentHealth / maxHealth;

    public void SetCurrentHealth(float value)
    {
        currentHealth = value;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    // -------------------------------------------------------------------------
    // IDamageable
    // -------------------------------------------------------------------------

    public void TakeDamage(float damage)
    {
        if (isInvulnerable || isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // ✅ Hit flash support (for bosses and regular enemies)
        HitFlash flash = GetComponent<HitFlash>();
        if (flash != null)
            flash.Flash();

        if (currentHealth <= 0f)
        {
            Die();
        }
        else
        {
            if (animator != null)
                animator.SetTrigger(AnimHit);
        }
    }

    // -------------------------------------------------------------------------
    // Invulnerability
    // -------------------------------------------------------------------------

    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }

    // -------------------------------------------------------------------------
    // Death
    // -------------------------------------------------------------------------

    private void Die()
    {
        if (isDead) return;
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

        // ✅ Fire the OnDied event (EarthBossDeathHandler listens to this)
        OnDied?.Invoke();

        // ✅ If this is a boss, let the DeathHandler manage coins, portal, etc.
        if (isBoss)
        {
            Debug.Log($"{gameObject.name}: Boss died. Letting DeathHandler manage cleanup.");

            if (tridentDrop != null)
            {
                Instantiate(tridentDrop, transform.position, Quaternion.identity);
            }
            else if (elementReward != ElementType.None && GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.CollectElement(elementReward);
            }

            // Fire event for listeners
            OnEnemyDied?.Invoke(gameObject);

            // ✅ DON'T destroy — let the DeathHandler handle it
            return;
        }

        // --- Regular enemy death (not a boss) ---
        DropCoins();

        if (LevelObjective.Instance != null)
            LevelObjective.Instance.EnemyDefeated();

        OnEnemyDied?.Invoke(gameObject);

        StartCoroutine(DestroyAfterDeathAnim());
    }

    private System.Collections.IEnumerator DestroyAfterDeathAnim()
    {
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

    public float GetHealthFraction() => currentHealth / maxHealth;
}