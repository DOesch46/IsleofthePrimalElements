using UnityEngine;

/// <summary>
/// Simple enemy AI that follows the player when in detection range.
/// Uses Rigidbody2D for movement to work with the top-down physics system.
///
/// SETUP:
/// 1. Attach to enemy GameObject (alongside EnemyHealth).
/// 2. Requires Rigidbody2D (added automatically) and a Collider2D.
/// 3. Tag the Player GameObject as "Player".
/// 4. Adjust detection range and speed in the Inspector.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float stopDistance = 0.6f;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private Rigidbody2D rb;
    private Transform player;
    private Animator animator;

    // Golem controller parameters
    private static readonly int AnimRun = Animator.StringToHash("Run");

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.bodyType = RigidbodyType2D.Kinematic;

        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        ResolvePlayerReference(logResult: true);
    }

    private void FixedUpdate()
    {
        if (player == null)
            ResolvePlayerReference();

        if (player == null) return;

        EnemyDamage dmg = GetComponent<EnemyDamage>();

        // Don't move if currently attacking
        if (dmg != null && dmg.IsAttacking)
        {
            UpdateAnimator(Vector2.zero);
            return;
        }

        Vector2 direction = (player.position - transform.position);
        float distance = direction.magnitude;
        float effectiveStopDistance = stopDistance;

        if (dmg != null && dmg.DamageRange > 0f)
        {
            // Enemies must be allowed to step into their actual hit range.
            effectiveStopDistance = Mathf.Min(stopDistance, dmg.DamageRange * 0.9f);
        }

        if (distance <= detectionRange && distance > effectiveStopDistance)
        {
            Vector2 moveDir = direction.normalized;
            rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
            UpdateAnimator(moveDir);
        }
        else
        {
            UpdateAnimator(Vector2.zero);
        }
    }

    // -------------------------------------------------------------------------
    // Animation
    // -------------------------------------------------------------------------

    private void UpdateAnimator(Vector2 direction)
    {
        if (animator == null) return;

        bool isMoving = direction.sqrMagnitude > 0.01f;
        animator.SetBool(AnimRun, isMoving);

        // Flip sprite to face movement direction
        if (Mathf.Abs(direction.x) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction.x < 0 ? -1f : 1f);
            transform.localScale = scale;
        }
    }

    private void ResolvePlayerReference(bool logResult = false)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;

            if (logResult)
                Debug.Log($"{gameObject.name}: Tracking player '{playerObj.name}'.");
        }
        else if (logResult)
        {
            Debug.LogWarning($"{gameObject.name}: No GameObject with tag 'Player' found.");
        }
    }

    // -------------------------------------------------------------------------
    // Editor Gizmos
    // -------------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
