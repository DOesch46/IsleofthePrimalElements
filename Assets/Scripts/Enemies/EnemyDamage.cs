using System.Collections;
using UnityEngine;

/// <summary>
/// Deals contact damage to the player when the enemy is close enough.
/// Uses distance checking instead of collider callbacks for reliability
/// with Kinematic Rigidbody2D enemies.
///
//// SETUP:
/// 1. Attach to the enemy GameObject (alongside EnemyAI and EnemyHealth).
/// 2. Tag the Player as "Player" and ensure it has PlayerHealth.
/// 3. Add an Animator to the enemy.
/// 4. In the Animator, create a Trigger parameter named "Attack".
/// 5. Make sure your attack animation transitions from Idle -> Attack
///    using the "Attack" trigger, then back to Idle after finishing.
/// </summary>
public class EnemyDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float damageCooldown = 1f;
    [SerializeField] private float damageRange = 0.8f;

    [Header("Attack Animation")]
    [Tooltip("Time before the attack animation deals damage.")]
    [SerializeField] private float attackWindup = 0.4f;

    [Tooltip("Extra time after damage before movement resumes.")]
    [SerializeField] private float attackRecovery = 0.4f;

    [Tooltip("Animator trigger used for the enemy attack animation.")]
    [SerializeField] private string attackTriggerName = "Attack";

    [Tooltip("Animator bool used for movement animation.")]
    [SerializeField] private string runBoolName = "Run";

    private float cooldownTimer = 0f;
    private Transform player;
    private PlayerHealth playerHealth;
    private Animator animator;
    private EnemyAI enemyAI;
    private bool isAttacking = false;

    public bool IsAttacking => isAttacking;

    private int animAttackHash;
    private int runBoolHash;

    private void Start()
    {
        animator = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();

        if (string.IsNullOrWhiteSpace(attackTriggerName))
            attackTriggerName = "Attack";

        if (string.IsNullOrWhiteSpace(runBoolName))
            runBoolName = "Run";

        animAttackHash = Animator.StringToHash(attackTriggerName);
        runBoolHash = Animator.StringToHash(runBoolName);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
            Debug.Log($"{name}: Found player. PlayerHealth attached: {playerHealth != null}");
        }
        else
        {
            Debug.LogWarning($"{name}: No GameObject with tag 'Player' found.");
        }

        if (animator == null)
        {
            Debug.LogWarning($"{name}: No Animator found. Attack animation will not play.");
        }
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (player == null || playerHealth == null)
            return;

        if (cooldownTimer > 0f)
            return;

        if (isAttacking)
            return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= damageRange)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        cooldownTimer = damageCooldown;

        Debug.Log($"{name}: Starting attack.");

        if (enemyAI != null)
            enemyAI.enabled = false;

        if (animator != null)
        {
            animator.SetBool(runBoolHash, false);
            yield return null;
            animator.SetTrigger(animAttackHash);
            Debug.Log($"{name}: Triggered attack animation using '{attackTriggerName}'.");
        }

        yield return new WaitForSeconds(attackWindup);

        if (player != null && playerHealth != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= damageRange * 1.5f)
            {
                playerHealth.TakeDamage(contactDamage);
                Debug.Log($"{name}: Dealt {contactDamage} damage to player.");
            }
            else
            {
                Debug.Log($"{name}: Player moved out of range before damage landed.");
            }
        }

        yield return new WaitForSeconds(attackRecovery);

        if (enemyAI != null)
            enemyAI.enabled = true;

        isAttacking = false;
        Debug.Log($"{name}: Attack finished.");
    }
}