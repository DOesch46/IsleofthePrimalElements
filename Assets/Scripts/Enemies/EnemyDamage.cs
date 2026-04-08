using UnityEngine;

/// <summary>
/// Deals contact damage to the player when the enemy is close enough.
/// Uses distance checking instead of collider callbacks for reliability
/// with Kinematic Rigidbody2D enemies.
///
/// SETUP:
/// 1. Attach to the enemy GameObject (alongside EnemyAI and EnemyHealth).
/// 2. Tag the Player as "Player" and ensure it has PlayerHealth.
/// </summary>
public class EnemyDamage : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Damage Settings")]
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float damageCooldown = 1f;
    [SerializeField] private float damageRange = 0.8f;

    [Header("Attack Animation")]
    [Tooltip("Time before the attack animation deals damage (windup).")]
    [SerializeField] private float attackWindup = 0.4f;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private float cooldownTimer = 0f;
    private Transform player;
    private PlayerHealth playerHealth;
    private Animator animator;
    private EnemyAI enemyAI;
    private bool isAttacking = false;

    /// <summary>Other scripts can check this to know if the enemy is mid-attack.</summary>
    public bool IsAttacking => isAttacking;

    // Golem controller parameter
    private static readonly int AnimAttack = Animator.StringToHash("Attack");

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Start()
    {
        animator = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
            Debug.Log($"EnemyDamage: Found player. PlayerHealth: {playerHealth != null}");
        }
        else
        {
            Debug.LogWarning("EnemyDamage: No Player found!");
        }
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (player == null || playerHealth == null) return;
        if (cooldownTimer > 0f) return;
        if (isAttacking) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= damageRange)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        isAttacking = true;
        cooldownTimer = damageCooldown;

        // Stop movement during attack
        if (enemyAI != null)
            enemyAI.enabled = false;

        // Set Run to false and wait a frame so the animator
        // transitions back to Idle before we trigger Attack
        if (animator != null)
            animator.SetBool(Animator.StringToHash("Run"), false);

        yield return null; // wait one frame for Idle state

        // Now trigger the attack animation (works from Idle state)
        if (animator != null)
            animator.SetTrigger(AnimAttack);

        // Wait for windup before dealing damage
        yield return new WaitForSeconds(attackWindup);

        // Deal damage (check range again in case player moved away)
        if (player != null && playerHealth != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= damageRange * 1.5f)
            {
                playerHealth.TakeDamage(contactDamage);
                Debug.Log($"Enemy dealt {contactDamage} damage to player!");
            }
        }

        // Wait for rest of animation to finish
        yield return new WaitForSeconds(0.4f);

        // Re-enable movement
        if (enemyAI != null)
            enemyAI.enabled = true;

        isAttacking = false;
    }
}
