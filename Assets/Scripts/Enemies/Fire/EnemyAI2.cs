using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI2 : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Settings
    // -------------------------------------------------------------------------

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float stopDistance = 0.6f;

    [Header("Activation")]
    [Tooltip("If true, enemy waits until the torch puzzle is solved before chasing.")]
    [SerializeField] private bool waitForPuzzle = true;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    private Rigidbody2D rb;
    private Transform player;
    private Animator animator;
    private TorchPuzzle torchPuzzle;  // ← NEW

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
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No GameObject with tag 'Player' found.");
        }

        // Find the torch puzzle in the scene  ← NEW
        torchPuzzle = FindFirstObjectByType<TorchPuzzle>();
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        // Don't chase until puzzle is solved  ← NEW
        if (waitForPuzzle && torchPuzzle != null && !torchPuzzle.IsSolved)
        {
            UpdateAnimator(Vector2.zero);
            return;
        }

        // Don't move if currently attacking
        EnemyDamage dmg = GetComponent<EnemyDamage>();
        if (dmg != null && dmg.IsAttacking)
        {
            UpdateAnimator(Vector2.zero);
            return;
        }

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

        bool isMoving = direction.sqrMagnitude > 0.01f;
        animator.SetBool(AnimRun, isMoving);

        if (Mathf.Abs(direction.x) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction.x < 0 ? -1f : 1f);
            transform.localScale = scale;
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