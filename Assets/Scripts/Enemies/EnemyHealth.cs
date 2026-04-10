using UnityEngine;
using System;
using UnityEngine.SceneManagement;

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
    public event Action OnDied;

    // Private State
    public bool isFinalBoss = false;

    private float currentHealth;
    private bool isInvulnerable = false;
    private Animator animator;
    private bool isDead = false;

    private static readonly int AnimHit = Animator.StringToHash("Hit");
    private static readonly int AnimDeath = Animator.StringToHash("Death");

    // Getters
    public float GetMaxHealth() => maxHealth;
    public float GetCurrentHealth() => currentHealth;
    public bool IsDead => isDead;
    public float HealthFraction => currentHealth / maxHealth;
    public GameObject RewardPickupPrefab => tridentDrop;
    public ElementType RewardElement => elementReward;

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

    public void TakeDamage(float damage)
    {
        if (isInvulnerable || isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

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

    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        bool hasCustomBossCleanup = GetComponent<EarthBossDeathHandler>() != null;

        Debug.Log($"{gameObject.name} died!");

        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        EnemyDamage dmg = GetComponent<EnemyDamage>();
        if (dmg != null) dmg.enabled = false;

        if (animator != null)
            animator.SetTrigger(AnimDeath);

        OnDied?.Invoke();

        if (isBoss)
        {
            Debug.Log($"{gameObject.name}: Boss died. Processing boss death cleanup.");

            if (tridentDrop != null)
            {
                Instantiate(tridentDrop, transform.position, Quaternion.identity);
            }
            else if (elementReward != ElementType.None && GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.CollectElement(elementReward);
            }

            OnEnemyDied?.Invoke(gameObject);

            if (hasCustomBossCleanup)
            {
                Debug.Log($"{gameObject.name}: Custom boss death handler found. Skipping generic cleanup.");
                return;
            }

            DropCoins();
            StartCoroutine(DestroyAfterDeathAnim());
            return;
        }

        DropCoins();

        if (LevelObjective.Instance != null)
            LevelObjective.Instance.EnemyDefeated();

        OnEnemyDied?.Invoke(gameObject);

        StartCoroutine(DestroyAfterDeathAnim());
    }

    private System.Collections.IEnumerator DestroyAfterDeathAnim()
    {
        yield return new WaitForSeconds(1f);

        if (isFinalBoss)
        {
            Debug.Log("Final Boss defeated → VictoryScene");
            SceneManager.LoadScene("VictoryScene");
            yield break;
        }

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
