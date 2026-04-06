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
    [SerializeField] private float stopDistance = 1.2f;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private Rigidbody2D rb;
    private Transform player;
    private Animator animator;

    private static readonly int AnimMoveX = Animator.StringToHash("MoveX");
    private static readonly int AnimMoveY = Animator.StringToHash("MoveY");
    private static readonly int AnimSpeed = Animator.StringToHash("Speed");

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
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No GameObject with tag 'Player' found.");
        }
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position);
        float distance = direction.magnitude;

        if (distance <= detectionRange && distance > stopDistance)
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

        animator.SetFloat(AnimMoveX, direction.x);
        animator.SetFloat(AnimMoveY, direction.y);
        animator.SetFloat(AnimSpeed, direction.sqrMagnitude);
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
